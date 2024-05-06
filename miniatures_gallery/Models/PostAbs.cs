using Microsoft.AspNetCore.Identity;
using MiniaturesGallery.Models.Abstracts;
using System.ComponentModel.DataAnnotations;

namespace MiniaturesGallery.Models
{
    public abstract class PostAbs : OwnedAbs
    {
        public PostAbs(string userID) : base(userID) { }
        public PostAbs() : base(OwnedAbs.Anynomus) { }

        public int ID { get; set; }
        [Display(Name = "Topic")]
        public string? Topic { get; set; }
        [Display(Name = "Description")]
        public string? Text { get; set; }
        [Display(Name = "Crate Date")]
        public DateTime CrateDate { get; set; }

        public IdentityUser? User { get; set; }
    }
}
