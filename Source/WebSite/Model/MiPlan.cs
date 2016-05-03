using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace openSPM.Model
{
    [TableName("MitigationPlan")]
    public class MiPlan
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        [Required]
        [Label("Patch")]
        [StringLength(80)]
        public string Title { get; set; }

        [Required]
        public int ThemeID { get; set; }

        [Required]
        public int BusinessUnitID { get; set; }

        public int ForeignKey1 { get; set; }

        public int ForeignKey2 { get; set; }

        public int ForeignKey3 { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

        public string Field1 { get; set; }

        public string Field2 { get; set; }

        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public string Field6 { get; set; }
        public string Field7 { get; set; }
        public string Field8 { get; set; }
        public string Field9 { get; set; }
        public string Description { get; set; }

        public string StatusNotes { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedByID { get; set; }

        public DateTime UpdatedOn { get; set; }

        public Guid UpdatedByID { get; set; }

        public bool IsCompleted { get; set; }

    }
}