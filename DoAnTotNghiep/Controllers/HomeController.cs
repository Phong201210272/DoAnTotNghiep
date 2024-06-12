using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DoAnTotNghiep.Models.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoAnTotNghiep.Controllers
{
    public class HomeController : Controller
    {
        QlphongKhamNhaKhoaContext db = new QlphongKhamNhaKhoaContext();
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [Authentication]
        public IActionResult Index()
        {
            var listdentist = db.Doctors.ToList();
            return View(listdentist);
        }
        [Authentication]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public string GenerateNewAppointmentId()
        {
            List<string> allAppointmentIds = db.Appointments.Select(d => d.AppointmentId).ToList();

            string newAppointmentId = null;

            if (allAppointmentIds.Count > 0)
            {
                for (int i = 0; i < allAppointmentIds.Count - 1; i++)
                {
                    int currentNumber = int.Parse(allAppointmentIds[i].Substring(2));
                    int nextNumber = int.Parse(allAppointmentIds[i + 1].Substring(2));

                    if (nextNumber - currentNumber > 1)
                    {
                        newAppointmentId = "AP" + (currentNumber + 1).ToString("D3");
                        break;
                    }
                }
                if (string.IsNullOrEmpty(newAppointmentId))
                {
                    int maxNumber = int.Parse(allAppointmentIds.Last().Substring(2));
                    newAppointmentId = "AP" + (maxNumber + 1).ToString("D3");
                }
            }
            else
            {
                newAppointmentId = "AP001";
            }
            return newAppointmentId;
        }
        [Authentication]
        [Route("NewAppointment")]
        [HttpGet]
        public IActionResult NewAppointment()
        {
            ViewBag.ServiceId = new SelectList(db.Services.ToList(), "ServiceId", "ServiceName");
            ViewBag.DoctorId = new SelectList(db.Doctors.ToList(), "DoctorId", "Name");
            return View();
        }
        [Route("NewAppointment")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewAppointment(Appointment newAppointment)
        {
            string username = HttpContext.Session.GetString("Username");

            // Truy vấn CSDL để lấy UserId của người dùng có Username tương ứng
            var user = db.Users.FirstOrDefault(x => x.Username == username);
            string userId = user.UserId;
            string appointmentid = GenerateNewAppointmentId();
            newAppointment.AppointmentId = appointmentid;
            newAppointment.Status = 0;
            newAppointment.CreateDate = DateTime.Now;
            newAppointment.UserId = userId;
            db.Appointments.Add(newAppointment);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Appointment created successfully";
            return RedirectToAction("Index");

        }
        [Authentication]
        [Route("profile")]
        [HttpGet]
        public IActionResult Profile()
        {
            string username = HttpContext.Session.GetString("Username");
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            ViewBag.GenderList = new List<SelectListItem>
            {
                new SelectListItem {Text="Male", Value = "Male"},
                new SelectListItem {Text="Female", Value = "Female"}
            };
            return View(user);
        }
        [Authentication]
        [Route("profile")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(User user)
        {
            ViewBag.GenderList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Male", Value = "Male" },
                new SelectListItem { Text = "Female", Value = "Female" }
            };
            if (!IsValidEmail(user.Email))
            {
                ModelState.AddModelError("Email", "Invalid email format");
                return View(user);
            }
            if (!IsValidPhoneNumber(user.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Phone number must have 10 digits and all characters must be number");
                return View(user);
            }
            try
            {
                db.Update(user);
                db.SaveChanges();
                HttpContext.Session.SetString("Username", user.Username);
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error");
            }

            return View(user);
        }
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            return phoneNumber != null && phoneNumber.Length == 10 && phoneNumber.All(char.IsDigit);
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        [Route("sercicelist")]
        [HttpGet]
        public IActionResult ServiceList()
        {
            var lstService = db.Services.ToList();
            ViewBag.Services = lstService;
            return View();
        }
    }
}
