//******************************************************************************************************
//  LatestVendorDiscoveryResult.cs - Gbtc
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
//  03/03/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel.DataAnnotations;
using openSPM.Attributes;

namespace openSPM.Models
{
    /// <summary>
    /// Model for openSPM.LatestVendorDiscoveryResult view.
    /// </summary>
    [PrimaryLabel("ReviewDate")]
    [IsDeletedFlag("IsDeleted")]
    public class LatestVendorDiscoveryResult
    {
        public int VendorID
        {
            get; set;
        }

        [PrimaryKey(true)]
        public int DiscoveryResultID
        {
            get; set;
        }

        public int VendorTypeKey
        {
            get; set;
        }

        [StringLength(200)]
        public string Name
        {
            get; set;
        }

        [StringLength(12)]
        public string Abbreviation
        {
            get; set;
        }

        [StringLength(200)]
        public string Company
        {
            get; set;
        }

        [Required]
        [Label("Security Patch Search Result")]
        public int ResultKey
        {
            get; set;
        }

        [Label("Discovery Notes")]
        public string Notes
        {
            get; set;
        }

        [Label("Checked Patch Availability On")]
        [InitialValue("new Date()")]
        public DateTime ReviewDate
        {
            get; set;
        }

        public bool Enabled
        {
            get; set;
        }

        public bool IsDeleted
        {
            get; set;
        }

        public DateTime CreatedOn
        {
            get; set;
        }

        public Guid CreatedByID
        {
            get; set;
        }
    }
}
