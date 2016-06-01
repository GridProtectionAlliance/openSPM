using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.DynamicData;

namespace openSPM.Model
{
    [TableName("NoticeLogView")]
    public class NoticeLogView : NoticeLog
    {
        public string VendorPatchName { get; set; }
    }
}