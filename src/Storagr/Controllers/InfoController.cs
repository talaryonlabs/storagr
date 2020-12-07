using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Storagr.Controllers
{
    [ApiController]
    [Authorize]
    [Route("info")]
    public class InfoController : ControllerBase
    {
    }
}