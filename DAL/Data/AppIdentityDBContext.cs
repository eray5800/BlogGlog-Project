using DAL.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Context
{
    public class AppIdentityDBContext : IdentityDbContext<AppUser>
    {

        public AppIdentityDBContext(DbContextOptions<AppIdentityDBContext> options) : base(options) {
        
        
        
        }

        DbSet<Blog> Blogs { get; set; }
        DbSet<BlogLike> BlogLikes { get; set; }

        DbSet<BlogImage> BlogImages { get; set; }
        DbSet<BlogTag> BlogTags { get; set; }
        DbSet<BlogView> BlogViews { get; set; }
        DbSet<Category> Categories {  get; set; }
        DbSet<Comment> Comments { get; set; }

        DbSet<CommentLike> CommentLikes { get; set; }

        DbSet<WriterRequest> WriterRequests { get; set; }

       



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ensuring the Email is unique
            builder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }




    }
}
