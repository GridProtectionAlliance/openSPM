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
        public int PatchDocumentCount { get; set; }
        public DateTime VendorReleaseDate { get; set; }
        public DateTime DiscoveryDate { get; set; }
        public int DiscoveryDelta { get; set; }
        public string VendorName { get; set; }
        public string ProductName { get; set; }
        public string VendorPatchName { get; set; }
        public int BusinessUnitID { get; set; }
        public int AssessmentID { get; set; }
        public int AssessmentDocumentCount { get; set; }
        public int AssessmentResultKey { get; set; }
        public DateTime AssessmentDate { get; set; }
        public int AssessmentDelta { get; set; }
        public int? InstallID { get; set; }
        public int? InstallDocumentCount { get; set; }
        public DateTime? InstallDate { get; set; }
        public int? MitigationPlanID { get; set; }
        public DateTime? MitigationPlanCreatedDate { get; set; }
        public int? MitigationPlanDocumentCount { get; set; }
        public DateTime CompletionDate { get; set; }
        public int CompletionDelta { get; set; }
        public int? PlanStatus { get; set; }

    }
}