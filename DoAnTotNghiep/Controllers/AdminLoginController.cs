using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Mvc;

namespace DoAnTotNghiep.Controllers
{
	public class AdminLoginController : Controller
	{
		QlphongKhamNhaKhoaContext db = new QlphongKhamNhaKhoaContext();
		[HttpGet]
		public IActionResult AdminLogin()
		{
			if (HttpContext.Session.GetString("Username") == null)
			{
				return View();
			}
			else
			{
				return RedirectToAction("Admin", "HomeAdmin"); ;
			}
		}
		[HttpPost]
		public IActionResult AdminLogin(Admin admin)
		{
			if (HttpContext.Session.GetString("Username") == null)
			{
				var u = db.Admins.Where(x => x.Username == admin.Username
				&& x.Password == admin.Password).FirstOrDefault();
				if (u != null)
				{
					HttpContext.Session.SetString("Username", u.Username.ToString());
					string name = u.Name;
					ViewData["Name"] = name;
					return RedirectToAction("AdminIndex", "HomeAdmin");
				}
				else
				{
                    ViewBag.ErrorMessage = "Wrong username or password";
                }
			}
			return View(admin);
		}
		public IActionResult AdminLogout()
		{
			HttpContext.Session.Clear();
			HttpContext.Session.Remove("Username");
			return RedirectToAction("AdminLogin", "AdminLogin");
		}
	}
}
