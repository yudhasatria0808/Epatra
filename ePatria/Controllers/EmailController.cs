using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ePatria.Models;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Collections;

namespace ePatria.Controllers
{
    [Authorize]
    public class EmailController : Controller
    {
        // GET: Email
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Sent()
        {
            return View();
        }

        public string TestKirim()
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
                        new System.Net.Mail.MailAddress("epatria@patraniaga.com"),
                        new System.Net.Mail.MailAddress("yudhasatria0808@gmail.com"));
            m.Subject = "Test email patra";
            m.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("mail.patraniaga.com", 587);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("epatria@patraniaga.com", "Patra12345", "patraniaga.com");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            smtp.Send(m);

            return "";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(EmailForm model, string emailFrom, MailAddressCollection emailTo, string Subject, string messageBody)
        {
            if (ModelState.IsValid)
            {
                var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
                var message = new MailMessage();
                message.To.Add(new MailAddress("yogiarjan@gmail.com"));  // replace with valid value 
                message.From = new MailAddress("sitpinus7@gmail.com");  // replace with valid value
                message.Subject = "Your email subject";
                message.Body = string.Format(body, model.FromName, model.FromEmail, model.Message);
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "sitpinus7@gmail.com",  // replace with valid value
                        Password = "solusindoit"  // replace with valid value
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(message);
                    return RedirectToAction("Sent");
                }
            }
            return View(model);
        }

        public bool SentEmailFeedback(string empEmailAddress, string empName, string content, string feedUrl)
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
            new System.Net.Mail.MailAddress("epatria@patraniaga.com", "ePatria Registration"),
            new System.Net.Mail.MailAddress(empEmailAddress));
            m.Subject = "Email feedback confirmation";
            m.Body = string.Format(content, empName, feedUrl);
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("mail.patraniaga.com", 587);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("epatria@patraniaga.com", "Patra12345", "patraniaga.com");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            smtp.Send(m);
            return true;
        }

        public bool SentEmailConsultingFeedback(string empEmailAddress, string empName, string content, string feedUrl)
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
            new System.Net.Mail.MailAddress("epatria@patraniaga.com", "ePatria Registration"),
            new System.Net.Mail.MailAddress(empEmailAddress));
            m.Subject = "Email feedback confirmation";
            m.Body = string.Format(content, empName, feedUrl);
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("mail.patraniaga.com", 587);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("epatria@patraniaga.com", "Patra12345", "patraniaga.com");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            smtp.Send(m);
            return true;
        }

        public bool SentEmailToCommentedUser(string empEmailAddress, string empName, string regId, string content, string feedUrl, string comment)
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
            new System.Net.Mail.MailAddress("epatria@patraniaga.com", "ePatria Registration"),
            new System.Net.Mail.MailAddress(empEmailAddress));
            m.Subject = "Email comment confirmation";
            m.Body = string.Format(content, empName, regId, feedUrl, comment);
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("mail.patraniaga.com", 587);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("epatria@patraniaga.com", "Patra12345", "patraniaga.com");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            smtp.Send(m);
            return true;
        }

        public bool SentEmailAPM(string empEmailAddress, string empName, string regId , string content, string feedUrl)
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
            new System.Net.Mail.MailAddress("epatria@patraniaga.com", "ePatria Registration"),
            new System.Net.Mail.MailAddress(empEmailAddress));
            m.Subject = "Email comment confirmation";
            m.Body = string.Format(content, empName, regId, feedUrl);
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("mail.patraniaga.com", 587);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("epatria@patraniaga.com", "Patra12345", "patraniaga.com");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            smtp.Send(m);
            return true;
        }

        public bool SentEmailPrelim(string empEmailAddress, string empName, string regId, string content, string feedUrl)
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
            new System.Net.Mail.MailAddress("epatria@patraniaga.com", "ePatria Registration"),
            new System.Net.Mail.MailAddress(empEmailAddress));
            m.Subject = "Email comment confirmation";
            m.Body = string.Format(content, empName, regId, feedUrl);
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("mail.patraniaga.com", 587);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("epatria@patraniaga.com", "Patra12345", "patraniaga.com");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            smtp.Send(m);
            return true;
        }

        public bool SentEmailLetter(string empEmailAddress, string empName, string regId, string content, string feedUrl)
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
            new System.Net.Mail.MailAddress("epatria@patraniaga.com", "ePatria Registration"),
            new System.Net.Mail.MailAddress(empEmailAddress));
            m.Subject = "Email comment confirmation";
            m.Body = string.Format(content, empName, regId, feedUrl);
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("mail.patraniaga.com", 587);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("epatria@patraniaga.com", "Patra12345", "patraniaga.com");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            smtp.Send(m);
            return true;
        }


        public bool SentEmailBPM(string empEmailAddress, string empName, string regId, string content, string feedUrl)
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
            new System.Net.Mail.MailAddress("epatria@patraniaga.com", "ePatria Registration"),
            new System.Net.Mail.MailAddress(empEmailAddress));
            m.Subject = "Email comment confirmation";
            m.Body = string.Format(content, empName, regId, feedUrl);
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("mail.patraniaga.com", 587);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("epatria@patraniaga.com", "Patra12345", "patraniaga.com");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            smtp.Send(m);
            return true;
        }

        public bool SentEmailRCM(string empEmailAddress, string empName, string regId, string content, string feedUrl)
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
            new System.Net.Mail.MailAddress("epatria@patraniaga.com", "ePatria Registration"),
            new System.Net.Mail.MailAddress(empEmailAddress));
            m.Subject = "Email comment confirmation";
            m.Body = string.Format(content, empName, regId, feedUrl);
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("mail.patraniaga.com", 587);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("epatria@patraniaga.com", "Patra12345", "patraniaga.com");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            smtp.Send(m);
            return true;
        }

        public bool SentEmailControl(string empEmailAddress, string empName, string regId, string content, string feedUrl)
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
            new System.Net.Mail.MailAddress("epatria@patraniaga.com", "ePatria Registration"),
            new System.Net.Mail.MailAddress(empEmailAddress));
            m.Subject = "Email comment confirmation";
            m.Body = string.Format(content, empName, regId, feedUrl);
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("mail.patraniaga.com", 587);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("epatria@patraniaga.com", "Patra12345", "patraniaga.com");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            smtp.Send(m);
            return true;
        }

        public bool SentEmailConsultingLetter(string empEmailAddress, string empName, string regId, string content, string feedUrl)
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
            new System.Net.Mail.MailAddress("epatria@patraniaga.com", "ePatria Registration"),
            new System.Net.Mail.MailAddress(empEmailAddress));
            m.Subject = "Email comment confirmation";
            m.Body = string.Format(content, empName, regId, feedUrl);
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("mail.patraniaga.com", 587);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("epatria@patraniaga.com", "Patra12345", "patraniaga.com");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            smtp.Send(m);
            return true;
        }

        public bool SentEmailApproval(string empEmailAddress, string empName, string regId, string content, string feedUrl)
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
            new System.Net.Mail.MailAddress("epatria@patraniaga.com", "ePatria Registration"),
            new System.Net.Mail.MailAddress(empEmailAddress));
            m.Subject = "Email approve confirmation";
            m.Body = string.Format(content, empName, regId, feedUrl);
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("mail.patraniaga.com", 587);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("epatria@patraniaga.com", "Patra12345", "patraniaga.com");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            smtp.Send(m);
            return true;
        }
    }
}