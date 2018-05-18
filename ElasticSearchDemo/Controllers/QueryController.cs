using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ElasticSearchDemo.Models.QueryBuilder;

namespace ElasticSearchDemo.Controllers
{
    public class QueryController : Controller
    {
        private QueryBuilderContext db = new QueryBuilderContext();

        public JsonResult GetFieldOperatorList(int fieldId)
        {
            var operators = db.Fields.Where(f => f.FieldId == fieldId).SelectMany(f => f.Operators).Select(o => new { OpId = o.OpId, Symbol = o.Symbol }).ToList();

            if (operators != null)
            {
                return Json(operators);
            }
            return Json(new List<BoolOperator>());
        }

        public JsonResult DeleteQuery(int id)
        {
            var query = db.Queries.Find(id);
            db.Queries.Remove(query);
            db.SaveChanges();
            return Json(new { });
        }

        public JsonResult UnwatchQuery(int id)
        {
            var query = db.Queries.Find(id);
            var profile = db.UserProfiles.Where(up => up.UserName == User.Identity.Name).FirstOrDefault();
            if (profile != null)
            {
                query.UserProfiles.Remove(profile);
            }
            db.SaveChanges();
            return Json(new { });
        }

        public JsonResult LoadSavedQueryDetails(int id)
        {
            var query = db.Queries.Where(q => q.QueryId == id).Select(q => new {QueryId = q.QueryId,  Name = q.Name, Users = q.UserProfiles.Select(u => u.UserId), Color = q.ColorCode, Public = q.Public }).FirstOrDefault();
            return Json(query);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}