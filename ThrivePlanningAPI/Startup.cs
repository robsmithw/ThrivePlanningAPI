using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MediatR;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

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

            services.AddSingleton<IAmazonDynamoDB>(factory =>
            {
                var dbConfig = HostingEnvironment.IsDevelopment()
                    ? new AmazonDynamoDBConfig() { ServiceURL = Configuration["DynamoDB:ServiceUrl"] }
                    : new AmazonDynamoDBConfig();
                return new AmazonDynamoDBClient(dbConfig);
            });

            services.AddSingleton<IDynamoDBContext>(s => new DynamoDBContext(s.GetService<IAmazonDynamoDB>()));
            // will want to seed data, but leave this out for now
            //services.AddSingleton(s => new SeedDataLoader(s.GetService<IAmazonDynamoDB>(), s.GetService<IDynamoDBContext>(), s.GetService<IConfiguration>(), s.GetService<IWebHostEnvironment>()));

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