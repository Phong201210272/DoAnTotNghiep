﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using DoAnTotNghiep.Models.Authentication;
using Microsoft.Identity.Client;
using Microsoft.EntityFrameworkCore;
namespace DoAnTotNghiep.Controllers
{
    [Route("admin")]
    [Authentication]
    public class HomeAdminController : Controller
    {
        QlphongKhamNhaKhoaContext db = new QlphongKhamNhaKhoaContext();
        private readonly IWebHostEnvironment webHostEnvironment;

        public HomeAdminController(IWebHostEnvironment webHostEnvironment)
        {
            webHostEnvironment = webHostEnvironment;
        }
        private string GenerateNewServiceId()
        {
            List<string> allServiceIds = db.Services.Select(d => d.ServiceId).ToList();
            string newServiceId = null;
            if (allServiceIds.Count > 0)
            {
                for (int i = 0; i < allServiceIds.Count - 1; i++)
                {
                    int currentNumber = int.Parse(allServiceIds[i].Substring(1));
                    int nextNumber = int.Parse(allServiceIds[i + 1].Substring(1));
                    if (nextNumber - currentNumber > 1)
                    {
                        newServiceId = "S" + (currentNumber + 1).ToString("D3");
                        break;
                    }
                }
                if (string.IsNullOrEmpty(newServiceId))
                {
                    int MaxNumber = int.Parse(allServiceIds.Last().Substring(1));
                    newServiceId = "S" + (MaxNumber + 1).ToString("D3");
                }
            }
            else
            {
                newServiceId = "S001";
            }
            return newServiceId;
        }
        private string GenerateNewDoctorId()
        {
            List<string> allDoctorIds = db.Doctors.Select(d => d.DoctorId).ToList();

            string newDoctorId = null;

            if (allDoctorIds.Count > 0)
            {
                for (int i = 0; i < allDoctorIds.Count - 1; i++)
                {
                    int currentNumber = int.Parse(allDoctorIds[i].Substring(1));
                    int nextNumber = int.Parse(allDoctorIds[i + 1].Substring(1));

                    if (nextNumber - currentNumber > 1)
                    {
                        newDoctorId = "D" + (currentNumber + 1).ToString("D3");
                        break;
                    }
                }

                if (string.IsNullOrEmpty(newDoctorId))
                {
                    int maxNumber = int.Parse(allDoctorIds.Last().Substring(1));
                    newDoctorId = "D" + (maxNumber + 1).ToString("D3");
                }
            }
            else
            {
                newDoctorId = "D001";
            }

            return newDoctorId;
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            return phoneNumber != null && phoneNumber.Length == 10 && phoneNumber.All(char.IsDigit);
        }

        private bool IsValidEmail(string email)
        {
            return email != null && email.Contains("@");
        }

        private User GetUserInformation(string userId)
        {
            return db.Users.FirstOrDefault(u => u.UserId == userId);
        }
        [Route("adminindex")]
        public IActionResult AdminIndex()
        {
            return View();
        }
       
        [Route("dentistlist")]
        public IActionResult DentistManagement()
        {
            var lstDentist = db.Doctors.ToList();
            ViewBag.Doctors = lstDentist;
            return View();
        }

        [Route("servicelist")]
        public IActionResult ServiceManagement()
        {
            var lstService = db.Services.ToList();
            ViewBag.Services = lstService;
            return View();
        }
        [Route("addservice")]
        [HttpGet]
        public IActionResult AddService()
        {
            return View();
        }

