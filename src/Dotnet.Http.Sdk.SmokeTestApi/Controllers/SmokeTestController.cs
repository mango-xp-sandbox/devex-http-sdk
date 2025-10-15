namespace Dotnet.Http.Sdk.SmokeTestApi.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Public;

    [ApiController]
    [Route("api/smoke-tests")]
    [AllowAnonymous]
    public class SmokeTestController(ISinchClient client) : ControllerBase
    {
        [HttpPost]
        [Route("contacts")]
        public async Task<IActionResult> Contacts([FromBody] ContactRequest req, CancellationToken ct)
        {
            var response = await client.Contacts.CreateAsync(req.Name, req.Phone, ct);
            return Ok(response);
        }

        [HttpPost]
        [Route("messages")]
        public async Task<IActionResult> Messages([FromBody] MessageRequest req, CancellationToken ct)
        {
            var response = await client.Messages.SendAsync(req.To, req.From, req.Content, ct);
            return Ok(response);
        }
    }

    public record ContactRequest(string Name, string Phone);

    public record MessageRequest(string To, string From, string Content);
}