using MiniaturesGallery.Models.Abstracts;

namespace MiniaturesGallery.Models
{
    public class Post : OwnedAbs
    {
        public Post(string userID) : base(userID){}
        public Post() : base(OwnedAbs.Anynomus) {}

        public int ID { get; set; }
        public string? Topic { get; set; }
        public string? Text { get; set; }
        public float Rating { get; set; }

        public ICollection<Attachment>? Attachments { get; set; }
        public ICollection<Comment>? Coments { get; set; }
        public ICollection<Rate>? Rates { get; set; }
    }
}
