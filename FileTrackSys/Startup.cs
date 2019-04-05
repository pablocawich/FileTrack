using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FileTracking.Startup))]
namespace FileTracking
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
