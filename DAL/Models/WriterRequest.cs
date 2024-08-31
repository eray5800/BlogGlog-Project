namespace DAL.Models
{
    public class WriterRequest
    {
        public Guid WriterRequestID { get; set; }

        public AppUser User { get; set; }

        public string RequestDescription { get; set; }

        public DateTime RequestDate { get; set; }
    }
}
