using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Gurutw.Startup))]
namespace Gurutw
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
