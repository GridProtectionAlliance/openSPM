using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace openSPM.Model
{
    public class PendingAssessmentViolations
    {
        public string VendorPatchName { get; set; }
        public string Title { get; set; }
        public DateTime EvaluationDeadline { get; set; }
        public string PlatformName { get; set; }
        public string BUName { get; set; }
        public int DaysTilViolation { get; set; }
        public Guid SME { get; set; }
    }
}