using PhoneCompare.Models;

namespace PhoneCompare.Services;

/// <summary>
/// Automatically saves scraper results to Firestore after successful scrapes.
/// </summary>
public class PhoneSyncService
{
    private readonly FirestorePhoneService _firestore;
    private readonly IAuthService _auth;

    public PhoneSyncService(FirestorePhoneService firestore, IAuthService auth)
    {
        _firestore = firestore;
        _auth = auth;
    }

    private bool CanSync => _auth.IsLoggedIn;

    /// <summary>
    /// Save a list of brands to Firestore in the background.
    /// </summary>
    public async Task SyncBrandsAsync(List<Brand> brands)
    {
        if (!CanSync || brands.Count == 0) return;

        foreach (var brand in brands)
        {
            try
            {
                await _firestore.SaveBrandAsync(brand);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Sync] Brand {brand.Name} failed: {ex.Message}");
            }
        }
        System.Diagnostics.Debug.WriteLine($"[Sync] Synced {brands.Count} brands to Firestore");
    }

    /// <summary>
    /// Save a list of phones to Firestore in the background.
    /// </summary>
    public async Task SyncPhonesAsync(List<Phone> phones)
    {
        if (!CanSync || phones.Count == 0) return;

        foreach (var phone in phones)
        {
            try
            {
                await _firestore.SavePhoneAsync(phone);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Sync] Phone {phone.Name} failed: {ex.Message}");
            }
        }
        System.Diagnostics.Debug.WriteLine($"[Sync] Synced {phones.Count} phones to Firestore");
    }

    /// <summary>
    /// Save a phone detail to Firestore in the background.
    /// </summary>
    public async Task SyncPhoneDetailAsync(PhoneDetail detail)
    {
        if (!CanSync || detail == null) return;

        try
        {
            await _firestore.SavePhoneDetailAsync(detail, "scraper");
            System.Diagnostics.Debug.WriteLine($"[Sync] Synced detail for {detail.Name} to Firestore");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Sync] Detail {detail.Name} failed: {ex.Message}");
        }
    }
}
