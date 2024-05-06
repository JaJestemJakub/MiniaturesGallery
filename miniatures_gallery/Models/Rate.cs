using MiniaturesGallery.Models.Abstracts;
using System.ComponentModel.DataAnnotations;

namespace MiniaturesGallery.Models
{
    public class Rate : OwnedAbs
    {
        public Rate(string userID) : base(userID) { }
        public Rate() : base(OwnedAbs.Anynomus) { }

        public int ID { get; set; }
        [Display(Name = "Rating")]
        public int Rating { get; set; }
        public int PostID { get; set; }
    }
}
