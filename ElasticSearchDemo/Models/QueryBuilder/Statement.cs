using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ElasticSearchDemo.Models.QueryBuilder
{
    public class Statement
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int QueryFieldId { set; get; }
        public string SearchParam1 { set; get; }
        public string SearchParam2 { set; get; }
        public int FieldId { set; get; }
        public int QueryId { set; get; }
        public int OperatorOpId { set; get; }
        public int? QueryOperatorOpId { set; get; }

        public virtual Field Field { set; get; }
        public virtual Query Query { set; get; }
        public virtual BoolOperator Operator { set; get; }
        public virtual BoolOperator QueryOperator { set; get; }
    }
}