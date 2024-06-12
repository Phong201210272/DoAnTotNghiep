using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
namespace DoAnTotNghiep.Controllers
{
    public class UserAppointmentController : Controller
    {
        QlphongKhamNhaKhoaContext db = new QlphongKhamNhaKhoaContext();

        [Route("userappointmentlist")]
        [HttpGet]
        public IActionResult UserAppointmentList(int? page, string statusFilter)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var username = HttpContext.Session.GetString("Username");
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                ViewBag.NoDataMessage = "No appointments found for the current user.";
                return View();
            }

            var userid = user.UserId;

            IQueryable<Appointment> appointments = db.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.User)
                .Where(a => a.UserId == userid);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                int status = int.Parse(statusFilter);
                appointments = appointments.Where(a => a.Status == status);
                ViewBag.SelectedStatus = statusFilter;
            }

            var pagedAppointments = appointments.ToPagedList(pageNumber, pageSize);

            return View(pagedAppointments);
        }

        [Route("userappointmentdetail")]
        [HttpGet]
        public IActionResult UserAppointmentDetail(string id)
        {
            var appointment = db.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .FirstOrDefault(a => a.AppointmentId == id);
            if(appointment == null)
            {
                return NotFound();
            }

            var user = db.Users.Find(appointment.UserId);
            ViewBag.UserName = user.Name;
            ViewBag.UserPhone = user.PhoneNumber;
            ViewBag.UserEmail = user.Email;

            return View(appointment);
        }

        [Route("edituserappointment")]
        [HttpGet]
        public IActionResult EditUserAppointment(string id)
        {
            var appointment = db.Appointments.Include(a => a.Doctor)
                .Include(a => a.Service)
                .Include(a => a.User)
                .FirstOrDefault(a => a.AppointmentId == id);

            if(appointment == null)
            {
                return NotFound();
            }

            ViewBag.UserName = appointment.User?.Name;
            ViewBag.UserPhone = appointment.User?.PhoneNumber;
            ViewBag.UserEmail = appointment.User?.Email;

            ViewBag.ServiceId = new SelectList(db.Services.ToList(), "ServiceId", "ServiceName", appointment.ServiceId);
            ViewBag.DoctorId = new SelectList(db.Doctors.ToList(), "DoctorId", "Name", appointment.DoctorId);

            return View(appointment);
        }

        [Route("edituserappointment")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUserAppointment(Appointment appointment)
        {
            // Kiểm tra xem đối tượng đã tồn tại trong cơ sở dữ liệu chưa
            var existingAppointment = db.Appointments.Find(appointment.AppointmentId);

            // Nếu không tìm thấy, trả về NotFound
            if (existingAppointment == null)
            {
                return NotFound();
            }

            // Cập nhật các thuộc tính của đối tượng đã tồn tại từ đối tượng mới
            existingAppointment.ServiceId = appointment.ServiceId;
            existingAppointment.DoctorId = appointment.DoctorId;

            // Lưu thay đổi vào cơ sở dữ liệu
            db.SaveChanges();

            // Cập nhật TempData
            TempData["SuccessMessage"] = "Appointment edited successfully";

            // Chuyển hướng đến action khác, ví dụ: UserAppointmentList
            return RedirectToAction("UserAppointmentList");
        }

        [Route("cancelappointment")]
        [HttpGet]
        public IActionResult CancelAppointment(string id)
        {
            var appointment = db.Appointments.FirstOrDefault(d => d.AppointmentId == id);
            db.Appointments.Remove(appointment);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Appointment canceled successfully";

            return RedirectToAction("UserAppointmentList");
        }
    }
}
