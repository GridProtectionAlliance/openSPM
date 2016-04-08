using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace openSPM.Model
{
    public class PatchStatusAssessmentDetail
    {
        public int PatchID { get; set; }
        public int BusinessUnitID { get; set; }
        //public string Title { get; set; }
        public string PatchMnemonic { get; set; }
        public int ID { get; set; }
        //public string AlarmCriticalDays { get; set; }
    }
}