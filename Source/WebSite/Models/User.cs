﻿//******************************************************************************************************
// User.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/19/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel.DataAnnotations;
using openSPM.Attributes;

namespace openSPM.Models
{
    /// <summary>
    /// Model for openSPM.User table.
    /// </summary>
    public class User
    {
        [PrimaryKey(true)]
        public int ID
        {
            get; set;
        }

        [Label("Parent User")]
        public int parentID
        {
            get; set;
        }

        [Required]
        [Label("Security ID")]
        [StringLength(184)]
        public string SID
        {
            get; set;
        }

        [Required]
        [Label("SPM ID")]
        [StringLength(64)]
        public string SPMidentifier
        {
            get; set;
        }

        [Label("System Role Key")]
        public int systemRoleIntKey
        {
            get; set;
        }

        [Label("Operator Role Key")]
        public int operatorRoleIntKey
        {
            get; set;
        }

        [Label("Enabled")]
        public bool enabled
        {
            get; set;
        }

        [Required]
        public DateTime dtCreated
        {
            get; set;
        }

        [Required]
        [StringLength(184)]
        public string createdBySID
        {
            get; set;
        }

    }
}