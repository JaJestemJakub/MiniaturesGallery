using Microsoft.AspNetCore.Identity;
using MiniaturesGallery.Models.Abstracts;
using System.ComponentModel.DataAnnotations;

namespace MiniaturesGallery.Models
{
    public class Comment : OwnedAbs
    {
        public Comment(string userID) : base(userID) { }
        public Comment() : base(OwnedAbs.Anynomus) { }

        public int ID { get; set; }
        [Display(Name = "Body")]
        public string? Body { get; set; }
        [Display(Name = "Crate Date")]
        public DateTime CrateDate { get; set; }

        public int PostID { get; set; }
        public int? CommentID { get; set; } //aswer to

        public IdentityUser? User { get; set; }
        public ICollection<Comment>? Comments { get; set; }
    }
}
