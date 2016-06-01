using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF.Data.Model;

namespace EmailService
{
    public class NoticeLog
    {
        [PrimaryKey(true)]
        public int ID { get; set; }
        public int PatchID { get; set; }
        public int NoticeMethodKey { get; set; }
        public int NoticeLevelKey { get; set; }
        public string ToUsers { get; set; }
        public string XcUsers { get; set; }
        public string Text { get; set; }
        public DateTime SentOn { get; set;}
        public DateTime CreatedOn { get; set; }
    }
}
