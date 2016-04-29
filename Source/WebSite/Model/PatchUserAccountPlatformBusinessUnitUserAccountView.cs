using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace openSPM.Model
{
    [Table("PatchUserAccountPlatformBusinessUnitUserAccountView")]
    public class PatchUserAccountPlatformBusinessUnitUserAccountView
    {
        public int PlatformID { get; set; }
        public int BusinessUnitID { get; set; }
    }
}