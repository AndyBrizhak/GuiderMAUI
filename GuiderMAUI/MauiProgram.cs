using Microsoft.Extensions.Logging;
using GuiderMAUI.Shared.Services;
using GuiderMAUI.Shared.Models;
using System.Net.Http; // Обязательно для HttpClientHandler

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

            // ==============================================================
            // НАСТРОЙКА HTTP CLIENT (HTTPS + ОБХОД SSL)
            // ==============================================================

            // Порт вашего API (Docker/HTTPS)
            const string PORT = "8081";

            // 1. Формируем URL с HTTPS
            // Для Android Эмулятора используем 10.0.2.2, для Windows - localhost
            string apiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? $"https://10.0.2.2:{PORT}/"
                : $"https://localhost:{PORT}/";

            // 2. Создаем специальный Handler, который отключает проверку SSL сертификатов.
            // Это критически важно для локальной работы с Docker/Self-signed certs.
            HttpClientHandler insecureHandler = GetInsecureHandler();

            // 3. Регистрируем HttpClient, передавая ему наш "всепрощающий" Handler
            builder.Services.AddScoped(sp =>
                new HttpClient(insecureHandler)
                {
                    BaseAddress = new Uri(apiBaseUrl)
                });

            // ==============================================================
            // РЕГИСТРАЦИЯ СЕРВИСОВ
            // ==============================================================

            builder.Services.AddScoped<IPlacesService, PlacesService>();

            // Раскомментируйте остальные, когда добавите файлы:
            // builder.Services.AddScoped<ICitiesService, CitiesService>();
            builder.Services.AddScoped<IProvincesService, ProvincesService>();
            // builder.Services.AddScoped<ITagsService, TagsService>();

            return builder.Build();
        }

        // --- Метод для игнорирования ошибок SSL сертификатов ---
        private static HttpClientHandler GetInsecureHandler()
        {
            var handler = new HttpClientHandler();

            // Эта лямбда возвращает true для любого сертификата сервера,
            // предотвращая ошибку "The remote certificate is invalid".
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            return handler;
        }
    }
}