using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ePatria.Startup))]
namespace ePatria
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
