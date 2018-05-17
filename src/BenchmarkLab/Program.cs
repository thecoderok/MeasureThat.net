using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;

namespace MeasureThat.Net
{
    public class Program
    {
        /*public static void Main(string[] args)
        {
            //AppDomain.CurrentDomain.UnhandledException += (s, e) => Log("*** Crash! ***", "UnhandledException");
            TaskScheduler.UnobservedTaskException += (s, e) => Log("*** Crash! ***", "UnobservedTaskException");

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();
            try
            {
                host.Run();
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine("Exception while running the application: " + e.Message);
                System.Console.Error.WriteLine(e.StackTrace);
            }
        }*/
        
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        internal static void Log(string message, [CallerMemberName] string caller = "")
        {
            Console.WriteLine("{0}: {1}", caller, message);
        }
    }
}
