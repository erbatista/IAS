﻿//«Copyright 2014 Balcazz HT, http://www.balcazzht.com»

//This file is part of IAS | Insurance Advanced Services.

//IAS is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//IAS is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see <http://www.gnu.org/licenses/>.


using IAS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Validation;
using System.Web.Security;
using System.Data.SqlClient;
using IAS.Infrastructure;
using System.Globalization;
using IAS.Constants;

namespace IAS.CaseManagment {

    public partial class CaseTransitionManager : System.Web.UI.UserControl {

        private void changeEffectiveDateEnableProperty()
        {
            if (this.ddlNewState.Items.Count == 0)
            {
                this.txtEffectiveDate.Enabled = false;
            }
            else
            {
                long caseID = long.Parse( hdnCaseID.Value );
                long newStateID = long.Parse(this.ddlNewState.SelectedValue);

                var db = new ApplicationDbContext();
                //var theCase = Session[SessionKeys.CurrentCase] as Case; 
                var theCase = db.Cases.SingleOrDefault(c => c.CaseID == caseID);
                if ( null == theCase)
                {
                    ErrorLabel.Text = string.Format(
                        "No hay ningun caso activo actualmente. Por favor contactar al administrador del sistema.Datos adicionales: Caso actual {0}",
                        theCase.CaseID);
                    return;
                }

                var stateTransition = db.WorkflowStateTransitions
                    .SingleOrDefault(t => t.InitialStateID == theCase.StateID && t.FinalStateID == newStateID && t.WorkflowID == theCase.WorkflowID);
                if (null == stateTransition)
                {
                    ErrorLabel.Text = string.Format(
                        "No se encontro una transicion de estado valida paar este Workflow, por favor contactar al administrador del sistema.Datos adicionales: flujo {0}, Estado actual {1}, estado final {2}",
                        theCase.WorkflowID, theCase.StateID,newStateID);                    
                    return;
                }

                if (stateTransition.EditableEffectiveDate != null && stateTransition.EditableEffectiveDate == true)
                    this.txtEffectiveDate.Enabled = true;
                else
                    this.txtEffectiveDate.Enabled = false;
            }
        }

        private System.Data.Entity.Infrastructure.DbRawSqlQuery<UserAssignmet> GetNewResponsible(long pStateID)
        {
            try
            {
                var db = new ApplicationDbContext();

                var users = db.Database.SqlQuery<UserAssignmet>(
                    "SELECT u.Id UserID, COUNT(u.Id) Caseload " +
                    "FROM AspNetUsers u LEFT JOIN [Case] c ON c.UserID = u.Id  " +
                    "WHERE  u.Id IN( " +
                    "SELECT dbo.[UserStateTransition].UserID " +
                    "FROM dbo.[UserStateTransition] INNER JOIN dbo.[WorkflowStateTransition]  " +
                    "ON dbo.[UserStateTransition].WorkflowStateTransitionID = dbo.[WorkflowStateTransition].WorkflowStateTransitionID " +
                    "WHERE dbo.[WorkflowStateTransition].InitialStateID = @StateID " +
                    "UNION " +
                    "SELECT dbo.AspNetUserRoles.UserId " +
                    "FROM dbo.[RoleStateTransition] INNER JOIN dbo.[WorkflowStateTransition]  " +
                    "ON dbo.[RoleStateTransition].WorkflowStateTransitionID = dbo.[WorkflowStateTransition].WorkflowStateTransitionID " +
                    "INNER JOIN dbo.AspNetRoles ON dbo.[RoleStateTransition].RoleID = dbo.AspNetRoles.Id " +
                    "INNER JOIN dbo.AspNetUserRoles ON dbo.AspNetUserRoles.RoleId = dbo.AspNetRoles.Id " +
                    "WHERE dbo.[WorkflowStateTransition].InitialStateID = @StateID ) group by u.Id", new SqlParameter("@StateID", pStateID)
                    );

                return users;
            }
            catch (Exception exc)
            {
                return null;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "js_ctrl_init",
                "$('#" + this.txtEffectiveDate.ClientID + "').datetimepicker({  lang: 'es', format:'d/m/Y H:i', step: 30 });", 
                true
            );
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (null == Request.QueryString["CaseID"])
                Response.Redirect("~/Default.aspx", false);
        }

        public IQueryable<CaseTransition> GetData([QueryString("CaseID")] long? caseID)
        {
            if (!caseID.HasValue)
                return null;

            hdnCaseID.Value = caseID.Value.ToString();
            var db = new ApplicationDbContext();
            var theCase = db.Cases.Where(c => c.CaseID == caseID).SingleOrDefault();
            if ( null == theCase ) {
                Session[SessionKeys.CurrentCase] = null;
                return null;
            }
            else {
                Session[SessionKeys.CurrentCase] = theCase;
                return theCase.StateTransitions.OrderByDescending( s => s.TransitionDate ).AsQueryable();
            }
        }

