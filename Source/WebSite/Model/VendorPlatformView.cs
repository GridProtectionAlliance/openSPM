using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExpressionEvaluator.Parser.Expressions;
using GSF.Data.Model;

namespace openSPM.Model
{
    public class VendorPlatformView
    {
        [PrimaryKey]
        public int ID { get; set; }
        public string NameVersion { get; set; }
        public int VendorID { get; set; }
    }
}