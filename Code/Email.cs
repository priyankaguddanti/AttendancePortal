using System;
using System.Net;
using System.Net.Mail;

namespace AttendancePortal.Code
{
    public class Email
    {
        public static bool SendEmail(string toAddress, string toDisplayName, string subject, string body)
        {
            try
            {
                var fromMailAddress = new MailAddress("attendancesystemnotification@gmail.com", "Attendance Admin");
                var toMailAddress = new MailAddress(toAddress, toDisplayName);
                const string fromPassword = "sivapriya";
               
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromMailAddress.Address, fromPassword)
                };
                using (var message = new MailMessage(fromMailAddress, toMailAddress)
                {
                    Subject = subject,
                    Body = body,
                })
                {
                    smtp.Send(message);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}