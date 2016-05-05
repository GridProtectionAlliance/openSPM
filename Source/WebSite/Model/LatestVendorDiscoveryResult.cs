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
using GSF.Data.Model;

namespace openSPM.Model
{
    /// <summary>
    /// Model for openSPM.LatestVendorDiscoveryResult view.
    /// </summary>
    [PrimaryLabel("ReviewDate")]
    [IsDeletedFlag("IsDeleted")]
    public class LatestVendorDiscoveryResult
    {
        public int VendorID { get; set; }

        [PrimaryKey]
        public int DiscoveryResultID{ get; set;}

        public int VendorTypeKey{ get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(12)]
        public string Abbreviation { get; set; }

        [StringLength(200)]
        public string Company { get; set; }

        public string Description { get; set; }

        [Label("Address")]
        [StringLength(200)]
        public string Address1 { get; set; }

        [Label("")]
        [StringLength(200)]
        public string Address2 { get; set; }

        [StringLength(200)]
        public string City { get; set; }

        [StringLength(200)]
        public string State { get; set; }

        [StringLength(200)]
        public string Country { get; set; }

        [Label("Zip-code")]
        [StringLength(10)]
        public string ZIP { get; set; }

        [Label("Web Site")]
        [StringLength(512)]
        [RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", ErrorMessage = "Invalid URL.")]
        public string Link { get; set; }

        [Label("Patch Notification Method")]
        public int NoticeMethodKey { get; set; }

        [Label("Primary Contact Name")]
        [StringLength(200)]
        public string ContactName1 { get; set; }

        [Label("Primary Contact E-Mail")]
        [StringLength(200)]
        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "Invalid e-mail address.")]
        public string ContactEmail1 { get; set; }

        [Label("Primary Contact Phone (Office)")]
        [StringLength(20)]
        public string ContactPhoneOfc1 { get; set; }

        [Label("Primary Contact Phone (Cell)")]
        [StringLength(20)]
        public string ContactPhoneCell1 { get; set; }

        [Label("Secondary Contact Name")]
        [StringLength(200)]
        public string ContactName2 { get; set; }

        [Label("Secondary Contact E-Mail")]
        [StringLength(200)]
        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "Invalid e-mail address.")]
        public string ContactEmail2 { get; set; }

        [Label("Secondary Contact Phone (Office)")]
        [StringLength(20)]
        public string ContactPhoneOfc2 { get; set; }

        [Label("Secondary Contact Phone (Cell)")]
        [StringLength(20)]
        public string ContactPhoneCell2 { get; set; }
        [Required]
        [Label("Security Patch Search Result")]
        public int ResultKey { get; set;}

        public string VendorNotes { get; set; }
        [Label("Discovery Notes")]
        public string Notes { get; set; }

        [Label("Checked Patch Availability On")]
        [InitialValue("new Date()")]
        public DateTime ReviewDate { get; set; }

        [Label("Patch Cadence")]
        [StringLength(20)]
        public int PatchCadence { get; set; }

        public bool Enabled { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedByID { get; set; }
    }
}
