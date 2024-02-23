using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniaturesGallery.Exceptions;
using MiniaturesGallery.Extensions;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;
using MiniaturesGallery.Services;

namespace MiniaturesGallery.Controllers.APIs
{
    [Route("AttachemntApiController")]
    [ApiController]
    public class AttachemntApiController : ControllerBase
    {
        private readonly IAttachmentsService _attachmentsService;
        private readonly IPostService _postService;
        private readonly IAuthorizationService _authorizationService;

        public AttachemntApiController(IAttachmentsService attachmentsService, IPostService postService, IAuthorizationService authorizationService)
        {
            _attachmentsService = attachmentsService;
            _postService = postService;
            _authorizationService = authorizationService;
        }

        [HttpGet("{id}")]
        public ActionResult<Post> Get([FromRoute] int id)
        {
            var post = _attachmentsService.Get(id);
            if (post == null) { throw new NotFoundException("Post not found"); }
            return Ok(post);
        }

        [HttpPost]
        public async Task<ActionResult> AddAttachemnt([FromBody] List<IFormFile> files, int postID)
        {
            var post = _postService.Get((int)postID, User.GetLoggedInUserId<string>());
            if (post == null) { throw new NotFoundException("Post not found"); }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, post, Operations.Create);
            if (!isAuthorized.Succeeded) { throw new AccessDeniedException("Access Denied"); }

            _attachmentsService.Create(files, postID, User.GetLoggedInUserId<string>());

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute][Bind("ID")] int id)
        {
            Attachment att = _attachmentsService.Get(id);
            if (att == null) { throw new NotFoundException("Post not found"); }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, att, Operations.Delete);
            if (!isAuthorized.Succeeded) { throw new AccessDeniedException("Access Denied"); }

            _attachmentsService.Delete(id);
            return NoContent();
        }
    }
}
