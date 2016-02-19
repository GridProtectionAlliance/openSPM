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

        public int ParentID
        {
            get; set;
        }

        [Required]
        public int SourceID
        {
            get; set;
        }

        [Required]
        [StringLength(64)]
        public string SPMidentifier
        {
            get; set;
        }

        [StringLength(64)]
        public string SourceIdentifier
        {
            get; set;
        }

        public DateTime dtSource
        {
            get; set;
        }

        [Required]
        public int ImpactValue
        {
            get; set;
        }

        public DateTime dtSubmitted
        {
            get; set;
        }

        [StringLength(80)]
        public string Title
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

        [StringLength(1024)]
        public string WorkAround
        {
            get; set;
        }

        [StringLength(512)]
        public string Link
        {
            get; set;
        }

        [Required]
        public int AlarmCriticalDays
        {
            get; set;
        }

        [Required]
        public string CloseOutNotes
        {
            get; set;
        }

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
        public string CreatedByID
        {
            get; set;
        }


        [Required]
        public DateTime dtUpdated
        {
            get; set;
        }

        [Required]
        public string UpdatedByID
        {
            get; set;
        }
    }
}
