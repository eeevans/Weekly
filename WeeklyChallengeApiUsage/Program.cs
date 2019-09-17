using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Serilog;

namespace WeeklyChallengeApiUsage
{
    class Program
    {
        static async Task Main()
        {
            await Bootstrap();
        }

        private static async Task Bootstrap()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            var services = new ServiceCollection()
                .AddLogging(builder =>
                {
                    // Add Serilog
                    builder.AddSerilog();
                });

            // Register a HTTP Client
            services.AddHttpClient<SwapiService>(x => x.BaseAddress = new Uri("https://www.swapi.co/api/people/"));

            var serviceProvider = services.BuildServiceProvider();

            // This is the Microsoft Logging interface
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            var service = serviceProvider.GetRequiredService<SwapiService>();
            while (true)
            {
                Console.WriteLine("Enter a person id to download:");
                var input = Console.ReadLine();
                try
                {
                    var id = Convert.ToInt32(input);
                    var response = await service.GetPersonById(id);
                    if (response.Success)
                    {
                        logger.LogInformation($"Name: {response.ServicePeople.Name}");
                        logger.LogInformation($"Gender: {response.ServicePeople.Gender}");
                    }
                    else
                    {
                        logger.LogInformation(response.ServiceException.Message);
                    }
                }
                catch
                {
                    Console.WriteLine("Invalid input");
                }
            }
        }
    }
}
