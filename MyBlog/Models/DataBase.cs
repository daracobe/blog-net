using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MyBlog.Models
{
    class Database : DbContext 
    {
        public Database() : base(global::System.Configuration.ConfigurationManager.ConnectionStrings["BlogEntities"].ConnectionString) //<- Pasar la connectionString
        {
        }

        public DbSet<Post> Post { get; set; }
        public DbSet<Tag> Tag { get; set; }
        public DbSet<PostTagMap> PostTagMap { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<User> User { get; set; }
    }
}