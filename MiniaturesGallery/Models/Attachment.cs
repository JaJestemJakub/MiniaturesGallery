using MiniaturesGallery.Models.Abstracts;
using static System.Net.Mime.MediaTypeNames;

namespace MiniaturesGallery.Models
{
    public class Attachment : OwnedAbs
    {
        public Attachment(string userID) : base(userID){}
        public Attachment() : base(OwnedAbs.Anynomus) {}

        public int ID { get; set; }
        public string? FileName { get; set; }
        public string? FullFileName { get; set; }
        public int PostID { get; set; }

        public Post? Post { get; set; }
    }
}
