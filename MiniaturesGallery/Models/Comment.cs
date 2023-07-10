using Microsoft.AspNetCore.Identity;
using MiniaturesGallery.Models.Abstracts;

namespace MiniaturesGallery.Models
{
    public class Comment : OwnedAbs
    {
        public Comment(string userID) : base(userID) { }
        public Comment() : base(OwnedAbs.Anynomus) { }

        public int ID { get; set; }
        public string? Body { get; set; }
        public DateTime CrateDate { get; set; }

        public int PostID { get; set; }
        public int? CommentID { get; set; } //aswer to

        public IdentityUser? User { get; set; }
        public ICollection<Comment>? Comments { get; set; }
    }
}
