//******************************************************************************************************
//  MenuItem.cs - Gbtc
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
using openSPM.Attributes;

namespace openSPM.Models
{
    /// <summary>
    /// Model for openSPM.MenuItem table.
    /// </summary>
    public class MenuItem
    {
        [PrimaryKey(true)]
        public int ID
        {
            get; set;
        }

        [Required]
        public int PageID
        {
            get; set;
        }

        [Label("Minimum View Level")]
        public int minViewLevel
        {
            get; set;
        }

        [Required]
        [Label("Image Source")]
        [StringLength(256)]
        public string image
        {
            get; set;
        }

        [StringLength(32)]
        [Label("Image Alternate Text")]
        public string imageAlt
        {
            get; set;
        }

        [Required]
        [Label("Menu Text")]
        [StringLength(20)]
        public string text
        {
            get; set;
        }

        [Label("URL")]
        [StringLength(256)]
        [RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", ErrorMessage = "Invalid URL.")]
        public string link
        {
            get; set;
        }

        [Label("Sort Order")]
        public int sortOrder
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
