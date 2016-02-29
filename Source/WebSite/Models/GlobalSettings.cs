﻿//******************************************************************************************************
//  GlobalSettings.cs - Gbtc
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
using System.Collections.Generic;

namespace openSPM.Models
{
    public class GlobalSettings
    {
        public Guid NodeID
        {
            get;
            set;
        }

        public string CompanyName
        {
            get;
            set;
        }

        public string CompanyAcronym
        {
            get;
            set;
        }

        public string ApplicationName
        {
            get;
            set;
        }

        public string ApplicationDescription
        {
            get;
            set;
        }

        public string ApplicationKeywords
        {
            get;
            set;
        }

        public string DateFormat
        {
            get;
            set;
        }

        public string TimeFormat
        {
            get;
            set;
        }

        public string DateTimeFormat
        {
            get;
            set;
        }

        public string PasswordRequirementsRegex
        {
            get;
            set;
        }

        public string PasswordRequirementsError
        {
            get;
            set;
        }

        public string BootstrapTheme
        {
            get;
            set;
        }

        public readonly Dictionary<string, string> LayoutSettings = new Dictionary<string, string>();

        public readonly Dictionary<string, string> PageDefaults = new Dictionary<string, string>();
    }
}
