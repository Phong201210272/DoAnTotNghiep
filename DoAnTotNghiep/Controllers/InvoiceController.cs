using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Mvc;
using DoAnTotNghiep.Models.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System;
using System.IO;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Controllers
{
    [Authentication]
    public class InvoiceController : Controller
    {
        QlphongKhamNhaKhoaContext db = new QlphongKhamNhaKhoaContext();
        public class InvoiceDetailViewModel
        {
            public string BillId { get; set; }
            public string AdminName { get; set; }
            public string UserName { get; set; }
            public DateTime CreateDate { get; set; }
            public string ServiceName { get; set; }
            public string DoctorName { get; set; }
            public decimal Price { get; set; }
        }
        public IActionResult ExportInvoiceDetailsToExcel(string billId)
        {
            // Set the license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var bill = db.Bills
                .Include(b => b.Admin)
                .Include(b => b.User)
                .Include(b => b.BillDetails)
                    .ThenInclude(d => d.Service)
                .Include(b => b.BillDetails)
                    .ThenInclude(d => d.Doctor)
                .FirstOrDefault(b => b.BillId == billId);

            if (bill == null)
            {
                return NotFound(); // Handle the case where the bill is not found
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Invoice");

                worksheet.Cells["A1"].Value = "Invoice ID";
                worksheet.Cells["B1"].Value = bill.BillId;
                worksheet.Cells["A2"].Value = "Create Date";
                worksheet.Cells["B2"].Value = bill.CreateDate.ToString("dd/MM/yyyy");
                worksheet.Cells["A3"].Value = "Total (VND)";
                worksheet.Cells["B3"].Value = bill.Total;
                worksheet.Cells["A4"].Value = "Admin Name";
                worksheet.Cells["B4"].Value = bill.Admin?.Name;
                worksheet.Cells["A5"].Value = "User Name";
                worksheet.Cells["B5"].Value = bill.User?.Name;

                worksheet.Cells["A7"].Value = "Service Name";
                worksheet.Cells["B7"].Value = "Doctor Name";
                worksheet.Cells["C7"].Value = "Price";

                int row = 8;
                foreach (var detail in bill.BillDetails)
                {
                    worksheet.Cells[row, 1].Value = detail.Service.ServiceName;
                    worksheet.Cells[row, 2].Value = detail.Doctor.Name;
                    worksheet.Cells[row, 3].Value = detail.Bill.Total;
                    row++;
                }

                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                // Return the file as a FileResult
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Invoice_{bill.BillId}.xlsx");
            }
        }


        private string GenerateNewInvoiceId()
        {
            List<string> allInvoiceIds = db.Bills.Select(d => d.BillId).ToList();
            string newInvoiceId = null;
            if (allInvoiceIds.Count > 0)
            {
                for (int i = 0; i < allInvoiceIds.Count - 1; i++)
                {
                    int currentNumber = int.Parse(allInvoiceIds[i].Substring(1));
                    int nextNumber = int.Parse(allInvoiceIds[i + 1].Substring(1));
                    if (nextNumber - currentNumber > 1)
                    {
                        newInvoiceId = "B" + (currentNumber + 1).ToString("D3");
                        break;
                    }
                }
                if (string.IsNullOrEmpty(newInvoiceId))
                {
                    int MaxNumber = int.Parse(allInvoiceIds.Last().Substring(1));
                    newInvoiceId = "B" + (MaxNumber + 1).ToString("D3");
                }
            }
            else
            {
                newInvoiceId = "B001";
            }
            return newInvoiceId;
        }

        [Route("newinvoice")]
        [HttpGet]
        public IActionResult NewInvoice(string id)
        {
            var appointment = db.Appointments
                                 .Where(a => a.AppointmentId == id && a.Status != 2)
                                 .Select(a => new
                                 {
                                     UserName = a.User.Name,
                                     UserEmail = a.User.Email,
                                     UserPhone = a.User.PhoneNumber,
                                     ServiceName = a.Service.ServiceName,
                                     DoctorName = a.Doctor.Name,
                                     Price = a.Service.Cost,
                                     a.User.UserId,
                                     a.Service.ServiceId,
                                     a.AppointmentId,
                                     a.Doctor.DoctorId
                                 })
                                 .FirstOrDefault();

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment không tồn tại hoặc đã hoàn thành.";
                return RedirectToAction("AppointmentManagement", "Appointment");
            }

            ViewBag.UserName = appointment.UserName;
            ViewBag.UserEmail = appointment.UserEmail;
            ViewBag.UserPhone = appointment.UserPhone;
            ViewBag.ServiceName = appointment.ServiceName;
            ViewBag.DoctorName = appointment.DoctorName;
            ViewBag.Price = appointment.Price;
            ViewBag.ServiceId = appointment.ServiceId;
            ViewBag.UserId = appointment.UserId;
            ViewBag.AppointmentId = appointment.AppointmentId;
            ViewBag.DoctorId = appointment.DoctorId;

            return View();
        }



        [Route("newinvoice")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewInvoice(Bill newBill, BillDetail newBillDetail)
        {
            string username = HttpContext.Session.GetString("Username");

            var admin = db.Admins.FirstOrDefault(x => x.Username == username);
            if (admin == null)
            {
                return Unauthorized();
            }

            string adminId = admin.AdminId;
            string billId = GenerateNewInvoiceId();

            newBill.BillId = billId;
            newBill.CreateDate = DateTime.Now;
            newBill.AdminId = adminId;

            // Assign the BillId to BillDetail
            newBillDetail.BillId = billId;

            var appointment = db.Appointments.FirstOrDefault(a => a.AppointmentId == newBill.AppointmentId);
            if (appointment == null || appointment.Status == 2)
            {
                TempData["ErrorMessage"] = "Appointment không tồn tại hoặc đã hoàn thành.";
                return View(newBill); // Trả về View với dữ liệu mới nhập
            }

            appointment.Status = 2;

            db.Bills.Add(newBill);
            db.BillDetails.Add(newBillDetail);
            db.Appointments.Update(appointment);

            try
            {
                db.SaveChanges();
                TempData["SuccessMessage"] = "Payment is completed";
                return RedirectToAction("InvoiceManagement", "Invoice");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
                return View(newBill); // Trả về View với dữ liệu mới nhập
            }
        }
        [Route("invoicelist")]
        [HttpGet]
        public IActionResult InvoiceManagement()
        {
            var lstInvoice = db.Bills.ToList();

            ViewBag.UserInformationDict = lstInvoice
                .Select(b => b.UserId)
                .Distinct()
                .ToDictionary(userId => userId, userId => GetUserInformation(userId));

            ViewBag.AdminInformationDict = lstInvoice
                .Select(b => b.AdminId)
                .Distinct()
                .ToDictionary(adminId => adminId, adminId => GetAdminInformation(adminId));

            var billDetails = lstInvoice
                .Select(b => GetBillDetail(b.BillId))
                .ToDictionary(detail => detail.BillId, detail => detail);

            var doctorInformationDict = lstInvoice
                .Select(b => b.AppointmentId)
                .Distinct()
                .ToDictionary(appointmentId => appointmentId, appointmentId => GetDoctorInformationFromAppointment(appointmentId));

            ViewBag.BillDetailsDict = billDetails;
            ViewBag.DoctorInformationDict = doctorInformationDict;

            return View(lstInvoice);
        }

        private User GetUserInformation(string userId)
        {
            return db.Users.FirstOrDefault(u => u.UserId == userId);
        }

        private Admin GetAdminInformation(string adminId)
        {
            return db.Admins.FirstOrDefault(u => u.AdminId == adminId);
        }

        private BillDetail GetBillDetail(string billId)
        {
            return db.BillDetails.FirstOrDefault(d => d.BillId == billId);
        }
        private string GetDoctorName(QlphongKhamNhaKhoaContext db, string doctorId)
        {
            var doctor = db.Doctors.AsNoTracking().FirstOrDefault(d => d.DoctorId == doctorId);
            return doctor != null ? doctor.Name : "";
        }

        private string GetServiceName(QlphongKhamNhaKhoaContext db, string serviceId)
        {
            var service = db.Services.AsNoTracking().FirstOrDefault(s => s.ServiceId == serviceId);
            return service != null ? service.ServiceName : "";
        }
        /* private static string GetServiceName(QlphongKhamNhaKhoaContext db, string serviceId)
         {
             var service = db.Services.AsEnumerable().FirstOrDefault(s => s.ServiceId == serviceId);
             return service != null ? service.ServiceName : "";

         }
         private static string GetDoctorName(QlphongKhamNhaKhoaContext db, string doctorId)
         {
             var doctor = db.Doctors.FirstOrDefault(d => d.DoctorId == doctorId);
             return doctor != null ? doctor.Name : "";
         }*/
        private Doctor GetDoctorInformationFromAppointment(string appointmentId)
        {
            var appointment = db.Appointments.FirstOrDefault(a => a.AppointmentId == appointmentId);
            return appointment != null ? db.Doctors.FirstOrDefault(d => d.DoctorId == appointment.DoctorId) : null;
        }
    }
}
