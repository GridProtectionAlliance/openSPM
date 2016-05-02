using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace openSPM.Model
{
    [Table("PatchStatusAssessmentView")]
    public class PatchStatusAssessmentView
    {
        public int ID { get; set; }
        public int PatchID { get; set; }
        public int PatchStatusKey { get; set; }
        public DateTime StatusChangeOn { get; set; }
        public int AssessmentResultKey { get; set; }
        public int BusinessUnitID { get; set; }
        public string Details { get; set; }
        public DateTime DueDate { get; set; }
    }
}