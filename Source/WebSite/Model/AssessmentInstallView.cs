using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    public class AssessmentInstallView
    {
        [PrimaryKey(true)]
        public int ID { get; set; }
        public int PatchStatusID { get; set; }
        public int PatchStatusKey { get; set; }
        public string Name { get; set; }
        public int BusinessUnitID { get; set; }
        public string VendorPatchName { get; set; }
        public int AssessmentResultKey { get; set; }
        public string Details { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedByID { get; set; }
        public string Summary { get; set; }
        [Required]
        [Label("Completed Date")]
        [InitialValue("new Date()")]
        public DateTime? CompletedOn { get; set; }

        [Label("Completed Notes")]
        public string CompletedNotes { get; set; }

        [Label("Work Management ID")]
        public string WorkManagementID { get; set; }

        public bool IsInstalled { get; set; }
        public string Title { get; set; }
        public string VendorName { get; set; }
        public string ProductName { get; set; }
        public DateTime EvaluationDeadline { get; set; }
        public string PatchDetails { get; set; }

    }
}