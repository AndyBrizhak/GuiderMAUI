using GuiderMAUI.Shared.Models;
using System.Net.Http.Json;

namespace GuiderMAUI.Shared.Services;

public class PlacesService : IPlacesService
{
    private readonly HttpClient _httpClient;

    public PlacesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PlacesResponse?> GetPlacesAsync(PlaceFilterParams filters)
    {
        // Объявляем переменную url здесь, чтобы видеть её в блоке catch
        string url = "";

        try
        {
            var queryString = BuildQueryString(filters);
            // Формируем относительный путь. BaseAddress (например http://10.0.2.2:5000/) уже задан в HttpClient
            url = string.IsNullOrEmpty(queryString) ? "places/filters" : $"places/filters?{queryString}";

            // --- БЛОК 1: ПОПЫТКА СЕТЕВОГО ЗАПРОСА ---
            HttpResponseMessage response;
            try
            {
                // Здесь часто падает из-за SSL или неправильного IP
                response = await _httpClient.GetAsync(url);
            }
            catch (HttpRequestException netEx)
            {
                // ПОСТАВЬТЕ БРЕЙКПОИНТ ЗДЕСЬ (строка ниже)
                var socketError = netEx.InnerException?.Message; // Тут будет "Connection refused" или "SSL error"
                System.Diagnostics.Debug.WriteLine($"Network Error: {netEx.Message} | Inner: {socketError}");
                throw; // Прокидываем ошибку дальше, чтобы увидеть её
            }

            // --- БЛОК 2: ПРОВЕРКА СТАТУСА ---
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // ПОСТАВЬТЕ БРЕЙКПОИНТ ЗДЕСЬ
                System.Diagnostics.Debug.WriteLine($"API Error ({response.StatusCode}): {errorContent}");
                return null; // Или throw new Exception($"API Error: {errorContent}");
            }

            // --- БЛОК 3: ЧТЕНИЕ ЗАГОЛОВКОВ ---
            long totalCount = 0;
            if (response.Headers.TryGetValues("X-Total-Count", out var totalCountValues))
            {
                long.TryParse(totalCountValues.FirstOrDefault(), out totalCount);
            }

            // --- БЛОК 4: ДЕСЕРИАЛИЗАЦИЯ JSON ---
            List<Place>? places;
            try
            {
                places = await response.Content.ReadFromJsonAsync<List<Place>>();
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                // ПОСТАВЬТЕ БРЕЙКПОИНТ ЗДЕСЬ
                // Если формат JSON не совпадает с моделью C#
                System.Diagnostics.Debug.WriteLine($"JSON Error: {jsonEx.Message}");
                var rawJson = await response.Content.ReadAsStringAsync(); // Посмотреть, что реально пришло
                System.Diagnostics.Debug.WriteLine($"Raw JSON: {rawJson}");
                return null;
            }

            if (places == null) return null;

            // Формируем ответ
            int perPageValue = filters.PerPage > 0 ? filters.PerPage : 20;
            int pageValue = filters.Page > 0 ? filters.Page : 1;

            return new PlacesResponse
            {
                Places = places,
                TotalCount = totalCount,
                Page = pageValue,
                PerPage = perPageValue,
                TotalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / perPageValue) : 1
            };
        }
        catch (Exception ex)
        {
            // ГЛОБАЛЬНЫЙ ОТЛОВ
            // ПОСТАВЬТЕ БРЕЙКПОИНТ ЗДЕСЬ
            var msg = ex.Message;
            var inner = ex.InnerException?.Message; // Самое важное!
            System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR in GetPlacesAsync: {msg}");
            System.Diagnostics.Debug.WriteLine($"Inner Exception: {inner}");

            // Для MVP можно вернуть null, но лучше видеть ошибку
            return null;
        }
    }

    public async Task<Place?> GetPlaceByIdAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Place>($"places/{id}");
            return response;
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Error fetching place: {ex.Message}");
            return null;
        }
    }

    public async Task<Place?> GetPlaceByUrlAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Place>($"places/url/{url}");
            return response;
        }
        catch (Exception ex)
        {
            // Логгируем ошибку, если место по URL не найдено или API вернул ошибку
            //Console.WriteLine($"Error fetching place by URL ({url}): {ex.Message}");
            return null; // Возвращаем null, чтобы страница PlaceDetails могла показать "не найдено"
        }
    }

    private string BuildQueryString(PlaceFilterParams filters)
    {
        var queryParams = new List<string>();

        // Добавляем только непустые параметры

        // Текстовый поиск
        if (!string.IsNullOrWhiteSpace(filters.Q))
            queryParams.Add($"q={Uri.EscapeDataString(filters.Q)}");

        // Географические фильтры
        if (!string.IsNullOrWhiteSpace(filters.Province))
            queryParams.Add($"province={Uri.EscapeDataString(filters.Province)}");

        if (!string.IsNullOrWhiteSpace(filters.City))
            queryParams.Add($"city={Uri.EscapeDataString(filters.City)}");

        // Основные фильтры
        if (!string.IsNullOrWhiteSpace(filters.Name))
            queryParams.Add($"name={Uri.EscapeDataString(filters.Name)}");

        if (!string.IsNullOrWhiteSpace(filters.Url))
            queryParams.Add($"url={Uri.EscapeDataString(filters.Url)}");

        if (!string.IsNullOrWhiteSpace(filters.Category))
            queryParams.Add($"category={Uri.EscapeDataString(filters.Category)}");

        if (!string.IsNullOrWhiteSpace(filters.Status))
            queryParams.Add($"status={Uri.EscapeDataString(filters.Status)}");

        // Теги (список через запятую)
        if (filters.Tags?.Any() == true)
        {
            var tagsString = string.Join(",", filters.Tags);
            queryParams.Add($"tags={Uri.EscapeDataString(tagsString)}");
            // Всегда принудительно используем "all" (AND) логику,
            // если выбраны хоть какие-то теги.
            queryParams.Add($"tagsMode=all");
        }

        
        //if (filters.Tags?.Any() == true || filters.TagsMode != "any")
        //    queryParams.Add($"tagsMode={filters.TagsMode}");

        // Геопространственный поиск
        if (filters.Latitude.HasValue)
            queryParams.Add($"latitude={filters.Latitude.Value}");

        if (filters.Longitude.HasValue)
            queryParams.Add($"longitude={filters.Longitude.Value}");

        if (filters.Distance.HasValue)
            queryParams.Add($"distance={filters.Distance.Value}");

        // Фильтр по времени работы
        if (filters.IsOpen.HasValue)
            queryParams.Add($"isOpen={filters.IsOpen.Value.ToString().ToLower()}");

        // Пагинация - отправляем только если отличаются от значений по умолчанию
        if (filters.Page > 1)
            queryParams.Add($"page={filters.Page}");

        if (filters.PerPage != 20) // 20 - значение по умолчанию в API
            queryParams.Add($"perPage={filters.PerPage}");

        // Сортировка - отправляем только если отличаются от значений по умолчанию
        if (filters.SortField != "name")
            queryParams.Add($"sortField={filters.SortField}");

        if (filters.SortOrder != "ASC")
            queryParams.Add($"sortOrder={filters.SortOrder}");

        return string.Join("&", queryParams);
    }
}