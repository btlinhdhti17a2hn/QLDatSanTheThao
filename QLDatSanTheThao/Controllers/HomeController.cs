using Microsoft.AspNetCore.Mvc;
using QLDatSanTheThao.Models;
using System.Diagnostics;

namespace QLDatSanTheThao.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult TestAdmin()
        {
            HttpContext.Session.SetString("VaiTro", "Admin");
            return RedirectToAction("Index", "DichVu");
        }
       
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
