using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using System.Drawing;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;
using static WebApplication3.Controllers.SearchController;
using static WebApplication3.Controllers.CommodityController;
using static WebApplication3.Controllers.StoreController;
using System.Security.Cryptography;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using WebApplication3.Controllers;
using Microsoft.VisualBasic;
using System.Reflection;
using Microsoft.AspNetCore.Routing.Constraints;

namespace WebApplication3
{
    public class DataBase
    { 
       // private string connectString = @"DATA SOURCE=localhost:1521/orcl;TNS_ADMIN=C:\Users\Administrator\Oracle\network\admin;PERSIST SECURITY INFO=True;USER ID=WGY;Password=123456";
        private string connectString = @"DATA SOURCE=124.70.7.210:1521/orcl;TNS_ADMIN=C:\Users\Administrator\Oracle\network\admin;PERSIST SECURITY INFO=True;USER ID=WGY;Password=123456";
        //private string connectString ="User Id=" + "system" + ";Password=" + "wy200286" + ";Data Source=" +"localhost:1521/orcl" + ";";
        OracleConnection con;
        public static DataBase oracleCon;
        public DataBase()
        {
            con = new OracleConnection(connectString);
            con.Open();
            Console.WriteLine("Oracle 连接成功\n");
        }
  

        /*插入某个特定的元组*/
        public int sqlInsertSingleItem(string sql)
        {
            Console.WriteLine("Get into function 'sqlInsertSingleItem'");
            int flag = 1;//用于是返回是否成功插入
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
        public int sqlInsertAtomicity(List<string>sql)
        {
            Console.WriteLine("Get into function 'sqlInsertAtomicity'");
            int flag = 1;
            OracleTransaction transaction = con.BeginTransaction();
            try
            {
                for(int i = 0; i < sql.Count; i++)
                {
                    var cmd=con.CreateCommand();
                    cmd.CommandText = sql[i];
                    Console.WriteLine("No. "+(i+1)+" sql： "+cmd.CommandText);
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
                Console.WriteLine("we have succesfully executed "+ sql.Count+ " sql sentences");
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
            byte[] image=File.ReadAllBytes(imagePath);
            var str = "data:image/jpeg;base64,"+ Convert.ToBase64String(image);
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
            if (model.com_categories.Count>0)
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
            searchSql = sqlCategories_1 + searchSql+ sqlCategories_2;
            var sortSql = "";
            bool isSorted=false;
            switch (model.sort_order)
            {
                case 0 : 
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
                                searchModel.com_firstImage = readerFitFirImage.GetString(1);
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
                list.Sort((x,y)=>x.com_price.CompareTo(y.com_price));
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
                                searchModel.com_firstImage = readerFitFirImage.GetString(1);
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
                                searchModel.com_firstImage = readerFitFirImage.GetString(1);
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
        public CommodityDetailModel sqlSearchCommodityByID(int com_id,int cus_id)
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
                                searchModel.com_images.Add(readerFitImage.GetString(1));                              
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
                                double temp = (int)(readerFitPrice.GetDouble(1)* searchModel.com_oriPrice*100);
                                singlePriceNode.com_pc_price = temp / 100;
                                singlePriceNode.com_pc_time = readerFitPrice.GetDateTime(0).ToString("yyyy-MM-dd");
                                searchModel.com_prices.Add(singlePriceNode);
                            }
                            searchModel.com_prices.Add(uploadPriceNode);
                            searchModel.com_prices.Add(exPriceNode);
                            searchModel.com_prices.Sort((x,y)=>y.com_pc_price.CompareTo(x.com_pc_price));
                            searchModel.com_price = sqlGetCommodityCurrPrice(searchModel.com_id);
                            readerFitPrice.Dispose();
                            Console.WriteLine("finish fitPriceSql\n");
                        }
                        using (var cmdFitFavor = con.CreateCommand())
                        {
                            var sqlQuery = $"SELECT * FROM FAVORITE WHERE COM_ID = {searchModel.com_id} AND CUS_ID ={cus_id}";
                            cmdFitFavor.CommandText = sqlQuery;
                            var my_reader= cmdFitFavor.ExecuteReader();
                            if (my_reader.HasRows)
                            {
                                searchModel.favor_state = 1;
                            }
                            else
                            {
                                searchModel.favor_state = 0;
                            }                            

                        }
                        using (var cmdFitCmt=con.CreateCommand())
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
                                comment.cmt_content= readerFitCmt.GetString(2);
                                comment.com_id = readerFitCmt.GetInt32(4);  
                                comment.user_id = readerFitCmt.GetInt32(5);
                                using(var cmdFitUserType=con.CreateCommand())
                                {
                                    var sqlFitUserType= $"SELECT USER_TYPE FROM USERS WHERE USER_ID = {comment.user_id}";
                                    cmdFitUserType.CommandText = sqlFitUserType;
                                    var readerFitUserType= cmdFitUserType.ExecuteReader();
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
                                            comment.buying_times =0;
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
                                if(comment!=null)
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
            var list= new List<string>();
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
                                searchModel.com_firstImage=readerFitImage.GetString(1);
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
                            while(readerFitFirImage.Read()) {
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
                            while (readerFitCom.Read()&&count<3)
                            {

                                var subCom = new SubCommodityListModel();
                                subCom.com_id =readerFitCom.GetInt32(0); ;
                                subCom.com_name = readerFitCom.GetString(1);
                                subCom.com_expirationDate = readerFitCom.GetDateTime(2).ToString("yyyy-MM-dd");
                                int my_com_id = readerFitCom.GetInt32(0); ;
                                subCom.com_price=sqlGetCommodityCurrPrice(subCom.com_id);
                                using (var cmdFitFirImage = con.CreateCommand())
                                {
                                    var fitFirImageSql = $"SELECT * FROM COMMODITY_IMAGE WHERE COM_ID={my_com_id} ";
                                    Console.WriteLine("In 'searchStoreByName' function going to execute: " + fitFirImageSql);
                                    cmdFitFirImage.CommandText = fitFirImageSql;
                                    OracleDataReader readerFitFirImage = cmdFitFirImage.ExecuteReader();
                                    if(readerFitFirImage.HasRows)
                                    {
                                        readerFitFirImage.Read();
                                        subCom.com_firstImage = readerFitFirImage.GetString(1);
                                        readerFitFirImage.Dispose();
                                        Console.WriteLine("finish FitFirImageSql");
                                    }
                                    else { Console.WriteLine($"The commodity with id={my_com_id} has no image"); }
                                   
                                }
                               

                                searchModel.com_list.Add(subCom);
                                count++;
                            }
                        }
                        using (var cmdFitIndent=con.CreateCommand())
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
        public StoreDetailModel sqlSearchStoreByID(int sto_id,int cus_id)
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
                                searchModel.sto_imageList.Add (readerFitImage.GetString(1));
                                
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
                                var tempNotice=new NoticeModel();
                                tempNotice.ntc_content = readerFitNotice.GetString(2);
                                tempNotice.ntc_time= readerFitNotice.GetDateTime(1).ToString("yyyy-MM-dd");
                                searchModel.sto_notice.Add (tempNotice);
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
                                subCom.com_id=readerFitCom.GetInt32(0);
                                subCom.com_name = readerFitCom.GetString(1);
                                subCom.com_expirationDate = readerFitCom.GetDateTime(2).ToString("yyyy-MM-dd");
                                int my_com_id = readerFitCom.GetInt32(0); ;
                                subCom.com_price=sqlGetCommodityCurrPrice(subCom.com_id);

                                using (var cmdFitFirImage = con.CreateCommand())
                                {
                                    var fitFirImageSql = $"SELECT * FROM COMMODITY_IMAGE WHERE COM_ID={my_com_id} ";
                                    Console.WriteLine("In searchCommodityByName function going to execute: " + fitFirImageSql);
                                    cmdFitFirImage.CommandText = fitFirImageSql;
                                    OracleDataReader readerFitFirImage = cmdFitFirImage.ExecuteReader();
                                    if (readerFitFirImage.HasRows)
                                    {
                                        readerFitFirImage.Read();
                                        subCom.com_firstImage = readerFitFirImage.GetString(1);
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
                if (count!=0)
                {
                    Console.WriteLine("The tuple exists,we gotta delete the turple");      
                    sqlFlag = 1;
                }
                else if (count == 0)
                {
                    Console.WriteLine("The tuple does not exist,we gotta insert the turple");
                    sqlFlag = 0;
                }
                using (var cmdDel= con.CreateCommand())
                {
                        cmdDel.CommandText= sqlList[sqlFlag];
                        Console.WriteLine("In function 'sqlSetFavorState' we gotta execute "+ sqlList[sqlFlag]);
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
                double com_oriPrice=-1;
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
                        singlePriceNode.com_pc_price = readerFitPrice.GetDouble(1) *  com_oriPrice;
                        singlePriceNode.com_pc_time = readerFitPrice.GetDateTime(0).ToString("yyyy-MM-dd");
                        com_prices.Add(singlePriceNode);
                    }
                    com_prices.Add(uploadPriceNode);
                    com_prices.Add(exPriceNode);
                    com_prices.Sort((x, y) => y.com_pc_price.CompareTo(x.com_pc_price));
         
                    
                    foreach (var priceNode in  com_prices)
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
