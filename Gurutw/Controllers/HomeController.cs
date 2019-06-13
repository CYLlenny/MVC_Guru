using Gurutw.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gurutw.ViewModels;
using System.Web.Security;
using RegisterViewModel = Gurutw.ViewModels.RegisterViewModel;
using System.Data.SqlClient;
using System.Configuration;
using Dapper;

namespace Gurutw.Controllers
{
    public class HomeController : Controller
    {
        private readonly SqlConnection conn;
        private static string connString;
        int nowp = 0;
        private MvcDataBaseEntities db = new MvcDataBaseEntities();

        public HomeController()
        {
            if (string.IsNullOrEmpty(connString))
            {
                connString = ConfigurationManager.ConnectionStrings["MvcDataBase"].ConnectionString;
            }
            conn = new SqlConnection(connString);
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Registration(RegisterViewModel m)
        {
            Guid GmailId = Guid.NewGuid();
            ViewData["username"] = m.UserName;

            if (!ModelState.IsValid)
            {
                return View(m);
            }
            Member memb = new Member()
            {
                m_name = m.UserName,
                m_email = m.Email,
                m_password = m.Password,
                m_email_id = GmailId
            };
            db.Member.Add(memb);
            db.SaveChanges();
            return View("RegisResult");
        }


        [HttpGet]
        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignIn(SignInViewModel m)
        {
            //if (ModelState.IsValid)
            //{
            //    using (MvcDataBaseContext db = new MvcDataBaseContext())
            //    {
            //        var obj = db.Member.Where(a => a.m_email.Equals(m.m_email) && a.m_password.Equals(m.m_password)).FirstOrDefault();
            //        if (obj != null)
            //        {
            //            Session["m_name"] = obj.m_name.ToString();
            //            return RedirectToAction("Index");
            //        }
            //    }
            //}
            //return View(m);

            //未通過Model驗證
            if (!ModelState.IsValid)
            {
                return View(m);
            }

            //通過Model驗證
            string email = HttpUtility.HtmlEncode(m.Email);
            string password = HttpUtility.HtmlEncode(m.Password);

            //以Name及Password查詢比對Account資料表記錄

            Member user = db.Member.Where(x => x.m_email == email && x.m_password == password).FirstOrDefault();

            if (user == null)
            {
                ModelState.AddModelError("", "The email or password is invalid.");
                return View();
            }

            Session["m_name"] = user.m_name.ToString();
            Session["m_id"] = user.m_id;

            //Create FormsAuthenticationTicket
            var ticket = new FormsAuthenticationTicket(
            version: 1,
            name: user.m_email.ToString(), //可以放使用者Id
            issueDate: DateTime.UtcNow,//現在UTC時間
            expiration: DateTime.UtcNow.AddMinutes(30),//Cookie有效時間=現在時間往後+30分鐘
            isPersistent: true,// 是否要記住我 true or false
            userData: "", //可以放使用者角色名稱
            cookiePath: FormsAuthentication.FormsCookiePath);

            // Encrypt the ticket.
            var encryptedTicket = FormsAuthentication.Encrypt(ticket); //把驗證的表單加密

            // Create the cookie.
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            Response.Cookies.Add(cookie);

            // Redirect back to original URL.
            var url = FormsAuthentication.GetRedirectUrl(email, true);

            //Response.Redirect(FormsAuthentication.GetRedirectUrl(name, true));

            return RedirectToAction("Index");
        }

        public ActionResult Logout()
        {
            Session["m_id"] = null;
            Session["m_name"] = null;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Search(string keywords)
        {
            if (string.IsNullOrEmpty(keywords))
            {
                return RedirectToAction("Index");
            }
            using (conn)
            {
                string sql = 
                    "SELECT distinct dbo.Product.p_id, " +
                    "dbo.Product.p_name, " +
                    "dbo.Product.p_lauchdate, " +
                    "dbo.Product.p_unitprice, " +
                       "( " +
                           "SELECT c.c_name " +
                           "FROM Category c " +
                           "WHERE c.c_id = dbo.Product.c_id " +
                       ")AS c_name, " +
                    "pic_path = STUFF(( " +
                        "SELECT ',' + dbo.Product_Picture.pp_path " +
                        "FROM dbo.Product_Picture " +
                        "where dbo.Product.p_id = dbo.Product_Picture.p_id " +
                        "FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') " +
                    "FROM dbo.Product " +
                    "INNER JOIN dbo.Product_Picture ON dbo.Product.p_id = dbo.Product_Picture.p_id " +
                    "where dbo.Product.p_name like" + "'%" + keywords + "%' ";


                var search_product_list = conn.Query(sql).ToList();
                ViewBag.searchlist = search_product_list;
            }
            return View();
        }

        /*鍵盤分類頁*/
        public ActionResult Keyboard_Category()
        {
            using (conn)
            {
                //string sql =
                //        " SELECT " +
                //            "   p.p_id, " +
                //            "   p.p_name, " +
                //            "   p.p_unitprice,  " +
                //            "   p.p_lauchdate," +
                //            "   p.p_status," +
                //            "   ( " +
                //                    "SELECT TOP 1 pp.pp_path " +
                //                    "FROM dbo.Product_Picture pp " +
                //                    "WHERE pp.p_id = p.p_id " +
                //                " ) AS pp_path " +
                //        " FROM dbo.Product p " +
                //        " WHERE p.c_id = 1 " +
                //        " AND p.p_status = 0 ";

                string sql =
                       "SELECT distinct " +
                       "p.p_id , " +
                       "p.p_name, " +
                       "p.p_unitprice , " +
                       "p.p_lauchdate , " +
                       "p.p_status , " +
                       "pic_path = STUFF(( " +
                              "SELECT ',' + dbo.Product_Picture.pp_path " +
                              "FROM dbo.Product_Picture " +
                             "where p.p_id = dbo.Product_Picture.p_id " +
                             "FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, ''),  " +
                        "d.d_discount, " +
                        "d.d_startdate, " +
                        "d.d_enddate " +
                    "FROM dbo.Product p " +
                    "INNER JOIN dbo.Product_Picture ON p.p_id = dbo.Product_Picture.p_id " +
                    "INNER JOIN dbo.Category c ON c.c_id = p.c_id " +
                    "INNER JOIN dbo.Discount d ON d.c_id = c.c_id " +
                    "where p.c_id = 1 " +
                    "and p.p_status = 0 " +
                    "AND DATEADD(HH,+8, GETDATE() ) BETWEEN d.d_startdate AND d.d_enddate ";

                var product = conn.Query(sql).ToList();
                ViewBag.p = product;

            }

            return View();
        }

        /*滑鼠分類頁*/
        public ActionResult Mouse_Category()
        {
            using (conn)
            {
                //string sql =
                //      " SELECT " +
                //          "   p.p_id, " +
                //          "   p.p_name, " +
                //          "   p.p_unitprice,  " +
                //          "   p.p_lauchdate," +
                //          "   p.p_status," +
                //          "   ( " +
                //                  "SELECT TOP 1 pp.pp_path " +
                //                  "FROM dbo.Product_Picture pp " +
                //                  "WHERE pp.p_id = p.p_id " +
                //              " ) AS pp_path " +
                //      " FROM dbo.Product p " +
                //      " WHERE p.c_id = 2 " +
                //      " AND p.p_status = 0 ";

                string sql =
                       "SELECT distinct " +
                       "p.p_id , " +
                       "p.p_name, " +
                       "p.p_unitprice , " +
                       "p.p_lauchdate , " +
                       "p.p_status , " +
                       "pic_path = STUFF(( " +
                              "SELECT ',' + dbo.Product_Picture.pp_path " +
                              "FROM dbo.Product_Picture " +
                             "where p.p_id = dbo.Product_Picture.p_id " +
                             "FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, ''),  " +
                        "d.d_discount, " +
                        "d.d_startdate, " +
                        "d.d_enddate " +
                    "FROM dbo.Product p " +
                    "INNER JOIN dbo.Product_Picture ON p.p_id = dbo.Product_Picture.p_id " +
                    "INNER JOIN dbo.Category c ON c.c_id = p.c_id " +
                    "INNER JOIN dbo.Discount d ON d.c_id = c.c_id " +
                    "where p.c_id = 2 " +
                    "and p.p_status = 0 " +
                    "AND DATEADD(HH,+8, GETDATE() ) BETWEEN d.d_startdate AND d.d_enddate ";


                var product = conn.Query(sql).ToList();
                ViewBag.p = product;
            }

            return View();
        }

        /*耳機分類頁*/
        public ActionResult Headset_Category()
        {
            using (conn)
            {
                //string sql =
                //             " SELECT " +
                //                 "   p.p_id, " +
                //                 "   p.p_name, " +
                //                 "   p.p_unitprice,  " +
                //                 "   p.p_lauchdate," +
                //                 "   p.p_status," +
                //                 "   ( " +
                //                         "SELECT TOP 1 pp.pp_path " +
                //                         "FROM dbo.Product_Picture pp " +
                //                         "WHERE pp.p_id = p.p_id " +
                //                     " ) AS pp_path " +
                //             " FROM dbo.Product p " +
                //             " WHERE p.c_id = 3 " +
                //             " AND p.p_status = 0 ";

                string sql =
                       "SELECT distinct " +
                       "p.p_id , " +
                       "p.p_name, " +
                       "p.p_unitprice , " +
                       "p.p_lauchdate , " +
                       "p.p_status , " +
                       "pic_path = STUFF(( " +
                              "SELECT ',' + dbo.Product_Picture.pp_path " +
                              "FROM dbo.Product_Picture " +
                             "where p.p_id = dbo.Product_Picture.p_id " +
                             "FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, ''),  " +
                        "d.d_discount, " +
                        "d.d_startdate, " +
                        "d.d_enddate " +
                    "FROM dbo.Product p " +
                    "INNER JOIN dbo.Product_Picture ON p.p_id = dbo.Product_Picture.p_id " +
                    "INNER JOIN dbo.Category c ON c.c_id = p.c_id " +
                    "INNER JOIN dbo.Discount d ON d.c_id = c.c_id " +
                    "where p.c_id = 3 " +
                    "and p.p_status = 0 " +
                    "AND DATEADD(HH,+8, GETDATE() ) BETWEEN d.d_startdate AND d.d_enddate ";


                var product = conn.Query(sql).ToList();
                ViewBag.p = product;
            }

            return View();
        }

        /*鍵盤產品頁*/
        public ActionResult Keyboard_item(int? id=0)
        {
            if (id == 0)
            {
                nowp = Convert.ToInt32(Session["nowproduct"].ToString());
                id = nowp;
            }

            List<Product> p_List = db.Product.Where((x) => x.p_id == id).ToList();
            List<Product_Detail> pd_List = db.Product_Detail.Where((x) => x.p_id == id).ToList();
            List<Product_Picture> pp_List = db.Product_Picture.Where((x) => x.p_id == id).ToList();
            List<Product_Feature> pf_List = db.Product_Feature.Where((x) => x.p_id == id).ToList();
            List<Classify> classift_List = db.Classify.Where((x) => x.p_id == id).ToList();
            ViewBag.p_List = p_List;
            ViewBag.pd_List = pd_List;
            ViewBag.pp_List = pp_List;
            ViewBag.pf_List = pf_List;
            ViewBag.classift_List = classift_List;

            Session["nowproduct"] = id;
            Session["Nowitem"] = "Keyboard_item";
            return View();
        }

        /*滑鼠產品頁*/
        public ActionResult Mouse_item(int? id = 0)
        {
            if (id == 0)
            {
                nowp = Convert.ToInt32(Session["nowproduct"].ToString());
                id = nowp;
            }

            List<Product> p_List = db.Product.Where((x) => x.p_id == id).ToList();
            List<Product_Detail> pd_List = db.Product_Detail.Where((x) => x.p_id == id).ToList();
            List<Product_Picture> pp_List = db.Product_Picture.Where((x) => x.p_id == id).ToList();
            List<Product_Feature> pf_List = db.Product_Feature.Where((x) => x.p_id == id).ToList();
            List<Classify> classift_List = db.Classify.Where((x) => x.p_id == id).ToList();
            ViewBag.p_List = p_List;
            ViewBag.pd_List = pd_List;
            ViewBag.pp_List = pp_List;
            ViewBag.pf_List = pf_List;
            ViewBag.classift_List = classift_List;

            Session["nowproduct"] = id;
            Session["Nowitem"] = "Mouse_item";
            return View();
        }

        /*耳機產品頁*/
        public ActionResult Headset_item(int? id = 0)
        {
            if (id == 0)
            {
                nowp = Convert.ToInt32(Session["nowproduct"].ToString());
                id = nowp;
            }
           

            List<Product> p_List = db.Product.Where(x => x.p_id == id).ToList();
            List<Product_Detail> pd_List = db.Product_Detail.Where((x) => x.p_id == id).ToList();
            List<Product_Picture> pp_List = db.Product_Picture.Where((x) => x.p_id == id).ToList();
            List<Product_Feature> pf_List = db.Product_Feature.Where((x) => x.p_id == id).ToList();
            List<Classify> classift_List = db.Classify.Where((x) => x.p_id == id).ToList();
            ViewBag.p_List = p_List;
            ViewBag.pd_List = pd_List;
            ViewBag.pp_List = pp_List;
            ViewBag.pf_List = pf_List;
            ViewBag.classift_List = classift_List;

            Session["nowproduct"] = id;
            Session["Nowitem"] = "Headset_item";
            return View();
        }


        [HttpPost]
        public ActionResult PassData(int id, int count)
        {
            //temp id count
            return View();
        }

        public ActionResult UserCart(int? num,int? id)
        {

            if(Session["m_id"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var userr = int.Parse(Session["m_id"].ToString());
                using (conn)
                {
                    if (num != null)
                    {
                        string sql = "SELECT *FROM dbo.Shopping_Cart WHERE dbo.Shopping_Cart.m_id =@mid  AND dbo.Shopping_Cart.pd_id =@pdid";
                        var Cart = conn.QuerySingleOrDefault(sql, new { mid = userr, pdid = id });

                        if (Cart != null)
                        {
                            num += Cart.cart_quantity;
                            string sqlUpdata = "Update dbo.Shopping_Cart SET cart_quantity=@Num WHERE dbo.Shopping_Cart.m_id=" + userr +
                                "AND dbo.Shopping_Cart.pd_id=" + Cart.pd_id;
                            conn.Execute(sqlUpdata, new { Num = num });
                        }
                        else
                        {
                            Shopping_Cart sc = new Shopping_Cart();
                            sc.m_id = userr;
                            sc.pd_id = id;
                            sc.cart_quantity = num;
                            db.Shopping_Cart.Add(sc);
                            db.SaveChanges();
                        }
                    }
                }

                string temp = Session["Nowitem"].ToString();
                int tempid = Convert.ToInt32(Session["nowproduct"].ToString());

                return RedirectToAction(Session["Nowitem"].ToString(), new { id = tempid });
            }

        }

    }
}