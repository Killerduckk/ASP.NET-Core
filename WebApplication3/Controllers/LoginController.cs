using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.Metadata.Ecma335;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
    
       [HttpPost]
       [Consumes("application/json")]
        public IActionResult Login([FromBody] LoginModel model) //
        {
            Console.WriteLine("username : " + model.username);
            Console.WriteLine("password : " + model.password);
            var sql = string.Format("SELECT* FROM USERS WHERE USER_PHONE='{0:G}'", model.username);
            var userInfo=WebApplication3.DataBase.oracleCon.sqlQuerySingelUser(sql);
            Console.WriteLine(userInfo.MyToString());
            if (userInfo.myIsNull()) { return StatusCode(200, new { usertype =-1, message = " 用户名不存在" }); }
            else if (userInfo.password != model.password) { return StatusCode(200, new { usertype = -1, message = "密码错误" }); }
            else if (userInfo.password == model.password) { return StatusCode(200, new { usertype =userInfo.userType, message = "登录成功" }); }
            //对于这个StatusCode的使用还是不太理解，特别是这个new
            else { return StatusCode(200, new { message = "未知错误" }); }
        }
    }
    public class LoginModel
    {
        private static  string defaultValue = "-1";
        public string username { get; set; } = defaultValue;
        //这个需要解决波浪线问题，给一个默认值
        public string password { get; set; } = defaultValue;

        public int userType { get; set; } = -1;
        public string MyToString(){ return "username:" + username + "+password:" + password; }

        public bool myIsNull() { return username == defaultValue; }
    }
}