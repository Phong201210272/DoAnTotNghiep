using Microsoft.AspNetCore.Mvc;
using DoAnTotNghiep.Models;
using Microsoft.Identity.Client;

namespace DoAnTotNghiep.Controllers
{
	public class UserLoginController : Controller
	{
		QlphongKhamNhaKhoaContext db = new QlphongKhamNhaKhoaContext();
		[HttpGet]
		public IActionResult Login()
		{
			if (HttpContext.Session.GetString("Username") == null)
			{
				return View();
			}
			else
			{
				return RedirectToAction("Index", "Home");
			}
		}

		[HttpPost]
		public IActionResult Login(User user)
		{
			if(HttpContext.Session.GetString("Username") == null)
			{
				var u = db.Users.Where(x => x.Username.Equals(user.Username)
				&& x.Password.Equals(user.Password)).FirstOrDefault();
				if(u!=null)
				{
					HttpContext.Session.SetString("Username", u.Username.ToString());
					return RedirectToAction("Index", "Home");
				}
				else
				{
					ViewBag.ErrorMessage = "Wrong username or password";
				}
			}
			return View(user);
		}
		public IActionResult Logout()
		{
			HttpContext.Session.Clear();
			HttpContext.Session.Remove("Username");
			return RedirectToAction("Login", "UserLogin");
		}
	}
}
