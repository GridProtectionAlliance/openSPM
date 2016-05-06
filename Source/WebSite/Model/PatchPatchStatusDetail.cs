using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    [Table("PatchPatchStatusDetail")]
    public class PatchPatchStatusDetail
    {
        public int PatchID { get; set; }
        public int BusinessUnitID { get; set; }

        [PrimaryKey]
        public int PatchStatusID { get; set; }
        public string PatchMnemonic { get; set; }
        public int ImpactKey { get; set; }
        public DateTime VendorReleaseDate { get; set; }
        public DateTime EvaluationDeadline { get; set; }
        public int PatchStatusKey { get; set; } 
        public bool IsExpedited { get; set; }
        public int VendorID { get; set; }
        public string VendorName { get; set; }
        public int PlatformID { get; set; }
        public string PlatformName { get; set; }
        public string PlatformVersion { get; set; }
        public int PatchClassKey { get; set; }
        public string Summary { get; set; }
        public string Detail { get; set; }
        public string Link { get; set; }

    }
}