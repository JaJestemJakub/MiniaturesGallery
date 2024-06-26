﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniaturesGallery.Exceptions;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;
using MiniaturesGallery.Services;

namespace MiniaturesGallery.Controllers.APIs
{
    [Route("CommentsApiController")]
    [ApiController]
    public class CommentsApiController : ControllerBase
    {
        private readonly ICommentsService _commentsService;
        private readonly IAuthorizationService _authorizationService;

        public CommentsApiController(IAuthorizationService authorizationService, ICommentsService commentsService)
        {
            _authorizationService = authorizationService;
            _commentsService = commentsService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetAsync([FromRoute] int id)
        {
            var comment = _commentsService.Get((int)id);
            if (comment == null) { throw new NotFoundException("Comment not found"); }
            return Ok(comment);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromForm][Bind("ID,Body,PostID,CommentID,UserID")] Comment comment)
        {
            int id = _commentsService.Create(comment);

            return Created($"PostsApiController/{id}", null);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute][Bind("ID")] int id)
        {
            var comment = _commentsService.Get(id);
            if (comment == null) { throw new NotFoundException("Comment not found"); }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, comment, Operations.Delete);
            if (!isAuthorized.Succeeded) { throw new AccessDeniedException("Access Denied"); }

            _commentsService.Delete(id);
            return NoContent();
        }

        [HttpPut]
        public async Task<ActionResult> Put([FromForm][Bind("ID,Body")] Comment comment)
        {
            Comment commentFromDB = _commentsService.Get(comment.ID);
            if (commentFromDB == null) { throw new NotFoundException("Comment not found"); }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, comment, Operations.Update);
            if (!isAuthorized.Succeeded) { throw new AccessDeniedException("Access Denied"); }

            _commentsService.Update(comment);
            return Ok();
        }
    }
}
