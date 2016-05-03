using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Models
{
    public class ThemeFields
    {
        [PrimaryKey]
        public int ID { get; set; }
        public int ThemeID { get; set; }
        public int FieldNumber { get; set; }
        public string FieldName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}