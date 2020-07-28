using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ePatria.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace ePatria.Controllers
{
    [Authorize]
    public class MyProfileController : Controller
    {

        private ePatriaDefault db = new ePatriaDefault();
 
        // GET: MyProfile/Edit/5
        public ActionResult EditProfile()
        {
            var Mywho = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(User.Identity.GetUserId());
            var ById = db.Employees.Where(p => p.Email.Equals(Mywho.Email)).Select(p => p.EmployeeID).FirstOrDefault();

            Employee empl = db.Employees.Find(Convert.ToInt32(ById));
            ViewBag.Username = Mywho.UserName;
            ViewBag.FirstName = Mywho.FirstName;
            ViewBag.LastName = Mywho.LastName;
            ViewBag.Email = Mywho.Email;
            ViewBag.EmployeeID = empl.EmployeeID;
            return View();
        }

        // POST: MyProfile/Edit/5
        [HttpPost]
        public ActionResult EditProfile(int EmployeeID, string FirstName, string LastName, string Email, string UserName)
        {

            if (ModelState.IsValid)
            {
                
                var Mywho = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(User.Identity.GetUserId());
                
                var empl = db.Employees.Find(EmployeeID);
                empl.Name = FirstName + " " + LastName;
                empl.Email = Email;
                Mywho.Id = Mywho.Id;
                Mywho.UserName = UserName;  
                Mywho.FirstName = FirstName;
                Mywho.LastName = LastName;
                Mywho.Email = Email;
                db.Entry(empl).State = EntityState.Modified;

                Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Update(Mywho);
                db.SaveChanges();

                string userId = User.Identity.GetUserId();
                var currentUserLogin = db.UserLoginDetail.Where(p => p.UserID.Equals(userId));
                if (currentUserLogin.Count() > 0)
                {
                    db.UserLoginDetail.Remove(currentUserLogin.FirstOrDefault());
                    db.SaveChanges();
                }
                AuthenticationManager.SignOut();
                return RedirectToAction("Login", "Account");
            }
            return View("EditProfile");
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordProfile()
        {

            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPasswordProfile(ResetPasswordProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(User.Identity.GetUserId());
                var pas = await Request.GetOwinContext().GetUserManager<ApplicationUserManager>().FindAsync(user.UserName, model.Password);

                if (pas != null)
                {
                    var code = await Request.GetOwinContext().GetUserManager<ApplicationUserManager>().GeneratePasswordResetTokenAsync(user.Id);
                    //var callbackUrl = Url.Action("ResetPassword", "Account",
                    //new { UserId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    //await UserManager.SendEmailAsync(user.Id, "Reset Password",
                    //"Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
                    System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
                    new System.Net.Mail.MailAddress("patra.no.reply@gmail.com", "ePatria Registration"),
                    new System.Net.Mail.MailAddress(user.Email));
                    m.Subject = "Reset Password";
                    m.Body = string.Format("Dear {0}, <BR/><BR/>Please reset your password by clicking <a href=\"{1}\" title=\"User reset password\">here</a>.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team",
                    user.UserName, Url.Action("ResetPassword", "MyProfile",
                    new { UserId = user.Id, email = user.Email, code = code }, Request.Url.Scheme));
                    m.IsBodyHtml = true;
                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com");
                    smtp.Port = 587;
                    smtp.Credentials = new System.Net.NetworkCredential("patra.no.reply@gmail.com", "epatria1234");
                    smtp.EnableSsl = true;
                    smtp.Send(m);
                    return View("PasswordConfirmation");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid password.");
                }                         
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult PasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code, string email)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await Request.GetOwinContext().GetUserManager<ApplicationUserManager>().FindByEmailAsync(model.Email);
            if (user == null)
            {
                string userId = User.Identity.GetUserId();
                var currentUserLogin = db.UserLoginDetail.Where(p => p.UserID.Equals(userId));
                if (currentUserLogin.Count() > 0)
                {
                    db.UserLoginDetail.Remove(currentUserLogin.FirstOrDefault());
                    db.SaveChanges();
                }
                AuthenticationManager.SignOut();
                return RedirectToAction("ResetPasswordConfirmation", "MyProfile");
            }
            var result = await Request.GetOwinContext().GetUserManager<ApplicationUserManager>().ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                string userId = User.Identity.GetUserId();
                var currentUserLogin = db.UserLoginDetail.Where(p => p.UserID.Equals(userId));
                if (currentUserLogin.Count() > 0)
                {
                    db.UserLoginDetail.Remove(currentUserLogin.FirstOrDefault());
                    db.SaveChanges();
                }
                AuthenticationManager.SignOut();
                return RedirectToAction("ResetPasswordConfirmation", "MyProfile");
            }
            AddErrors(result);
            return View();
        }


       private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        // GET: MyProfile/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MyProfile/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
}
     
    
}
