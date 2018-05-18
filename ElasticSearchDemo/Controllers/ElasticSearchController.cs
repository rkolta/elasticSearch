using Elasticsearch.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ElasticSearchDemo.Models;
using Nest;
using ElasticSearchDemo.Services;
using ElasticSearchDemo.Models.QueryBuilder;

namespace ElasticSearchDemo.Controllers
{
    [Authorize]
    public class ElasticSearchController : Controller
    {
        //
        // GET: /ElasticSearch/
        private QueryBuilderContext db = new QueryBuilderContext();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult QueryBuilder()
        {
            ViewBag.Fields = db.Fields.ToList();
            ViewBag.SavedQueries = db.Queries.Where(q => q.Owner.UserName == User.Identity.Name).ToList();
            ViewBag.PublicSavedQueries = db.Queries.Where(q => q.UserProfiles.Contains(db.UserProfiles.Where(up => up.UserName == User.Identity.Name && q.Public).FirstOrDefault())).ToList();
            ViewBag.Users = db.UserProfiles.Where(u => u.UserName != User.Identity.Name).ToList();
            return View(new Query());
        }

        [HttpPost]
        public ActionResult QueryBuilder(int id)
        {
            ViewBag.Fields = db.Fields.ToList();
            ViewBag.SavedQueries = db.Queries.Where(q => q.Owner.UserName == User.Identity.Name).ToList();
            ViewBag.PublicSavedQueries = db.Queries.Where(q => q.UserProfiles.Contains(db.UserProfiles.Where(up => up.UserName == User.Identity.Name && q.Public).FirstOrDefault())).ToList();
            ViewBag.Users = db.UserProfiles.Where(u => u.UserName != User.Identity.Name).ToList();
            var query = db.Queries.Find(id);
            if (query != null)
            {
                return View(query);
            }
            return View(new Query());
        }

        [HttpPost]
        public ActionResult QuickSearch(string[] options, string searchparam, int page = 1)
        {
            ISearchResponse<Doc> searchResponse = null;
            if (options.FirstOrDefault() == "match")
            {
                searchResponse = ESQueryService.Match(searchparam, page);
            }
            else
            {
                searchResponse = ESQueryService.MatchPhrase(searchparam, page);
            }
            ViewBag.Match = options.FirstOrDefault();
            ViewBag.Page = page;
            ViewBag.SearchParam = searchparam;

            return View(searchResponse);
        }

        [HttpPost]
        public JsonResult QueryBuilderSearch(Query query, int page = 1)
        {
            var searchResponse = ESQueryService.QueryBuilder(query, page);
            return Json(searchResponse);
        }

        [HttpPost]
        public JsonResult QueryBuilderSave(Query query)
        {
            var owner = db.UserProfiles.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (owner != null)
            {
                query.Owner = owner;
            }
            List<UserProfile> userProfiles = new List<UserProfile>();

            foreach (var up in query.UserProfiles)
            {
                userProfiles.Add(db.UserProfiles.Find(up.UserId));
            }
            query.UserProfiles = userProfiles;

            db.Queries.Add(query);
            db.SaveChanges();
            return Json(new { QueryId = query.QueryId });
        }

        [HttpPost]
        public JsonResult QueryBuilderUpdate(Query query)
        {
            var queryUpdate = db.Queries.Find(query.QueryId);

            List<UserProfile> userProfiles = new List<UserProfile>();

            foreach (var up in query.UserProfiles)
            {
                userProfiles.Add(db.UserProfiles.Find(up.UserId));
            }
            queryUpdate.UserProfiles.Clear();
            queryUpdate.UserProfiles = userProfiles;
            queryUpdate.Public = query.Public;
            queryUpdate.Name = query.Name;
            queryUpdate.ColorCode = query.ColorCode;

            db.SaveChanges();
            return Json(new { QueryId = queryUpdate.QueryId, ColorCode = queryUpdate.ColorCode, Name = queryUpdate.Name });
        }
        
        public FileResult DownloadElastic()
        {
            var rand = new Random();
            var searchResponse = ESQueryService.MatchAll(rand.Next(1, 36));
            int element;
            do
            {
                element = rand.Next(0, 10);
            } while (element >= searchResponse.Documents.Count);

            var doc = searchResponse.Documents.ElementAt(element);
            var virtualPath = String.Format(ESQueryService.FileLocation, doc.Path.Virtual.Replace('/', '\\'));
            return File(virtualPath, System.Net.Mime.MediaTypeNames.Application.Octet, System.IO.Path.GetFileName(virtualPath));
        }

        public FileResult DownloadFile(string esVirtualPath)
        {
            var virtualPath = String.Format(ESQueryService.FileLocation, esVirtualPath.Replace('/', '\\'));
            return File(virtualPath, System.Net.Mime.MediaTypeNames.Application.Octet, System.IO.Path.GetFileName(virtualPath));
        }

    }
}
