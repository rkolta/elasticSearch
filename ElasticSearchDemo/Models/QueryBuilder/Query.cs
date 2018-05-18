using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ElasticSearchDemo.Models.QueryBuilder
{
    public class Query
    {
        public Query()
        {
            this.Statements = new List<Statement>();
            this.UserProfiles = new List<UserProfile>();

        }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int QueryId { set; get; }
        public string Name { set; get; }
        public string ColorCode { set; get; }
        public bool Public { set; get; }
        public int? OwnerUserId { set; get; }

        public virtual List<Statement> Statements { set; get; }
        public virtual List<UserProfile> UserProfiles { set; get; }
        [ForeignKey("OwnerUserId")]
        public virtual UserProfile Owner { set; get; }
    }
}