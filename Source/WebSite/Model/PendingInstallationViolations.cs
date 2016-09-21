using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    public class PendingInstallationViolations
    {
        [PrimaryKey(true)]
        public int PatchStatusID { get; set; }

        [PrimaryKey]
        public Guid SME { get; set; }

        public int ID { get; set; }
        public string VendorPatchName { get; set; }
        public DateTime DueDate { get; set; }
        public string PlatformName { get; set; }
        public string BUName { get; set; }
        public int DaysTilViolation { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}