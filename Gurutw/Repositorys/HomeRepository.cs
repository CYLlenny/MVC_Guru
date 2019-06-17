using Gurutw.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Gurutw.Repositorys
{
    public class HomeRepository
    {
        private readonly SqlConnection conn;
        private static string connString;
        //int nowp = 0;
        private MvcDataBaseEntities db = new MvcDataBaseEntities();

        public HomeRepository()
        {
            if (string.IsNullOrEmpty(connString))
            {
                connString = ConfigurationManager.ConnectionStrings["MvcDataBase"].ConnectionString;
            }
            conn = new SqlConnection(connString);
        }

        public string GetCategorySqlString(int Category)
        {
            string sql =
                    " SELECT distinct " +
                       " p.p_id , " +
                       " p.p_name, " +
                       " p.p_unitprice , " +
                       " p.p_lauchdate , " +
                       " p.p_status , " +
                       " pic_path = STUFF(( " +
                              " SELECT ',' + dbo.Product_Picture.pp_path " +
                              " FROM dbo.Product_Picture " +
                             " where p.p_id = dbo.Product_Picture.p_id " +
                             " FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, ''),  " +
                                                " ( SELECT TOP 1 dbo.Discount.d_discount " +
                        " FROM dbo.Discount " +
                        " WHERE p.c_id = dbo.Discount.c_id " +
                        " ORDER BY dbo.Discount.d_discount " +
                        " )AS d_discount, " +
                        "( SELECT TOP 1 dbo.Discount.d_startdate " +
                        "FROM dbo.Discount " +
                        "WHERE p.c_id = dbo.Discount.c_id " +
                        "AND DATEADD(HH,+8, GETDATE() ) BETWEEN dbo.Discount.d_startdate AND dbo.Discount.d_enddate " +
                        "ORDER BY dbo.Discount.d_discount " +
                        ")AS d_startdate, " +
                        "( SELECT TOP 1 dbo.Discount.d_enddate " +
                        "FROM dbo.Discount " +
                        "WHERE p.c_id = dbo.Discount.c_id " +
                        "AND DATEADD(HH,+8, GETDATE() ) BETWEEN dbo.Discount.d_startdate AND dbo.Discount.d_enddate " +
                        "ORDER BY dbo.Discount.d_discount " +
                        ")AS d_enddate " +
                    " FROM dbo.Product p " +
                    " INNER JOIN dbo.Product_Picture ON p.p_id = dbo.Product_Picture.p_id " +
                    " where p.c_id = " + Category +
                    " and p.p_status = 0 ";
 

            return sql;
        }


        public IEnumerable<Product> GetProductData(int id)
        {      
            return (  db.Product.Where((x) => x.p_id == id)  );     
        }

        public IEnumerable<Product_Detail> GetProductDetailData(int id)
        {
            return (db.Product_Detail.Where((x) => x.p_id == id) );
        }

        public IEnumerable<Product_Picture> GetProductPicturelData(int id)
        {
            return (db.Product_Picture.Where((x) => x.p_id == id) );
        }

        public IEnumerable<Product_Feature> GetProductFeaturelData(int id)
        {
            return (db.Product_Feature.Where((x) => x.p_id == id) );
        }

        public IEnumerable<Classify> GetClassifylData(int id)
        {
            return (db.Classify.Where((x) => x.p_id == id));
        }

    }
}