using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(eMedLis.Startup))]
namespace eMedLis
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
