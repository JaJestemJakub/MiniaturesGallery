using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;

namespace MiniaturesGallery.ViewModels
{
    public class PostsOfUserViewModel
    {
        public PaginatedList<Post>? PaginatedList { get; set; }
        public string? UserID { get; set; }
        public string? UserName { get; set; }
    }
}
