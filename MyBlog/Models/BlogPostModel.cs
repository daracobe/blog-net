using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Text;


namespace MyBlog.Models
{
    public class BlogPostModel
    {
        // General properties
        public int ID { get; set; }


        [Required(ErrorMessage = "The title is required")]
        [StringLength(150)]
        public string Title { get; set; }

        [Required(ErrorMessage = "The content is required")]
        public string Content { get; set; }
        public List<string> Tags { get; set; }

        // Time based properties
        public DateTime CreateTime { get; set; }
    }

    public class PostManager
    {
        // Define the members
        private static string PostsFile = HttpContext.Current.Server.MapPath("~/App_Data/Posts.json");
        private static List<BlogPostModel> posts = new List<BlogPostModel>();

        // The CRUD functions
        public static void Create(Post newpost, List<string> tags, string userEmail)
        {
            PostManager newObj = new PostManager();

            //Save Post, tags and PostTagMap
            using (Database bd = new Database())
            {
                var NewPost = new Post()
                {
                    Title = newpost.Title,
                    ShortDescription = newpost.ShortDescription,
                    Description = newpost.Description,
                    Published = true,
                    PostedOn = DateTime.Now.Date,
                    Category_id = newpost.Category_id,
                    UserID = UserModel.getUserIDbyEmail(userEmail)
                };
                try
                {
                    // Add Post
                    bd.Post.Add(NewPost);
                    // Add Tags
                    foreach (string tagDesc in tags)
                    {
                        int TagExist = -1;
                        TagExist = newObj.findTag(tagDesc);
                        if (TagExist == -1)
                        {
                            Tag newTag = new Tag();
                            newTag.Name = tagDesc.ToLower().Trim();
                            newTag.Description = tagDesc.ToLower().Trim();
                            bd.Tag.Add(newTag);
                            bd.PostTagMap.Add(new PostTagMap
                            {
                                Post_ID = NewPost.PostID,
                                Post_Tag = newTag.TagID
                            });
                        }
                        else
                        {
                            bd.PostTagMap.Add(new PostTagMap
                            {
                                Post_ID = NewPost.PostID,
                                Post_Tag = TagExist
                            });
                        }
                        bd.SaveChanges();
                    }
                }
                catch (Exception ex) { }
            }
        }

        public int findTag(string tagName)
        {
            try
            {
                tagName = tagName.Trim();
                int tagId = -1;
                Tag FTag = new Tag();
                using (Database bd = new Database())
                {
                    FTag = bd.Tag.Where(b => b.Name.ToLower().Equals(tagName.ToLower())).First();
                    if (FTag != null)
                        tagId = FTag.TagID;
                }
                return tagId;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }



        public static List<BlogPostModel> Read()
        {
            posts.Clear();
            /////Entity Framewok
            using (Database bd = new Database())
            {

                var objPost = new Post();

                List<Post> ListPost = new List<Post>();
                Tag Tags = new Tag();

                // leer Post de la BD
                ListPost = bd.Post.Where(b => b.Published == true).ToList();
                foreach (var xPost in ListPost)
                {
                    List<string> ListTag = new List<string>();
                    ListTag.Clear();

                    var PostTags = from b in bd.PostTagMap
                                   where b.Post_ID == xPost.PostID
                                   select b;
                    foreach (var TagPostObj in PostTags)
                    {
                        Tags = bd.Tag.Where(b => b.TagID == TagPostObj.Post_Tag).First();
                        ListTag.Add(Tags.Name);
                    }

                    posts.Add(new BlogPostModel
                    {
                        Tags = ListTag,
                        Title = xPost.Title,
                        Content = xPost.Description,
                        CreateTime = (DateTime)xPost.PostedOn,
                        ID = xPost.PostID
                    });
                }
            }
            return posts;
        }

        public static void Update(int id, Post obj, List<string> tags)
        {
            PostManager newObj = new PostManager();
            //Save Post, tags and PostTagMap
            using (Database bd = new Database())
            {
                try
                {
                    Post NewPost = bd.Post.Where(b => b.PostID == id).First();
                    NewPost.Title = obj.Title;
                    NewPost.ShortDescription = obj.ShortDescription;
                    NewPost.Description = obj.Description;
                    NewPost.Published = true;
                    NewPost.Modified = DateTime.Now.Date;
                    NewPost.Category_id = obj.Category_id;
                    bd.SaveChanges();

                    var CurrentTags = bd.PostTagMap.Where(b => b.Post_ID == NewPost.PostID).ToList();
                    foreach (PostTagMap xx in CurrentTags)
                    {
                        bd.PostTagMap.Attach(xx);
                        bd.PostTagMap.Remove(xx);
                        bd.SaveChanges();
                    }

                    // Update Tags
                    foreach (string tagDesc in tags)
                    {
                        int TagExist = -1;
                        TagExist = newObj.findTag(tagDesc);
                        if (TagExist == -1)
                        {
                            Tag newTag = new Tag();
                            newTag.Name = tagDesc.ToLower();
                            newTag.Description = tagDesc.ToLower();
                            bd.Tag.Add(newTag);
                            bd.PostTagMap.Add(new PostTagMap
                            {
                                Post_ID = NewPost.PostID,
                                Post_Tag = newTag.TagID
                            });
                        }
                        else
                        {
                            bd.PostTagMap.Add(new PostTagMap
                            {
                                Post_ID = NewPost.PostID,
                                Post_Tag = TagExist
                            });
                        }
                        bd.SaveChanges();
                    }
                }
                catch (Exception ex) { }
            }
        }
    }

    public static class Crypto
    {
        public static string Hash(string value)
        {
            return Convert.ToBase64String(
                System.Security.Cryptography.MD5.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(value)));
        }
    }

    public class UserModel
    {
        public enum SignInStatus
        {
            Success = 0,
            EmailExist = 1,
            Failure = 2
        }

        public int ValidateUser(string userName, string userPassword)
        {
            int login;

            using (Database bd = new Database())
            {
                userPassword = Crypto.Hash(userPassword);
                User currentUser = bd.User.Where(u => u.UserEmail == userName && u.UserPassword == userPassword).FirstOrDefault();

                if (currentUser != null)
                {
                    login = 1;

                }
                else
                    login = -1;

            }

            return login;
        }

        public SignInStatus saveUser(User currentUser)
        {
            SignInStatus userSaved = SignInStatus.Failure;

            using (Database db = new Database())
            {
                User existUser = db.User.Where(u => u.UserEmail == currentUser.UserEmail).FirstOrDefault();

                if (existUser != null)
                {
                    userSaved = SignInStatus.EmailExist;
                }
                else
                {
                    db.User.Add(currentUser);
                    db.SaveChanges();

                    userSaved = SignInStatus.Success;
                }

            }

            return userSaved;
        }

        public static int getUserIDbyEmail(string userEmail)
        {
            int userID;

            using (Database db = new Database())
            {
                userID = db.User.Where(u => u.UserEmail == userEmail).Single().UserID;
            }

            return userID;
        }
    }
}