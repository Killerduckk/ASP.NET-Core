using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using Oracle.ManagedDataAccess.Client;
using System.Drawing;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;
using System.Xml.Linq;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellerController : ControllerBase
    {
        [HttpPost("commodityPost")]
        [Consumes("application/json")]
        public IActionResult PostImage([FromBody] CommodityModel data)
        {
            //c#转64需要截头，传的时候需要加头
            Console.WriteLine("Get Into PostImage Function");
            if (data.MyIsNull())
            {
                //statusCode这个函数可以用来返回http状态码+一个数据。
                return StatusCode(200, data);
            }
            int prefixLength = "data:image/jpeg;base64,".Length;        
            string imageBase64 = data.com_image.Substring(prefixLength);
            byte[] imageBytes;
        //Console.WriteLine(data.com_image);
        //=>关于http状态码的知识：https://tool.oschina.net/commons?type=5
            string fileName;
            try { 
                 
                imageBytes = Convert.FromBase64String(imageBase64);
                fileName = data.com_id.ToString() + ".jpg";
                var filePath = "D:\\pictures\\" + fileName;
                Console.WriteLine("233");
                System.IO.File.WriteAllBytes(filePath, imageBytes);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); return StatusCode(400, data); }

            var sqlList = new List<string>();

            var sqlInsertStr_1 = string.Format("INSERT INTO COMMODITY(COM_ID,COM_NAME,COM_INTRODUCTION," +
                "COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE,COM_LEFT,COM_RATING,STO_ID)" +
                "VALUES({0:G},'{1:G}','{2:G}',{3:G},TO_DATE('{4:G}','YYYY-MM-DD'),TO_DATE('{5:G}','YYYY-MM-DD'),{6:G},{7:G},{8:G})",
                data.com_id, data.com_name, data.com_introduction, data.com_oriPrice,
                data.com_expirationDate, data.com_uploadDate, data.com_left, data.com_rating, data.sto_id);

            var sqlInsertStr_2 = string.Format("INSERT INTO COMMODITY_IMAGE(COM_ID,COM_IMAGE)" +
            "VALUES({0:G},'{1:G}')",
            data.com_id, fileName);
            sqlList.Add(sqlInsertStr_1);
            sqlList.Add(sqlInsertStr_2);

            int insertResult = DataBase.oracleCon.sqlInsertAtomicity(sqlList);
            if (insertResult == -1)
                return StatusCode(200 );
            else if (insertResult == 1)
                return StatusCode(200, data);
            else            
                return StatusCode(200);
        }
        [HttpPost("commodityGet")]
        [Consumes("application/json")]
        public IActionResult GetStoreDetail([FromBody] IDModel data)
        {
            
            Console.WriteLine("gotta get result com_id: ");
            Console.WriteLine(data.com_id);
            string sqlStr_1 = string.Format("SELECT * FROM COMMODITY WHERE COM_ID ={0:G}", data.com_id);
            var commodityInfo=DataBase.oracleCon.sqlQuerySingleCommodity(sqlStr_1);
            string sqlStr_2 = string.Format("SELECT * FROM COMMODITY_IMAGE WHERE COM_ID ={0:G}", data.com_id);
            commodityInfo.com_image= DataBase.oracleCon.sqlQuerySingleCommodityImage(sqlStr_2);
            if (commodityInfo.MyIsNull()) { Console.WriteLine("com_id is not exist"); return StatusCode(200,commodityInfo); }
            else { return Ok(commodityInfo); };
        }

      
        public class CommodityModel
        {
            //private string _com_expirationDate = "0000-00-00";
            //需要分离写，不然会重复调用，需要写一个  _变量 来存 变量的值
            //private static int defaultValueCom_Id = -1;
            public double sto_id { get; set; } = 111;
            public double com_id { get; set; } = -1;
            public string com_name { get; set; } = "-1";
            public string com_introduction { get; set; } = "-1";
            public double com_oriPrice { get; set; } = -1;

            public string com_expirationDate { get; set; } = "0000-00-00";
            //日期字符串和日期对象，日期字符串转为日期对象：DateTime.parse(string s)
            // 日期对象转为日期字符串：date.ToString("yyyy-mm-dd")
            public string com_uploadDate { get; set; } = "0000-00-00";
            public double com_left { get; set; } = -1;
            public double com_rating { get; set; } = -1;
            public string com_image { get; set; } = "-1";
            public bool MyIsNull() { return com_id == -1; }
        };
        public class StoreDetailModel
        {
            //private string _com_expirationDate = "0000-00-00";
            //需要分离写，不然会重复调用，需要写一个  _变量 来存 变量的值
            //private static int defaultValueCom_Id = -1;
            //日期字符串和日期对象，日期字符串转为日期对象：DateTime.parse(string s)
            // 日期对象转为日期字符串：date.ToString("yyyy-mm-dd")
            public double sto_id { get; set; } = 111;
            public string sto_introduction { get; set; } = "-1";

            public string sto_name { get; set; } = "-1";

            public List<string> com_categories { get; set; } = new List<string>();

          
            public string user_address { get; set; } = "-1";

            public string sto_licenseImg { get; set; } = "-1";


            public List<string> sto_imageList { get; set; } = new List<string>();

            public List<string> sto_notices { get; set; } = new List<string>();

            public List<SubCommodityListModel> com_list { get; set; } = new List<SubCommodityListModel>();

        };
        public class IDModel
        {
            public double com_id { get; set; }
        }
       
    }
}
