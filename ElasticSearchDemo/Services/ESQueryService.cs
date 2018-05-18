using ElasticSearchDemo.Models;
using ElasticSearchDemo.Models.QueryBuilder;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace ElasticSearchDemo.Services
{
    public sealed class ESQueryService
    {
        private static string index_name = "files_doc";
        private static ElasticClient ESClient = new ElasticClient();
        private static QueryBuilderContext db = new QueryBuilderContext();
        public static string FileLocation = "~/Files{0}";
        public static int ResultSize = 10;

        public static ISearchResponse<Doc> MatchPhrase(string searchparam, int page)
        {
            return ESClient.Search<Doc>(s => s
                    .Index(index_name)
                    .Query(q => q
                        .MatchPhrase(m => m
                            .Field(f => f.Content)
                            .Query(searchparam)
                            )
                        )
                        .From((page - 1) * ResultSize)
                        .Size(ResultSize)
                        .Highlight(h => h
                            .PreTags("<span class='highlight'>")
                            .PostTags("</span>")
                            .Fields(fs => fs
                                .Field(d => d.Content)
                                )
                            )
                    );
        }


        public static ISearchResponse<Doc> Match(string searchparam, int page)
        {
            return ESClient.Search<Doc>(s => s
                    .Index(index_name)
                //.Source(sf => sf.Includes(
                //    e => e.Fields(f => f.File))
                //    )
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.Content)
                            .Query(searchparam)
                            )
                        )

                        .From((page - 1) * ResultSize)
                        .Size(ResultSize)
                        .Highlight(h => h
                            .PreTags("<span class='highlight'>")
                            .PostTags("</span>")
                            .Fields(fs => fs
                                .Field(d => d.Content)
                                )
                            )
                    );
        }


        public static ISearchResponse<Doc> MatchAll( int page)
        {
            return ESClient.Search<Doc>(s => s
                    .Index(index_name)
                    .Source(sf => sf.Excludes(
                        e => e.Fields(f => f.Content))
                    )
                    .Query(q => q
                        .MatchAll()
                        )

                        .From((page - 1) * ResultSize)
                        .Size(ResultSize)
                        .Highlight(h => h
                            .PreTags("<span class='highlight'>")
                            .PostTags("</span>")
                            .Fields(fs => fs
                                .Field(d => d.Content)
                                )
                            )
                    );
        }

        public static ISearchResponse<Doc> QueryBuilder(Query query, int page)
        {
            QueryContainer container = null;

            foreach (var statement in query.Statements)
            {
                var op = db.Operators.Find(statement.OperatorOpId);
                QueryContainer tempContainer = null;

                switch (op.Symbol)
                {
                    case "match":
                        tempContainer = QBMatch(statement);
                        break;
                    case "match phrase":
                        tempContainer = QBMatchPhrase(statement);
                        break;
                    case ">":
                        tempContainer = QBGreaterThan(statement);
                        break;
                    case "<":
                        tempContainer = QBLessThan(statement);
                        break;
                    case ">=":
                        tempContainer = QBGreaterThanOrEquals(statement);
                        break;
                    case "<=":
                        tempContainer = QBLessThanOrEquals(statement);
                        break;
                    case "between":
                        tempContainer = QBBetween(statement);
                        break;
                }

                if (statement.QueryOperatorOpId != null)
                {
                    var qo = db.Operators.Find(statement.QueryOperatorOpId);
                    if (qo.Symbol == "and")
                    {
                        container = container && tempContainer;
                    }
                    else
                    {
                        container = container || tempContainer;
                    }
                }
                else
                {
                    container = tempContainer;
                }


            }

            return ESClient.Search<Doc>(s => s
                   .Index(index_name)
                   .Source(sf => sf.Excludes(
                        e => e.Fields(f => f.Content))
                    )
                   .Query(
                        q => container
                       )
                    .From((page - 1) * ResultSize)
                    .Size(ResultSize)
                    .Highlight(h => h
                        .PreTags("<span class='highlight'>")
                        .PostTags("</span>")
                        .Fields(fs => fs
                            .Field(d => d.Content)
                            )
                        )
                   );
        }

        public static QueryContainer QBMatch(Statement statement)
        {
            var field = db.Fields.Find(statement.FieldId);

            return new QueryContainerDescriptor<Doc>().Match(d =>
                d.Field(CreateLamdaExpression(field))
                .Query(statement.SearchParam1)
                );
        }

        public static QueryContainer QBMatchPhrase(Statement statement)
        {
            var field = db.Fields.Find(statement.FieldId);

            return new QueryContainerDescriptor<Doc>().MatchPhrase(d =>
                d.Field(CreateLamdaExpression(field))
                .Query(statement.SearchParam1)
                );
        }

        public static QueryContainer QBGreaterThan(Statement statement)
        {
            var field = db.Fields.Find(statement.FieldId);

            return new QueryContainerDescriptor<Doc>().Range(d =>
                d.Field(CreateLamdaExpression(field))
                .GreaterThan(Double.Parse(statement.SearchParam1))
                );
        }

        public static QueryContainer QBGreaterThanOrEquals(Statement statement)
        {
            var field = db.Fields.Find(statement.FieldId);

            return new QueryContainerDescriptor<Doc>().Range(d =>
                d.Field(CreateLamdaExpression(field))
                .GreaterThanOrEquals(Double.Parse(statement.SearchParam1))
                );
        }

        public static QueryContainer QBLessThan(Statement statement)
        {
            var field = db.Fields.Find(statement.FieldId);

            return new QueryContainerDescriptor<Doc>().Range(d =>
                d.Field(CreateLamdaExpression(field))
                .LessThan(Double.Parse(statement.SearchParam1))
                );
        }

        public static QueryContainer QBLessThanOrEquals(Statement statement)
        {
            var field = db.Fields.Find(statement.FieldId);

            return new QueryContainerDescriptor<Doc>().Range(d =>
                d.Field(CreateLamdaExpression(field))
                .LessThanOrEquals(Double.Parse(statement.SearchParam1))
                );
        }

        public static QueryContainer QBBetween(Statement statement)
        {
            var field = db.Fields.Find(statement.FieldId);

            return new QueryContainerDescriptor<Doc>().Range(d =>
                d.Field(CreateLamdaExpression(field))
                .GreaterThanOrEquals(Double.Parse(statement.SearchParam1))
                .LessThanOrEquals(Double.Parse(statement.SearchParam2))
                );
        }

        private static Expression CreateLamdaExpression(ElasticSearchDemo.Models.QueryBuilder.Field field)
        {
            var fieldNames = field.Name.Split('.');
            var param = Expression.Parameter(typeof(Doc), "f");
            Expression parent = param;
            foreach (var fieldname in fieldNames)
            {
                parent = Expression.Property(parent, fieldname);
            }
            if (parent.Type.IsValueType)
            {
                var converted = Expression.Convert(parent, typeof(object));
                return Expression.Lambda<Func<Doc, object>>(converted, param);
            }
            return Expression.Lambda<Func<Doc, object>>(parent, param);
        }
    }
}