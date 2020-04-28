using System.Text;
using GameService.Context;
using GameService.Hubs;
using GameService.Models;
using GameService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PokerEvaluatorClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GameService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddControllers();
            services.AddControllersWithViews();
            services.AddSingleton<IGame, Game>();
            services.AddSingleton<IPokerEvaluator, EvaluatorGrpcClient>();
            services.AddScoped<IIdentityService, IdentityService>();

            services.AddDbContext<GameDbContext>(options =>
            {
                options.UseMySQL(
                    Configuration.GetConnectionString("MySqlConnection") /*,b => b.MigrationsAssembly("NetCorePoker")*/
                    );
            });

            #region JWT 

            var key = Encoding.UTF8.GetBytes(Configuration.GetSection("CryptographicKey").Value);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = async ctx =>
                    {
                        var principal = ctx.Principal;
                    },
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/pokerHub")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
                options.RequireHttpsMetadata = true; //TODO: load from config.
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidIssuer = "NetCorePoker"
                };
            });

            #endregion JWT


            services.AddSignalR()
                 .AddMessagePackProtocol(options =>
                 {
                     options.FormatterResolvers = new List<MessagePack.IFormatterResolver>(){
                         MessagePack.Resolvers.ContractlessStandardResolver.Instance,
                         MessagePack.Resolvers.StandardResolver.Instance
                     };
                 });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder =>
            {
                builder.WithOrigins("http://localhost:7456")
                    .AllowAnyHeader()
                    .AllowCredentials();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}"
                   );
                endpoints.MapHub<PokerHub>("/pokerHub");
            });
        }
    }
}
