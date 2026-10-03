using GatewayServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GatewayServer.Controllers;

[ApiController]
[Route("api/v1")]
public class AuthController(HttpClient client, IConfiguration config) : ControllerBase
{
    [HttpPost("authorize")]
    [AllowAnonymous]
    public async Task<ActionResult> AuthorizeAsync([FromBody] LoginRequest request)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = config["Auth0:ClientId"]!,
            ["client_secret"] = config["Auth0:ClientSecret"]!,
            ["username"] = request.Username,
            ["password"] = request.Password,
            ["audience"] = config["Auth0:Audience"]!,
            ["scope"] = "openid profile email"
        };
        
        var response = await client.PostAsync(
            new Uri($"https://{config["Auth0:Domain"]}/oauth/token"),
            new FormUrlEncodedContent(form));

        var json = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
            return BadRequest(json);
        
        return Content(json, "application/json");
    }
}
