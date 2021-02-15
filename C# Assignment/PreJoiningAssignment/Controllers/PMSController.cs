using PMS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using PagedList;
using NLog;

namespace PMS.MVC.Controllers
{
    public class PMSController : Controller
    {

        private readonly HttpClient _client;
        readonly Logger loggerFile;
        readonly Logger loggerDB;
        public PMSController()
        {
            _client = new HttpClient();
            loggerFile = LogManager.GetCurrentClassLogger();
            loggerDB = LogManager.GetLogger("databaseLogger");
        }

        // GET: Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        } 

        // POST: Login
        [HttpPost]
        public ActionResult Login(UserData model)
        {
            var task = _client.PostAsJsonAsync<UserData>("https://localhost:44379/api/User/PostAuthenticateUser/", model);
            task.Wait();

            var result = task.Result.Content.ReadAsAsync<List<String>>();
            result.Wait();

            var check = result.Result;
            if (check != null)
            {
                HttpContext.Session.Add("userName", check[0]);
                HttpContext.Session.Add("id", check[1]);
                loggerFile.Info(check[0] + " Logged In.");
                loggerDB.Info(check[1] + " Logged In.");
                return RedirectToAction("Index");
            }
            else
            {
                TempData["LoginMessage"] = "Email Or Password Is Wrong!";
            }
            return View();
        }

        // GET: CreateUser
        [HttpGet]
        public ActionResult CreateUser()
        {
            return View();
        }
        
        // POST: CreateUser
        [HttpPost]
        public ActionResult CreateUser(UserData model)
        {
            var task = _client.PostAsJsonAsync<UserData>("https://localhost:44379/api/User/PostCreateUser", model);
            task.Wait();

            var result = task.Result.Content.ReadAsAsync<List<string>>();
            result.Wait();

            var check = result.Result;
            if(check.Count == 0)
            {
                TempData["Error"] = check;
            }
            else
            {
                HttpContext.Session.Add("userName", check[0]);
                HttpContext.Session.Add("id", check[1]);
                loggerFile.Info(check[0] + " Created And Logged In.");
                loggerDB.Info(check[1] + " Created And Logged In.");
                return RedirectToAction("Index");
            }
            return View();
        }

        // GET: Index or Dashboard
        [HttpGet]
        public ActionResult Index()
        {
            if (HttpContext.Session["userName"] == null && HttpContext.Session["id"] == null)
            {
                TempData["LoginMessage"] = "Please Login";
                return View("Login");
            }
            return View();
        }

        // GET: AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            if (HttpContext.Session["userName"] == null && HttpContext.Session["id"] == null)
            {
                TempData["LoginMessage"] = "Please Login";
                return View("Login");
            }
            return View();
        }

        // POST: AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductData model,HttpPostedFileBase smallFile, HttpPostedFileBase largeFile)
        {
            model.UserId = Convert.ToInt32(HttpContext.Session[1]);
            string smallImageExtension = Path.GetExtension(smallFile.FileName);
            string smallImageFilename = model.UserId + model.ProductName + smallImageExtension;
            model.SmallImage = "~/Images/SmallImages/" + smallImageFilename;
            string smallImagepath = Path.Combine(Server.MapPath("~/Images/SmallImages/"), smallImageFilename);
            string largeImageExtension = Path.GetExtension(largeFile.FileName);
            string largeImageFilename = model.UserId + model.ProductName + largeImageExtension;
            model.LongImage = "~/Images/LargeImages" + smallImageFilename;
            string largeImagepath = Path.Combine(Server.MapPath("~/Images/LargeImages/"), largeImageFilename);
            model.smallFile = null;
            model.largeFile = null;
            var task = _client.PostAsJsonAsync<ProductData>("https://localhost:44379/api/Product/PostCreateProduct", model);
            task.Wait();

            var result = task.Result.Content.ReadAsStringAsync();
            result.Wait();
            if(result.Result.Contains("Successfully Added!"))
            {
                smallFile.SaveAs(smallImagepath);
                largeFile.SaveAs(largeImagepath);
                loggerFile.Info(model.ProductName + " Added.");
                loggerDB.Info(model.ProductName + " Added.");
                return RedirectToAction("Index");
            }
            return View();
        }
       
        // GET: Specific Product By Id
        public ActionResult ProductById(int id)
        {
            if (HttpContext.Session["userName"] == null && HttpContext.Session["id"] == null)
            {
                TempData["LoginMessage"] = "Please Login";
                return View("Login");
            }
            ProductData product = null;
            var task = _client.GetAsync("https://localhost:44379/api/Product/GetProduct?id=" + id);
            task.Wait();
            var result = task.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsStringAsync().Result;
                product = JsonConvert.DeserializeObject<ProductData>(readTask);

            }
            return View(product);
        }

      

        // GET: Products List
        public ActionResult Products(int? page, string Sorting_Order, string searchBy, string search)
        {
            if (HttpContext.Session["userName"] == null && HttpContext.Session["id"] == null)
            {
                TempData["LoginMessage"] = "Please Login First!";
                return View("Login");
            }
            IEnumerable<ProductData> products = null;//For storing retrived data
            ViewBag.CurrentSortOrder = Sorting_Order;//Sorting order
            ViewBag.SortingName = String.IsNullOrEmpty(Sorting_Order) ? "Name_Description" : "";//Sort By Name
            ViewBag.SortingPrice = Sorting_Order == "Price_Low_To_High" ? "Price_High_To_Low" : "Price_Low_To_High";//Sort By Price

            var responseTask = _client.GetAsync("https://localhost:44379/api/Product/GetAllProducts?id=" + HttpContext.Session["id"] + "&Sorting_Order=" + Sorting_Order + "&searchBy=" + searchBy + "&search=" + search);
            responseTask.Wait();

            var result = responseTask.Result;
            var readTask = result.Content.ReadAsAsync<IList<ProductData>>();
            readTask.Wait();

            products = readTask.Result;
            if (products.Count()!=0)
            {
                return View(products.ToList().ToPagedList(page ?? 1, 6));
            }
            else
            {
                TempData["EmptyProduct"] = "Please Add Products First!";
                return View("Index");
            }
        }

        // Delete Specific Product
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            if (HttpContext.Session["userName"] == null && HttpContext.Session["id"] == null)
            {
                TempData["LoginMessage"] = "Please Login First!";
                return View("Login");
            }
            var task = _client.DeleteAsync("https://localhost:44379/api/Product/DeleteProduct?id=" + id);
            task.Wait();
            var taskResult = task.Result.Content.ReadAsStringAsync();
            taskResult.Wait();
            if (taskResult.Result.Contains("Deleted!"))
            {
                loggerFile.Info(id + " Deleted");
                loggerDB.Info(id + " Deleted");
                
                return Content("<script language='javascript' type='text/javascript'>alert('" + id + " Deleted Successfully!');window.location.href = '/PMS/Products';</script>");//return view with product data
            }
            else
            {
                 return Content("<script language='javascript' type='text/javascript'>alert('Something Went Wrong!');window.location.href = '/PMS/Product';</script>");
            }
        }

        //Logout Get Method
        [HttpGet]
        public ActionResult Logout()
        {
            loggerFile.Info(Session["UserName"] + " Logged out at" + DateTime.Now.ToString());
            loggerDB.Info(Session["UserName"] + " Logged out at" + DateTime.Now.ToString());
            HttpContext.Session.Clear();
            return Content("<script language='javascript' type='text/javascript'>alert('Log Out Successfully!');window.location.href = '/PMS/Login';</script>");
        }
    }
}
