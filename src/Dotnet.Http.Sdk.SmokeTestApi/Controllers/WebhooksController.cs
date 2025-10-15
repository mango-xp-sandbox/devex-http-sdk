namespace Dotnet.Http.Sdk.SmokeTestApi.Controllers
{
    using System.Text;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Public;

    [ApiController]
    [Route("webhooks")]
    [AllowAnonymous]
    public class WebhooksController(ISinchSignatureVerifier verifier) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Webhook(CancellationToken ct)
        {
            // read raw body exactly as signed
            using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms, ct);
            var payload = ms.ToArray();

            var auth = Request.Headers.Authorization.ToString();
            if (!verifier.Verify(auth, payload)) return Unauthorized();

            Console.WriteLine($"[WEBHOOK] {Encoding.UTF8.GetString(payload)}");
            return Ok();
        }
    }
}