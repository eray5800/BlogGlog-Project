namespace DAL.Models.HelperModels
{
    public class UserBlogs
    {
        public AppUser User { get; set; }
        public IEnumerable<Blog> Blogs { get; set; }
    }
}
