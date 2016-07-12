using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    [Table("HistoryView")]
    public class HistoryView
    {
        [PrimaryKey(true)]
        public int PatchStatusID { get; set; }
        public int PatchID { get; set; }
        public string VendorName { get; set; }
        public string ProductName { get; set; }
        public string VendorPatchName { get; set; }
        public int BusinessUnitID { get; set; }
        public int AssessmentID { get; set; }
        public int AssessmentResultKey { get; set; }
        public DateTime AssessmentDate { get; set; }
        public int? InstallID { get; set; }
        public DateTime? InstallDate { get; set; }
        public int? MitigationPlanID { get; set; }
        public DateTime? MitigationPlanCreatedDate { get; set; }
        public DateTime ClosedDate { get; set; }

    }
}