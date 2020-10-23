using Microsoft.AspNetCore.Mvc;

namespace Storagr.Controllers
{
    [ApiController]
    [Route("/{user}/{repository}/info/lfs/locks/")]
    public class LockingController
    {
        [HttpGet]
        public void ListLocks([FromQuery] string path, [FromQuery] string id, [FromQuery] string cursor, [FromQuery] int limit, [FromQuery] string refspec)
        {
            
        }
        
        [HttpPost]
        public void CreateLock()
        {
            
        }

        [HttpPost("{id}/unlock")]
        public void DeleteLock(string id)
        {
            
        }
    }
}