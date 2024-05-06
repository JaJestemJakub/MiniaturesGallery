using System.ComponentModel.DataAnnotations;

namespace MiniaturesGallery.Models.Abstracts
{
    public abstract class OwnedAbs
    {
        public static string Anynomus = "Anynomus";
        [Display(Name = "User ID")]
        public string UserID { get; set; }
        protected OwnedAbs(string userID)
        {
            UserID = userID;
        }
    }
}
