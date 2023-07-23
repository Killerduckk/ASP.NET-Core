using Microsoft.AspNetCore.Mvc;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : Controller
    {
        [HttpGet("detail")]
        [Consumes("application/json")]
        public IActionResult GetStoreDetail(int sto_id)
        {
            
            try
            {
                int data = sto_id;
                Console.WriteLine("Get into  GetStoreDetail function");
                return StatusCode(200, (DataBase.oracleCon.searchStoreByID(data)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(200, new { msg = ex });
            }
        }
    }
}
