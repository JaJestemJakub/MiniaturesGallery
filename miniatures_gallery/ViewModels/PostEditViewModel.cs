using MiniaturesGallery.Models;

namespace MiniaturesGallery.ViewModels
{
    public class PostEditViewModel
    {
        public PostAbs? PostAbs { get; set; }
        public List<IFormFile>? Files { get; set; }
    }
}
