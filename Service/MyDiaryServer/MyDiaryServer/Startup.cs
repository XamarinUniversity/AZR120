using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MyDiaryServer.Startup))]

namespace MyDiaryServer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}