using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SampleWebMVC.Portable.Startup))]
namespace SampleWebMVC.Portable
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
