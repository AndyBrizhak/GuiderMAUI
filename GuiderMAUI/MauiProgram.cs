using Microsoft.Extensions.Logging;
using GuiderMAUI.Shared.Services; 
using GuiderMAUI.Shared.Models;

namespace GuiderMAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // --- НАСТРОЙКА HTTP CLIENT ---

            //  Определяем базовый URL API
            // Для Android Эмулятора localhost это 10.0.2.2
            // Для Windows/iOS это localhost
            string apiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:8081/" // <-- ВАЖНО: ПОРТ  локального API 
                : "http://localhost:8081/"; // <-- И здесь тот же порт

            // Если  API работает по HTTPS с самоподписанным сертификатом,
            // на Android понадобятся дополнительные настройки (HttpsClientHandler).
            // Для начала  пробуем HTTP, если возможно, то реальный HTTPS домен.

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

            //  Регистрируем  сервисы
            // Если нет интерфейсов (IPlacesService), регистрируйте так:
            // builder.Services.AddScoped<PlacesService>();

            // Если интерфейсы есть, то так:
            builder.Services.AddScoped<IPlacesService, PlacesService>();
            builder.Services.AddScoped<ICitiesService, CitiesService>();
            builder.Services.AddScoped<IProvincesService, ProvincesService>();
            builder.Services.AddScoped<ITagsService, TagsService>();

            return builder.Build();
        }
    }
}