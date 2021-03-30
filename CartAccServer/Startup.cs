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
        /// Конфигурация приложения.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        /// <summary>
        /// Подключение зависимостей.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Добавление контекста базы данных.
            services.AddDbContext<CartAccDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DbConnection")));
            // Добавление SignalR с сериализатором от Newtonsoft.
            services.AddSignalR().AddNewtonsoftJsonProtocol(configure =>
            {
                configure.PayloadSerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None;
                configure.PayloadSerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
            // Добавление аутентификации на основе куки.
            services.AddAuthentication("Cookie").AddCookie("Cookie", option => 
            { 
                option.AccessDeniedPath = new PathString("/cartaccadmin/administration/AccessDenied"); 
            });
            // Добавление MVC.
            services.AddControllersWithViews();
            // Добавление сервиса работы с репозиториями БД.
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Добавление сервиса работы с сущностями БД.
            services.AddScoped<IDataService, DataService>();
            // Добавление сервиса работы с обновлениями клиента.
            services.AddScoped<IClientUpdateService, FileClientUpdateService>();
            // Добавление сервиса работы с заблокированными для редактирования документами.
            services.AddSingleton<IBlockedDocumentService, BlockedDocumentService>();
            // Добавление сервиса работы с подключенными пользователями.
            services.AddSingleton<IConnectedUserProvider, ConnectedUserProvider>();
        }

        /// <summary>
        /// Конвейер обработки запроса.
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
