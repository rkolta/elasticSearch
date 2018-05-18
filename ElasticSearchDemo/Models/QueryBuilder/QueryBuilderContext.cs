using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ElasticSearchDemo.Models.QueryBuilder
{
    public class QueryBuilderContext : DbContext
    {
        public QueryBuilderContext()
            : base("DefaultConnection")
        {

        }
        public DbSet<Query> Queries { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<BoolOperator> Operators { get; set; }
        public DbSet<Statement> Statements { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
    }
}