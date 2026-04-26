using PhoneCompare.Models;

namespace PhoneCompare.Services;

public interface IPhoneApiService
{
    Task<List<Brand>> GetBrandsAsync();
    Task<List<Phone>> GetPhonesByBrandAsync(string brandSlug);
    Task<PhoneDetail?> GetPhoneDetailAsync(string phoneSlug);
    Task<List<Phone>> SearchPhonesAsync(string query);
    Task<List<Phone>> GetLatestPhonesAsync();
    Task<List<Phone>> GetTopPhonesAsync();
}
