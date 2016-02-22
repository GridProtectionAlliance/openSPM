//******************************************************************************************************
//  Page.cs - Gbtc
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
//  02/17/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.ComponentModel.DataAnnotations;

namespace openSPM.Models
{
    /// <summary>
    /// Model for openSPM.Page table.
    /// </summary>
    public class Page
    {
        // This is NOT currently an identity field - if this changes, set to [PrimaryKey(true)]
        [PrimaryKey]
        public int ID
        {
            get; set;
        }

        [Required]
        [Label("Name")]
        [StringLength(32)]
        public string name
        {
            get; set;
        }

        [Required]
        [Label("Title")]
        [StringLength(32)]
        public string title
        {
            get; set;
        }

        [Label("Menu")]
        public int menuID
        {
            get; set;
        }

        [Label("Server-side Configuration Parameters")]
        [StringLength(1024)]
        public string serverConfiguration
        {
            get; set;
        }

        [Label("Client-side Configuration Parameters")]
        [StringLength(1024)]
        public string clientConfiguration
        {
            get; set;
        }

        [Label("Description")]
        [StringLength(512)]
        public string description
        {
            get; set;
        }

        [Label("Enabled")]
        public bool enabled
        {
            get; set;
        }
    }
}
