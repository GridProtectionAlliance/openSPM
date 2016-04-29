//******************************************************************************************************
//  Assessment.cs - Gbtc
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
using GSF.Data.Model;

namespace openSPM.Model
{
    /// <summary>
    /// Model for openSPM.Assessment table.
    /// </summary>
    public class Assessment
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        [Required]
        [Label("Patch")]
        public int PatchStatusID { get; set; }

        [Required]
        [Label("Assessment Result")]
        public int AssessmentResultKey { get; set; }

        [Label("Assessment Details")]
        public string Details { get; set; }

        [Label("Updated On")]
        [InitialValue("new Date()")]
        public DateTime UpdatedOn { get; set; }

        [Label("Updated By")]
        public Guid UpdatedByID { get; set; }

      
        [Label("Created On")]
        [InitialValue("new Date()")]
        public DateTime CreatedOn { get; set; }

       
        [Label("Created By")]
        public Guid CreatedByID { get; set; }

        public bool IsAssessed { get; set; }
    }
}
