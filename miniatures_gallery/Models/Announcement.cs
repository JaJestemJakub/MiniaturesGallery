using MiniaturesGallery.Models.Abstracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniaturesGallery.Models
{
    public class Announcement : PostAbs
    {
        public Announcement(string userID) : base(userID) { }
        public Announcement() : base(OwnedAbs.Anynomus) { }

        [Display(Name = "Private Note")]
        public string? PrivateNote { get; set; }
    }
}
