using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    [Table("InstallHistoryView")]
    public class InstallHistoryView
    {
        [PrimaryKey(true)]
        public int InstallID { get; set; }
        public string VendorPatchName { get; set; }
        public string BusinessUnit { get; set; }
        public string CompletedNotes { get; set; }
        public string WorkManagementID { get; set; }
        public DateTime CompletedOn { get; set; }
    }
}