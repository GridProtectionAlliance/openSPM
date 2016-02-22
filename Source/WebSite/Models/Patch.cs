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

namespace openSPM.Models
{
    /// <summary>
    /// Model for openSPM.Patch table.
    /// </summary>
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

        [Required]
        [Label("Source Patch")]
        public int SourceID
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

        [Label("Source ID")]
        [StringLength(64)]
        public string SourceIdentifier
        {
            get; set;
        }

        [Label("Source Date")]
        [InitialValue("(new Date()).addDays(30)")] // <-- Example to set inital value to 30 days from now
        public DateTime dtSource
        {
            get; set;
        }

        [Label("Impact Value")]
        public int ImpactValue
        {
            get; set;
        }

        [Required]
        [Label("Submission Date")]
        [InitialValue("new Date()")]
        public DateTime dtSubmitted
        {
            get; set;
        }

        [StringLength(80)]
        public string Title
        {
            get; set;
        }

        [StringLength(512)]
        public string Target
        {
            get; set;
        }

        [StringLength(1024)]
        public string Summary
        {
            get; set;
        }

        public string Detail
        {
            get; set;
        }

        [StringLength(1024)]
        public string Reference
        {
            get; set;
        }

        [Label("Work Around")]
        [StringLength(1024)]
        public string WorkArounds
        {
            get; set;
        }

        [StringLength(512)]
        [RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", ErrorMessage = "Invalid URL.")]
        public string Link
        {
            get; set;
        }

        [Label("Alarm Critical Days")]
        public int AlarmCriticalDays
        {
            get; set;
        }

        [Required]
        [Label("Close Out Notes")]
        public string CloseOutNotes
        {
            get; set;
        }

        [Label("Not In Compliance")]
        public bool isNotCompliance
        {
            get; set;
        }

        [Required]
        public DateTime CreatedOn
        {
            get; set;
        }

        [Required]
        [StringLength(184)]
        public string CreatedBy
        {
            get; set;
        }


        [Required]
        public DateTime UpdatedOn
        {
            get; set;
        }

        [Required]
        [StringLength(184)]
        public string UpdatedBy
        {
            get; set;
        }
    }
}
