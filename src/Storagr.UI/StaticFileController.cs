using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Storagr.UI
{
    public class StaticFileController : Controller
    {
        [HttpGet]
        [Route("site.webmanifest")] // because .webmanifest is not in MIME list of app.UseStaticFiles();
        public ContentResult GetSiteManifest()
        {
            return Content(System.IO.File.ReadAllText("wwwroot/site.webmanifest"), "application/manifest+json", Encoding.UTF8);
        }
    }
}
