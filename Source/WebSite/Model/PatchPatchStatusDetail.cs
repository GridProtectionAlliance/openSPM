using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace openSPM.Model
{
    public class PatchPatchStatusDetail
    {
        public int PatchID { get; set; }
        public int BusinessUnitID { get; set; }
        public int PatchStatusID { get; set; }
        public string PatchMnemonic { get; set; }
    }
}