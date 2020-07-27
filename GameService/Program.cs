using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameService.Context;
using GameService.Utilities;
using MessagePack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PokerClassLibrary;

namespace GameService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MessagePackSecurity.Active = MessagePackSecurity.UntrustedData;
            var host = CreateHostBuilder(args).Build();

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
