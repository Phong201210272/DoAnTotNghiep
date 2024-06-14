using DoAnTotNghiep.Models;
using DoAnTotNghiep.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using X.PagedList;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DoAnTotNghiep.Controllers
{
    [Authentication]
    public class AppointmentController : Controller
    {
        private readonly QlphongKhamNhaKhoaContext db = new QlphongKhamNhaKhoaContext();

        private User GetUserInformation(string userId)
        {
            return db.Users.FirstOrDefault(u => u.UserId == userId);
        }

        [Route("appointmentdetail")]
        [HttpGet]
        public IActionResult AppointmentDetail(string id)
        {
            var appointment = db.Appointments
                                .Include(a => a.Doctor)  // Include Doctor entity
                                .Include(a => a.Service) // Include Service entity
                                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            var user = db.Users.Find(appointment.UserId);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.UserName = user.Name;
            ViewBag.UserPhone = user.PhoneNumber;
            ViewBag.UserEmail = user.Email;

            return View(appointment);
        }

        [Route("appointmentlist")]
        public IActionResult AppointmentManagement(int? page, string statusFilter, string phoneFilter, string dateFilter)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var appointments = db.Appointments.AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                int status = int.Parse(statusFilter);
                appointments = appointments.Where(a => a.Status == status);
                ViewBag.SelectedStatus = statusFilter;
            }
            if (!string.IsNullOrEmpty(dateFilter))
            {
                if (DateTime.TryParse(dateFilter, out DateTime date))
                {
                    appointments = appointments.Where(a => a.AppointmentTime.HasValue && a.AppointmentTime.Value.Date == date.Date);
                    ViewBag.DateFilter = dateFilter;
                }
                else
                {
                    ModelState.AddModelError("", "Invalid date format.");
                }
            }

            if (!string.IsNullOrEmpty(phoneFilter))
            {
                appointments = appointments.Where(a => a.User.PhoneNumber.Contains(phoneFilter));
                ViewBag.PhoneFilter = phoneFilter;
            }
            var pagedAppointments = appointments.ToPagedList(pageNumber, pageSize);
            ViewBag.UserInformationDict = pagedAppointments
                .Select(a => a.UserId)
                .Distinct()
                .ToDictionary(userId => userId, userId => GetUserInformation(userId));
            return View(pagedAppointments);
        }

        [Route("editappointment")]
        [HttpGet]
        public IActionResult EditAppointment(string appointmentid)
        {
            var appointment = db.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .Include(a => a.User) // Include User to get the related user info
                .FirstOrDefault(a => a.AppointmentId == appointmentid);

            if (appointment == null)
            {
                return NotFound();
            }

            var relatedAppointments = db.Appointments
                .Where(a => a.Status == 1 && a.AppointmentId != appointmentid)
                .Include(a => a.User)
                .Include(a => a.Service)
                .ToList();

            /*ViewBag.PatientName = appointment.PatientName;
            ViewBag.UserPhone = appointment.User?.PhoneNumber;
            ViewBag.UserEmail = appointment.User?.Email;*/
            ViewBag.RelatedAppointments = relatedAppointments;

            ViewBag.ServiceId = new SelectList(db.Services.ToList(), "ServiceId", "ServiceName", appointment.ServiceId);
            ViewBag.DoctorId = new SelectList(db.Doctors.ToList(), "DoctorId", "Name", appointment.DoctorId);

            ViewBag.Doctors = db.Doctors.ToList(); // Get all doctors for the filter

            return View(appointment);
        }

        [Route("editappointment")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAppointment(Appointment appointment)
        {
            var relatedAppointments = db.Appointments
                .Where(a => a.Status == 1 && a.AppointmentId != appointment.AppointmentId)
                .Include(a => a.User)
                .Include(a => a.Service)
                .ToList();

            ViewBag.RelatedAppointments = relatedAppointments;

            var existingAppointment = db.Appointments.Find(appointment.AppointmentId);
            if (existingAppointment == null)
            {
                return NotFound();
            }

            // Kiểm tra xem có lịch hẹn nào khác với cùng bác sĩ và thời gian đã tồn tại không
            var duplicateAppointment = db.Appointments.Any(a =>
                a.DoctorId == appointment.DoctorId &&
                a.AppointmentTime == appointment.AppointmentTime &&
                a.AppointmentId != appointment.AppointmentId);

            if (duplicateAppointment)
            {
                ModelState.AddModelError("", "An appointment with the same doctor at the same time already exists.");
                // Re-populate dropdown lists
                ViewBag.ServiceId = new SelectList(db.Services.ToList(), "ServiceId", "ServiceName", appointment.ServiceId);
                ViewBag.DoctorId = new SelectList(db.Doctors.ToList(), "DoctorId", "Name", appointment.DoctorId);
                ViewBag.Doctors = db.Doctors.ToList(); // Get all doctors for the filter

                // Re-populate user information
                var user = db.Users.Find(appointment.UserId);
                if (user != null)
                {
                    ViewBag.UserName = user.Name;
                    ViewBag.UserPhone = user.PhoneNumber;
                    ViewBag.UserEmail = user.Email;
                }

                return View(appointment);
            }

            existingAppointment.AppointmentTime = appointment.AppointmentTime;
            existingAppointment.ServiceId = appointment.ServiceId;
            existingAppointment.DoctorId = appointment.DoctorId;
            existingAppointment.Status = 1;

            db.Update(existingAppointment);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Appointment updated successfully.";
            return RedirectToAction("AppointmentManagement");
        }


        [Route("removeappointment")]
        [HttpGet]
        public IActionResult RemoveAppointment(string Appointmentid)
        {
            var AppointmentToRemove = db.Appointments.FirstOrDefault(d => d.AppointmentId == Appointmentid);

            if (AppointmentToRemove != null)
            {
                db.Appointments.Remove(AppointmentToRemove);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Appointment removed successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Appointment not found";
            }

            return RedirectToAction("AppointmentManagement");
        }

    }
}