        [Route("addservice")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddService(Service service)
        {
            try
            {
                string newServiceId = GenerateNewServiceId();
                service.ServiceId = newServiceId;

                db.Services.Add(service);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Service added successfully";
                return RedirectToAction("ServiceManagement");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View("AddDentist", service);
            }
        }

        [Route("removeservice")]
        [HttpGet]
        public IActionResult RemoveService(string serviceid)
        {
            var serviceToRemove = db.Services.FirstOrDefault(d => d.ServiceId == serviceid);

            if (serviceToRemove != null)
            {
                db.Services.Remove(serviceToRemove);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Service removed successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Service not found";
            }

            return RedirectToAction("ServiceManagement", "HomeAdmin");
        }

        [Route("editservice")]
        [HttpGet]
        public IActionResult EditService(string serviceid)
        {
            var servicedetail = db.Services.FirstOrDefault(d => d.ServiceId == serviceid);

            if (servicedetail != null)
            {
                return View(servicedetail);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("editservice")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditService(Service service)
        {
            try
            {
                db.Update(service);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Service edited successfully";
                return RedirectToAction("ServiceManagement");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error");
            }
            return View(service);
        }

        [Route("adddentist")]
        [HttpGet]
        public IActionResult AddDentist()
        {
            ViewBag.GenderList = new List<SelectListItem>
            {
                new SelectListItem {Text = "Male", Value = "Male"},
                new SelectListItem {Text = "Female", Value = "Female"}
            };
            return View();
        }

        [Route("adddentist")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddDentist(Doctor dentist)
        {
            ViewBag.GenderList = new List<SelectListItem>
            {
                new SelectListItem {Text = "Male", Value = "Male"},
                new SelectListItem {Text = "Female", Value = "Female"}
            };
            if (!IsValidEmail(dentist.Email))
            {
                ModelState.AddModelError("Email", "Invalid email format");
                return View("AddDentist", dentist);
            }
            if (!IsValidPhoneNumber(dentist.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Phone number must has 10 digits and all characters must be number");
                return View("AddDentist", dentist);
            }
            if (db.Doctors.Any(d => d.Email == dentist.Email))
            {
                ModelState.AddModelError("Email", "This email is already exist");
                return View("AddDentist", dentist);
            }
            if (db.Doctors.Any(d => d.PhoneNumber == dentist.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "This phone number is already exist");
                return View("AddDentist", dentist);
            }

            try
            {
                string newDoctorId = GenerateNewDoctorId();
                dentist.DoctorId = newDoctorId;
                db.Doctors.Add(dentist);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Dentist added successfully";
                return RedirectToAction("DentistManagement");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error" + ex.Message);
                return View("AddDentist", dentist);
            }
        }

        [HttpGet]
        [Route("removedentist")]
        public IActionResult RemoveDentist(string dentistid)
        {
            var dentistToRemove = db.Doctors.FirstOrDefault(d => d.DoctorId == dentistid);

            if (dentistToRemove != null)
            {
                db.Doctors.Remove(dentistToRemove);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Dentist removed successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Dentist not found";
            }
            return RedirectToAction("DentistManagement", "HomeAdmin");
        }

        [HttpGet]
        [Route("editdentist")]
        public IActionResult EditDentist(string dentistid)
        {
            var dentistprofile = db.Doctors.FirstOrDefault(d => d.DoctorId == dentistid);
            if (dentistprofile != null)
            {
                ViewBag.OldImageName = dentistprofile.Images;
                ViewBag.GenderList = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Male", Value = "Male" },
                    new SelectListItem { Text = "Female", Value = "Female" }
                };
                return View(dentistprofile);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("editdentist")]
        [ValidateAntiForgeryToken]
        public IActionResult EditDentist(Doctor dentist)
        {
            ViewBag.GenderList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Male", Value = "Male" },
                new SelectListItem { Text = "Female", Value = "Female" }
            };
            if (!IsValidEmail(dentist.Email))
            {
                ModelState.AddModelError("Email", "Invalid email format");
                return View("EditDentist", dentist);
            }

            if (!IsValidPhoneNumber(dentist.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Phone number must have 10 digits and all digits must be numbers");
                return View("EditDentist", dentist);
            }

            try
            {
                db.Update(dentist);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Dentist edited successfully";
                return RedirectToAction("DentistManagement");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error");
            }
            return View(dentist);
        }
        public IActionResult Patient()
        {
            return View();
        }
    }
}
