using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using GSF.Data.Model;


namespace openSPM.Model
{
    [PrimaryLabel("VendorPatchName")]
    [IsDeletedFlag("IsDeleted")]
    [TableName("PatchView")]
    public class PatchView: Patch
    {
        public string VendorName { get; set; }
        public string ProductName { get; set; }
    }
}