//******************************************************************************************************
//  UserAccount.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/27/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel.DataAnnotations;
using openSPM.Attributes;

namespace openSPM.Models
{
    /// <summary>
    /// Model for openSPM.UserAccount table.
    /// </summary>
    public class UserAccount
    {
        [PrimaryKey(true)]
        public Guid ID { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Password { get; set; }

        [StringLength(200)]
        public string FirstName { get; set; }

        [StringLength(200)]
        public string LastName { get; set; }

        public Guid DefaultNodeID { get; set; }

        [StringLength(200)]
        public string Phone { get; set; }

        [StringLength(200)]
        public string Email { get; set; }

        public bool LockedOut { get; set; }

        [Label("Use Active Directory Authentication")]
        public bool UseADAuthentication { get; set; }

        public DateTime? ChangePasswordOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public DateTime UpdatedOn { get; set; }

        public string UpdatedBy { get; set; }
    }
}
