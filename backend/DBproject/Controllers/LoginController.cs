using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using System;


namespace DBproject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpPost()]
        [Consumes("application/json")]
        public IActionResult PostLogin([FromBody] LoginModel model)
        {
            Console.WriteLine("登录信息如下:");
            Console.WriteLine("login_type: " + model.login_type);
            Console.WriteLine("user_ID_phonenumber: " + model.user_ID_phonenumber);
            Console.WriteLine("user_password: " + model.user_password);
            string message = "";
            if (model.login_type == "0")//ID登录
            {
                message = DataBase.oracleCon.IDLogin(model.user_ID_phonenumber, model.user_password);
            }
            else if(model.login_type == "1")//电话登录
            {
                message = DataBase.oracleCon.PhonenumberLogin(model.user_ID_phonenumber, model.user_password);
            }

            LoginReturnModel returnmodel = new LoginReturnModel();
            if (char.IsDigit(message[0]))
            {
                returnmodel.user_type = message;
                returnmodel.message = "success";
                if (model.login_type == "1")
                {
                    returnmodel.user_ID = DataBase.oracleCon.GetNewUserID(model.user_ID_phonenumber);
                }
                else
                {
                    returnmodel.user_ID = model.user_ID_phonenumber;
                }
            }
            else
            {
                returnmodel.user_type = "-1";
                returnmodel.message = message;
            }
            return new JsonResult(returnmodel);
        }
    }

    public class LoginModel
    {
        public string login_type { get; set; }
        public string user_ID_phonenumber { get; set; }
        public string user_password { get; set; }
    }
    public class LoginReturnModel
    {
        public string user_type { get; set; }
        public string message { get; set; }
        public string user_ID { get; set; }
    }
}
