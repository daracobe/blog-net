using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyBlog.Models;
using System;
using Newtonsoft.Json;
using System.Data.Entity;
using System.Web.Security;
using System.Collections.Generic;
using System.Net;
namespace MyBlog.Controllers
{
    public class HomeController : Controller
    {
        private MyBlog.Models.Database db = new MyBlog.Models.Database();
        public ActionResult Index()
        {
            var post = db.Post.Include(p => p.Category);
            return View(post.ToList());
        }

        [Route("home/tag/{id}")]
        public ActionResult Tag(int? id)
        {
            List<string> tagList = new List<string>();
            string CurrentTag = "-";
            var post = db.Post.Include(p => p.Category);
            try
            {
                var tg = from s in db.Tag
                         where s.TagID == id
                         select s.Name;
                CurrentTag = tg.First();

                post = from s in db.Post
                       join sa in db.PostTagMap on s.PostID equals sa.Post_ID
                       where sa.Post_Tag == id
                       select s;                
            }
            catch (Exception ex)
            {
                post = db.Post.Include(p => p.Category);
            }
            ViewBag.tagText = CurrentTag;
            return View(post.ToList());
        }

        [Route("home/details/{id}")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Post.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // GET: Posts/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.Category_id = new SelectList(db.Category, "CategoryID", "Name");
            return View();
        }

        [Route("home/create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "PostID,Title,ShortDescription,Description,Meta,UrlSlug,Published,PostedOn,Modified,Category_id")] Post newPost)
        {

            if (ModelState.IsValid)
            {
                var tags = Request.Form["tags"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                PostManager.Create(newPost, tags.ToList(), User.Identity.Name);
                return RedirectToAction("Index");
            }

            ViewBag.Category_id = new SelectList(db.Category, "CategoryID", "Name", newPost.Category_id);
            return View(newPost);
        }


        [Route("home/edit/{id}")]
        [Authorize]
        public ActionResult Edit(int id, [Bind(Include = "PostID,Title,ShortDescription,Description,Published,PostedOn,Modified,Category_id")] Post newPost)
        {
            List<string> tagList = new List<string>();
            if (Request.HttpMethod == "POST")
            {
                // Post request method                
                var tags = Request.Form["tags"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                // Save content
                PostManager.Update(id, newPost, tags.ToList());
                // Redirect
                Response.Redirect("~/home");
            }
            else
            {
                // Find the required post.                                       
                Post NewPost = new Post();
                using (Models.Database bd = new Models.Database())
                {
                    NewPost = bd.Post.Where(b => b.PostID == id).First();
                    var tg = from s in db.Tag
                             join sa in db.PostTagMap on s.TagID equals sa.Post_Tag
                             where sa.Post_ID == id
                             select s.Name;
                    tagList = tg.ToList();
                }
                ViewBag.Found = true;
                ViewBag.Tags = tagList;
                ViewBag.Category_id = new SelectList(db.Category, "CategoryID", "Name", NewPost.Category_id);
                return View(NewPost);

            }
            return RedirectToAction("Index");
        }

    }
}