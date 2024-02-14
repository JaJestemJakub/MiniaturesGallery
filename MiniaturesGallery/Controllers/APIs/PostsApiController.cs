using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniaturesGallery.Extensions;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;
using MiniaturesGallery.Services;

namespace MiniaturesGallery.Controllers.APIs
{
    [Route("PostsApiController")]
    [ApiController]
    public class PostsApiController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IAuthorizationService _authorizationService;

        public PostsApiController(IPostService postService, IAuthorizationService authorizationService, UserManager<IdentityUser> userManager)
        {
            _postService = postService;
            _authorizationService = authorizationService;
        }

        [HttpGet("{id}")]
        public ActionResult<Post> Get([FromRoute] int id)
        {
            var post = _postService.Get((int)id, User.GetLoggedInUserId<string>());
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post);
        }

        [HttpGet]
        public ActionResult<IEnumerable<Post>> GetSortedBy([FromQuery] string? searchString, [FromQuery] string? orderByFilter, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
        {
            var posts = _postService.Get(searchString, orderByFilter, dateFrom, dateTo, User.GetLoggedInUserId<string>());

            return Ok(posts);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody][Bind("ID,Topic,Text")] Post post)
        {
            int id = await _postService.CreateAsync(post, User.GetLoggedInUserId<string>());

            return Created($"PostsApiController/{id}", null);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute][Bind("ID")] int id)
        {
            var post = _postService.Get((int)id, User.GetLoggedInUserId<string>());
            if (post != null)
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, post, Operations.Delete);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
            }
            await _postService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPut]
        public async Task<ActionResult> Put([FromBody][Bind("ID,Topic,Text")] Post post)
        {
            var postFromDB = _postService.Get((int)post.ID, User.GetLoggedInUserId<string>());
            if (postFromDB == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, postFromDB, Operations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            await _postService.UpdateAsync(post);
            return Ok();
        }
    }
}
