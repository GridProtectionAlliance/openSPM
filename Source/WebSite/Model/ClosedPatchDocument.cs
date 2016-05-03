using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    /// <summary>
    /// Model for openSPM.ClosedPatchDocument table.
    /// </summary>
    [PrimaryLabel("ClosedPatchID")]
    public class ClosedPatchDocument
    {
        [PrimaryKey]
        public int ClosedPatchID { get; set; }

        [PrimaryKey]
        public int DocumentID { get; set; }
    }
}