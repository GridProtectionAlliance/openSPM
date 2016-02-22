//******************************************************************************************************
//  ValueList.cs - Gbtc
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
//  02/21/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace openSPM.Models
{
    /// <summary>
    /// Model for openSPM.ValueList table.
    /// </summary>
    public class ValueList
    {
        [PrimaryKey(true)]
        public int ID
        {
            get; set;
        }

        public int groupID
        {
            get; set;
        }

        [Label("Integer Key")]
        public int intKey
        {
            get; set;
        }

        [Label("Alpha Key")]
        [StringLength(64)]
        public string alphaKey
        {
            get; set;
        }


        [Label("Alpha Value")]
        [StringLength(64)]
        public string alphaValue
        {
            get; set;
        }

        [Label("Alias Value")]
        [StringLength(64)]
        public string aliasValue
        {
            get; set;
        }

        [Label("Integer Value")]
        public int intValue
        {
            get; set;
        }

        [Label("Abv Value")]
        [StringLength(12)]
        public string abvValue
        {
            get; set;
        }

        [Label("Bit Value")]
        public bool bitValue
        {
            get; set;
        }

        [Label("Description")]
        [StringLength(512)]
        public string description
        {
            get; set;
        }

        [Label("Sort Order")]
        public int sortOrder
        {
            get; set;
        }

        [FieldName("private")] // <-- private is a reserved word in C#
        [Label("Private")]
        public bool @private
        {
            get; set;
        }

        [Label("Enabled")]
        public bool enabled
        {
            get; set;
        }

        public DateTime dtCreated
        {
            get; set;
        }
    }
}
