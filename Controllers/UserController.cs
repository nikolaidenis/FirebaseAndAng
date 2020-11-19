using FirebaseAndAng.Web.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FirebaseAndAng.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpPost]
        [Route("verify")]
        public async Task<IActionResult> Verify(TokenVerifyModel tokenModel)
        {
            try
            {
                var auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
                var response = await auth.VerifyIdTokenAsync(tokenModel.Token);

                return response != null ? Accepted() : BadRequest();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet("secrets")]
        [Authorize]
        public IEnumerable<string> GetSecrets()
        {
            return new List<string>()
            {
                "This is from the secret controller",
                "Seeing this means you are authenticated",
                "You have logged in using your google account from firebase",
                "Have a nice day!!"
            };
        }
    }
}
