using MiniaturesGallery.Models.Abstracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniaturesGallery.Models
{
    public class Post : PostAbs
    {
        public Post(string userID) : base(userID) { }
        public Post() : base(OwnedAbs.Anynomus) { }

        [Display(Name = "Rating")]
        public float Rating { get; set; }
        [NotMapped]
        public int NoOfRates { get; set; }
        [NotMapped]
        public int NoOfComments { get; set; }

        public ICollection<Attachment>? Attachments { get; set; }
        public ICollection<Comment>? Coments { get; set; }
        public ICollection<Rate>? Rates { get; set; }
    }
}
