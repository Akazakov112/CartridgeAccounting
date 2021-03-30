using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CartAccLibrary.Dto;
using CartAccLibrary.Entities;
using CartAccServer.Hubs;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Services;
using CartAccServer.Models.Interfaces.Utility;
using CartAccServer.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CartAccServer.Controllers
{
    /// <summary>
    /// Контроллер страницы администрирования.
    /// </summary>
    [Authorize]
    public class AdministrationController : Controller
    {
        /// <summary>
        /// Логгер.
        /// </summary>
        private ILogger<AdministrationController> Logger { get; }

        /// <summary>
        /// Хаб для работы.
        /// </summary>
        private IHubContext<CartAccWorkHub> WorkHub { get; }

        /// <summary>
        /// Сервис обновления клиентов.
        /// </summary>
        private IClientUpdateService ClientUpdate { get; }

        /// <summary>
        /// Сервис работы с данными.
        /// </summary>
        private IDataService DataService { get; }

        /// <summary>
        /// Сервис работы с подключенными пользователями.
        /// </summary>
        private IConnectedUserProvider ConnectedUserProvider { get; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="hostEnviroment"></param>
        /// <param name="context"></param>
        public AdministrationController(ILogger<AdministrationController> logger, IHubContext<CartAccWorkHub> hub,
            IClientUpdateService clientUpdateService, IDataService dataService, IConnectedUserProvider provider)
        {
            Logger = logger;
            WorkHub = hub;
            ClientUpdate = clientUpdateService;
            DataService = dataService;
            ConnectedUserProvider = provider;
        }


        /// <summary>
        /// Аутентифицирует пользователя.
        /// </summary>
        /// <param name="user">Объект пользователя</param>
        /// <returns></returns>
        private async Task AuthenticateAsync(UserDTO user)
        {
            // Создать claims логина и уровня доступа.
            var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Access?.Name)
                };
            // Создаем объект ClaimsIdentity.
            ClaimsIdentity id = new ClaimsIdentity(claims, "Cookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // Установка аутентификационных куки.
            await HttpContext.SignInAsync("Cookie", new ClaimsPrincipal(id));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //// Проверка на присвоенную роль.
            Claim roleClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            if (roleClaim is null)
            {
                // Логин.
                string login = HttpContext.User.Identity.Name;
                // Найти пользователя по логину.
                UserDTO user = login is null
                    ? null
                    : DataService.Users.Find(x => login.Contains(x.Login) && x.Active).FirstOrDefault();
                if (user is null)
                {
                    // Записать лог.
                    Logger.LogWarning($"Администрирование - неудачная аутентификация пользователя - {login}, доступ запрещен.");
                    return View("AccessDenied");
                }
                else
                {
                    // Аутентифицировать пользователя.
                    await AuthenticateAsync(user);
                    return RedirectToAction("Statistic");
                }
            }
            else
            {
                return RedirectToAction("Statistic");
            }
        }

        /// <summary>
        /// Возвращает представление запрета доступа.
        /// </summary>
        /// <param name="ReturnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Возвращает главную страницу.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Statistic()
        {
            var statisticVm = new StatisticVm(ConnectedUserProvider.ConnectedUsers);
            return View(statisticVm);
        }

        [HttpGet]
        [Authorize(Roles = "Администратор")]
        public IActionResult UploadUpdate()
        {
            // Сформировать список обновлений клиента.
            IEnumerable<ClientUpdate> updates = ClientUpdate.GetAllUpdates();
            var uploadVm = updates is null
                ? new UploadUpdateVm() { AllUpdates = new List<ClientUpdate>(), Version = 1000 }
                : new UploadUpdateVm() { AllUpdates = updates, Version = updates.LastOrDefault().Version + 1 };
            // Вернуть представление.
            return View(uploadVm);
        }

        /// <summary>
        /// Возвращает представление для отправки сообщений клиентам.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Администратор")]
        public IActionResult SendMessage()
        {
            List<Recepient> recepients = ConnectedUserProvider.ConnectedUsers
                .Select(x => new Recepient(x.ConnectionId, x.Name))
                .ToList();
            recepients.Insert(0, new Recepient("0", "Все"));
            var sendMessageVm = new SendMessageVm() { Recipients = recepients };
            return View(sendMessageVm);
        }


        /// <summary>
        /// Загружает файл обновления.
        /// </summary>
        /// <param name="uploadUpdate">Файл обновления</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadUpdate(UploadUpdateVm updateVm)
        {
            try
            {
                // Загрузить обновление.
                ClientUpdate.AddUpdate(updateVm.UpdateFile, updateVm.Description, updateVm.Version);
                // Поулчить последнее загруженное обновление.
                ClientUpdateDTO update = ClientUpdate.GetLastUpdate();
                // Отправить всем клиентам вызов проверки обновления.
                await WorkHub.Clients.All.SendAsync("CheckUpdate");
                return RedirectToAction("UploadUpdate");
            }
            catch (DbUpdateException dbEx)
            {
                // При ошибке записать в лог и вывести страницу ошибок.
                string error = dbEx.InnerException is null ? dbEx.Message : dbEx.InnerException.Message;
                Logger.LogError($"Ошибка загрузки обновления - {error}");
                return View("Error", new ErrorVm() { ErrorMessage = error });
            }
        }

        /// <summary>
        /// Отправляет сообщение получателю.
        /// </summary>
        /// <param name="sendMessageVm"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(SendMessageVm sendMessageVm)
        {
            if (sendMessageVm.RecepientConnectionId == "0")
            {
                // Отправить всем клиентам сообщение.
                await WorkHub.Clients.All.SendAsync("Alert", sendMessageVm.Message);
            }
            else
            {
                // Отправить сообщение конкретному пользователю.
                await WorkHub.Clients.Client(sendMessageVm.RecepientConnectionId).SendAsync("Alert", sendMessageVm.Message);
            }
            return RedirectToAction("SendMessage");
        }
    }
}
