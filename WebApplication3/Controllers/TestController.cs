using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using Oracle.ManagedDataAccess.Client;
using System.Drawing;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;
using System.Xml.Linq;
using static WebApplication3.Controllers.SellerController;
using System.Runtime.InteropServices;
using System.Reflection;
using System;
using System.Diagnostics;
using System.IO;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost("commodityInsertByName")]
        [Consumes("application/json")]
        /*
        作用：按照seq_for_commodity_id序列往数据库中添加不同com_name的内容
        传入：不同name的商品
        输出：无
        */
        public IActionResult insertCommodityByName([FromBody] nameModel data)
        {
            Console.WriteLine("Get Into testSearch Function");
            var toBeInsert = new CommodityModel();
            var sqlList = new List<string>();

            toBeInsert.com_name = data.com_name;
            toBeInsert.com_left = 3;
            toBeInsert.com_expirationDate = "2024-03-08";
            toBeInsert.com_uploadDate = "2023-03-08";
            toBeInsert.com_expirationDate = "2024-03-08";

            var sqlInsertStr_1 = sqlInsertCommodity(toBeInsert);
            Console.WriteLine(sqlInsertStr_1);
            var sqlInsertStr_2 = "INSERT INTO COMMODITY_IMAGE(COM_ID,COM_IMAGE)" +
             "VALUES(seq_for_commodity_id.CURRVAL,'image.jpg')";
            Console.WriteLine(sqlInsertStr_2);
            sqlList.Add(sqlInsertStr_1);
            sqlList.Add(sqlInsertStr_2);
            int insertResult = WebApplication3.DataBase.oracleCon.sqlInsertAtomicity(sqlList);
            if (insertResult == -1)
                return StatusCode(200, insertResult);
            else if (insertResult == 1)
                return StatusCode(200, insertResult);
            else
                return StatusCode(200, insertResult);
        }


        [HttpPost("commodityTest")]
        [Consumes("application/json")]
        public IActionResult testSeq()
        {
            Console.WriteLine("Get Into testSeq Function");
            var toBeInsert = new CommodityModel();
            var sqlList = new List<string>();

            toBeInsert.com_left = 3;
            toBeInsert.com_expirationDate = "2024-03-08";
            toBeInsert.com_uploadDate = "2023-03-08";
            toBeInsert.com_expirationDate = "2024-03-08";

            var sqlInsertStr_1 = sqlInsertCommodity(toBeInsert);
            Console.WriteLine(sqlInsertStr_1);
            var sqlInsertStr_2 = "INSERT INTO COMMODITY_IMAGE(COM_ID,COM_IMAGE)" +
             "VALUES(seq_for_commodity_id.CURRVAL,'image.jpg')";
            Console.WriteLine(sqlInsertStr_2);
            sqlList.Add(sqlInsertStr_1);
            sqlList.Add(sqlInsertStr_2);

            int insertResult = WebApplication3.DataBase.oracleCon.sqlInsertAtomicity(sqlList);
            if (insertResult == -1)
                return StatusCode(200, insertResult);
            else if (insertResult == 1)
                return StatusCode(200, insertResult);
            else
                return StatusCode(200, insertResult);
        }
        /*
         作用：生成一条完整的插入table commodity的完整的sql语句
         传入：CommodityModel 类型数据
         输出：完整的sql语句。        
         */
        public string sqlInsertCommodity(CommodityModel data)
        {
            //记住序列号是seq_for_commodity_id,最大到200000
            var sqlInsertStr = string.Format("INSERT INTO COMMODITY(COM_ID,COM_NAME,COM_INTRODUCTION," +
            "COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE,COM_LEFT,COM_RATING,STO_ID)" +
            "VALUES(seq_for_commodity_id.NEXTVAL,'{0:G}','{1:G}',{2:G},TO_DATE('{3:G}','YYYY-MM-DD'),TO_DATE('{4:G}','YYYY-MM-DD'),{5:G},{6:G},{7:G})",
            data.com_name, data.com_introduction, data.com_oriPrice,
            data.com_expirationDate, data.com_uploadDate, data.com_left, data.com_rating, data.sto_id);
            return sqlInsertStr;
        }

        [HttpPost("commoditySearchByName")]
        [Consumes("application/json")]
        /*
         作用：输入name，查找数据库中模糊匹配的数据，并且返回所查找到的id-name的数组
         传入：CommodityModel 类型数据
         输出：完整的sql语句。        
         */
        public IActionResult searchCommodityByName([FromBody] nameModel data)
        {
            //Console.WriteLine(RuntimeInformation.FrameworkDescription);
            try
            {
                List<nameModel> list = new List<nameModel>();
                list =  DataBase.oracleCon.testGetCommodityByName(data.com_name);
                string length = list.Count().ToString();
                Console.WriteLine("successfully search " + list.Count().ToString() + " results");
                return StatusCode(200, list);
            }
            catch (Exception ex)
            {
                Console.WriteLine("in searchCommodityByName function error: ");
                Console.WriteLine(ex.Message);
                return StatusCode(200);
            }
        }
        [HttpPost("testPathWay")]
        [Consumes("application/json")]
        /*
        1.这个函数展示了c#中常用的路径相关的函数以及路径表示知识

           C:\Documents\Newsletters\Summer2018.pdf	            C: 驱动器的根目录中的绝对文件路径。
            \Program Files\Custom Utilities\StringFinder.exe	当前驱动器根路径上的相对路径。
            2018\January.xlsx	                                指向当前目录的子目录中的文件的相对路径。
            ..\Publications\TravelBrochure.pdf	                指向从当前目录开始的目录中的文件的相对路径。
            C:\Projects\apilibrary\apilibrary.sln	            C: 驱动器的根目录中的文件的绝对路径。
            C:Projects\apilibrary\apilibrary.sln	            C: 驱动器的当前目录中的相对路径，建议不要使用
         
        2.需要注意@的用法：在c#中会将\作为转义符，所以无法直接表示路径，可以使用\\或者使用@+\来表示，如下：
          同时$也是好用的
          "C:\\User\\Administor\\pictures" 或者@"C:\User\Administor\pictures",二者同含义，@的含义是表示后面的\不做转义符
         
        3.一些可用的函数
          Environment.CurrentDirectory 返回当前的工作路径
          Directory.SetCurrentDirectory("d:\\pictures"); 设置当前的工作路径
          var path = Path.GetFullPath(".\\wwwroot");设置当前的完整路径
          
        4.使用
          如果需要表示相对工作路径的话就用".\\wwwroot"
          如果仅仅使用"\\wwwroot"则会在前自动加上D: 根路径还是D:
          
         */
        public IActionResult testPathWay()
        {
            Console.WriteLine($"当前当前的工作路径{Environment.CurrentDirectory}");
            //当前的工作路径

            var path = Path.GetFullPath(".\\wwwroot");
            //使用相对工作路径下的相对路径
            Console.WriteLine($"使用相对工作路径下的相对路径{path}");

            path = Path.GetFullPath("..\\wwwroot");
            //使用相对工作路径下的父路径
            Console.WriteLine($"使用相对工作路径下的父路径{path}");

            path = Path.GetFullPath("\\wwwroot");
            //使用相对根路径下的路径
            Console.WriteLine($"使用相对根路径下的路径{path}");

           
            Directory.SetCurrentDirectory("d:\\pictures");
            //需要注意运行时该路径存在
            Console.WriteLine($"重新设置之后当前的工作路径{Environment.CurrentDirectory}");
            //当前的工作路径

            path = Path.GetFullPath(".\\wwwroot");
            //使用相对工作路径下的相对路径
            Console.WriteLine($"使用相对工作路径下的相对路径{path}");

            path = Path.GetFullPath("..\\wwwroot");
            //使用相对工作路径下的父路径
            Console.WriteLine($"使用相对工作路径下的父路径{path}");

            path = Path.GetFullPath("\\wwwroot");
            //使用相对根路径下的路径
            Console.WriteLine($"使用相对根路径下的路径{path}");

            return Ok();    
        }
        public class nameModel
        {
            public string com_name { get; set; } = "-1";
            public double com_id { get; set; } = 1;
        }
    }
}