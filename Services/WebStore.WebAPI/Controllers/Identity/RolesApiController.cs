using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebStore.Interfaces;

namespace WebStore.WebAPI.Controllers.Identity
{
    [Route(WebAPIAddresses.Identity.Roles)]
    [ApiController]
    public class RolesApiController : ControllerBase
    {
    }
}
