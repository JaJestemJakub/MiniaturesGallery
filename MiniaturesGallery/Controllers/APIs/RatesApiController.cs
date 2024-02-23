using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniaturesGallery.Exceptions;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;
using MiniaturesGallery.Services;

namespace MiniaturesGallery.Controllers.APIs
{
    [Route("RatesApiController")]
    [ApiController]
    public class RatesApiController : ControllerBase
    {
        private readonly IRatesService _ratesService;
        private readonly IAuthorizationService _authorizationService;

        public RatesApiController(IAuthorizationService authorizationService, IRatesService ratesService)
        {
            _authorizationService = authorizationService;
            _ratesService = ratesService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Rate>> Get([FromRoute] int id)
        {
            Rate rate = _ratesService.Get(id);
            if (rate == null) { throw new NotFoundException("Rate not found"); }
            return Ok(rate);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody][Bind("ID,Rating,PostID,UserID")] Rate rate)
        {
            int id = _ratesService.Create(rate);

            return Created($"PostsApiController/{id}", null);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var rate = _ratesService.Get(id);
            if (rate == null) { throw new NotFoundException("Rate not found"); }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, rate, Operations.Delete);
            if (!isAuthorized.Succeeded) { throw new AccessDeniedException("Access Denied"); }

            _ratesService.Delete(id);
            return NoContent();
        }

        [HttpPut]
        public async Task<ActionResult> Put([FromForm][Bind("ID,Rating")] Rate rate)
        {
            var rateFromDB = _ratesService.Get(rate.ID);
            if (rateFromDB == null) { throw new NotFoundException("Rate not found"); }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, rateFromDB, Operations.Update);
            if (!isAuthorized.Succeeded) { throw new AccessDeniedException("Access Denied"); }

            _ratesService.Update(rate);
            return Ok();
        }
    }
}
