using PhoneCompare.Models;

namespace PhoneCompare.Services;

public interface IFavoritesService
{
    Task<List<Favorite>> GetFavoritesAsync(string userId, string idToken);
    Task<bool> AddFavoriteAsync(Favorite favorite, string idToken);
    Task<bool> RemoveFavoriteAsync(string favoriteId, string userId, string idToken);
    Task<bool> IsFavoriteAsync(string userId, string phoneSlug, string idToken);
}
