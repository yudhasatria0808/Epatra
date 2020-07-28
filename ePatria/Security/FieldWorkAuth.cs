using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ePatria.Security
{
    public class FieldWorkAuth : AuthorizeAttribute
    {
        public string aksesName { get; set; }
        public void IsMember()
        { 
        
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var usr = Users;
            //filterContext.RequestContext.RouteData
            //if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            //{
            //    if (Menu != null && Menu != "")
            //    {
            //        if (!CurrentUser.HasActionAccess(Menu, Action))
            //        {
            //            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "AccessDenied" }));
            //        }
            //    }
            //    else
            //    {
            //        base.OnAuthorization(filterContext);
            //    }
            //}
            //else
            //{
            //    base.OnAuthorization(filterContext);
            //}
        }
    }
}