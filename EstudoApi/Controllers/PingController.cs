using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstudoApi.Controllers;

[ApiController]
[Route("api/v1/ping")]
public class PingController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get() => Ok(new { message = "pong (autenticado)" });
}
