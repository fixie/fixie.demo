namespace ContactList
{
    using System;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Serilog;

    public class Program
    {
        public static readonly string ApplicationName = typeof(Program).Assembly.GetName().Name;

        public static int Main(string[] args)
        {
            Console.Title = ApplicationName;

            try
            {
                BuildWebHost(args).Run();
                return 0;
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(x =>
                {
                    x.AddEnvironmentVariables(ApplicationName + ":");
                })
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
    }
}