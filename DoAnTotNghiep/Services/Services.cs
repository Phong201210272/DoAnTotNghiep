using System;
using System.Net.Mail;
using DoAnTotNghiep.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
namespace DoAnTotNghiep.Services
{
    public class SmtpEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public SmtpEmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public bool SendAppointmentConfirmationEmail(Appointment appointment)
        {
            try
            {
                // Tạo và gửi email
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                message.To.Add(new MailboxAddress(appointment.User.Name, appointment.User.Email));
                message.Subject = "Appointment Confirmation";

                // Nội dung email
                message.Body = new TextPart("plain")
                {
                    Text = $"Dear {appointment.User.Name},\n\nYour appointment has been confirmed.\n\nThank you."
                };

                // Gửi email bằng SMTP
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect("smtp.yourserver.com", 587, false);
                    client.Authenticate("your_username", "your_password");
                    client.Send(message);
                    client.Disconnect(true);
                }

                return true; // Trả về true nếu email được gửi thành công
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và ghi log nếu cần
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false; // Trả về false nếu có lỗi xảy ra khi gửi email
            }
        }
    }
}
