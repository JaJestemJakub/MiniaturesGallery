using MiniaturesGallery.Models.Abstracts;

namespace MiniaturesGallery.Models
{
    public class Rate : OwnedAbs
    {
        public Rate(string userID) : base(userID){}
        public Rate() : base(OwnedAbs.Anynomus) {}

        public int ID { get; set; }
        public int Rating { get; set; }
        public int PostID { get; set; }
    }
}
