using MiniaturesGallery.Models.Abstracts;
using System.ComponentModel.DataAnnotations;

namespace MiniaturesGallery.Models
{
    public class Attachment : OwnedAbs
    {
        public Attachment(string userID) : base(userID) { }
        public Attachment() : base(OwnedAbs.Anynomus) { }

        public int ID { get; set; }
        [Display(Name = "File Name")]
        public string? FileName { get; set; }
        [Display(Name = "Full File Name")]
        public string? FullFileName { get; set; }
        public int PostID { get; set; }

        public Post? Post { get; set; }
    }
}
