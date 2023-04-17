using BlazorChat.Client.Managers;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlazorChat.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            //регистрирует HttpClient, который будет использоваться для отправки запросов к серверу
            builder.Services.AddHttpClient("BlazorChat.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
            //регистрируется клиент
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BlazorChat.ServerAPI"));
            //добавляются настройки для библиотеки MudBlazor
            builder.Services.AddMudServices(c => { c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight; });
            //cервис, который будет использоваться для авторизации пользователей на сервере
            builder.Services.AddApiAuthorization();
            //регистрируется класс ChatManager, который будет использоваться для управления чатом
            builder.Services.AddTransient<IChatManager, ChatManager>();
            await builder.Build().RunAsync();
        }
    }
}
