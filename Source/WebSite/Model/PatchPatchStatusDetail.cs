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
    }
}