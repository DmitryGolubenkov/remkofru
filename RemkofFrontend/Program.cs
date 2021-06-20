using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Net;

namespace RemkofFrontend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //��������� ������ ����������
            var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

            

            //�������� � ����������� ���-����������
            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 80);
                    try
                    {
                        options.Listen(IPAddress.Any, 443, listenOptions =>
                        {
                            //��������� ������ � �����������
                            var certificateSettings = config.GetSection("Kestrel").GetSection("EndPoints").GetSection("Https").GetSection("certificateSettings");
                            string certificateFileName = certificateSettings.GetValue<string>("filename");
                            string certificatePassword = certificateSettings.GetValue<string>("password");

                            listenOptions.UseHttps(certificateFileName, certificatePassword);
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"�� ������� ��������� HTTPS: {e.Message}. ���-���������� ����� �������� ������ �� HTTP.");
                    }
                }
                )
                .UseStartup<Startup>()
                .UseIISIntegration()
                .Build();

            //��������� ���
            host.Run();
        }
    }
}
