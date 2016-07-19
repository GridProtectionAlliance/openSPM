using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    [Table("PatchVendorPlatformView")]
    public class PatchVendorPlatformView
    {
        [PrimaryKey(true)]
        public int ID { get; set; }
        public string VendorPatchName { get; set; }
        public DateTime VendorReleaseDate { get; set; }
        public string PlatformName { get; set; }
        public string Detail { get; set; }
        public string VendorName { get; set; }
        public string Title { get; set; }
    }
}