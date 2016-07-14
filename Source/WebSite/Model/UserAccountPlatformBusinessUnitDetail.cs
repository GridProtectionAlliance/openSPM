using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    [TableName("UserAccountPlatformBusinessUnitDetail")]
    public class UserAccountPlatformBusinessUnitDetail
    {
        [PrimaryKey]
        public string BUName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [PrimaryKey]
        public Guid UserAccountID { get; set; }


        public int PlatformID { get; set; }

        public string VendorName { get; set; }

        public string PlatformName { get; set; }


    }
}