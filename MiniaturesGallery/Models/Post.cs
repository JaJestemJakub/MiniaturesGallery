using Microsoft.AspNetCore.Identity;
using MiniaturesGallery.Models.Abstracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniaturesGallery.Models
{
    public class Post : OwnedAbs
    {
        public Post(string userID) : base(userID){}
        public Post() : base(OwnedAbs.Anynomus) {}

        public int ID { get; set; }
        public string? Topic { get; set; }
        [Display(Name = "Description")]
        public string? Text { get; set; }
        public float Rating { get; set; }
        public DateTime CrateDate { get; set; }
        [NotMapped]
        public int NoOfRates { get; set; }

        public ICollection<Attachment>? Attachments { get; set; }
        public ICollection<Comment>? Coments { get; set; }
        public ICollection<Rate>? Rates { get; set; }
        public IdentityUser? User { get; set; }
    }
}
