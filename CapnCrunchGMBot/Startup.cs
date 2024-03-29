using CapnCrunchGMBot.Interfaces;
using CapnCrunchGMBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using RestEase;

namespace CapnCrunchGMBot
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
            services.AddSwaggerGen();
            services.AddSingleton(RestClient.For<IDeadCapApi>("https://mfl-capn.herokuapp.com"));
            services.AddSingleton(RestClient.For<IGroupMeApi>("https://api.groupme.com"));
            services.AddSingleton(RestClient.For<IMflApi>("https://www64.myfantasyleague.com/2021"));
            services.AddScoped<IGroupMeService, GroupMeService>();
            services.AddScoped<IRumorService, RumorService>();
            services.AddHttpClient();
            

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CapnCrunch GroupMe Bot");
            });
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
