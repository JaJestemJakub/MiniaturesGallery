using static System.Net.Mime.MediaTypeNames;

namespace MiniaturesGallery.Models
{
    public class Attachment
    {
        public int ID { get; set; }
        public string FileName { get; set; }
        public string FullFileName { get; set; }
        public int PostID { get; set; }

        public Post? Post { get; set; }
    }
}
