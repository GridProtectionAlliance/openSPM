//******************************************************************************************************
//  NoticeLog.cs - Gbtc
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

namespace openSPM.Models
{
    /// <summary>
    /// Model for openSPM.NoticeLog table.
    /// </summary>
    public class NoticeLog
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        [Label("Patch")]
        public int PatchID { get; set; }

        [Label("Notice Method")]
        public int? NoticeMethodKey { get; set; }

        [Label("Notice Level")]
        public int? NoticeLevelKey { get; set; }

        [Label("To Users")]
        [StringLength(1024)]
        public string ToUsers { get; set; }

        [Label("XC Users")]
        [StringLength(1024)]
        public string XcUsers { get; set; }

        public string Text { get; set; }

        public DateTime? SentOn { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
