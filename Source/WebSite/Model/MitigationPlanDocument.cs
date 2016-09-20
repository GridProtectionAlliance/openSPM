using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    /// <summary>
    /// Model for openSPM.MitigationPlanDocument table.
    /// </summary>
    [PrimaryLabel("MitigationPlanID")]
    public class MitigationPlanDocument
    {
        [PrimaryKey]
        public int MitigationPlanID { get; set; }

        [PrimaryKey]
        public int DocumentID { get; set; }

    }
}