        public IQueryable<State> GetNewStates([QueryString("CaseID")] long? caseID)
        {
            try
            {
                var userId = HttpContext.Current.User.Identity.GetUserId();
                var db = new ApplicationDbContext();
                var user = db.Users.SingleOrDefault(u => u.Id == userId);
                var theCase = db.Cases.Where(c => c.CaseID == caseID).SingleOrDefault();
                /*var userRoleIds = db.Users.Where( u => u.Id == userId ) Esto esta bien tambien
                    .SelectMany( u => u.Roles.Select( r => r.RoleId ) )
                    .ToList(); */

                var states = db.WorkflowStateTransitions
                    .Where(wst => wst.WorkflowID == theCase.WorkflowID && wst.InitialStateID == theCase.StateID)
                    .Join(db.UserStateTransitions.Where(ust => ust.UserID == userId),
                        wst => wst.WorkflowStateTransitionID,
                        ust => ust.WorkflowStateTransitionID,
                        (wst, ust) => wst.FinalState)
                    .Union(db.WorkflowStateTransitions
                        .Where(wst => wst.WorkflowID == theCase.WorkflowID && wst.InitialStateID == theCase.StateID)
                        .Join(db.RoleStateTransitions.Where(r => user.ApplicationRoles.Contains(r.RoleID)),
                            wst => wst.WorkflowStateTransitionID,
                            rst => rst.WorkflowStateTransitionID,
                            (wst, rst) => wst.FinalState));

                //var states1 = from wst in db.WorkflowStateTransitions
                //             join rst in db.RoleStateTransitions
                //                 on wst.WorkflowStateTransitionID equals rst.WorkflowStateTransitionID
                //             join role in db.Roles
                //                 on rst.RoleID equals role.Id
                //             where role.Users.Any( u => u.UserId == id )
                //             select wst.FinalState;

                //var states1 = from wst in db.WorkflowStateTransitions
                //              where wst.WorkflowID == theCase.WorkflowID && wst.InitialStateID == theCase.StateID
                //              join rst in db.RoleStateTransitions on wst.WorkflowStateTransitionID equals rst.WorkflowStateTransitionID
                //              where rst.RoleID in user.ApplicationRoles                              
                //              select wst.FinalState; 


                return states;
            }
            catch (Exception exc)
            {
               
                return null;
            }
        }

        protected void buttonChangeState_Click(object sender, EventArgs e)
        {
            var currentUserId = HttpContext.Current.User.Identity.GetUserId();
            var caseID = long.Parse(hdnCaseID.Value);
            
            var db = new ApplicationDbContext();
            var theCase = db.Cases.Where(c => c.CaseID == caseID).SingleOrDefault();
            if (null == theCase)
            {
                this.ErrorLabel.Text = "No es posible procesar el cambio de estado debido que no se puede recuperar la informacion del caso actual . <br />Actualice la pagina y vuelta a intetar procesar el cambio de estado.";
                this.ErrorLabel.Visible = true;
                return;
            }

            //var users = this.GetNewResponsible(int.Parse(this.ddlNewState.SelectedValue));
            //if (users == null || users.Count() == 0)
            //{
            //    this.ErrorLabel.Text = "No es posible procesar el cambio de estado porque no se definieron responsables para el mismo. <br />Contacte con el administrador del sistema.";
            //    this.ErrorLabel.Visible = true;
            //    return;
            //}
            //var newResponsableID = users.OrderBy(t => t.Caseload).First().UserID;

            DateTime effectiveDate;
            if ( this.txtEffectiveDate.Enabled == false  ) {
                effectiveDate = DateTime.Now;
            }
            else {
                try
                {
                    effectiveDate = DateTime.ParseExact(txtEffectiveDate.Text, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    effectiveDate = DateTime.Now;
                }
                
                //string[] components = txtEffectiveDate.Text.Split( '/' );                
                
                //int day = int.Parse( components[0] );
                //int month = int.Parse( components[1] );

                //string[] yearhour = components[2].Split( ' ' );                
                //string[] hourminute = yearhour[1].Split( ':' );

                //int year = int.Parse( yearhour[0] );
                //int hour = int.Parse( hourminute[0] );
                //int minutes = int.Parse( hourminute[1] );

                //effectiveDate = new DateTime( year, month, day, hour, minutes, 0 );
            }

            var transition = new CaseTransition();
            transition.CaseID = theCase.CaseID;
            transition.PreviousStateID = theCase.StateID;
            transition.NewStateID = int.Parse(this.ddlNewState.SelectedValue);
            transition.Comment = this.txtComments.Text;
            transition.UserID = currentUserId;
            transition.TransitionDate = DateTime.Now;
            transition.EffectiveDate = effectiveDate;

            theCase.StateID = transition.NewStateID;
            //theCase.UserID = newResponsableID;
            theCase.EffectiveDate = effectiveDate;
            theCase.StateTransitions.Add(transition);
            db.SaveChanges();

            this.ddlNewState.DataBind();
            this.CaseTransitionsListView.DataBind();

            //if (newResponsableID == currentUserId)
            //{
            //    this.ddlNewState.DataBind();
            //    this.CaseTransitionsListView.DataBind();
            //}
            //else
            //{
            //    this.buttonChangeState.Enabled = false;
            //    this.ddlNewState.Enabled = false;
            //    this.txtComments.Enabled = false;
            //    this.txtEffectiveDate.Enabled = false;
            //}

            this.txtComments.Text = string.Empty;
            this.txtEffectiveDate.Text = string.Empty;
            RaiseBubbleEvent(this, new CaseStateChangedEventArgs());

        }

        private class UserAssignmet {
            public UserAssignmet() {

            }
            public string UserID { get; set; }
            public int Caseload{get;set;}
        }

        protected void ddlNewState_SelectedIndexChanged(object sender, EventArgs e)
        {
            changeEffectiveDateEnableProperty();
        }

        protected void ddlNewState_DataBound(object sender, EventArgs e)
        {
            changeEffectiveDateEnableProperty();
        }

    }
}