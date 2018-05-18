using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ElasticSearchDemo.Models.QueryBuilder
{
    public class BoolOperator
    {

        public BoolOperator()
        {
            this.Fields = new HashSet<Field>();
        }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int OpId { set; get; }
        public string Symbol { set; get; }

        public virtual ICollection<Field> Fields { set; get; }
    }
}