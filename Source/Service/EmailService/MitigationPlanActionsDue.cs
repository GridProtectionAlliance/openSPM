using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF.Data.Model;

namespace EmailService
{
    public class MitigationPlanActionsDue
    {
        [PrimaryKey]
        public int ID { get; set; }
        [PrimaryKey]
        public int ActionID { get; set; }
        [PrimaryKey]
        public Guid UserAccountID { get; set; }

        public string Title { get; set; }
        public string Action { get; set; }
        public string Name { get; set; }
        public DateTime ScheduledEndDate{ get; set;}
        public  int DaysLeft { get; set; }

    }
}
