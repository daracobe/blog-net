using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using MyBlog.Models;
using System.Web.Security;

namespace MyBlog.Controllers
{
    public class UserController : Controller
    {
        private MyBlog.Models.Database db = new MyBlog.Models.Database();
        

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(User user)
        {
            UserModel userModel = new UserModel();


            user.UserCreatedDateTime = DateTime.Now;
            user.UserPassword = Crypto.Hash(user.UserPassword);

            UserModel.SignInStatus userSaved = userModel.saveUser(user);

            string message = string.Empty;

            switch (userSaved)
            {
                case UserModel.SignInStatus.EmailExist:
                    ModelState.AddModelError("", "Email already exists. Please choose a different username.");
                    break;
            
                default:
                    message = "Registration successful";
                    break;
            }

            ViewBag.Message = message;

            return View(user);
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(User user)
        {
            UserModel userModel = new UserModel();

            int? userId = userModel.ValidateUser(user.UserEmail, user.UserPassword);

            string message = string.Empty;

            switch (userId.Value)
            {
                case -1:
                    ModelState.AddModelError("", "Username and/or password is incorrect.");
                    return View(user);

                default:
                    FormsAuthentication.SetAuthCookie(user.UserEmail,false);
                    return Redirect("~/Home/Index");
            }

        }

        [HttpPost]
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/Home/Index");
        }

    }


}