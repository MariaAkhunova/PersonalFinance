// Controllers/TestController.cs или в Program.cs для Minimal API
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            status = "✅ API работает",
            time = DateTime.UtcNow,
            apiUrl = "https://localhost:7165",
            frontendUrl = "https://localhost:7001",
            message = "Связь настроена правильно!"
        });
    }
}