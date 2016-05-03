using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    [Table("MitigationPlanHistoryView")]
    public class MitigationPlanHistoryView
    {
        [PrimaryKey(true)]
        public int MitigationPlanID { get; set; }
        public string VendorPatchName { get; set; }
        public string BusinessUnit { get; set; }
        public int MiPlanID { get; set; }
        public string Summary { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
    }
}