using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace openSPM.Model
{
    [Table("ClosedPatch")]
    public class ClosedPatch
    {
        public int ID { get; set; }
        public int PatchStatusID { get; set; }
        public int ActionKey { get; set; }
        public DateTime ClosedDate { get; set; }
        public string Details { get; set; }
    }
}