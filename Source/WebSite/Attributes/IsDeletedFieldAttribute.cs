﻿//******************************************************************************************************
//  IsDeletedFlagAttribute.cs - Gbtc
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
//  2/29/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

// TODO: Move into GSF

namespace openSPM.Attributes
{
    /// <summary>
    /// Defines field name that represents a record marked for deletion.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IsDeletedFieldAttribute : Attribute
    {
        /// <summary>
        /// Field name used as is deleted record marker.
        /// </summary>
        public readonly string FieldName;

        /// <summary>
        /// Creates a new <see cref="IsDeletedFieldAttribute"/>.
        /// </summary>
        /// <param name="fieldName">Field name used as is deleted record marker.</param>
        public IsDeletedFieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}