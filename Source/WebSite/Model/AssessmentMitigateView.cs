using System;
using System.Collections.Generic;
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
    }
}