using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using WebApi_AWS_Starter.DataAccess;
using System.Net;

namespace WebApi_AWS_Starter
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Due To https://github.com/dotnet/corefx/issues/8768
            // Temporary fix here https://github.com/StackExchange/StackExchange.Redis/issues/463
            var dns_Redis_Task = Dns.GetHostAddressesAsync("pub-redis-10931.us-west-2-1.1.ec2.garantiadata.com");
            var addresses = dns_Redis_Task.Result;
            var connect_Redis = string.Join(",", addresses.Select(x => x.MapToIPv4().ToString() + ":" + "10931"));
            
            // Add framework services.
            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = "";
                //options.Configuration = "pub-redis-10931.us-west-2-1.1.ec2.garantiadata.com:10931";
                options.Configuration = connect_Redis;
            });
            services.AddMvc();
            services.AddCors(options => 
            {
                options.AddPolicy("DynamoPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Dynamo API", Version = "v1" });
                var basePath = System.AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "WebApi_AWS_Starter.xml"); 
                c.IncludeXmlComments(xmlPath);
            });
            services.AddTransient<IPatientDataAccess, PatientDataAccess>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddFile("Logs/WebApi_AWS_Starter-{Date}.txt", isJson: true);
            var options = new JwtBearerOptions
            {
                Audience = Configuration["Auth0:ApiIdentifier"],
                Authority = $"https://{Configuration["Auth0:Domain"]}/"
            };
            app.UseJwtBearerAuthentication(options);

            app.UseMvc();
            app.UseCors("DynamoPolicy");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DynamoAPI V1");
            });
        }
    }
}
