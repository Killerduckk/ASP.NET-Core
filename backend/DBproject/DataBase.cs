using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Transactions;
using DBproject.Controllers;
using DBproject.Models;
using static WebApplication3.Controllers.CommodityController;
using static WebApplication3.Controllers.StoreController;
using WebApplication3.Controllers;

namespace DBproject
{
    public class DataBase
    {
        private string connectString = @"DATA SOURCE=124.70.7.210:1521/orcl;TNS_ADMIN=C:\Users\Administrator\Oracle\network\admin;PERSIST SECURITY INFO=True;USER ID=WST;Password=123456";
        public OracleConnection con;
        //静态类声明
        public static DataBase oracleCon;
        //构造函数
        public DataBase()
        {
            con = new OracleConnection(connectString);
            con.Open();
            Console.WriteLine("Oracle 连接成功\n");
        }

        //判断用户是否存在
        public bool IsUserExist(string ID)
        {
            bool exists = false;
            using (OracleCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM users WHERE user_ID = :ID";
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ID", OracleDbType.Varchar2).Value = ID;
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                exists = count > 0;
            }
            return exists;
        }
        //判断用户电话号码是否存在
        public bool IsUserPhoneExist(string phonenumber)
        {
            bool exists = false;
            using (OracleCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM users WHERE user_phone = :phone";
                cmd.Parameters.Clear();
                cmd.Parameters.Add("phone", OracleDbType.Varchar2).Value = phonenumber;
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                exists = count > 0;
            }
            return exists;
        }
        //判断商家是否存在
        public bool IsStoreExist(string ID)
        {
            bool exists = false;
            using (OracleCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM store WHERE sto_ID = :storeID";
                cmd.Parameters.Clear();
                cmd.Parameters.Add("storeID", OracleDbType.Int32).Value = Int32.Parse(ID);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                exists = count > 0;
            }
            return exists;
        }
        //判断顾客是否存在
        public bool IsCusExist(string ID)
        {
            bool exists = false;
            using (OracleCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM customer WHERE cus_ID = :cusID";
                cmd.Parameters.Clear();
                cmd.Parameters.Add("cusID", OracleDbType.Int32).Value = Int32.Parse(ID);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                exists = count > 0;
            }
            return exists;
        }

        //用户注册
        public string RegisterUser(string phonenumber, string password, string address, string type)
        {
            if (IsUserPhoneExist(phonenumber))
            {
                return "电话号码已经存在!";
            }

            try
            {
                DateTime now = DateTime.Now;
                using (OracleCommand cmd = con.CreateCommand())
                {
                    // 使用参数化查询
                    cmd.CommandText = "INSERT INTO USERS(USER_PHONE,USER_PASSWORD,USER_ADDRESS,USER_STATE,USER_BALANCE,USER_REGTIME,USER_TYPE,USER_ID)" +
                        " VALUES(:phone, :Password, :address, 0, 0, :regTime, :type, USERIDSEQUENCE.NEXTVAL)";
                    // 添加参数
                    cmd.Parameters.Add("phone", OracleDbType.Varchar2, 20).Value = phonenumber;
                    cmd.Parameters.Add("Password", OracleDbType.Varchar2, 20).Value = password;
                    cmd.Parameters.Add("address", OracleDbType.Varchar2, 100).Value = address;
                    cmd.Parameters.Add("regTime", OracleDbType.Date).Value = now;
                    cmd.Parameters.Add("type", OracleDbType.Byte).Value = Convert.ToByte(type);
                    int rowsInserted = cmd.ExecuteNonQuery();//ExecuteNonQuery() 方法返回一个整数，表示被影响的行数，即执行 SQL 命令后有多少行数据受到影响。
                    if (rowsInserted > 0)
                    {
                        return "success";
                    }
                    else
                    {
                        return "顾客注册失败，请重试。";
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("数据库操作异常：" + ex.Message);
                return "数据库操作异常：" + ex.Message;
            }
            catch (Exception ex)
            {
                Console.WriteLine("其他异常：" + ex.Message);
                return "其他异常：" + ex.Message;
            }

        }
        //注册结束后获取新ID
        public string GetNewUserID(string phonenumber)
        {
            if (!IsUserPhoneExist(phonenumber))
            {
                return "电话号码不存在!";
            }
            string newID = "0000000";
            using (OracleCommand cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = "SELECT USER_ID FROM USERS WHERE USER_PHONE = :phone";
                    cmd.Parameters.Add("phone", OracleDbType.Varchar2).Value = phonenumber;
                    OracleDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        newID = reader.GetString(0);
                    }
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("返回新ID异常" + ex.Message);
                    return "返回新ID异常" + ex.Message;
                }
            }
            return newID;
        }

        //商店注册
        public string RegiserSto(string ID, string name, string introduction, string imageURL)
        {
            string message = "";
            if (!IsUserExist(ID))
            {
                message = "商家ID不存在";
                return message;
            }

            try
            {
                using (OracleCommand cmd = con.CreateCommand())
                {

                    // 插入商家信息
                    cmd.CommandText = "INSERT INTO STORE(STO_ID, STO_NAME, STO_INTRODUCTION, STO_LICENSEIMG, STO_STATE) " +
                                      "VALUES(:stoID, :stoName, :stoIntro, :stoImageURL, 1)";
                    cmd.Parameters.Add(new OracleParameter("stoID", OracleDbType.Int32)).Value = Int32.Parse(ID);
                    cmd.Parameters.Add(new OracleParameter("stoName", OracleDbType.Varchar2, 20)).Value = name;
                    cmd.Parameters.Add(new OracleParameter("stoIntro", OracleDbType.Clob)).Value = introduction;
                    cmd.Parameters.Add(new OracleParameter("stoImageURL", OracleDbType.Varchar2, 50)).Value = imageURL;
                    cmd.ExecuteNonQuery();
                    // 提交事务

                    message = "success";
                }
            }
            catch (Exception ex)
            {
                // 出现异常时回滚事务

                Console.WriteLine("数据库操作异常：" + ex.Message);
                message = "数据库操作异常：" + ex.Message;
            }

            return message;
        }

