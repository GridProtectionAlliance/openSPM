using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF.Data.Model;

namespace EmailService
{
    [TableName("NewAssessmentsView")]
    public class NewAssessmentsView
    {
        public int ID { get; set; }
        public int PatchStatusID { get; set; }
        public int AssessmentResultKey { get; set; }
        public DateTime CreatedOn { get; set; }
        public string VendorPatchName { get; set; }
        public string Title { get; set; }
        public string Product { get; set; }
        public string BusinessUnit { get; set; }
        public string Text { get; set; }
        public Guid SME { get; set; }
    }
}
