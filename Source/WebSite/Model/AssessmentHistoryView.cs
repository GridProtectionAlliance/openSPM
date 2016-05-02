using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    [Table("AssessmentHistoryView")]
    public class AssessmentHistoryView
    {
        [PrimaryKey(true)]
        public int AssessmentID { get; set; }
        public string VendorPatchName { get; set; }
        public string BusinessUnit { get; set; }
        public string AssessmentResult { get; set; }
        public string Details { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}