namespace DAL.Models
{
    public class CommentLike : Base
    {
        public Guid CommentLikeID { get; set; }




        public AppUser User { get; set; }


        public Blog Blog { get; set; }

        public Comment Comment { get; set; }
    }
}
