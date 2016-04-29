using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    /// <summary>
    /// Model for openSPM.InstallDocument table.
    /// </summary>
    [PrimaryLabel("AssessmentID")]
    public class AssessmentDocument
    {
        [PrimaryKey]
        public int AssessmentID { get; set; }

        [PrimaryKey]
        public int DocumentID { get; set; }
    }
}