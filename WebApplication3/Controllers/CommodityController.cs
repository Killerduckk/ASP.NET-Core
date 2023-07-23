using Microsoft.AspNetCore.Mvc;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommodityController : Controller
    {

        [HttpGet("detail")]
        [Consumes("application/json")]
        public IActionResult GetCommodityDetail(int com_id)
        {
            try {
                Console.WriteLine("Get into  GetCommodityDetail function");
                return StatusCode(200, (DataBase.oracleCon.searchCommodityByID(com_id))); 
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
                return StatusCode(200, new {msg=ex}); 
            }
            
        }
        public class CommodityDetailModel
        {
            //private string _com_expirationDate = "0000-00-00";
            //需要分离写，不然会重复调用，需要写一个  _变量 来存 变量的值
            //private static int defaultValueCom_Id = -1;
            //日期字符串和日期对象，日期字符串转为日期对象：DateTime.parse(string s)
            // 日期对象转为日期字符串：date.ToString("yyyy-mm-dd")
            public int com_id { get; set; } = -1;
            public string com_name { get; set; } = "-1";
            public string com_introduction { get; set; } = "-1";
            public double com_oriPrice { get; set; } = -1;
            public string com_expirationDate { get; set; } = "0000-00-00";

            public string com_uploadDate { get; set; } = "0000-00-00";
            public int com_left { get; set; } = -1;
            public double com_rating { get; set; } = -1;
            public string sto_name { get; set; } = "-1";
            public double sto_id { get; set; } = 111;
            public List<string> com_categories { get; set; } = new List<string>();
            //不知是否需要一个数据结构



            public List<string> com_images { get; set; } = new List<string>();

            public  double com_price { get; set; }
            public List<PriceCurveModel> com_prices { get; set; } = new List<PriceCurveModel>();

            public List<string> comments { get; set; } =new List<string>();

            public bool MyIsNull() { return com_id == -1; }
        };

        public class PriceCurveModel
        {

            public string com_pc_time { get; set; } = "-1";

            public double com_pc_price { get; set; }=-1;
        }


    }
}
