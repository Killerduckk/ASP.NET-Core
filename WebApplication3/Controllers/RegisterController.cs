using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;


namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Post([FromBody] RegisterModel model) //
        {
            Console.WriteLine("username : " + model.username);
            Console.WriteLine("password : " + model.password);
            return Ok();
        }

        

    }

    public class RegisterModel
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}