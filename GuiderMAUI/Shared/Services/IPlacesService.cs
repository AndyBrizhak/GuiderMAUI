using GuiderMAUI.Shared.Models;


namespace GuiderMAUI.Shared.Services
{
    public interface IPlacesService
    {
        Task<PlacesResponse?> GetPlacesAsync(PlaceFilterParams filters);
        Task<Place?> GetPlaceByIdAsync(string id);
        Task<Place?> GetPlaceByUrlAsync(string url);
    }
}
