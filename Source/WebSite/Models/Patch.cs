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
        public int parentID
        {
            get; set;
        }

        [Required]
        [Label("Patch Source")]
        public int sourceID
        {
            get; set;
        }

        [Label("SPM Patch Reference")]
        [StringLength(64)]
        public string patchReference
        {
            get; set;
        }

        [Label("Source Patch Reference")]
        [StringLength(64)]
        public string sourceReference
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
        public int impactIntKey
        {
            get; set;
        }

        [Label("Operator Group")]
        public int operatorGroupID
        {
            get; set;
        }

        [StringLength(80)]
        public string title
        {
            get; set;
        }

        [StringLength(512)]
        public string target
        {
            get; set;
        }

        [StringLength(1024)]
        public string summary
        {
            get; set;
        }

        public string detail
        {
            get; set;
        }

        [StringLength(1024)]
        public string reference
        {
            get; set;
        }

        [Label("Work Around")]
        [StringLength(1024)]
        public string workArounds
        {
            get; set;
        }

        [StringLength(512)]
        [RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", ErrorMessage = "Invalid URL.")]
        public string link
        {
            get; set;
        }

        [Label("Alarm Critical Days")]
        public int alarmCriticalDays
        {
            get; set;
        }

        [Required]
        [Label("Close Out Notes")]
        public string closeOutNotes
        {
            get; set;
        }

        [Label("Not In Compliance")]
        public bool isNotCompliance
        {
            get; set;
        }

        [Required]
        public DateTime dtCreated
        {
            get; set;
        }

        [Required]
        public int createdByID
        {
            get; set;
        }

        [Required]
        public DateTime dtUpdated
        {
            get; set;
        }

        [Required]
        public int updatedByID
        {
            get; set;
        }
    }
}
