using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MediatR;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThrivePlanningAPI.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using ThrivePlanningAPI.Features.UserManagement;

namespace ThrivePlanningAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (HostingEnvironment.IsDevelopment())
            {
                services.AddCors(options => 
                {
                    options.AddDefaultPolicy(
                        builder => 
                        {
                            builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                        }
                    );
                });
            }
            services.AddMemoryCache();
            services.AddControllers().AddNewtonsoftJson();
            services.AddMediatR(typeof(Startup).Assembly);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = GetCognitoTokenValidationParams();
            });
            services.AddScoped<ICognitoUserManagement, CognitoUserManagement>();

            services.AddDbContextPool<ThrivePlanContext>((srv, builder) =>
            {
                if (HostingEnvironment.IsDevelopment())
                {
                    builder.EnableDetailedErrors();
                    builder.EnableSensitiveDataLogging();
                }
                var conn = Configuration.GetConnectionString("DefaultConnection");

                builder.UseMySql(conn, new MySqlServerVersion(new Version(5, 7, 32)));
            });

        }

        private TokenValidationParameters GetCognitoTokenValidationParams()
        {
            var cognitoIssuer = $"https://cognito-idp.{Configuration["AWS:Region"]}.amazonaws.com/{Configuration["AWS:UserPoolId"]}";
            var jwtKeySetUrl = $"{cognitoIssuer}/.well-known/jwks.json";
            var cognitoAudience = Configuration["AWS:AppClientId"];
            
            return new TokenValidationParameters
            {
                IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
                {
                    // get JsonWebKeySet from AWS
                    var json = new WebClient().DownloadString(jwtKeySetUrl);
                    
                    // serialize the result
                    var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(json).Keys;
                    
                    // cast the result to be the type expected by IssuerSigningKeyResolver
                    return (IEnumerable<SecurityKey>)keys;
                },
                ValidIssuer = cognitoIssuer,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidAudience = cognitoAudience,
                ValidateAudience = false
            };
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseCors();
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}