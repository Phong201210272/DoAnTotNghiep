using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoAnTotNghiep.Controllers
{
    public class RegisterController : Controller
    {
        QlphongKhamNhaKhoaContext db = new QlphongKhamNhaKhoaContext();
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            return phoneNumber != null && phoneNumber.Length == 10 && phoneNumber.All(char.IsDigit);
        }
        private bool IsValidEmail(string email)
        {
            return email != null && email.Contains("@");
        }
        private string GenerateNewUserId()
        {
            List<string> allUserIds = db.Users.Select(d => d.UserId).ToList();
            string newUserId = null;
            if (allUserIds.Count > 0)
            {
                for (int i = 0; i < allUserIds.Count - 1; i++)
                {
                    int currentNumber = int.Parse(allUserIds[i].Substring(1));
                    int nextNumber = int.Parse(allUserIds[i + 1].Substring(1));
                    if (nextNumber - currentNumber > 1)
                    {
                        newUserId = "U" + (currentNumber + 1).ToString("D3");
                        break;
                    }
                }
                if(string.IsNullOrEmpty(newUserId))
                {
                    int maxNumber = int.Parse(allUserIds.Last().Substring(1));
                    newUserId = "U" + (maxNumber + 1).ToString("D3");
                }
            }
            else
            {
                newUserId = "U001";
            }
            return newUserId;
        }
        private string GenerateNewAdminId()
        {
            List<string> allAdminIds = db.Admins.Select(d=>d.AdminId).ToList();
            string newAdminId = null;
            if(allAdminIds.Count > 0)
            {
                for(int i=0; i<allAdminIds.Count - 1; i++)
                {
                    int currentNumber = int.Parse(allAdminIds[i].Substring(1));
                    int nextNumber = int.Parse(allAdminIds[i + 1].Substring(1));
                    if(nextNumber - currentNumber > 1)
                    {
                        newAdminId = "A" + (currentNumber + 1).ToString("D3");
                        break;
                    }
                }
                if (string.IsNullOrEmpty(newAdminId))
                {
                    int MaxNumber = int.Parse(allAdminIds.Last().Substring(1));
                    newAdminId = "A" + (MaxNumber + 1).ToString("D3");
                }
            }
            else
            {
                newAdminId = "A001";
            }
            return newAdminId;
        }
        [Route("admin/register")]
        [HttpGet]
        public IActionResult AdminRegister()
        {
            return View();
        }

        [Route("admin/register")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminRegister(Admin admin)
        {
            if (db.Admins.Any(d => d.Username == admin.Username))
            {
                ModelState.AddModelError("Username", "This username already exists");
                return View("AdminRegister", admin);
            }

            try
            {
                string newAdminId = GenerateNewAdminId();
                admin.AdminId = newAdminId;
                db.Admins.Add(admin);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Admin signed up successfully";
                return RedirectToAction("AdminLogin", "AdminLogin");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View("AdminRegister", admin);
            }
        }

        [Route("register")]
        [HttpGet]
        public IActionResult UserRegister()
        {
            ViewBag.GenderList = new List<SelectListItem>
            {
                new SelectListItem {Text = "Male", Value="Male"},
                new SelectListItem {Text = "Female",Value = "Female"}
            };
            return View();
        }

        [Route("register")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UserRegister(User user)
        {
            ViewBag.GenderList = new List<SelectListItem>
            {
                new SelectListItem {Text = "Male", Value ="Male"},
                new SelectListItem {Text = "Female", Value = "Female"}
            };
            if(!IsValidEmail(user.Email))
            {
                ModelState.AddModelError("Email", "Invalid Email format");
                return View("UserRegister", user);
            }
            if(!IsValidPhoneNumber(user.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Phone number must has 10 digits and all characters must be number");
                return View("UserRegister", user);
            }
            if(db.Users.Any(d=>d.Email == user.Email))
            {
                ModelState.AddModelError("Email", "This email is already exist");
                return View("UserRegister", user);
            }
            if(db.Users.Any(d => d.PhoneNumber == user.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "This phone number is already exist");
                return View("UserRegister", user);
            }
            if(db.Users.Any(d=>d.Username == user.Username))
            {
                ModelState.AddModelError("Username", "This username is already exist");
                return View("UserRegister", user);
            }
            try
            {
                string newUserId = GenerateNewUserId();
                user.UserId = newUserId;
                db.Users.Add(user);
                db.SaveChanges();
                TempData["SuccessMessage"] = "User signed up successfully";
                return RedirectToAction("Login", "UserLogin");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error" + ex.Message);
                return View("UserRegister", user);
            }
        }
    }
}
