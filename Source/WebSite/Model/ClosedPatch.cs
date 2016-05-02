using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    [Table("ClosedPatch")]
    public class ClosedPatch
    {
        [PrimaryKey(true)]
        public int ID { get; set; }
        public int PatchStatusID { get; set; }
        public int ActionKey { get; set; }
        public DateTime ClosedDate { get; set; }
        public string Details { get; set; }
    }
}