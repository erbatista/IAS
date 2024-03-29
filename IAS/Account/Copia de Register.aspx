﻿<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="IAS.Account.Register" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: Title %>.</h2>
    <p class="text-danger">
        <asp:Literal runat="server" ID="ErrorMessage" />
    </p>

    <div class="form-horizontal">
        <h4>Create una cuenta.</h4>
        <hr />
        <asp:ValidationSummary runat="server" CssClass="text-danger" />
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtUserName" CssClass="col-md-2 control-label">User name</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtUserName" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtUserName"
                    CssClass="text-danger" ErrorMessage="The user name field is required." />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtPassword" CssClass="col-md-2 control-label">txtPassword</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtPassword" TextMode="Password" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword"
                    CssClass="text-danger" ErrorMessage="The password field is required." />
            </div>
        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtConfirmPassword" CssClass="col-md-2 control-label">Confirm password</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtConfirmPassword" TextMode="Password" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtConfirmPassword"
                    CssClass="text-danger" Display="Dynamic" ErrorMessage="The confirm password field is required." />
                <asp:CompareValidator runat="server" ControlToCompare="txtPassword" ControlToValidate="txtConfirmPassword"
                    CssClass="text-danger" Display="Dynamic" ErrorMessage="The password and confirmation password do not match." />
            </div>

        </div>
        <div class="form-group">

            <asp:Label runat="server" AssociatedControlID="txtFirstName" CssClass="col-md-2 control-label">First Name</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtFirstName" CssClass="form-control" />
            </div>

        </div>
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="txtLastName" CssClass="col-md-2 control-label">Last Name</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtLastName" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" CssClass="col-md-2 control-label">Email</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtEmail" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <asp:Label ID="lblPhones" runat="server" AssociatedControlID="txtPhones" CssClass="col-md-2 control-label">Telefonos (separados por coma)</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="txtPhones" CssClass="form-control" />
            </div>
        </div>
        <div class="form-group">
            <%--            <asp:Label runat="server" AssociatedControlID="ddlRoles" CssClass="col-md-2 control-label">Roles</asp:Label>
            <div class="col-md-10" style="vertical-align:central">
                <asp:DropDownList ID="ddlRoles" runat="server" CssClass="col-md-2 control-label" DataTextField="group" DataValueField="id_group" Width="300px"></asp:DropDownList>
            </div>--%>


            <asp:UpdatePanel ID="upnlRoles" runat="server">

                <ContentTemplate>

                    <asp:Label ID="ErrorLabel" runat="server" Visible="False" CssClass="msg-box bg-danger" />

                    <asp:ListView ID="UserRolesListView" runat="server"
                        ItemType="Microsoft.AspNet.Identity.EntityFramework.IdentityUserRole" DataKeyNames="RoleId"
                        SelectMethod="GetUserRole">
                        <LayoutTemplate>
                            <div class="table responsive">
                                <table class="table table-striped">
                                    <thead>
                                        <tr>
                                            <th style="width: 25px;">&nbsp;</th>
                                            <th>Este usuario pertenece a estos roles</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr runat="server" id="itemPlaceholder" />
                                    </tbody>
                                </table>
                            </div>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <asp:CheckBox CssClass="selectOneRole" ID="chkSelectOneRole" runat="server" ClientIDMode="Static" />
                                </td>
                                <td>
                                    <asp:Label ID="lblRoleName" runat="server" Text='<%# Item.Role.Name %>' />
                                </td>
                            </tr>
                        </ItemTemplate>
                        <EmptyDataTemplate>
                            <div class="msg-box bg-warning">Este usuario no pertenece a ningun role</div>
                        </EmptyDataTemplate>
                    </asp:ListView>
                    <div class="text-right">

                        <asp:Button ID="AddButton" runat="server" Text="Asignar" CssClass="btn btn-info" OnClick="AddButton_Click" />
                        <asp:Button ID="RemoveButton" runat="server" Text="Quitar" CssClass="btn btn-danger" OnClick="RemoveButton_Click" />

                    </div>
                    <asp:ListView ID="RolesListView" runat="server"
                        ItemType="Microsoft.AspNet.Identity.EntityFramework.IdentityRole" DataKeyNames="Id"
                        SelectMethod="GetRoles"
                        CheckBoxes="true">
                        <LayoutTemplate>
                            <div class="table responsive">
                                <table class="table table-striped">
                                    <thead>
                                        <tr>
                                            <th style="width: 25px;">&nbsp;</th>
                                            <th>No pertenece a estos roles</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr runat="server" id="itemPlaceholder" />
                                    </tbody>
                                </table>
                            </div>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <asp:CheckBox CssClass="selectOneRole" ID="chkSelectOneRole" runat="server" ClientIDMode="Static" />
                                </td>
                                <td>
                                    <asp:Label ID="lblRoleName" runat="server" Text='<%# Item.Name%>' />
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:ListView>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <asp:Button runat="server" OnClick="CreateUser_Click" Text="Registrar" CssClass="btn btn-default" />
            </div>
        </div>


    </div>
</asp:Content>