        //添加商店主营类别
        public string EditStoCategories(string ID, List<string> categories)
        {
            string message = "";
            if (!IsStoreExist(ID))
            {
                return "商家ID不存在";
            }
            try
            {
                using (OracleCommand cmd = con.CreateCommand())
                {
                    // 先查询数据库中已存在的商家主营类别
                    cmd.CommandText = "SELECT com_category FROM store_categories WHERE store_ID = :storeID";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("storeID", OracleDbType.Int32).Value = Int32.Parse(ID);
                    List<string> existingCategories = new List<string>();
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            existingCategories.Add(reader.GetString(0));
                        }
                    }
                    // 检查传入的列表中的类别是否在数据库中已存在，不存在则新增
                    foreach (string category in categories)
                    {
                        if (!existingCategories.Contains(category))
                        {
                            // 执行新增操作，使用参数化查询
                            cmd.CommandText = "INSERT INTO store_categories (store_ID, com_category) VALUES (:storeID, :category)";
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("storeID", OracleDbType.Int32).Value = Int32.Parse(ID);
                            cmd.Parameters.Add("category", OracleDbType.Varchar2).Value = category;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    // 检查数据库中已存在的类别是否在传入的列表中，不在则删除
                    foreach (string existingCategory in existingCategories)
                    {
                        if (!categories.Contains(existingCategory))
                        {
                            // 执行删除操作，使用参数化查询
                            cmd.CommandText = "DELETE FROM store_categories WHERE store_ID = :storeID AND com_category = :existingCategory";
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("storeID", OracleDbType.Int32).Value = Int32.Parse(ID);
                            cmd.Parameters.Add("existingCategory", OracleDbType.Varchar2).Value = existingCategory;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    message = "success";
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("数据库操作异常：" + ex.Message);
                message = "数据库操作异常：" + ex.Message;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                message = "其他错误：" + ex.Message;
            }

            return message;
        }

        //顾客注册
        public string RegisterCus(string ID, string nickname, string notes, string paypassword)
        {
            string message = "";
            try
            {
                // 首先检查是否存在相同的顾客ID，避免重复注册
                if (IsCusExist(ID))
                    return "顾客已经存在";
                // 检查是否存在userID
                if (!IsUserExist(ID))
                    return "用户ID不存在";
                // 插入顾客信息到数据库
                using (OracleCommand insertCmd = con.CreateCommand())
                {
                    insertCmd.CommandText = "INSERT INTO customer (cus_ID, cus_nickname, cus_notes, cus_payPassword, cus_state) " +
                                            "VALUES (:cusID, :nickname, :notes, :payPassword, 1)";
                    insertCmd.Parameters.Add("cusID", OracleDbType.Int32).Value = Int32.Parse(ID);
                    insertCmd.Parameters.Add("nickname", OracleDbType.Varchar2).Value = nickname;
                    insertCmd.Parameters.Add("notes", OracleDbType.Clob).Value = notes;
                    insertCmd.Parameters.Add("payPassword", OracleDbType.Varchar2).Value = paypassword;
                    int rowsInserted = insertCmd.ExecuteNonQuery();//ExecuteNonQuery() 方法返回一个整数，表示被影响的行数，即执行 SQL 命令后有多少行数据受到影响。
                    if (rowsInserted > 0)
                    {
                        message = "success";
                    }
                    else
                    {
                        message = "顾客注册失败，请重试。";
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("数据库操作异常：" + ex.Message);
                message = "数据库操作异常：" + ex.Message;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                message = "其他错误：" + ex.Message;
            }
            return message;
        }

        //顾客注册时添加喜好
        public string EditCusLove(string ID, List<string> loves)
        {
            string message = "";
            bool customerExists = IsCusExist(ID);

            if (!customerExists)
            {
                return "该顾客ID不存在";
            }
            try
            {
                using (OracleCommand cmd = con.CreateCommand())
                {
                    // 先查询数据库中已存在的顾客喜好类别及权重
                    cmd.CommandText = "SELECT com_category, cus_love_weight FROM customer_love WHERE cus_ID = :cusID";
                    cmd.Parameters.Add("cusID", OracleDbType.Int32).Value = Int32.Parse(ID);

                    Dictionary<string, decimal> existingLoves = new Dictionary<string, decimal>();
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string category = reader.GetString(0);
                            decimal weight = reader.GetDecimal(1);
                            existingLoves[category] = weight;
                        }
                    }
                    // 检查传入的列表中的类别是否在数据库中已存在，不存在则新增
                    foreach (string category in loves)
                    {
                        if (!existingLoves.ContainsKey(category))
                        {
                            // 执行新增操作，使用参数化查询
                            cmd.CommandText = "INSERT INTO customer_love (cus_ID, com_category, cus_love_weight) VALUES (:cusID, :category, 1.0)";
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("cusID", OracleDbType.Int32).Value = Int32.Parse(ID);
                            cmd.Parameters.Add("category", OracleDbType.Varchar2).Value = category;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    // 检查数据库中已存在的类别是否在传入的列表中，不在则删除
                    foreach (string existingCategory in existingLoves.Keys)
                    {
                        if (!loves.Contains(existingCategory))
                        {
                            // 执行删除操作，使用参数化查询
                            cmd.CommandText = "DELETE FROM customer_love WHERE cus_ID = :cusID AND com_category = :existingCategory";
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("cusID", OracleDbType.Int32).Value = Int32.Parse(ID);
                            cmd.Parameters.Add("existingCategory", OracleDbType.Varchar2).Value = existingCategory;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    message = "success";
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("数据库操作异常：" + ex.Message);
                message = "数据库操作异常：" + ex.Message;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                message = "其他错误：" + ex.Message;
            }
            return message;
        }

        //返回所有喜好信息
        public List<string> GetAllLoves()
        {
            List<string> loves = new List<string>();
            try
            {
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT com_category FROM commodities_categories";

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        // 将查询结果添加到 loves 列表中
                        foreach (DataRow row in dt.Rows)
                        {
                            loves.Add(row["com_category"].ToString());
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("数据库操作异常：" + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return loves;
        }

        //ID登录
        public string IDLogin(string ID, string password)
        {
            string flag = "用户不存在";
            using (OracleCommand cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = "SELECT USER_PASSWORD, USER_TYPE FROM USERS WHERE USER_ID = :userID";
                    cmd.Parameters.Add(new OracleParameter("userID", OracleDbType.Int32)).Value = Int32.Parse(ID);
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if ((string)reader.GetString(0).Trim() == password)
                        {
                            Console.WriteLine((string)reader.GetString(0));
                            flag = reader.GetString(1);
                            Console.WriteLine("登录成功,用户类别为" + flag);
                            reader.Dispose();
                            return flag;
                        }
                    }
                    reader.Dispose();

                    cmd.CommandText = "SELECT USER_ID FROM USERS WHERE USER_ID=" + ID;
                    OracleDataReader reader_id = cmd.ExecuteReader();
                    while (reader_id.Read())
                    {
                        if (ID == reader_id.GetString(0))
                            flag = "密码错误";
                    }
                    reader_id.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    flag = ex.Message;
                }

            }
            return flag;
        }

        //电话号码登录
        public string PhonenumberLogin(string phonenumber, string password)
        {

            string flag = "用户不存在";
            using (OracleCommand cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = "SELECT USER_PASSWORD,USER_TYPE FROM USERS WHERE USER_PHONE=" + phonenumber;
                    OracleDataReader reader = cmd.ExecuteReader();
                    cmd.CommandText = "SELECT USER_PHONE FROM USERS WHERE USER_PHONE=" + phonenumber;
                    OracleDataReader reader_id = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.GetString(0).Trim() == password)
                        {
                            Console.WriteLine((string)reader.GetString(0));
                            flag = reader.GetString(1);
                            Console.WriteLine("登录成功,用户类别为" + flag);
                            reader.Dispose();
                            reader_id.Dispose();
                            return flag;
                        }
                    }
                    while (reader_id.Read())
                    {
                        if (phonenumber == reader_id.GetString(0))
                            flag = "密码错误";
                    }
                    reader.Dispose();
                    reader_id.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    flag = ex.Message;
                }
            }
            return flag;
        }

        //通过电话号码重置密码
        public string ResetPassword(string phonenumber, string password)
        {
            string message = "";
            using (OracleCommand cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = "SELECT USER_PASSWORD FROM USERS WHERE USER_PHONE=" + phonenumber;
                    OracleDataReader reader = cmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        message = "用户电话号码不存在！";
                        reader.Dispose();
                        return message;
                    }
                    else
                    {
                        if (reader.GetString(0).Trim() == password)
                        {
                            message = "新密码不能与旧密码相同";
                            reader.Dispose();
                            return message;
                        }
                    }
                    reader.Dispose();
                    cmd.CommandText = "UPDATE USERS SET USER_PASSWORD = '" + password + "' WHERE USER_PHONE=" + phonenumber;
                    cmd.ExecuteNonQuery();
                    Console.WriteLine(cmd.CommandText);
                    message = "success";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    message = "其他错误：" + ex.Message;
                }
                return message;
            }
        }

        //获取用户信息
        public UserInfo getUserInfo(string user_ID)
        {
            UserInfo userInfo = new UserInfo();
            // 假设 con 是你的数据库连接对象
            using (OracleCommand cmd = con.CreateCommand())
            {
                try
                {
                    // 根据用户ID查询用户信息
                    cmd.CommandText = "SELECT user_ID, user_phone, user_password, user_address, user_balance, user_regTime, user_type FROM users WHERE user_ID = :userID";
                    cmd.Parameters.Add(new OracleParameter("userID", OracleDbType.Int32)).Value = Int32.Parse(user_ID);

                    // 执行查询，并读取结果
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userInfo.message = "success";
                            userInfo.user_ID = reader["user_ID"].ToString();
                            userInfo.user_phone = reader["user_phone"].ToString();
                            userInfo.user_password = reader["user_password"].ToString();
                            userInfo.user_address = reader["user_address"].ToString();
                            userInfo.user_balance = reader["user_balance"].ToString();
                            userInfo.user_regTime = reader["user_regTime"].ToString();
                            userInfo.user_type = reader["user_type"].ToString();
                        }
                        else
                        {
                            userInfo.message = "用户不存在";
                        }
                    }
                }
                catch (OracleException ex)
                {
                    userInfo.message = "数据库操作异常：" + ex.Message;
                }
                catch (Exception ex)
                {
                    userInfo.message = "其他错误：" + ex.Message;
                }
            }
            return userInfo;
        }

        //获取顾客信息
        public CusInfo GetCusInfoWithLoves(string cusID)
        {
            CusInfo cusInfo = new CusInfo();
            using (OracleCommand cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = "SELECT cus_ID, cus_nickname, cus_notes, cus_payPassword, cus_state FROM customer WHERE cus_ID = :cusID";
                    cmd.Parameters.Add(new OracleParameter("cusID", OracleDbType.Int32)).Value = Int32.Parse(cusID);
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cusInfo.cus_ID = reader["cus_ID"].ToString();
                            cusInfo.cus_nickname = reader["cus_nickname"].ToString();
                            cusInfo.cus_notes = reader["cus_notes"].ToString();
                            cusInfo.cus_payPassword = reader["cus_payPassword"].ToString();
                            cusInfo.cus_state = reader.GetBoolean("cus_state") ? "1" : "0"; // 转换为字符串类型
                        }
                        else
                        {
                            cusInfo.message = "顾客不存在";
                            return cusInfo;
                        }
                    }

                    // 查询顾客喜好信息
                    cmd.CommandText = "SELECT com_category FROM customer_love WHERE cus_ID = :cusID";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new OracleParameter("cusID", OracleDbType.Int32)).Value = Int32.Parse(cusID);

                    cusInfo.cus_loves = new List<string>();
                    using (OracleDataReader loveReader = cmd.ExecuteReader())
                    {
                        while (loveReader.Read())
                        {
                            cusInfo.cus_loves.Add(loveReader["com_category"].ToString());
                        }
                    }

                    cusInfo.message = "success";
                }
                catch (OracleException ex)
                {
                    cusInfo.message = "数据库操作异常：" + ex.Message;
                }
                catch (Exception ex)
                {
                    cusInfo.message = "其他错误：" + ex.Message;
                }
            }
            return cusInfo;
        }

        //获取商家信息
        public StoInfo GetStoInfoWithCategories(string stoID)
        {
            StoInfo stoInfo = new StoInfo();
            using (OracleCommand cmd = con.CreateCommand())
            {
                try
                {
                    // 查询商家信息
                    cmd.CommandText = "SELECT sto_ID, sto_name, sto_introduction, sto_licenseImg, sto_state FROM store WHERE sto_ID = :stoID";
                    cmd.Parameters.Add(new OracleParameter("stoID", OracleDbType.Int32)).Value = Int32.Parse(stoID);
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stoInfo.sto_ID = reader["sto_ID"].ToString();
                            stoInfo.sto_name = reader["sto_name"].ToString();
                            stoInfo.sto_introduction = reader["sto_introduction"].ToString();
                            stoInfo.sto_licenseImg = reader["sto_licenseImg"].ToString();
                            stoInfo.sto_state = reader.GetByte("sto_state").ToString(); // 转换为字符串类型
                        }
                        else
                        {
                            stoInfo.message = "商家不存在";
                            return stoInfo;
                        }
                    }

                    // 查询商家主营类别信息
                    cmd.CommandText = "SELECT com_category FROM store_categories WHERE store_ID = :stoID";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new OracleParameter("stoID", OracleDbType.Int32)).Value = Int32.Parse(stoID);

                    stoInfo.categories = new List<string>();
                    using (OracleDataReader categoryReader = cmd.ExecuteReader())
                    {
                        while (categoryReader.Read())
                        {
                            stoInfo.categories.Add(categoryReader["com_category"].ToString());
                        }
                    }

                    stoInfo.message = "success";
                }
                catch (OracleException ex)
                {
                    stoInfo.message = "数据库操作异常：" + ex.Message;
                }
                catch (Exception ex)
                {
                    stoInfo.message = "其他错误：" + ex.Message;
                }
            }
            return stoInfo;
        }

        //更新用户信息
        public string UpdateUserInfo(string user_ID, string user_phone, string user_password, string user_address)
        {
            using (OracleCommand cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = "UPDATE USERS SET USER_PHONE = :phone, USER_PASSWORD = :password, USER_ADDRESS = :address " +
                        "WHERE USER_ID = :userID";
                    cmd.Parameters.Add("phone", OracleDbType.Varchar2).Value = user_phone;
                    cmd.Parameters.Add("password", OracleDbType.Varchar2).Value = user_password;
                    cmd.Parameters.Add("address", OracleDbType.Varchar2).Value = user_address;
                    cmd.Parameters.Add("userID", OracleDbType.Int32).Value = Int32.Parse(user_ID);
                    cmd.ExecuteNonQuery();
                    return "success";
                }
                catch (Exception ex)
                {
                    return "更新用户信息时发生错误(原因可能是电话号码已经被其他用户注册)：" + ex.Message;
                }
            }
        }

        //更行顾客信息
        public string UpdateCustomerInfo(string cus_ID, string cus_nickname, string cus_notes, string cus_payPassword)
        {
            string message = "";
            if (!IsCusExist(cus_ID))
            {
                return ("顾客不存在");
            }
            try
            {
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "UPDATE customer SET cus_nickname = :nickname, cus_notes = :notes, cus_payPassword = :payPassword WHERE cus_ID = :cusID";
                    cmd.Parameters.Add("nickname", OracleDbType.Varchar2).Value = cus_nickname;
                    cmd.Parameters.Add("notes", OracleDbType.Varchar2).Value = cus_notes;
                    cmd.Parameters.Add("payPassword", OracleDbType.Varchar2).Value = cus_payPassword;
                    cmd.Parameters.Add("cusID", OracleDbType.Int32).Value = Int32.Parse(cus_ID);
                    cmd.ExecuteNonQuery();
                    message = "success";
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("数据库操作异常：" + ex.Message);
                message = "数据库操作异常：" + ex.Message;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                message = "其他错误：" + ex.Message;
            }
            return message;
        }

        //更改商家信息
        public string UpdateStoInfo(string stoID, string name, string introduction)
        {
            string message = "";
            if (!IsStoreExist(stoID))
            {
                return "商家不存在";
            }
            try
            {
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "UPDATE store SET sto_name = :name, sto_introduction = :introduction WHERE sto_ID = :stoID";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("name", OracleDbType.Varchar2).Value = name;
                    cmd.Parameters.Add("introduction", OracleDbType.Varchar2).Value = introduction;
                    cmd.Parameters.Add("stoID", OracleDbType.Int32).Value = Int32.Parse(stoID);
                    cmd.ExecuteNonQuery();
                    message = "success";
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("数据库操作异常：" + ex.Message);
                message = "数据库操作异常：" + ex.Message;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                message = "其他错误：" + ex.Message;
            }
            return message;
        }

        public bool RechargeMoney(string user_ID, string amount)
        {
            try
            {
                decimal rechargeAmount = Convert.ToDecimal(amount);
                using (OracleCommand updateCmd = con.CreateCommand())
                {
                    updateCmd.CommandText = "UPDATE users SET user_balance = user_balance + :rechargeAmount WHERE user_ID = :user_ID";
                    updateCmd.Parameters.Add("rechargeAmount", OracleDbType.Decimal).Value = rechargeAmount;
                    updateCmd.Parameters.Add("user_ID", OracleDbType.Varchar2, 10).Value = user_ID;
                    updateCmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("充值失败：" + ex.Message);
                return false;
            }
        }
        //获取数据库中所用对应user_ID和cus_ID的信息
        public List<Chat> GetChatHistory(string cusID, string storeID)
        {
            List<Chat> chatHistoryForUser = new List<Chat>();
            try
            {
                // 准备 SQL 查询语句，并在其中进行排序
                string sqlQuery = "SELECT chat_time, cus_ID, store_ID, chat_content, chat_sender " +
                                  "FROM chat " +
                                  "WHERE (cus_ID = :cusID AND store_ID = :storeID)" +
                                  "ORDER BY chat_time ASC"; // 按时间从小到大排序

                using (OracleCommand cmd = new OracleCommand(sqlQuery, con))
                {
                    // 添加参数到查询语句中
                    cmd.Parameters.Add(":cusID", OracleDbType.Int32).Value = cusID;
                    cmd.Parameters.Add(":storeID", OracleDbType.Int32).Value = storeID;

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Chat chat = new Chat
                            {
                                ChatTime = reader.GetDateTime(0).ToString(),
                                CusID = reader.GetInt32(1).ToString(),
                                StoreID = reader.GetInt32(2).ToString(),
                                ChatContent = reader.GetString(3),
                                ChatSender = reader.GetBoolean(4)
                            };
                            chatHistoryForUser.Add(chat);
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                // 处理数据库异常
                Console.WriteLine("数据库操作异常：" + ex.Message);
            }
            catch (Exception ex)
            {
                // 处理其他异常
                Console.WriteLine("其他错误：" + ex.Message);
            }
            return chatHistoryForUser;
        }

        //将插入对应的聊天信息
        public void InsertChatIntoDataBase(Chat chat)
        {
            try
            {
                string sqlQuery = "INSERT INTO chat (chat_time, cus_ID, store_ID, chat_content, chat_sender) " +
                                  "VALUES (:chatTime, :cusID, :storeID, :chatContent, :chatSender)";

                using (OracleCommand cmd = new OracleCommand(sqlQuery, con))
                {
                    DateTime chatTime = DateTime.Parse(chat.ChatTime);
                    cmd.Parameters.Add(":chatTime", OracleDbType.TimeStamp).Value = chatTime;
                    cmd.Parameters.Add(":cusID", OracleDbType.Int32).Value = int.Parse(chat.CusID); // Convert to int
                    cmd.Parameters.Add(":storeID", OracleDbType.Int32).Value = int.Parse(chat.StoreID); // Convert to int
                    cmd.Parameters.Add(":chatContent", OracleDbType.Varchar2).Value = chat.ChatContent;
                    cmd.Parameters.Add(":chatSender", OracleDbType.Byte).Value = chat.ChatSender ? 1 : 0;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("数据库操作异常：" + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("其他错误：" + ex.Message);
            }
        }




        /********************************
         * lhx组后端函数如下：
         **********************************/



        /*插入某个特定的元组*/
        public int sqlInsertSingleItem(string sql)
        {
            Console.WriteLine("Get into function 'sqlInsertSingleItem'");
            int flag = 1;
            using (OracleCommand cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = sql;
                    Console.WriteLine("In function 'sqlInsertSingleItem' gotta excute :" + sql);
                    int rowsUpdated = cmd.ExecuteNonQuery();
                    Console.WriteLine(rowsUpdated + " rows updated");
                    Console.WriteLine("func sqlInsert sucessed!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Go wrong in function 'sqlInsertSingleItem'");
                    Console.WriteLine(ex.Message);
                    flag = -1;
                    return flag;
                }
                return flag;
            }
        }


        /*原子化插入一组sql语句*/
        public int sqlInsertAtomicity(List<string> sql)
        {
            Console.WriteLine("Get into function 'sqlInsertAtomicity'");
            int flag = 1;
            OracleTransaction transaction = con.BeginTransaction();
            try
            {
                for (int i = 0; i < sql.Count; i++)
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandText = sql[i];
                    Console.WriteLine("No. " + (i + 1) + " sql： " + cmd.CommandText);
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
                Console.WriteLine("we have succesfully executed " + sql.Count + " sql sentences");
                return flag;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Go wrong in function 'sqlInsertAtomicity'");
                Console.WriteLine(ex.Message);
                flag = -1;
                transaction.Rollback();
                return flag;
            }

        }
        public string ImageToBase64(string imagePath)
        {
            byte[] image = File.ReadAllBytes(imagePath);
            var str = "data:image/jpeg;base64," + Convert.ToBase64String(image);
            return str;
        }


        /*根据输入的字符串对商品名称进行模糊匹配，返回搜索到的一组商品详情*/
        public List<CommodityListModel> sqlSearchCommodityByName(searchCommodityModel model)
        {
            DateTime date = DateTime.Now;
            Console.WriteLine($"current time =>{date}");
            var list = new List<CommodityListModel>();

            var sqlArray = string.Join(",", model.com_categories.Select(c => $"'{c}'"));
            Console.WriteLine(sqlArray);
            var sqlCategories_1 = "";
            var sqlCategories_2 = "";
            if (model.com_categories.Count > 0)
            {
                sqlCategories_1 =
                $"WITH FAVOR_COM_ID AS (" +
                $" SELECT COM_ID AS SUB_ID" +
                $" FROM COMMODITY_CATEGORIES" +
                $" WHERE COM_CATEGORY IN ({sqlArray}) " +
                $" GROUP BY COM_ID" +
                $" )";
                sqlCategories_2 = "AND COMMODITY.COM_ID IN(SELECT SUB_ID FROM FAVOR_COM_ID)";
            }
            var searchSql =
                $" SELECT " +
                $" COM_ID,COM_NAME,COM_INTRODUCTION,COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE,COM_LEFT,COM_RATING,COMMODITY.STO_ID,STORE.STO_NAME" +
                $" FROM COMMODITY" +
                $" JOIN STORE ON COMMODITY.STO_ID=STORE.STO_ID" +
                $" WHERE COM_NAME LIKE '%{model.search_str}%' ";
            //$" WHERE COM_NAME LIKE '%{model.search_str}%' AND COMMODITY.COM_ID IN (SELECT SUB_ID FROM FAVOR_COM_ID)";
            searchSql = sqlCategories_1 + searchSql + sqlCategories_2;
            var sortSql = "";
            bool isSorted = false;
            switch (model.sort_order)
            {
                case 0:
                    sortSql = " ORDER BY COM_RATING DESC";
                    isSorted = true;
                    break;
                case 1:
                    isSorted = true;//地理位置远近排序未实现
                    break;
                case 2:
                    sortSql = " ORDER BY COM_EXPIRATIONDATE DESC";
                    isSorted = true;
                    break;
                case 3:
                    break;
                default:
                    break;
            }
            searchSql += sortSql;
            Console.WriteLine("In searchCommodityByName function going to execute: " + searchSql + "\n");
            //搜索商品
            using (var cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = searchSql;
                    OracleDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var searchModel = new CommodityListModel();
                        searchModel.com_id = reader.GetInt32(0);
                        searchModel.com_name = reader.GetString(1);
                        searchModel.com_introduction = reader.GetString(2);
                        searchModel.com_oriPrice = reader.GetDouble(3);
                        searchModel.com_expirationDate = reader.GetDateTime(4).ToString("yyyy-MM-dd");
                        searchModel.com_uploadDate = reader.GetDateTime(5).ToString("yyyy-MM-dd");
                        searchModel.com_left = reader.GetInt32(6);
                        searchModel.com_rating = reader.GetDouble(7);
                        searchModel.sto_id = reader.GetDouble(8);
                        searchModel.sto_name = reader.GetString(9);

                        //搜索某个商品的商品类别
                        using (var cmdFitCategory = con.CreateCommand())
                        {
                            var fitCategorySql = $"SELECT COM_CATEGORY FROM COMMODITY_CATEGORIES WHERE COM_ID = {searchModel.com_id}";
                            cmdFitCategory.CommandText = fitCategorySql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitCategorySql);
                            OracleDataReader readerFitCategory = cmdFitCategory.ExecuteReader();

                            while (readerFitCategory.Read())
                            {
                                searchModel.com_categories.Add(readerFitCategory.GetString(0));
                            }
                            readerFitCategory.Dispose();
                            Console.WriteLine("finish fitCategorySql");
                        }

                        //搜索固定商品的头图
                        using (var cmdFitFirImage = con.CreateCommand())
                        {
                            var fitFirImageSql = $"SELECT * FROM COMMODITY_IMAGE WHERE COM_ID={searchModel.com_id} ORDER BY COM_ID DESC ";
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitFirImageSql);
                            cmdFitFirImage.CommandText = fitFirImageSql;
                            OracleDataReader readerFitFirImage = cmdFitFirImage.ExecuteReader();
                            if (readerFitFirImage.HasRows)
                            {
                                readerFitFirImage.Read();
                                searchModel.com_firstImage = readerFitFirImage.GetString(1).Substring(10);
                                readerFitFirImage.Dispose();
                                Console.WriteLine("finish FitFirImageSql");
                            }
                            else { Console.WriteLine($"The commodity with id={searchModel.com_id} has no image"); }
                        }
                        using (var cmdFitFavor = con.CreateCommand())
                        {
                            var sqlQuery = $"SELECT * FROM FAVORITE WHERE COM_ID = {searchModel.com_id} AND CUS_ID ={model.cus_id}";
                            cmdFitFavor.CommandText = sqlQuery;
                            var fitFavorReader = cmdFitFavor.ExecuteReader();
                            if (fitFavorReader.HasRows)
                            {
                                searchModel.favor_state = 1;
                            }
                            else
                            {
                                searchModel.favor_state = 0;
                            }

                        }
                        //搜索固定某个商品现在价格

                        searchModel.com_price = sqlGetCommodityCurrPrice(searchModel.com_id);

                        list.Add(searchModel);
                    }
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("In searchCommodityByName function erorr \n");
                    Console.WriteLine(ex.Message);
                }
            }
            if (!isSorted)
                list.Sort((x, y) => x.com_price.CompareTo(y.com_price));
            ;

            return list;
        }

        /*搜索特定ID客户的喜欢商品*/
        public List<CommodityListModel> sqlSearchFavorCommodity(int cus_id)
        {
            DateTime date = DateTime.Now;
            Console.WriteLine($"current time =>{date}");
            var list = new List<CommodityListModel>();
            var searchSql =
                $"SELECT " +
                $" FAVORITE.COM_ID,COM_NAME,COM_INTRODUCTION,COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE,COM_LEFT,COM_RATING,COMMODITY.STO_ID,STORE.STO_NAME" +
                $" FROM COMMODITY,STORE,FAVORITE" +
                $" WHERE FAVORITE.CUS_ID ={cus_id} AND FAVORITE.COM_ID= COMMODITY.COM_ID AND COMMODITY.STO_ID=STORE.STO_ID" +
                $" ORDER BY COM_RATING DESC";
            Console.WriteLine("In sqlSearchFavorCommodity function going to execute: " + searchSql + "\n");
            //搜索商品
            using (var cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = searchSql;
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var searchModel = new CommodityListModel();
                        searchModel.com_id = reader.GetInt32(0);
                        searchModel.com_name = reader.GetString(1);
                        searchModel.com_introduction = reader.GetString(2);
                        searchModel.com_oriPrice = reader.GetDouble(3);
                        searchModel.com_expirationDate = reader.GetDateTime(4).ToString("yyyy-MM-dd");
                        searchModel.com_uploadDate = reader.GetDateTime(5).ToString("yyyy-MM-dd");
                        searchModel.com_left = reader.GetInt32(6);
                        searchModel.com_rating = reader.GetDouble(7);
                        searchModel.sto_id = reader.GetDouble(8);
                        searchModel.sto_name = reader.GetString(9);
                        searchModel.favor_state = 1;

                        //搜索某个商品的商品类别
                        using (var cmdFitCategory = con.CreateCommand())
                        {
                            var fitCategorySql = $"SELECT COM_CATEGORY FROM COMMODITY_CATEGORIES WHERE COM_ID = {searchModel.com_id}";
                            cmdFitCategory.CommandText = fitCategorySql;
                            Console.WriteLine("In sqlSearchFavorCommodity function going to execute: " + fitCategorySql);
                            OracleDataReader readerFitCategory = cmdFitCategory.ExecuteReader();

                            while (readerFitCategory.Read())
                            {
                                searchModel.com_categories.Add(readerFitCategory.GetString(0));
                            }
                            readerFitCategory.Dispose();
                            Console.WriteLine("finish fitCategorySql");
                        }

                        //搜索固定商品的头图
                        using (var cmdFitFirImage = con.CreateCommand())
                        {
                            var fitFirImageSql = $"SELECT * FROM COMMODITY_IMAGE WHERE COM_ID={searchModel.com_id} ORDER BY COM_ID DESC ";
                            Console.WriteLine("In sqlSearchFavorCommodity function going to execute: " + fitFirImageSql);
                            cmdFitFirImage.CommandText = fitFirImageSql;
                            OracleDataReader readerFitFirImage = cmdFitFirImage.ExecuteReader();
                            if (readerFitFirImage.HasRows)
                            {
                                readerFitFirImage.Read();
                                searchModel.com_firstImage = readerFitFirImage.GetString(1).Substring(10);
                                readerFitFirImage.Dispose();
                                Console.WriteLine("finish FitFirImageSql");
                            }
                            else { Console.WriteLine($"The commodity with id={searchModel.com_id} has no image"); }
                        }
                        searchModel.com_price = sqlGetCommodityCurrPrice(searchModel.com_id);
                        list.Add(searchModel);
                    }
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("In sqlSearchFavorCommodity function erorr \n");
                    Console.WriteLine(ex.Message);
                }
            }
            return list;
        }


        /*搜索特定ID客户的浏览记录*/
        public List<CommodityListModel> sqlSearchBrowseHistory(int cus_id)
        {
            DateTime date = DateTime.Now;
            Console.WriteLine($"current time =>{date}");
            var list = new List<CommodityListModel>();
            var searchSql =
                $" SELECT " +
                $" BROWSE.COM_ID,COM_NAME,COM_INTRODUCTION,COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE,COM_LEFT,COM_RATING,COMMODITY.STO_ID,STORE.STO_NAME" +
                $" FROM COMMODITY,STORE,BROWSE" +
                $" WHERE BROWSE.BROWSER_ID ={cus_id} AND BROWSE.COM_ID= COMMODITY.COM_ID AND COMMODITY.STO_ID=STORE.STO_ID" +
                $" ORDER BY BRO_TIME_END DESC";
            Console.WriteLine("In sqlSearchBrowseHistory function going to execute: " + searchSql + "\n");
            //搜索商品
            using (var cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = searchSql;
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var searchModel = new CommodityListModel();
                        searchModel.com_id = reader.GetInt32(0);
                        searchModel.com_name = reader.GetString(1);
                        searchModel.com_introduction = reader.GetString(2);
                        searchModel.com_oriPrice = reader.GetDouble(3);
                        searchModel.com_expirationDate = reader.GetDateTime(4).ToString("yyyy-MM-dd");
                        searchModel.com_uploadDate = reader.GetDateTime(5).ToString("yyyy-MM-dd");
                        searchModel.com_left = reader.GetInt32(6);
                        searchModel.com_rating = reader.GetDouble(7);
                        searchModel.sto_id = reader.GetDouble(8);
                        searchModel.sto_name = reader.GetString(9);
                        searchModel.favor_state = 1;

                        //搜索某个商品的商品类别
                        using (var cmdFitCategory = con.CreateCommand())
                        {
                            var fitCategorySql = $"SELECT COM_CATEGORY FROM COMMODITY_CATEGORIES WHERE COM_ID = {searchModel.com_id}";
                            cmdFitCategory.CommandText = fitCategorySql;
                            Console.WriteLine("sqlSearchBrowseHistory function going to execute: " + fitCategorySql);
                            OracleDataReader readerFitCategory = cmdFitCategory.ExecuteReader();

                            while (readerFitCategory.Read())
                            {
                                searchModel.com_categories.Add(readerFitCategory.GetString(0));
                            }
                            readerFitCategory.Dispose();
                            Console.WriteLine("finish fitCategorySql");
                        }

                        //搜索固定商品的头图
                        using (var cmdFitFirImage = con.CreateCommand())
                        {
                            var fitFirImageSql = $"SELECT * FROM COMMODITY_IMAGE WHERE COM_ID={searchModel.com_id} ORDER BY COM_ID DESC ";
                            Console.WriteLine("In sqlSearchBrowseHistory function going to execute: " + fitFirImageSql);
                            cmdFitFirImage.CommandText = fitFirImageSql;
                            OracleDataReader readerFitFirImage = cmdFitFirImage.ExecuteReader();
                            if (readerFitFirImage.HasRows)
                            {
                                readerFitFirImage.Read();
                                searchModel.com_firstImage = readerFitFirImage.GetString(1).Substring(10);
                                readerFitFirImage.Dispose();
                                Console.WriteLine("finish FitFirImageSql");
                            }
                            else { Console.WriteLine($"The commodity with id={searchModel.com_id} has no image"); }
                        }

                        searchModel.com_price = sqlGetCommodityCurrPrice(searchModel.com_id);

                        using (var cmdFitFavor = con.CreateCommand())
                        {
                            var sqlQuery = $"SELECT * FROM FAVORITE WHERE COM_ID = {searchModel.com_id} AND CUS_ID ={cus_id}";
                            cmdFitFavor.CommandText = sqlQuery;//sqlList[0]为插入语句，sqlList[1]为删除语句
                            var my_reader = cmdFitFavor.ExecuteReader();
                            if (my_reader.HasRows)
                            {
                                searchModel.favor_state = 1;
                            }
                            else
                            {
                                searchModel.favor_state = 0;
                            }
                        }
                        list.Add(searchModel);
                    }
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("In sqlSearchBrowseHistory function erorr \n");
                    Console.WriteLine(ex.Message);
                }
            }
            return list;
        }



        /*按照商品的ID搜索商品*/
        public CommodityDetailModel sqlSearchCommodityByID(int com_id, int cus_id)
        {
            Console.WriteLine("Get into function searchCommodityByID \n");
            DateTime date = DateTime.Now;
            var searchSql =
                $"SELECT " +
                $" COM_ID,COM_NAME,COM_INTRODUCTION,COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE,COM_LEFT,COM_RATING,COMMODITY.STO_ID,STORE.STO_NAME" +
                $" FROM COMMODITY, STORE" +
                $" WHERE COM_ID ={com_id} AND COMMODITY.STO_ID=STORE.STO_ID";
            Console.WriteLine("In searchCommodityByName function going to execute: " + searchSql + "\n");
            var searchModel = new CommodityDetailModel();
            using (var cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = searchSql;
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        searchModel.com_id = reader.GetInt32(0);
                        searchModel.com_name = reader.GetString(1);
                        searchModel.com_introduction = reader.GetString(2);
                        searchModel.com_oriPrice = reader.GetDouble(3);
                        searchModel.com_expirationDate = reader.GetDateTime(4).ToString("yyyy-MM-dd");
                        searchModel.com_uploadDate = reader.GetDateTime(5).ToString("yyyy-MM-dd");
                        searchModel.com_left = reader.GetInt32(6);
                        searchModel.com_rating = reader.GetDouble(7);
                        searchModel.sto_id = reader.GetDouble(8);
                        searchModel.sto_name = reader.GetString(9);
                        using (var cmdFitCategory = con.CreateCommand())
                        {
                            var fitCategorySql = $"SELECT COM_CATEGORY FROM COMMODITY_CATEGORIES WHERE COM_ID = {searchModel.com_id}";
                            cmdFitCategory.CommandText = fitCategorySql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitCategorySql);
                            OracleDataReader readerFitCategory = cmdFitCategory.ExecuteReader();

                            while (readerFitCategory.Read())
                            {
                                searchModel.com_categories.Add(readerFitCategory.GetString(0));

                            }
                            readerFitCategory.Dispose();
                            Console.WriteLine("finish fitCategorySql");
                        }
                        using (var cmdFitImage = con.CreateCommand())
                        {
                            var fitImageSql = $"SELECT * FROM COMMODITY_IMAGE WHERE COM_ID={searchModel.com_id} ORDER BY COM_ID DESC ";
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitImageSql);
                            cmdFitImage.CommandText = fitImageSql;
                            OracleDataReader readerFitImage = cmdFitImage.ExecuteReader();
                            while (readerFitImage.Read())
                            {
                                searchModel.com_images.Add(readerFitImage.GetString(1).Substring(10));
                            }
                            readerFitImage.Dispose();
                            Console.WriteLine("finish FitImageSql");
                        }
                        using (var cmdFitPrice = con.CreateCommand())
                        {

                            var fitPriceSql = $"SELECT * FROM COMMODITY_PRICE_CURVE WHERE COM_ID ={searchModel.com_id} ORDER BY COM_PC_TIME ASC";
                            cmdFitPrice.CommandText = fitPriceSql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitPriceSql);
                            OracleDataReader readerFitPrice = cmdFitPrice.ExecuteReader();
                            //double subPrice = -1;
                            //bool findCurrPrice = false;
                            var uploadPriceNode = new PriceCurveModel();
                            uploadPriceNode.com_pc_price = searchModel.com_oriPrice;
                            uploadPriceNode.com_pc_time = searchModel.com_uploadDate;

                            var exPriceNode = new PriceCurveModel();
                            exPriceNode.com_pc_price = 0;
                            exPriceNode.com_pc_time = searchModel.com_expirationDate;
                            while (readerFitPrice.Read())
                            {
                                var singlePriceNode = new PriceCurveModel();
                                double temp = (int)(readerFitPrice.GetDouble(1) * searchModel.com_oriPrice * 100);
                                singlePriceNode.com_pc_price = temp / 100;
                                singlePriceNode.com_pc_time = readerFitPrice.GetDateTime(0).ToString("yyyy-MM-dd");
                                searchModel.com_prices.Add(singlePriceNode);
                            }
                            searchModel.com_prices.Add(uploadPriceNode);
                            searchModel.com_prices.Add(exPriceNode);
                            searchModel.com_prices.Sort((x, y) => y.com_pc_price.CompareTo(x.com_pc_price));
                            searchModel.com_price = sqlGetCommodityCurrPrice(searchModel.com_id);
                            readerFitPrice.Dispose();
                            Console.WriteLine("finish fitPriceSql\n");
                        }
                        using (var cmdFitFavor = con.CreateCommand())
                        {
                            var sqlQuery = $"SELECT * FROM FAVORITE WHERE COM_ID = {searchModel.com_id} AND CUS_ID ={cus_id}";
                            cmdFitFavor.CommandText = sqlQuery;
                            var my_reader = cmdFitFavor.ExecuteReader();
                            if (my_reader.HasRows)
                            {
                                searchModel.favor_state = 1;
                            }
                            else
                            {
                                searchModel.favor_state = 0;
                            }

                        }
                        using (var cmdFitCmt = con.CreateCommand())
                        {
                            var fitCmtSql = $"SELECT * FROM COMMODITY_COMMENT WHERE COM_ID = {searchModel.com_id}";
                            cmdFitCmt.CommandText = fitCmtSql;
                            Console.WriteLine("In searchCommodityByID function going to execute: " + fitCmtSql);
                            OracleDataReader readerFitCmt = cmdFitCmt.ExecuteReader();

                            while (readerFitCmt.Read())
                            {

                                var comment = new SendCommentModel();
                                comment.cmt_time = readerFitCmt.GetDateTime(3).ToString("yyyy-MM-dd HH:mm:ss");
                                comment.cmt_id = readerFitCmt.GetInt32(0);
                                comment.cmt_father = readerFitCmt.GetInt32(1);
                                comment.cmt_content = readerFitCmt.GetString(2);
                                comment.com_id = readerFitCmt.GetInt32(4);
                                comment.user_id = readerFitCmt.GetInt32(5);
                                using (var cmdFitUserType = con.CreateCommand())
                                {
                                    var sqlFitUserType = $"SELECT USER_TYPE FROM USERS WHERE USER_ID = {comment.user_id}";
                                    cmdFitUserType.CommandText = sqlFitUserType;
                                    var readerFitUserType = cmdFitUserType.ExecuteReader();
                                    var sqlFitUser = "";
                                    Console.WriteLine("In searchCommodityByID function going to execute: " + sqlFitUserType);
                                    while (readerFitUserType.Read())
                                    {
                                        if (readerFitUserType.GetInt32(0) == 0)
                                        {
                                            sqlFitUser = $"SELECT CUS_NICKNAME FROM CUSTOMER WHERE CUS_ID = {comment.user_id}";
                                            comment.user_type = 0;
                                            using (var cmdFitBuyTimes = con.CreateCommand())
                                            {
                                                var sqlFitBuyTimes =
                                                    $"SELECT COUNT (*) " +
                                                    $"FROM INDENT " +
                                                    $"WHERE COM_ID = {comment.com_id} AND CUS_ID = {comment.user_id} " +
                                                    $"GROUP BY CUS_ID ";
                                                cmdFitBuyTimes.CommandText = sqlFitBuyTimes;
                                                Console.WriteLine("In searchCommodityByID function going to execute: " + sqlFitBuyTimes);
                                                var readerFitBuyTimes = cmdFitBuyTimes.ExecuteReader();

                                                while (readerFitBuyTimes.Read())
                                                {
                                                    comment.buying_times = readerFitBuyTimes.GetInt32(0);
                                                    break;
                                                }
                                            }
                                        }
                                        else if (readerFitUserType.GetInt32(0) == 1)
                                        {
                                            sqlFitUser = $"SELECT STO_NAME FROM STORE WHERE STO_ID = {comment.user_id}";
                                            comment.user_type = 1;
                                            comment.buying_times = 0;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Something Wrong In cmdFitUserType");
                                        }
                                        using (var cmdFitUser = con.CreateCommand())
                                        {
                                            cmdFitUser.CommandText = sqlFitUser;
                                            Console.WriteLine("In searchCommodityByID function going to execute: " + sqlFitUser);
                                            var readerUser = cmdFitUser.ExecuteReader();
                                            while (readerUser.Read())
                                            {
                                                comment.cmt_name = readerUser.GetString(0);
                                            }
                                        }
                                        break;
                                    }
                                }

                                Console.WriteLine(comment.cmt_content);
                                if (comment != null)
                                    searchModel.comments.Add(comment);
                            }
                            readerFitCmt.Dispose();
                            Console.WriteLine("finish fitCategorySql");
                        }
                    }
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("In searchCommodityByName function erorr \n");
                    Console.WriteLine(ex.Message);
                }
            }
            return searchModel;
        }

        public List<string> sqlSearchCategories()
        {
            var list = new List<string>();
            using (var cmd = con.CreateCommand())
            {
                var sql = $"SELECT * FROM COMMODITIES_CATEGORIES";
                cmd.CommandText = sql;
                Console.WriteLine("In searchCommodityByID function going to execute: " + sql);
                OracleDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(reader.GetString(0));
                reader.Dispose();
                Console.WriteLine("finish fitCategorySql");
            }
            return list;
        }
        public CommodityListModel sqlSearchShoppingCart(int com_id, int cus_id)
        {
            Console.WriteLine("Get into function searchCommodityByID \n");
            DateTime date = DateTime.Now;
            var searchSql =
                $"SELECT " +
                $" COM_ID,COM_NAME,COM_INTRODUCTION,COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE,COM_LEFT,COM_RATING,COMMODITY.STO_ID,STORE.STO_NAME" +
                $" FROM COMMODITY, STORE" +
                $" WHERE COM_ID ={com_id} AND COMMODITY.STO_ID=STORE.STO_ID";
            Console.WriteLine("In searchCommodityByName function going to execute: " + searchSql + "\n");
            var searchModel = new CommodityListModel();
            using (var cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = searchSql;
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.Write($"{reader.ToString()}");
                        searchModel.com_id = reader.GetInt32(0);
                        searchModel.com_name = reader.GetString(1);
                        searchModel.com_introduction = reader.GetString(2);
                        searchModel.com_oriPrice = reader.GetDouble(3);
                        searchModel.com_expirationDate = reader.GetDateTime(4).ToString("yyyy-MM-dd");
                        searchModel.com_uploadDate = reader.GetDateTime(5).ToString("yyyy-MM-dd");
                        searchModel.com_left = reader.GetInt32(6);
                        searchModel.com_rating = reader.GetDouble(7);
                        searchModel.sto_id = reader.GetDouble(8);
                        searchModel.sto_name = reader.GetString(9);
                        using (var cmdFitCategory = con.CreateCommand())
                        {
                            var fitCategorySql = $"SELECT COM_CATEGORY FROM COMMODITY_CATEGORIES WHERE COM_ID = {searchModel.com_id}";
                            cmdFitCategory.CommandText = fitCategorySql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitCategorySql);
                            OracleDataReader readerFitCategory = cmdFitCategory.ExecuteReader();

                            while (readerFitCategory.Read())
                            {
                                searchModel.com_categories.Add(readerFitCategory.GetString(0));

                            }
                            readerFitCategory.Dispose();
                            Console.WriteLine("finish fitCategorySql");
                        }
                        using (var cmdFitImage = con.CreateCommand())
                        {
                            var fitImageSql = $"SELECT * FROM COMMODITY_IMAGE WHERE COM_ID={searchModel.com_id} ORDER BY COM_ID DESC ";
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitImageSql);
                            cmdFitImage.CommandText = fitImageSql;
                            OracleDataReader readerFitImage = cmdFitImage.ExecuteReader();
                            while (readerFitImage.Read())
                            {
                                searchModel.com_firstImage = readerFitImage.GetString(1).Substring(10);
                                break;
                            }
                            readerFitImage.Dispose();
                            Console.WriteLine("finish FitImageSql");
                        }

                        searchModel.com_price = sqlGetCommodityCurrPrice(searchModel.com_id);

                        using (var cmdFitFavor = con.CreateCommand())
                        {
                            var sqlQuery = $"SELECT * FROM FAVORITE WHERE COM_ID = {searchModel.com_id} AND CUS_ID ={cus_id}";
                            cmdFitFavor.CommandText = sqlQuery;
                            var my_reader = cmdFitFavor.ExecuteReader();
                            if (my_reader.HasRows)
                            {
                                searchModel.favor_state = 1;
                            }
                            else
                            {
                                searchModel.favor_state = 0;
                            }

                        }

                    }
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("In searchCommodityByName function erorr \n");
                    Console.WriteLine(ex.Message);
                }
            }
            return searchModel;
        }

        //按照商家名称搜索特定的商家，返回这些商家的基本信息和每个商家旗下三款商品的基本信息，用于商家搜索页面
        public List<StoreListModel> sqlSearchStoreByName(searchStoreModel model)
        {
            DateTime date = DateTime.Now;
            Console.WriteLine($"current time =>{date}");
            var list = new List<StoreListModel>();
            var sqlArray = string.Join(",", model.com_categories.Select(c => $"'{c}'"));
            Console.WriteLine(sqlArray);
            var sqlCategories_1 = "";
            var sqlCategories_2 = "";
            if (model.com_categories.Count > 0)
            {
                sqlCategories_1 =
                $" WITH FAVOR_STO_ID AS (" +
                $" SELECT STORE_CATEGORIES.STORE_ID AS SUB_ID" +
                $" FROM STORE_CATEGORIES" +
                $" WHERE COM_CATEGORY IN ({sqlArray}) " +
                $" GROUP BY STORE_CATEGORIES.STORE_ID" +
                $") ";
                sqlCategories_2 = "AND STORE.STO_ID IN (SELECT SUB_ID FROM FAVOR_STO_ID)";
            }
            var searchSql =
                $" SELECT " +
                $" STO_ID,STO_NAME,STO_INTRODUCTION,USER_ADDRESS" +
                $" FROM USERS,STORE" +
                $" WHERE STO_NAME LIKE '%{model.search_str}%' AND STORE.STO_ID=USERS.USER_ID ";

            searchSql = sqlCategories_1 + searchSql + sqlCategories_2;
            var sortSql = "";
            bool isSorted = false;
            switch (model.sort_order)
            {
                case 0:
                    isSorted = false;
                    break;
                case 1:
                    isSorted = true;//地理位置远近排序未实现
                    break;
                default:
                    break;
            }
            searchSql += sortSql;
            Console.WriteLine("In 'searchStoreByName' function going to execute: " + searchSql + "\n");
            using (var cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = searchSql;
                    OracleDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var searchModel = new StoreListModel();
                        searchModel.sto_id = reader.GetInt32(0);
                        searchModel.sto_name = reader.GetString(1);
                        searchModel.sto_introduction = reader.GetString(2);
                        searchModel.user_address = reader.GetString(3);
                        using (var cmdFitCategory = con.CreateCommand())
                        {
                            var fitCategorySql = $"SELECT COM_CATEGORY FROM STORE_CATEGORIES WHERE STORE_ID = {searchModel.sto_id}";
                            cmdFitCategory.CommandText = fitCategorySql;
                            Console.WriteLine("In 'searchStoreByName' function going to execute: " + fitCategorySql);
                            OracleDataReader readerFitCategory = cmdFitCategory.ExecuteReader();

                            while (readerFitCategory.Read())
                            {
                                searchModel.com_categories.Add(readerFitCategory.GetString(0));
                            }
                            readerFitCategory.Dispose();
                            Console.WriteLine("finish fitCategorySql");
                        }
                        using (var cmdFitFirImage = con.CreateCommand())
                        {
                            var fitFirImageSql = $"SELECT * FROM STOREIMAGE WHERE STO_ID={searchModel.sto_id} ";
                            Console.WriteLine("In 'searchStoreByName' function going to execute: " + fitFirImageSql);
                            cmdFitFirImage.CommandText = fitFirImageSql;
                            OracleDataReader readerFitFirImage = cmdFitFirImage.ExecuteReader();
                            while (readerFitFirImage.Read())
                            {
                                searchModel.sto_firstImage = readerFitFirImage.GetString(1);
                                break;
                            }

                            readerFitFirImage.Dispose();
                            Console.WriteLine("finish FitFirImageSql");
                        }
                        using (var cmdFitCom = con.CreateCommand())
                        {
                            var fitComSql = $"SELECT COM_ID,COM_NAME,COM_EXPIRATIONDATE FROM COMMODITY WHERE STO_ID ={searchModel.sto_id} ORDER BY COM_RATING";
                            cmdFitCom.CommandText = fitComSql;
                            Console.WriteLine("In 'searchStoreByName' function going to execute: " + fitComSql);
                            OracleDataReader readerFitCom = cmdFitCom.ExecuteReader();
                            int count = 0;
                            while (readerFitCom.Read() && count < 3)
                            {

                                var subCom = new SubCommodityListModel();
                                subCom.com_id = readerFitCom.GetInt32(0); ;
                                subCom.com_name = readerFitCom.GetString(1);
                                subCom.com_expirationDate = readerFitCom.GetDateTime(2).ToString("yyyy-MM-dd");
                                int my_com_id = readerFitCom.GetInt32(0); ;
                                subCom.com_price = sqlGetCommodityCurrPrice(subCom.com_id);
                                using (var cmdFitFirImage = con.CreateCommand())
                                {
                                    var fitFirImageSql = $"SELECT * FROM COMMODITY_IMAGE WHERE COM_ID={my_com_id} ";
                                    Console.WriteLine("In 'searchStoreByName' function going to execute: " + fitFirImageSql);
                                    cmdFitFirImage.CommandText = fitFirImageSql;
                                    OracleDataReader readerFitFirImage = cmdFitFirImage.ExecuteReader();
                                    if (readerFitFirImage.HasRows)
                                    {
                                        readerFitFirImage.Read();
                                        subCom.com_firstImage = readerFitFirImage.GetString(1).Substring(10);
                                        readerFitFirImage.Dispose();
                                        Console.WriteLine("finish FitFirImageSql");
                                    }
                                    else { Console.WriteLine($"The commodity with id={my_com_id} has no image"); }

                                }


                                searchModel.com_list.Add(subCom);
                                count++;
                            }
                        }
                        using (var cmdFitIndent = con.CreateCommand())
                        {
                            var fitIndentSql =
                                $"SELECT STORE.STO_ID,COUNT(IND_ID) " +
                                $"FROM STORE,INDENT,COMMODITY " +
                                $"WHERE STORE.STO_ID={searchModel.sto_id} AND COMMODITY.STO_ID=STORE.STO_ID AND INDENT.COM_ID =COMMODITY.COM_ID " +
                                $"GROUP BY STORE.STO_ID";
                            Console.WriteLine("In 'searchStoreByName' function going to execute: " + fitIndentSql);
                            cmdFitIndent.CommandText = fitIndentSql;
                            OracleDataReader readerFitIndent = cmdFitIndent.ExecuteReader();
                            while (readerFitIndent.Read())
                            {
                                searchModel.indentNum = readerFitIndent.GetInt32(1);
                                break;
                            }

                            readerFitIndent.Dispose();
                            Console.WriteLine("finish FitFirImageSql");
                        }
                        list.Add(searchModel);
                    }
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("In searchStoreByName function erorr \n");
                    Console.WriteLine(ex.Message);
                }
            }
            if (!isSorted)
                list.Sort((x, y) => y.indentNum.CompareTo(x.indentNum));
            Console.WriteLine("finish searchStoreByName function");
            return list;
        }



        //按照商家ID搜索特定的商家，返回这个商家的基本信息和每个商家旗下三款商品的基本信息，用于商家详情页面
        public StoreDetailModel sqlSearchStoreByID(int sto_id, int cus_id)
        {

            Console.WriteLine("Get tnto searchStoreByID function ");
            DateTime date = DateTime.Now;
            Console.WriteLine($"current time =>{date}");
            var list = new List<StoreListModel>();
            var searchSql =
                $" SELECT " +
                $" STO_ID,STO_NAME,STO_INTRODUCTION,STO_LICENSEIMG,USER_ADDRESS" +
                $" FROM USERS,STORE" +
                $" WHERE STORE.STO_ID={sto_id} AND STORE.STO_ID=USERS.USER_ID";
            Console.WriteLine("In searchStoreByName function going to execute: " + searchSql + "\n");
            var searchModel = new StoreDetailModel();
            using (var cmd = con.CreateCommand())
            {
                try
                {
                    cmd.CommandText = searchSql;
                    OracleDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        searchModel.sto_id = reader.GetInt32(0);
                        searchModel.sto_name = reader.GetString(1);
                        searchModel.sto_introduction = reader.GetString(2);
                        searchModel.sto_licenseImg = reader.GetString(3);
                        searchModel.sto_licenseImg = reader.GetString(3);
                        searchModel.user_address = reader.GetString(4);

                        using (var cmdFitCategory = con.CreateCommand())
                        {
                            var FitCategorySql = $"SELECT COM_CATEGORY FROM STORE_CATEGORIES WHERE STORE_ID = {searchModel.sto_id}";
                            cmdFitCategory.CommandText = FitCategorySql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + FitCategorySql);
                            OracleDataReader readerFitCategory = cmdFitCategory.ExecuteReader();

                            while (readerFitCategory.Read())
                            {
                                searchModel.com_categories.Add(readerFitCategory.GetString(0));
                            }
                            readerFitCategory.Dispose();
                            Console.WriteLine("finish FitCategorySql");
                        }
                        using (var cmdFitImage = con.CreateCommand())
                        {
                            var fitImageSql = $"SELECT * FROM STOREIMAGE WHERE STO_ID={searchModel.sto_id} ";
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitImageSql);
                            cmdFitImage.CommandText = fitImageSql;
                            OracleDataReader readerFitImage = cmdFitImage.ExecuteReader();
                            while (readerFitImage.Read())
                            {
                                searchModel.sto_imageList.Add(readerFitImage.GetString(1));

                            }

                            readerFitImage.Dispose();
                            Console.WriteLine("finish FitFirImageSql");
                        }
                        using (var cmdFitNotice = con.CreateCommand())
                        {
                            var fitNoticeSql = $"SELECT * FROM  NOTICE WHERE STO_ID={searchModel.sto_id} ORDER BY NTC_TIME DESC ";
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitNoticeSql);
                            cmdFitNotice.CommandText = fitNoticeSql;
                            OracleDataReader readerFitNotice = cmdFitNotice.ExecuteReader();
                            while (readerFitNotice.Read())
                            {
                                var tempNotice = new NoticeModel();
                                tempNotice.ntc_content = readerFitNotice.GetString(2);
                                tempNotice.ntc_time = readerFitNotice.GetDateTime(1).ToString("yyyy-MM-dd");
                                searchModel.sto_notice.Add(tempNotice);
                            }
                            readerFitNotice.Dispose();
                            Console.WriteLine("finish FitFirNoticeSql");
                        }
                        using (var cmdFitCom = con.CreateCommand())
                        {
                            var fitComSql = $"SELECT COM_ID,COM_NAME,COM_EXPIRATIONDATE FROM COMMODITY WHERE STO_ID ={searchModel.sto_id} ORDER BY COM_RATING";
                            cmdFitCom.CommandText = fitComSql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitComSql);
                            OracleDataReader readerFitCom = cmdFitCom.ExecuteReader();
                            while (readerFitCom.Read())
                            {
                                var subCom = new SubCommodityListModel();
                                subCom.com_id = readerFitCom.GetInt32(0);
                                subCom.com_name = readerFitCom.GetString(1);
                                subCom.com_expirationDate = readerFitCom.GetDateTime(2).ToString("yyyy-MM-dd");
                                int my_com_id = readerFitCom.GetInt32(0); ;
                                subCom.com_price = sqlGetCommodityCurrPrice(subCom.com_id);

                                using (var cmdFitFirImage = con.CreateCommand())
                                {
                                    var fitFirImageSql = $"SELECT * FROM COMMODITY_IMAGE WHERE COM_ID={my_com_id} ";
                                    Console.WriteLine("In searchCommodityByName function going to execute: " + fitFirImageSql);
                                    cmdFitFirImage.CommandText = fitFirImageSql;
                                    OracleDataReader readerFitFirImage = cmdFitFirImage.ExecuteReader();
                                    if (readerFitFirImage.HasRows)
                                    {
                                        readerFitFirImage.Read();
                                        subCom.com_firstImage = readerFitFirImage.GetString(1).Substring(10);
                                        readerFitFirImage.Dispose();
                                        Console.WriteLine("finish FitFirImageSql");
                                    }
                                    else { Console.WriteLine($"The commodity with id={my_com_id} has no image"); }
                                }
                                using (var cmdFitFavor = con.CreateCommand())
                                {
                                    var sqlQuery = $"SELECT * FROM FAVORITE WHERE COM_ID = {subCom.com_id} AND CUS_ID ={cus_id}";
                                    cmdFitFavor.CommandText = sqlQuery;//sqlList[0]为插入语句，sqlList[1]为删除语句
                                    var my_reader = cmdFitFavor.ExecuteReader();
                                    if (my_reader.HasRows)
                                    {
                                        subCom.favor_state = 1;
                                    }
                                    else
                                    {
                                        subCom.favor_state = 0;
                                    }
                                }
                                searchModel.com_list.Add(subCom);
                            }
                        }
                    }
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("In searchStoreByID function erorr \n");
                    Console.WriteLine(ex.Message);
                }
            }
            Console.WriteLine("finish searchStoreByID function");
            return searchModel;
        }


        public int sqlSetFavorState(List<string> sqlList)
        {
            int sqlFlag = 1;//如果为0则执行插入语句，如果为1则执行执行删除语句
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sqlList[2];//sqlList[0]为插入语句，sqlList[1]为删除语句
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count != 0)
                {
                    Console.WriteLine("The tuple exists,we gotta delete the turple");
                    sqlFlag = 1;
                }
                else if (count == 0)
                {
                    Console.WriteLine("The tuple does not exist,we gotta insert the turple");
                    sqlFlag = 0;
                }
                using (var cmdDel = con.CreateCommand())
                {
                    cmdDel.CommandText = sqlList[sqlFlag];
                    Console.WriteLine("In function 'sqlSetFavorState' we gotta execute " + sqlList[sqlFlag]);
                    cmdDel.ExecuteNonQuery();

                }
            }
            Console.WriteLine("Finish function 'sqlSetFavorState'");
            return sqlFlag;
        }


        public double sqlGetCommodityCurrPrice(int com_id)
        {
            DateTime date = DateTime.Now;
            var searchSql =
                $" SELECT " +
                $" COM_ID,COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE" +
                $" FROM COMMODITY" +
                $" WHERE COM_ID ={com_id}";
            double com_price = 0;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = searchSql;
                OracleDataReader reader = cmd.ExecuteReader();
                double com_oriPrice = -1;
                string com_expirationDate = " ";
                string com_uploadDate = "";
                while (reader.Read())
                {
                    com_oriPrice = reader.GetDouble(1);
                    com_expirationDate = reader.GetDateTime(2).ToString("yyyy-MM-dd");
                    com_uploadDate = reader.GetDateTime(3).ToString("yyyy-MM-dd");
                }
                using (var cmdFitPrice = con.CreateCommand())
                {
                    var com_prices = new List<PriceCurveModel>();
                    var fitPriceSql = $"SELECT COM_PC_TIME,COM_PC_PRICE FROM COMMODITY_PRICE_CURVE WHERE COM_ID ={com_id} ORDER BY COM_PC_TIME ASC";
                    var uploadPriceNode = new PriceCurveModel();
                    var exPriceNode = new PriceCurveModel();

                    cmdFitPrice.CommandText = fitPriceSql;
                    Console.WriteLine("In searchCommodityByName function going to execute: " + fitPriceSql);
                    OracleDataReader readerFitPrice = cmdFitPrice.ExecuteReader();
                    uploadPriceNode.com_pc_price = com_oriPrice;
                    uploadPriceNode.com_pc_time = com_uploadDate;
                    exPriceNode.com_pc_price = 0;
                    exPriceNode.com_pc_time = com_expirationDate;
                    while (readerFitPrice.Read())
                    {
                        var singlePriceNode = new PriceCurveModel();
                        singlePriceNode.com_pc_price = readerFitPrice.GetDouble(1) * com_oriPrice;
                        singlePriceNode.com_pc_time = readerFitPrice.GetDateTime(0).ToString("yyyy-MM-dd");
                        com_prices.Add(singlePriceNode);
                    }
                    com_prices.Add(uploadPriceNode);
                    com_prices.Add(exPriceNode);
                    com_prices.Sort((x, y) => y.com_pc_price.CompareTo(x.com_pc_price));


                    foreach (var priceNode in com_prices)
                    {

                        if (DateTime.Parse(priceNode.com_pc_time) <= date)
                            com_price = priceNode.com_pc_price;
                    }
                    readerFitPrice.Dispose();

                    if (com_price == -1)
                        Console.WriteLine("发生错误，可能存在上传日期大于当前日期的情况");

                }
            }
            int temp = (int)(com_price * 100);
            com_price = temp;
            com_price /= 100;
            return com_price;
        }
    }
}
