using Microsoft.Extensions.Logging;
using GuiderMAUI.Shared.Services;
using GuiderMAUI.Shared.Models;
using System.Reflection; // Для чтения встроенного файла
using Microsoft.Extensions.Configuration; // Для работы с JSON конфигом

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

            // ==============================================================
            // 1. ЗАГРУЗКА APPSETTINGS.JSON
            // ==============================================================
            var a = Assembly.GetExecutingAssembly();
            // ВАЖНО: Имя ресурса = ИмяВашегоПроекта.ИмяФайла
            // Если проект называется GuiderMAUI, то строка ниже верная:
            using var stream = a.GetManifestResourceStream("GuiderMAUI.appsettings.json");

            if (stream != null)
            {
                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();

                builder.Configuration.AddConfiguration(config);
            }

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // ==============================================================
            // 2. НАСТРОЙКА HTTP CLIENT (ВЫБОР МЕЖДУ LOCAL И REMOTE)
            // ==============================================================

            // Читаем настройки из JSON
            bool useLocal = builder.Configuration.GetValue<bool>("ApiSettings:UseLocal");
            string remoteUrl = builder.Configuration["ApiSettings:RemoteUrl"];
            string localPort = builder.Configuration["ApiSettings:LocalPort"];

            string apiBaseUrl;
            HttpClientHandler handler;

            if (useLocal)
            {
                // ЛОГИКА ДЛЯ ЛОКАЛЬНОГО API
                string localHost = DeviceInfo.Platform == DevicePlatform.Android
                    ? builder.Configuration["ApiSettings:LocalAndroidHost"] // 10.0.2.2
                    : builder.Configuration["ApiSettings:LocalWindowsHost"]; // localhost

                apiBaseUrl = $"https://{localHost}:{localPort}/";

                // Для локалки используем "всепрощающий" SSL хендлер
                handler = GetInsecureHandler();
            }
            else
            {
                // ЛОГИКА ДЛЯ УДАЛЕННОГО СЕРВЕРА
                apiBaseUrl = remoteUrl;

                // Для продакшена (guider.pro) используем стандартный безопасный хендлер.
                // Если там стоит настоящий SSL сертификат (Let's Encrypt и т.д.),
                // то insecureHandler НЕ НУЖЕН и даже опасен.
                handler = new HttpClientHandler();
            }

            // Регистрируем HttpClient с выбранным URL и Handler-ом
            builder.Services.AddScoped(sp =>
                new HttpClient(handler)
                {
                    BaseAddress = new Uri(apiBaseUrl)
                });

            // ==============================================================
            // РЕГИСТРАЦИЯ СЕРВИСОВ
            // ==============================================================
            // Ваши сервисы полностью совместимы, менять их код не нужно.

            builder.Services.AddScoped<IPlacesService, PlacesService>();
            builder.Services.AddScoped<ICitiesService, CitiesService>();
            builder.Services.AddScoped<IProvincesService, ProvincesService>();
            builder.Services.AddScoped<ITagsService, TagsService>();

            return builder.Build();
        }

        // --- Метод для игнорирования ошибок SSL сертификатов (Только для локалки) ---
        private static HttpClientHandler GetInsecureHandler()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return handler;
        }
    }
}