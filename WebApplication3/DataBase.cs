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
        public List<CommodityListModel> sqlSearchCommodityByName(string searchName,int sortType,int cus_id)
        {       
            DateTime date = DateTime.Now;
            Console.WriteLine($"current time =>{date}");
            var list = new List<CommodityListModel>();
            var searchSql =
                $"WITH FAVOR_COM_ID AS (" +
                $" SELECT COMMODITY_CATEGORIES.COM_ID AS SUB_ID" +
                $" FROM COMMODITY_CATEGORIES" +
                $" WHERE " +
                $" INSTR('{searchName}', COMMODITY_CATEGORIES.COM_CATEGORY) > 0)" +
                $" SELECT " +
                $" COM_ID,COM_NAME,COM_INTRODUCTION,COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE,COM_LEFT,COM_RATING,COMMODITY.STO_ID,STORE.STO_NAME" +
                $" FROM COMMODITY" +
                $" JOIN STORE ON COMMODITY.STO_ID=STORE.STO_ID" +
                $" WHERE COM_NAME LIKE '%{searchName}%'" +
                $" UNION " +
                $" SELECT COM_ID,COM_NAME,COM_INTRODUCTION,COM_ORIPRICE,COM_EXPIRATIONDATE,COM_UPLOADDATE,COM_LEFT,COM_RATING,COMMODITY.STO_ID,STORE.STO_NAME" +
                $" FROM COMMODITY" +
                $" JOIN STORE ON COMMODITY.STO_ID=STORE.STO_ID" +
                $" JOIN FAVOR_COM_ID ON COMMODITY.COM_ID=FAVOR_COM_ID.SUB_ID" +
                $" ORDER BY COM_RATING DESC";
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
                            var sqlQuery = $"SELECT * FROM FAVORITE WHERE COM_ID = {searchModel.com_id} AND CUS_ID ={cus_id}";
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
                        using (var cmdFitPrice = con.CreateCommand())
                        {
                            var fitPriceSql = $"SELECT * FROM COMMODITY_PRICE_CURVE WHERE COM_ID ={searchModel.com_id} ORDER BY COM_PC_TIME ASC";
                            cmdFitPrice.CommandText = fitPriceSql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitPriceSql);
                            OracleDataReader readerFitPrice = cmdFitPrice.ExecuteReader();       
                            double subPrice = 0;
                            //记住，数据库处理的时候需要
                            while (readerFitPrice.Read())
                            {
                               
                                if (readerFitPrice.GetDateTime(1) <= date)
                                {                                   
                                    subPrice = readerFitPrice.GetDouble(2);                                 
                                }
                                else {
                                  
                                    searchModel.com_price = subPrice;
                                    Console.WriteLine($"{searchModel.com_name}找到了现有价格{searchModel.com_price}");
                                  
                                    break;                     
                                }                             
                               
                            }
                            readerFitPrice.Dispose();
                            Console.WriteLine("finish fitPriceSql\n");
                        }
                        list.Add(searchModel);
                    }
                    reader.Dispose();
                }
                catch(Exception ex) { 
                    Console.WriteLine("In searchCommodityByName function erorr \n"); 
                    Console.WriteLine(ex.Message); }
            }
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
                        searchModel.favor_state = 1;

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

                        //搜索固定某个商品现在价格
                        //注意！算法要求商品没有过期！
                        using (var cmdFitPrice = con.CreateCommand())
                        {
                            var fitPriceSql = $"SELECT * FROM COMMODITY_PRICE_CURVE WHERE COM_ID ={searchModel.com_id} ORDER BY COM_PC_TIME ASC";
                            cmdFitPrice.CommandText = fitPriceSql;
                            Console.WriteLine("In searchCommodityByName function going to execute: " + fitPriceSql);
                            OracleDataReader readerFitPrice = cmdFitPrice.ExecuteReader();
                            double subPrice = -1;
                            while (readerFitPrice.Read())
                            {
                                
                                //如果data比所有的readerFitPrice.GetDateTime(1)都大的，现实意义上是过期了，这里无法给其进行赋值
                                if (readerFitPrice.GetDateTime(1) <= date)
                                {
                                    subPrice = readerFitPrice.GetDouble(2);
                                }
                                else
                                {

                                    searchModel.com_price = subPrice;
                                    Console.WriteLine($"{searchModel.com_name}找到了现有价格{searchModel.com_price}");

                                    break;
                                }

                            }
                            readerFitPrice.Dispose();
                            if (searchModel.com_price == -1)
                                Console.WriteLine($"未找到现有价格，商品可能已经过期\n");
                            readerFitPrice.Dispose();
                            Console.WriteLine("finish fitPriceSql\n");
                        }
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
                            double subPrice = -1;
                            bool findCurrPrice = false;
                            while (readerFitPrice.Read())
                            {
                                var singlePriceNode = new PriceCurveModel();
                                singlePriceNode.com_pc_price = readerFitPrice.GetDouble(2);
                                singlePriceNode.com_pc_time = readerFitPrice.GetDateTime(1).ToString("yyyy-MM-dd");
                                Console.WriteLine(readerFitPrice.GetDateTime(1).ToString("yyyy-MM-DD"));
                                searchModel.com_prices.Add(singlePriceNode);
                                if (readerFitPrice.GetDateTime(1) <= date && !findCurrPrice)
                                {
                                    subPrice = readerFitPrice.GetDouble(2);
                                }
                                else if (readerFitPrice.GetDateTime(1) > date && !findCurrPrice)
                                {

                                    searchModel.com_price = subPrice;
                                    Console.WriteLine($"{searchModel.com_name}找到了现有价格{searchModel.com_price}");
                                    findCurrPrice = true;

                                }

                            }
                            readerFitPrice.Dispose();
                            if (searchModel.com_price == -1)
                                Console.WriteLine($"未找到现有价格\n");
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
        public List<StoreListModel> sqlSearchStoreByName(string searchName)
        {
            
            DateTime date = DateTime.Now;
            Console.WriteLine($"current time =>{date}");
            var list = new List<StoreListModel>();
            var searchSql =
                $"SELECT " +
                $" STO_ID,STO_NAME,STO_INTRODUCTION,USER_ADDRESS" +
                $" FROM USERS,STORE" +
                $" WHERE STO_NAME LIKE '%{searchName}%' AND STORE.STO_ID=USERS.USER_ID";

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
                                using (var cmdFitPrice = con.CreateCommand())
                                {
                                    var fitPriceSql = $"SELECT * FROM COMMODITY_PRICE_CURVE WHERE COM_ID ={my_com_id} ORDER BY COM_PC_TIME ASC";
                                    cmdFitPrice.CommandText = fitPriceSql;
                                    Console.WriteLine("In 'searchStoreByName' function going to execute: " + fitPriceSql);
                                    OracleDataReader readerFitPrice = cmdFitPrice.ExecuteReader();
                                    double subPrice = -1;
                                    while (readerFitPrice.Read())
                                    {
                                        if (readerFitPrice.GetDateTime(1) <= date)
                                        {
                                            subPrice = readerFitPrice.GetDouble(2);
                                        }
                                        else if(readerFitPrice.GetDateTime(1) > date)
                                        {
                                            subCom.com_price = subPrice;
                                            Console.WriteLine($"{subCom.com_name}找到了现有价格{subCom.com_price}");
                                            break;
                                        }

                                    }
                                    readerFitPrice.Dispose();
                                    if (subCom.com_price == -1)
                                        Console.WriteLine($"未找到现有价格\n");
                                    readerFitPrice.Dispose();
                                    Console.WriteLine("finish fitPriceSql\n");
                                }
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
                                //fit
                                using (var cmdFitPrice = con.CreateCommand())
                                {
                                    var fitPriceSql = $"SELECT * FROM COMMODITY_PRICE_CURVE WHERE COM_ID ={my_com_id} ORDER BY COM_PC_TIME ASC";
                                    cmdFitPrice.CommandText = fitPriceSql;
                                    Console.WriteLine("In searchCommodityByName function going to execute: " + fitPriceSql);
                                    OracleDataReader readerFitPrice = cmdFitPrice.ExecuteReader();
                                    double subPrice = -1;
                                    while (readerFitPrice.Read())
                                    {
                                        if (readerFitPrice.GetDateTime(1) <= date)
                                        {
                                            subPrice = readerFitPrice.GetDouble(2);
                                        }
                                        else if (readerFitPrice.GetDateTime(1) > date)
                                        {
                                            subCom.com_price = subPrice;
                                            Console.WriteLine($"{subCom.com_name}找到了现有价格{subCom.com_price}");
                                            break;
                                        }

                                    }
                                    readerFitPrice.Dispose();
                                    if (subCom.com_price == -1)
                                        Console.WriteLine($"未找到现有价格\n");
                                    readerFitPrice.Dispose();
                                    Console.WriteLine("finish FitPriceSql\n");
                                }

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

    }
}
