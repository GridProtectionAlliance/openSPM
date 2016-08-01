using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF.Data.Model;

namespace EmailService
{
    [TableName("NewPlanView")]
    public class NewPlanView
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public Guid UserAccountID { get; set; }
        public int ID { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
