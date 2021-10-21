using Gmtl.MailerAPI.WebAPI.Events;
using Gmtl.MailerAPI.WebAPI.Persistance;
using Gmtl.MailerAPI.WebAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Gmtl.MailerAPI.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<MailerDbContext>(options => options.UseInMemoryDatabase(databaseName: "MailerDb"));
            services.AddScoped<MailDeliveryService>();
            services.AddScoped<MailDataService>();

            services.AddSingleton<IncomingMailQueue>();
            services.AddSingleton<MailForSendingQueue>();

            services.AddHostedService<IncomingEmailHandlerService>();
            services.AddHostedService<EmailSenderHandlerService>();
            services.AddHostedService<MailerBackgroundWorkerService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mailer API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Mailer API V1");
            });
        }
    }
}
