using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    public class ClosingReviewView
    {
        [PrimaryKey(true)]
        public int ID { get; set; }
        public int PatchID { get; set; }
        public string VendorName { get; set; }
        public string PlatformName { get; set; }
        public int BusinessUnitID { get; set; }
        public string BusinessUnitName { get; set; }
        public string VendorPatchName { get; set; }
        public int AssessmentResultKey { get; set; }
        public DateTime CompletionDate { get; set; }
        public string Details { get; set; }
        public int PatchStatusKey { get; set; }
        public string Title { get; set; }
        public string PatchDetails { get; set; }
    }
}