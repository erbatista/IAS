//�Copyright 2014 Balcazz HT, http://www.balcazzht.com�

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


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAS.Models
{
    public partial class ClaimContactHistory
    {
        [Key]
        public long ContactID { get; set; }
        public long ClaimID { get; set; }
        public short ContactTypeID { get; set; }
        public short ContactDestinationID { get; set; }
        public System.DateTime ContactDate { get; set; }
        public string Message { get; set; }
        public virtual Claim Claim { get; set; }
        public virtual ContactDestination ContactDestination { get; set; }
        public virtual ContactType ContactType { get; set; }
    }
}
