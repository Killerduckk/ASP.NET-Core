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

    public class SearchController : ControllerBase
    {
        [HttpPost("commodityList")]
        [Consumes("application/json")]
        public IActionResult searchCommodity([FromBody] searchCommodityModel model)
        {
           
            try
            {
                var list = DataBase.oracleCon.searchCommodityByName(model.search_str, model.sort_order);
                return StatusCode(200, new {com_list = list });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(200, new { message = "in searchCommodity function error" });
            }
           
        }
        [HttpPost("storeList")]
        [Consumes("application/json")]
        public IActionResult searchStore([FromBody] searchStoreModel model)
        {

            try
            {
                var list = DataBase.oracleCon.searchStoreByName(model.search_str);
                return StatusCode(200, new { sto_list = list });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(200, new { message = "in searchCommodity function error" });
            }

        }
    }
    public class searchCommodityModel
    {
        public string search_str { get; set; } = "-1";
        public int sort_order { get; set; } = 0;
    }
    public class searchStoreModel
    {
        public string search_str { get; set; } = "-1";
    }

    public class CommodityListModel
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
        
        public List<string> com_categories { get; set; }= new List<string>();
        public string  com_firstImage { get; set; } = "-1";
        public double com_price { get; set; } = -1;
        public bool MyIsNull() { return com_id == -1; }
    };
    public class CommodityBaseInfoModel
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
      
    };
    public class StoreListModel
    {
        //private string _com_expirationDate = "0000-00-00";
        //需要分离写，不然会重复调用，需要写一个  _变量 来存 变量的值
        //private static int defaultValueCom_Id = -1;
        //日期字符串和日期对象，日期字符串转为日期对象：DateTime.parse(string s)
        // 日期对象转为日期字符串：date.ToString("yyyy-mm-dd")
        public double sto_id { get; set; } = 111;

        public string sto_name { get; set; } = "-1";
        public string sto_introduction { get; set; } = "-1";
      
      

        public List<string> com_categories { get; set; } = new List<string>();

        public string user_address { get; set; } = "-1";
        public string sto_firstImage { get; set; } = "-1";
  

        public List<SubCommodityListModel> com_list{ get; set; } = new List<SubCommodityListModel>();

    };
    public class SubCommodityListModel
    {
        //private string _com_expirationDate = "0000-00-00";
        //需要分离写，不然会重复调用，需要写一个  _变量 来存 变量的值
        //private static int defaultValueCom_Id = -1;
        //日期字符串和日期对象，日期字符串转为日期对象：DateTime.parse(string s)
        // 日期对象转为日期字符串：date.ToString("yyyy-mm-dd") 
        public string com_name { get; set; } = "-1";
        public string com_expirationDate { get; set; } = "0000-00-00";
        public string com_firstImage { get; set; } = "-1";
        public double com_price { get; set; } = -1;

    };



}
