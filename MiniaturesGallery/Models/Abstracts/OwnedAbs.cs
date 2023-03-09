namespace MiniaturesGallery.Models.Abstracts
{
    public abstract class OwnedAbs
    {
        public static string Anynomus = "Anynomus";
        public string UserID { get; set; }
        protected OwnedAbs(string userID)
        {
            UserID = userID;
        }
    }
}
