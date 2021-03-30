using CartAccServer.Hubs;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Repository;
using CartAccServer.Models.Interfaces.Services;
using CartAccServer.Models.Interfaces.Utility;
using CartAccServer.Models.Repositories;
using CartAccServer.Models.Services;
using CartAccServer.Models.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CartAccServer
{
    public class Startup
    {
        /// <summary>
        /// ������������ ����������.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// �����������.
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        /// <summary>
        /// ����������� ������������.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // ���������� ��������� ���� ������.
            services.AddDbContext<CartAccDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DbConnection")));
            // ���������� SignalR � �������������� �� Newtonsoft.
            services.AddSignalR().AddNewtonsoftJsonProtocol(configure =>
            {
                configure.PayloadSerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None;
                configure.PayloadSerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
            // ���������� �������������� �� ������ ����.
            services.AddAuthentication("Cookie").AddCookie("Cookie", option => 
            { 
                option.AccessDeniedPath = new PathString("/cartaccadmin/administration/AccessDenied"); 
            });
            // ���������� MVC.
            services.AddControllersWithViews();
            // ���������� ������� ������ � ������������� ��.
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // ���������� ������� ������ � ���������� ��.
            services.AddScoped<IDataService, DataService>();
            // ���������� ������� ������ � ������������ �������.
            services.AddScoped<IClientUpdateService, FileClientUpdateService>();
            // ���������� ������� ������ � ���������������� ��� �������������� �����������.
            services.AddSingleton<IBlockedDocumentService, BlockedDocumentService>();
            // ���������� ������� ������ � ������������� ��������������.
            services.AddSingleton<IConnectedUserProvider, ConnectedUserProvider>();
        }

        /// <summary>
        /// �������� ��������� �������.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<CartAccWorkHub>(pattern: "/cartaccuser");
                endpoints.MapControllerRoute(name: "default", pattern: "cartaccadmin/{controller=Administration}/{action=Index}");
            });
        }
    }
}
