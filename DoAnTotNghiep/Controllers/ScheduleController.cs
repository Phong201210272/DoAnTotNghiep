using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using System.Globalization;

namespace DoAnTotNghiep.Controllers
{
    public class ScheduleController : Controller
    {
        QlphongKhamNhaKhoaContext db = new QlphongKhamNhaKhoaContext();

        /*public ScheduleController(QlphongKhamNhaKhoaContext context)
        {
            db = context;
        }*/

        public IActionResult Index(string doctorId = "", DateTime? startDate = null, DateTime? endDate = null, bool clearFilter = false)
        {
            var doctors = db.Doctors.ToList();
            ViewBag.Doctors = new SelectList(doctors, "DoctorId", "Name");

            if (clearFilter)
            {
                // Nếu người dùng chọn xóa bộ lọc, đặt lại các giá trị ngày bắt đầu và kết thúc là null
                startDate = null;
                endDate = null;
            }

            List<DoctorScheduleViewModel> schedule = null;

            if (!string.IsNullOrEmpty(doctorId))
            {
                if (startDate.HasValue)
                {
                    if (!endDate.HasValue)
                    {
                        endDate = DateTime.MaxValue; // Ngày kết thúc mặc định là ngày cuối cùng trong chuỗi thời gian
                    }
                    schedule = GetWeeklySchedule(doctorId, startDate.Value, endDate.Value);
                }
                else
                {
                    schedule = GetWeeklySchedule(doctorId);
                }
            }

            ViewBag.SelectedDoctorId = doctorId;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return View(schedule);
        }


        private List<DoctorScheduleViewModel> GetWeeklySchedule(string doctorId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = db.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.User)
                .OrderBy(a => a.AppointmentTime)
                .Where(a => a.Status == 1);

            if (!string.IsNullOrEmpty(doctorId))
            {
                query = query.Where(a => a.DoctorId == doctorId);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(a => a.AppointmentTime >= startDate && a.AppointmentTime <= endDate);
            }

            var schedules = query.ToList().Select(a => new DoctorScheduleViewModel
            {
                DoctorId = a.Doctor.DoctorId,
                DoctorName = a.Doctor.Name,
                Date = a.AppointmentTime.Value.Date.ToString("dd/MM/yyyy"),
                Day = a.AppointmentTime.Value.DayOfWeek.ToString(),
                TimeSlot = a.AppointmentTime.Value.ToString("HH:mm"),
                UserName = a.User.Name,
                UserPhone = a.User.PhoneNumber,
                UserEmail = a.User.Email,
                WeekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(a.AppointmentTime.Value, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday),
                Year = a.AppointmentTime.Value.Year
            }).ToList();

            return schedules;
        }


        [HttpGet]
        public IActionResult GetScheduleByDoctor(string doctorId)
        {
            var doctorSchedule = GetWeeklySchedule(doctorId);
            return Json(doctorSchedule);
        }

        private List<DoctorScheduleViewModel> GetWeeklySchedule(string doctorId)
        {
            var query = db.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.User)
                .OrderBy(a => a.AppointmentTime)
                .Where(a => a.Status == 1);

            if (!string.IsNullOrEmpty(doctorId))
            {
                query = query.Where(a => a.DoctorId == doctorId);
            }

            var schedules = query.ToList().Select(a => new DoctorScheduleViewModel
            {
                DoctorId = a.Doctor.DoctorId,
                DoctorName = a.Doctor.Name,
                Date = a.AppointmentTime.Value.Date.ToString("dd/MM/yyyy"),
                Day = a.AppointmentTime.Value.DayOfWeek.ToString(),
                TimeSlot = a.AppointmentTime.Value.ToString("HH:mm"),
                UserName = a.User.Name,
                UserPhone = a.User.PhoneNumber,
                UserEmail = a.User.Email,
                WeekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(a.AppointmentTime.Value, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday),
                Year = a.AppointmentTime.Value.Year
            }).ToList();

            return schedules;
        }

        public class DoctorScheduleViewModel
        {
            public string DoctorId { get; set; }
            public string DoctorName { get; set; }
            public string Date { get; set; }
            public string Day { get; set; }
            public string TimeSlot { get; set; }
            public string UserName { get; set; }
            public string UserPhone { get; set; }
            public string UserEmail { get; set; }
            public int WeekOfYear { get; set; }
            public int Year { get; set; }
        }

        public IActionResult ExportToExcel(string doctorId = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            // Thiết lập ngữ cảnh giấy phép cho EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var schedules = startDate.HasValue && endDate.HasValue
                ? GetWeeklySchedule(doctorId, startDate.Value, endDate.Value)
                : GetWeeklySchedule(doctorId);

            // Lấy thông tin bác sĩ
            var doctor = db.Doctors.FirstOrDefault(d => d.DoctorId == doctorId);
            var doctorName = doctor != null ? doctor.Name : doctorId;

            var groupedByWeek = schedules
                .GroupBy(s => new { s.Year, s.WeekOfYear })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.WeekOfYear)
                .ToList();

            using (var excelPackage = new ExcelPackage())
            {
                foreach (var weekGroup in groupedByWeek)
                {
                    var firstDayOfWeek = FirstDateOfWeekISO8601(weekGroup.Key.Year, weekGroup.Key.WeekOfYear);
                    var lastDayOfWeek = firstDayOfWeek.AddDays(6);

                    var worksheetName = $"{firstDayOfWeek:dd/MM/yyyy} - {lastDayOfWeek:dd/MM/yyyy}";
                    var worksheet = excelPackage.Workbook.Worksheets.Add(worksheetName);

                    var daysOfWeek = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                    for (int i = 0; i < daysOfWeek.Count; i++)
                    {
                        worksheet.Cells[1, i + 2].Value = daysOfWeek[i];
                    }

                    var timeSlots = new List<string> { "Morning", "Afternoon", "Evening" };
                    for (int i = 0; i < timeSlots.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = timeSlots[i];
                    }

                    foreach (var schedule in weekGroup)
                    {
                        var time = TimeSpan.Parse(schedule.TimeSlot);
                        string timeSlot;
                        if (time < TimeSpan.FromHours(12))
                            timeSlot = "Morning";
                        else if (time < TimeSpan.FromHours(18))
                            timeSlot = "Afternoon";
                        else
                            timeSlot = "Evening";

                        int row = timeSlots.IndexOf(timeSlot) + 2;
                        int column = daysOfWeek.IndexOf(schedule.Day) + 2;

                        worksheet.Cells[row, column].Value = $"{schedule.UserName}\n  {schedule.TimeSlot}";
                    }

                    worksheet.Cells.AutoFitColumns();
                }

                var fileContents = excelPackage.GetAsByteArray();
                var fileName = $"WeeklySchedule_{doctorName}.xlsx";
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }



        private static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }

    }
}