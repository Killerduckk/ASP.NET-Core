using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net;
namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : Controller
    {
        [HttpGet]
        public string GetImage(int com_id)
        {
            Console.WriteLine("image\n");
          
            Console.WriteLine(String.Format("select COM_IMAGE from COMMODITY_IMAGE where COM_ID = {0:G}", com_id));
            string relativePath = WebApplication3.DataBase.oracleCon.sqlOperation
                (String.Format("select COM_IMAGE from COMMODITY_IMAGE where COM_ID = {0:G}", com_id));
           
            string path = @"C:\Users\Administrator\Pictures\" + com_id.ToString()+".jpg";
            Console.WriteLine(relativePath);
            return ImageToBase64(path);
        }

        [HttpGet("com_detail")]
        public string GetDetail(int com_id)
        {
            Console.WriteLine("detail\n");
            // 图片路径
            Console.WriteLine(String.Format("select COM_INTRODUCTION from COMMODITY where COM_ID = {0:G}", com_id));
            string result = WebApplication3.DataBase.oracleCon.sqlOperation
                (String.Format("select COM_INTRODUCTION from COMMODITY where COM_ID = {0:G}", com_id),1);
            Console.WriteLine(result);
            return result;
        }

        public string ImageToBase64(string imagePath)
        {
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }
    }
}
