using Dapper;
using Gurutw.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gurutw.Controllers
{
    public class MemberController : Controller
    {
        // GET: Member
        //public ActionResult Index()
        //{
        //    return View();
        //}
        private readonly SqlConnection conn;
        private static string connString;
        private MvcDataBaseEntities db;
        public MemberController()
        {
            if (string.IsNullOrEmpty(connString))
            {
                connString = ConfigurationManager.ConnectionStrings["MvcDataBase"].ConnectionString;
            }
            conn = new SqlConnection(connString);
            db = new MvcDataBaseEntities();
        }
        public ActionResult AddCart(int? pdid)
        {
            var user = int.Parse(Session["m_id"].ToString());
            var sure = db.Product_Detail.Where(x => x.pd_id == pdid).FirstOrDefault();
            var pp = db.Shopping_Cart.Where(x => x.pd_id == pdid).Where(i => i.m_id == user).FirstOrDefault();
            if (pdid != 0)
            {
                if (sure.pd_stock > sure.pd_onorder)
                {
                    sure.pd_onorder++;
                    pp.cart_quantity++;
                    pdid = 0;
                    db.SaveChanges();
                }
                //否則跳出已沒庫存
            }
            return RedirectToAction("Cart");
        }
        public ActionResult LessCart(int? pdid)
        {
            var user = int.Parse(Session["m_id"].ToString());
            var sure = db.Product_Detail.Where(x => x.pd_id == pdid).FirstOrDefault();
            var pp = db.Shopping_Cart.Where(x => x.pd_id == pdid).Where(i => i.m_id == user).FirstOrDefault();
            if (pdid != 0)
            {
                if (pp.cart_quantity == 0)
                {
                    return RedirectToAction("Cart", pdid);
                }
                sure.pd_onorder--;
                pp.cart_quantity--;
                db.SaveChanges();

            }
            return RedirectToAction("Cart");
        }
        public ActionResult DelCart(int? pdid = 0)
        {
            if (pdid != 0)
            {
                string delsql = "DELETE from Shopping_Cart Where pd_id=@pdid";
                conn.Execute(delsql, new { pdid = pdid });
            }
            return RedirectToAction("Cart");
        }


        public ActionResult Cart()
        {
            Response.Cache.SetNoStore();
            using (conn)
            {
                var nowtime = DateTime.Now;
                var user = int.Parse(Session["m_id"].ToString());
                string sql = "SELECT dbo.Member.m_id,  " +
                               "dbo.Shopping_Cart.pd_id,  " +
                               "dbo.Product_Detail.pd_color,  " +
                               "dbo.Product.p_unitprice,  " +
                               "dbo.Product.p_name, " +
                               "(select dbo.Discount.d_discount from dbo.Discount where CONVERT(VARCHAR(10), GETDATE(), 120) " +
                               "between dbo.Discount.d_startdate And dbo.Discount.d_enddate " +
                               "And dbo.Discount.c_id = dbo.Category.c_id ) d_discount, " +
                               "(SELECT TOP 1 dbo.Product_Picture.pp_path " +
                               "FROM dbo.Product_Picture " +
                               "WHERE dbo.Product_Picture.p_id = dbo.Product.p_id) AS pp_path," +
                               "dbo.Category.c_id,  " +
                               "dbo.Shopping_Cart.cart_quantity " +
                               "FROM dbo.Product_Detail " +
                               "INNER JOIN dbo.Product ON dbo.Product_Detail.p_id = dbo.Product.p_id " +
                               "INNER JOIN dbo.Shopping_Cart ON dbo.Product_Detail.pd_id = dbo.Shopping_Cart.pd_id " +
                               "INNER JOIN dbo.Member ON dbo.Shopping_Cart.m_id = dbo.Member.m_id " +
                               "INNER JOIN dbo.Category ON dbo.Product.c_id = dbo.Category.c_id " +
                               "WHERE dbo.Shopping_Cart.m_id = " + user + ";"; //依照會員id查詢

                var CartTotal = conn.Query(sql).ToList();
                ViewBag.od = CartTotal;
            }

            return View();
        }

        public ActionResult OrderDeliver()
        {
            ViewBag.dw_id = new SelectList(db.Delive_Way, "dw_id", "dw_name");
            ViewBag.pay_id = new SelectList(db.Payment, "pay_id", "pay_name");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OrderDeliver([Bind(Include = "o_receiver,o_address,pay_id,dw_id")]Order or)
        {

            var user = int.Parse(Session["m_id"].ToString());

            //Order or = new Order();
            if (ModelState.IsValid)
            {
                or.m_id = user;
                or.o_status = "0";
                or.o_date = DateTime.Now;
                db.Order.Add(or);
                db.SaveChanges();
                return RedirectToAction("CreateOrderDetail", new { id = or.o_id });
            }
            return View(or);
        }
        public ActionResult CreateOrderDetail(int? id)
        {
            var user = int.Parse(Session["m_id"].ToString());
            var cart = db.Shopping_Cart.Where(x => x.m_id == user).ToList();

            foreach (var u in cart)
            {
                var pdetail = db.Product_Detail.Where(x => x.pd_id == u.pd_id).FirstOrDefault();
                var product = db.Product.Where(x => x.p_id == pdetail.p_id).FirstOrDefault();
                var date = DateTime.Now;
                var discountList = db.Discount.Where(x => x.c_id == product.c_id).ToList();
                float discount = 0;
                foreach (var t in discountList)
                {
                    if (t.d_startdate <= date && t.d_enddate >= date)
                    {
                        discount = (float)t.d_discount;
                    }
                    else
                    {
                        discount = 1;
                    }
                }
                Order_Detail od = new Order_Detail();
                od.pd_id = u.pd_id;
                od.o_id = id;
                od.od_quantity = u.cart_quantity;
                od.od_discount = discount;
                od.od_price = (float)product.p_unitprice;

                db.Order_Detail.Add(od);
                db.SaveChanges();
            }

            //db.SaveChanges();
            return RedirectToAction("result");
        }
        //[HttpGet]
        //public ActionResult result()
        //{

        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult result()
        {
            var user = int.Parse(Session["m_id"].ToString());
            using (conn)
            {
                string sql = "SELECT dbo.[Order].o_id,dbo.[Order].o_receiver, dbo.[Order].o_address, dbo.[Order].pay_id, dbo.[Order].dw_id, dbo.[Order].m_id, dbo.Payment.pay_name, dbo.Delive_Way.dw_name FROM dbo.[Order] " +
                    "INNER JOIN dbo.Payment ON dbo.[Order].pay_id = dbo.Payment.pay_id INNER JOIN dbo.Delive_Way ON dbo.[Order].dw_id = dbo.Delive_Way.dw_id" +
                    " WHERE(dbo.[Order].m_id = " + user + "and dbo.[Order].o_status != 4 and dbo.[Order].o_status != 8 );";

                var o = conn.Query(sql, new { mid = user });
                ViewBag.t = o;
            }
            return View();
        }

        public ActionResult Outcome(int? Id)
        {
            var user = int.Parse(Session["m_id"].ToString());
            //var orde = db.Order.Where(x => x.m_id == user).FirstOrDefault();

            //var order_tail = db.Order_Detail.Where(x => x.o_id == Id).ToList();
            var check = db.Order.Where(x => x.o_id == Id).FirstOrDefault().m_id;

            if (user != check)
            {
                return RedirectToAction("result");
            }

            //if (order_tail.o_id == Id)" 
            //{
            using (conn)
            {
                string sql = "SELECT dbo.Order_Detail.od_price, dbo.Order_Detail.od_quantity, dbo.Order_Detail.od_discount, dbo.Product_Detail.pd_color, dbo.[Order].o_receiver, dbo.Product.p_name, dbo.Product.p_unitprice FROM dbo.[Order] " +
                            "INNER JOIN dbo.Order_Detail ON dbo.[Order].o_id = dbo.Order_Detail.o_id " +
                            "INNER JOIN dbo.Product_Detail ON dbo.Order_Detail.pd_id = dbo.Product_Detail.pd_id " +
                            "INNER JOIN dbo.Product ON dbo.Product_Detail.p_id = dbo.Product.p_id " +
                            "WHERE dbo.[Order].m_id = " + user + "and dbo.[Order_Detail].o_id = " + Id;
                var o = conn.Query(sql, new { mid = user });
                ViewBag.t = o;
            }
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Home");
            //}

            return View();
        }
    }
}