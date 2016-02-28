//******************************************************************************************************
//  ActionItem.cs - Gbtc
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
using System.ComponentModel.DataAnnotations.Schema;
using openSPM.Attributes;

namespace openSPM.Models
{
    /// <summary>
    /// Model for openSPM.ActionItem table.
    /// </summary>
    public class ActionItem
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        [Label("Parent Action Item")]
        public int? ParentID { get; set; }

        public int PlanID { get; set; }

        [Label("Action Type")]
        public int? ActionTypeKey { get; set; }

        [Label("Priority")]
        public int? PriorityKey { get; set; }

        [StringLength(80)]
        public string Title { get; set; }

        public string Details { get; set; }

        [Label("Scheduled Start Date")]
        [Column(TypeName = "date")]
        public DateTime? ScheduledStartDate { get; set; }

        [Label("Scheduled End Date")]
        [Column(TypeName = "date")]
        public DateTime? ScheduledEndDate { get; set; }

        public bool Enabled { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedByID { get; set; }

        public DateTime UpdatedOn { get; set; }

        public Guid UpdatedByID { get; set; }
    }
}
