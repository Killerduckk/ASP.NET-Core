using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using DBproject;
namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : Controller
    {
        [HttpGet("getBrowsingHistory")]
        [Consumes("application/json")]
        public IActionResult GetBrowsingHistory([FromQuery] int cus_id)
        {
            try
            {
                Console.WriteLine("Get GetBrowsingHistory function");
                List<CommodityListModel> list = DataBase.oracleCon.sqlSearchBrowseHistory(cus_id);
                return StatusCode(200, new { com_list = list });
            }
            catch (Exception ex)
            {
                Console.WriteLine("In GetBrowsingHistory function go wrong.");
                Console.WriteLine(ex);
                return StatusCode(200, new { msg = "发生错误" });
            }

        }

        [HttpPost("setBrowsingHistory")]
        [Consumes("application/json")]
        public IActionResult SetBrowsingHistory([FromBody] BrowsingHistoryModel model)
        {
            var sql = $"Insert into BROWSE (BRO_TIME_START,BRO_TIME_END,COM_ID,BROWSER_ID,WHETHER_BUY) " +
                $"values ( TO_DATE('{model.bro_time_start}', 'YYYY-MM-DD HH24:MI:SS'),TO_DATE('{model.bro_time_end}', 'YYYY-MM-DD HH24:MI:SS'),{model.com_id},{model.browser_id},{model.whether_buy})";
            try
            {
                Console.WriteLine("Get into   SetBrowsingHistory function");
                int result = DataBase.oracleCon.sqlInsertSingleItem(sql);
                if (result == 1)
                    return StatusCode(200, new { msg = "插入浏览记录成功" });
                else
                    return StatusCode(200, new { msg = "插入浏览记录失败" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(200, new { msg = "插入浏览记录失败" });
            }

        }
    }

    public class BrowsingHistoryModel
    {
        public string bro_time_start { get; set; } = "-1";
        public string bro_time_end { get; set; } = "-1";

        public int com_id { get; set; }

        public int browser_id { get; set; }

        public int whether_buy { get; set; }

    }

    public class GetBrowsingHistoryModel
    {

        public int cus_id { get; set; }

    }
}
