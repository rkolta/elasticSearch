using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ElasticSearchDemo.Models.QueryBuilder
{
    public class Field
    {

        public Field()
        {
            this.Operators = new HashSet<BoolOperator>();
            this.Statements = new HashSet<Statement>();
        }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int FieldId { set; get; }
        public string DisplayName { set; get; }
        public string Name { set; get; }

        public virtual ICollection<BoolOperator> Operators { set; get; }
        public virtual ICollection<Statement> Statements { set; get; }

    }
}