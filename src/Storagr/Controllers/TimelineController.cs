﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared;

namespace Storagr.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("repositories/{repositoryId}/timeline")]
    public class TimelineController : StoragrController
    {
        
    }
}