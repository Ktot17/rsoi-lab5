using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RatingBL.OutputPorts;
using RatingServer.Models;

namespace RatingServer.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/rating/")]
public class RatingController(IRatingManager ratingManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserRatingResponse>> GetUserRating()
    {
        var username = User.Identity?.Name!;
        return Ok(new UserRatingResponse(await ratingManager.GetRating(username)));
    }

    [HttpPatch]
    public async Task<ActionResult> UpdateUserRating(int stars)
    {
        var username = User.Identity?.Name!;
        await ratingManager.UpdateRating(username, stars);
        return Ok();
    }
}
