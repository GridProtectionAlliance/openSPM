using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    public class AssessmentMitigateView
    {
        [PrimaryKey(true)]
        public int ID { get; set; }
        public string Title { get; set; }
        public int PatchStatusID { get; set; }
        public int PatchStatusKey { get; set; }
        public string Name { get; set; }
        public int BusinessUnitID { get; set; }
        public string VendorPatchName { get; set; }
        public int AssessmentResultKey { get; set; }
        public string Details { get; set; }
        public DateTime CreatedOn { get; set; }
        public int PlatformID { get; set; }
        public string PlatformName { get; set; }

        public int MiPlanID { get; set; }
        public string PlanURL { get; set; }
        public string Justification { get; set; }

        [Required]
        public string Summary { get; set; }
        public string Risk { get; set; }
        public string Detail { get; set; }
        public Guid CreatedByID { get; set; }

    }
}