//******************************************************************************************************
//  Platform.cs - Gbtc
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
    /// Model for openSPM.Platform table.
    /// </summary>
    [PrimaryLabel("Name")]
    [IsDeletedFlag("IsDeleted")]
    public class Platform
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        [Required]
        [Label("Patch Vendor")]
        public int VendorID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [StringLength(12)]
        [Label("Product Abbreviation")]
        public string Abbreviation { get; set; }

        public string Version { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime UpdatedOn { get; set; }

        public Guid UpdatedByID { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedByID { get; set; }

        public DateTime DatePlatformEnrolled { get; set; }
        public DateTime? DatePlatformRetired { get; set; }

        public string Notes { get; set; }

        public DateTime? DeletedON { get; set; }
        public Guid? DeletedByID { get; set; }
    }
}
