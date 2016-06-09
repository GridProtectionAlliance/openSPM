//******************************************************************************************************
//  Patch.cs - Gbtc
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
using GSF.Data.Model;

namespace openSPM.Model
{
    /// <summary>
    /// Model for openSPM.Patch table.
    /// </summary>
    [PrimaryLabel("VendorPatchName")]
    [IsDeletedFlag("IsDeleted")]
    public class Patch
    {
        [PrimaryKey(true)]
        public int ID
        {
            get; set;
        }

        [Label("Parent Patch")]
        public int ParentID
        {
            get; set;
        }

        [Label("Patch Title")]
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [Label("Patch Vendor")]
        public int VendorID
        {
            get; set;
        }

        [Required]
        [Label("Target Product")]
        public int PlatformID
        {
            get; set;
        }

        [Required]
        [Label("Patch Class")]
        public int PatchClassKey
        {
            get; set;
        }

        [Label("Vendor Patch Name/Identifier")]
        [StringLength(64)]
        public string VendorPatchName
        {
            get; set;
        }

        [Required]
        [Label("Vendor Patch Release Date")]
        [InitialValue("new Date()")]
        public DateTime VendorReleaseDate
        {
            get; set;
        }

        [Label("Vendor Stated Impact")]
        public int ImpactKey
        {
            get; set;
        }

        [Label("Patch Summary")]
        public string Summary
        {
            get; set;
        }

        [Label("Patch Detail")]
        public string Detail
        {
            get; set;
        }

        [Label("Vendor References")]
        public string Citations
        {
            get; set;
        }

        [Label("Patch Source Link")]
        [StringLength(512)]
        //[RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", ErrorMessage = "Invalid URL.")]
        public string Link
        {
            get; set;
        }

        // Initial value loaded in page from settings table...
        [Label("Alarm Critical Business Days")]
        public int AlarmCriticalDays
        {
            get; set;
        }

        [Label("Close Out Notes")]
        public string CloseOutNotes
        {
            get; set;
        }

        [Label("Vulnerability - No Patch Available")]
        public bool Vulnerability
        {
            get; set;
        }

  
        [Label("Evaluation Deadline Date")]
        [InitialValue("new Date(new Date().setDate(new Date().getDate() + 35))")]
        public DateTime EvaluationDeadline { get; set; }

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

        public DateTime UpdatedOn
        {
            get; set;
        }

        public Guid UpdatedByID
        {
            get; set;
        }

        public bool IsInitiated { get; set; }

        [Label("Requires Expedited Attention")]
        public bool IsExpedited { get; set; }
    }
}
