using Gurutw.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Gurutw.Repositorys
{
    public class MemberRepository
    {
        private static string connString;
        private readonly SqlConnection conn;

        private MvcDataBaseEntities db;
        public MemberRepository()
        {
            if (string.IsNullOrEmpty(connString))
            {
                connString = ConfigurationManager.ConnectionStrings["MvcDataBase"].ConnectionString;
            }
            conn = new SqlConnection(connString);
            db = new MvcDataBaseEntities();
        }

        public void CountCartQuantity(int pdid, int mid, int cc)
        {
            var sure = db.Product_Detail.Where(x => x.pd_id == pdid).FirstOrDefault();
            //紀錄購物車產品數量
            var pp = db.Shopping_Cart.Where(x => x.pd_id == pdid).Where(i => i.m_id == mid).FirstOrDefault();
            if (cc > 0)
            {
                if (sure.pd_stock > sure.pd_onorder)//確認是否有庫存
                {
                    sure.pd_onorder++;
                    pp.cart_quantity++;
                    pdid = 0;
                    db.SaveChanges();
                }
                //否則跳出 顯示已沒庫存
            }
            if (cc < 0)
            {
                if (pp.cart_quantity > 1)
                {
                    sure.pd_onorder--;
                    pp.cart_quantity--;
                    db.SaveChanges();
                }
            }
        }

    }
}