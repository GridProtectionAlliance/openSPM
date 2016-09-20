using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    /// <summary>
    /// Model for openSPM.MitigationPlanView table.
    /// </summary>
    [PrimaryLabel("ID")]
    public class MitigationPlanView : MitigationPlan
    {
        

        public string VendorPatchName { get; set; }
        public string Title { get; set; }
        public string BusinessUnitName { get; set; }
        public string PlatformName { get; set; }

    }
}