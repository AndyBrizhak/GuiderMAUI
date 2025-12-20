using GuiderMAUI.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuiderMAUI.Shared.Services
{
    public interface IProvincesService
    {
        /// <summary>
        /// Получает список активных провинций (имя + slug) на основе выбранной категории.
        /// </summary>
        /// <param name="category">Текущая категория (опционально).</param>
        /// <returns>Список объектов провинций.</returns>
        Task<List<ProvinceDto>> GetActiveProvincesAsync(string? category = null);
    }
}
