using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ePatria.Models;
using System.Data.Entity;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

namespace ePatria.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ePatriaDefault db = new ePatriaDefault();
        private AuditTrailsController auditTransact = new AuditTrailsController();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }


        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    if (user.EmailConfirmed == true)
                    {
                        string userId = User.Identity.GetUserId();
                        var currentUserLogin = db.UserLoginDetail.Where(p => p.UserID.Equals(userId));
                        if (currentUserLogin.Count() > 0)
                        {
                            db.UserLoginDetail.Remove(currentUserLogin.FirstOrDefault());
                            db.SaveChanges();
                        }
                        var userLogin = db.UserLoginDetail.Where(p => p.UserID.Equals(user.Id)).ToList();
                        if (userLogin.Count() > 0)
                        {
                            UserLoginDetail loginUser = userLogin.FirstOrDefault();
                            var diffLastActUser = DateTime.Now.Subtract((DateTime)loginUser.LastActivity);
                            int loginTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["Login_Timeout"]);
                            if (diffLastActUser.TotalMinutes > loginTimeout)
                            {
                                await SignInManager.SignInAsync(user, isPersistent: true, rememberBrowser: true);
                                loginUser.LastActivity = DateTime.Now;

                                db.Entry(loginUser).State = EntityState.Modified;
                                db.SaveChanges();
                                return RedirectToLocal(returnUrl);
                            } else
                                ModelState.AddModelError("", "Could not login, You still Logged In on another device.");
                        }
                        else {
                            await SignInManager.SignInAsync(user, isPersistent: true, rememberBrowser: true);
                            
                            UserLoginDetail userDetail = new UserLoginDetail();
                            userDetail.UserID = user.Id;
                            userDetail.LastActivity = DateTime.Now;
                            userDetail.status = "Logged In";
                            db.UserLoginDetail.Add(userDetail);
                            db.SaveChanges();
                            return RedirectToLocal(returnUrl);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Confirm Email Address.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual ActionResult UpdateLastActivityDate(string userName)
        {
            ApplicationUser user = UserManager.FindByName(userName);

            UserLoginDetail userLogin = db.UserLoginDetail.Where(p => p.UserID.Equals(user.Id)).FirstOrDefault();

            userLogin.LastActivity = DateTime.Now;

            db.Entry(userLogin).State = EntityState.Modified;

            db.SaveChanges();

            return new EmptyResult();
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }
        

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewBag.RoleId = new SelectList(RoleManager.Roles.ToList(), "Name", "Name");
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
				string username = User.Identity.Name;
                string name = model.FirstName + " " + model.LastName;
                bool nameExist = db.Employees.Any(p => p.Name == name);
                if (!nameExist)
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName
                    };

                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        Employee emp = new Employee();
                        emp.Email = model.Email;
                        emp.Name = name;
                        emp.Status = model.UserName;
                        emp.UserName = model.UserName;
                        emp.OrganizationID = 1;
                        emp.PositionID = 1;
                        emp.Type = "Auditee";
                        emp.NoPEK = "1";
                        emp.PhoneNumber = "0";
                        db.Employees.Add(emp);
                        db.SaveChanges();
                        var result1 = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                        var token = UserManager.GenerateEmailConfirmationToken(user.Id);
                        System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
                        new System.Net.Mail.MailAddress("epatria@patraniaga.com", "ePatria Registration"),
                        new System.Net.Mail.MailAddress(user.Email));
                        m.Subject = "Email confirmation";
                        m.Body = string.Format("Dear {0}, <BR/><BR/>Thank you for your registration, please click on this <a href=\"{1}\" title=\"User Email Confirm\">link</a> to complete your registration.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team",
                        user.UserName, Url.Action("ConfirmEmail", "Account",
                        new { UserId = user.Id, Token = token, Email = user.Email }, Request.Url.Scheme));
                        m.IsBodyHtml = true;
                        System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("mail.patraniaga.com");
                        smtp.Port = 587;
                        smtp.Credentials = new System.Net.NetworkCredential("epatria@patraniaga.com", "Patra12345");
                        smtp.EnableSsl = true;
                        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                        smtp.Send(m);
						Employee createemployee = new Employee();
                    	auditTransact.CreateAuditTrail("Create", emp.EmployeeID, "User", createemployee, emp, username);
                        TempData["message"] = "Email Confirmation successfully sent!";
                        return RedirectToAction("Confirm", "Account", new { Email = user.Email });
                    }
                    AddErrors(result);
                }
                else
                    ViewBag.NameExist = "First Name and Last Name combination already taken";
            }
            // If we got this far, something failed, redisplay form
            ViewBag.RoleId = new SelectList(RoleManager.Roles.ToList(), "Name", "Name");
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Confirm(string Email)
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            ViewBag.Email = Email; 
            return View();
        } 

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string token)
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            if (userId == null && token == null)
            {
                return View("Error");
            }
            TempData["message"] = "User Admin successfully created!";
            var result = await UserManager.ConfirmEmailAsync(userId, token);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                //var callbackUrl = Url.Action("ResetPassword", "Account",
                //new { UserId = user.Id, code = code }, protocol: Request.Url.Scheme);
                //await UserManager.SendEmailAsync(user.Id, "Reset Password",
                //"Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
                System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
                new System.Net.Mail.MailAddress("patra.no.reply@gmail.com", "ePatria Registration"),
                new System.Net.Mail.MailAddress(user.Email));
                m.Subject = "Forgot Password";
                m.Body = string.Format("Dear {0}, <BR/><BR/>Please reset your password by clicking <a href=\"{1}\" title=\"User forgot password\">here</a>.<BR/><BR/><BR/> Regards,<BR/><BR/> ePatria Team",
                user.UserName, Url.Action("ResetPassword", "Account",
                new { UserId = user.Id, email = user.Email, code = code}, Request.Url.Scheme));
                m.IsBodyHtml = true;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com");
                smtp.Port = 587;
                smtp.Credentials = new System.Net.NetworkCredential("patra.no.reply@gmail.com", "epatria1234");
                smtp.EnableSsl = true;
                smtp.Send(m);
                return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
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
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult ExternalLogin(string provider, string returnUrl)
        //{
        //    // Request a redirect to the external login provider
        //    return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        //}

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        //[AllowAnonymous]
        //public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        //{
        //    var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
        //    if (loginInfo == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    // Sign in the user with this external login provider if the user already has a login
        //    var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            return RedirectToLocal(returnUrl);
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.RequiresVerification:
        //            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
        //        case SignInStatus.Failure:
        //        default:
        //            // If the user does not have an account, then prompt the user to create an account
        //            ViewBag.ReturnUrl = returnUrl;
        //            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
        //            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
        //    }
        //}

        //
        // POST: /Account/ExternalLoginConfirmation
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("Index", "Manage");
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        // Get the information about the user from the external login provider
        //        var info = await AuthenticationManager.GetExternalLoginInfoAsync();
        //        if (info == null)
        //        {
        //            return View("ExternalLoginFailure");
        //        }
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await UserManager.CreateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            result = await UserManager.AddLoginAsync(user.Id, info.Login);
        //            if (result.Succeeded)
        //            {
        //                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                return RedirectToLocal(returnUrl);
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    ViewBag.ReturnUrl = returnUrl;
        //    return View(model);
        //}

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
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

        //
        // GET: /Account/ExternalLoginFailure
        //[AllowAnonymous]
        //public ActionResult ExternalLoginFailure()
        //{
        //    return View();
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

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

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}