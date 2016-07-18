using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSF.Data.Model;
using System.ComponentModel.DataAnnotations;



namespace openSPM.Model
{
    /// <summary>
    /// Model for openSPM.PlatformView Table
    /// </summary>
    [PrimaryLabel("Name")]
    [IsDeletedFlag("IsDeleted")]
    [TableName("PlatformView")]
    public class PlatformView : Platform
    {
        public string VendorName { get; set; }
        public string VendorAbbreviation { get; set; }
    }
}