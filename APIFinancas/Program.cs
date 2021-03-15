using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace APIFinancas
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var hostBuilder = CreateHostBuilder(args).Build();
                Serilog.Log.Information("Iniciando Web Host");
                hostBuilder.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal(ex, "Host encerrado inesperadamente");
                return 1;
            }
            finally
            {
                Serilog.Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var settings = config.Build();
                    Serilog.Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Elasticsearch(
                            options:
                                new ElasticsearchSinkOptions(
                                    new Uri(settings["Elasticsearch:Uri"]))
                                {
                                    AutoRegisterTemplate = true,
                                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                                    IndexFormat = "apifinancas-{0:yyyy.MM}"
                                })
                        .WriteTo.Console()
                        .CreateLogger();
                })
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}