namespace WebApp_OpenIDConnect_DotNet
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.AspNetCore.Hosting;

    public class Program
    {
        #region methods

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder().UseKestrel().UseContentRoot(Directory.GetCurrentDirectory()).UseIISIntegration().UseStartup<Startup>().Build();
            host.Run();
        }

        #endregion
    }
}