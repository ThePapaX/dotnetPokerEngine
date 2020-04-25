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
            services.AddControllers();
            services.AddSingleton<IGame, Game>();
            services.AddSingleton<IPokerEvaluator, EvaluatorGrpcClient>();

            services.AddDbContext<GameDbContext>(options =>
            {
                options.UseMySQL(
                    Configuration.GetConnectionString("MySqlConnection") /*,b => b.MigrationsAssembly("NetCorePoker")*/
                    );

            });

            #region JWT 
            
            var key = Encoding.UTF8.GetBytes(Configuration.GetSection("SecurityKey").Value);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                };
            });

            services.AddScoped<IIdentityService, IdentityService>();

            #endregion JWT


            services.AddSignalR()
                 //.AddJsonProtocol(options =>
                 //{
                 //    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
                 //});
                 .AddMessagePackProtocol();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<PokerHub>("/pokerHub");
            });
        }
    }
}
