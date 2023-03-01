using MiniaturesGallery.Models;

namespace MiniaturesGallery.ViewModels
{
    public class PostEditViewModel
    {
        public Post? Post { get; set; }
        public List<IFormFile>? Files { get; set; }
    }
}
