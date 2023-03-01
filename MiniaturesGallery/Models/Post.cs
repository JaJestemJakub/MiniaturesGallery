namespace MiniaturesGallery.Models
{
    public class Post
    {
        public int ID { get; set; }
        public string Topic { get; set; }
        public string Text { get; set; }

        public ICollection<Attachment>? Attachments { get; set; }
    }
}
