using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PersonalFinance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        protected int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("User not authenticated");
        }

        protected bool IsUserAuthenticated()
        {
            return User.Identity?.IsAuthenticated == true;
        }
    }
}