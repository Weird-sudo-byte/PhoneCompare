using PhoneCompare.Models;

namespace PhoneCompare.Services;

/// <summary>
/// Seeds MockPhoneData into Firestore collections.
/// Idempotent — safe to run multiple times (uses PATCH/upsert).
/// </summary>
public class PhoneDataSeeder
{
    private readonly FirestorePhoneService _firestore;

    public event Action<string>? OnProgress;
    public event Action<int, int>? OnProgressPercent; // (current, total)

    public PhoneDataSeeder(FirestorePhoneService firestore)
    {
        _firestore = firestore;
    }

    /// <summary>
    /// Seed all mock data (brands, phones, phone details) into Firestore.
    /// Returns (brandsOk, phonesOk, detailsOk) counts.
    /// </summary>
    public async Task<(int Brands, int Phones, int Details, List<string> Errors)> SeedAllAsync(
        CancellationToken ct = default)
    {
        var errors = new List<string>();
        int brandsOk = 0, phonesOk = 0, detailsOk = 0;

        var brands = MockPhoneData.GetBrands();
        var phones = MockPhoneData.GetPhones();

        int total = brands.Count + phones.Count + phones.Count; // brands + phones + details
        int current = 0;

        // ── Brands ──────────────────────────────────────────────────────
        Report("Seeding brands...");
        foreach (var brand in brands)
        {
            if (ct.IsCancellationRequested) break;
            try
            {
                var ok = await _firestore.SaveBrandAsync(brand);
                if (ok) brandsOk++;
                else errors.Add($"Brand failed: {brand.Name}");
            }
            catch (Exception ex)
            {
                errors.Add($"Brand {brand.Name}: {ex.Message}");
            }
            current++;
            OnProgressPercent?.Invoke(current, total);
        }
        Report($"Brands done: {brandsOk}/{brands.Count}");

        // ── Phones ──────────────────────────────────────────────────────
        Report("Seeding phones...");
        foreach (var phone in phones)
        {
            if (ct.IsCancellationRequested) break;
            try
            {
                var ok = await _firestore.SavePhoneAsync(phone);
                if (ok) phonesOk++;
                else errors.Add($"Phone failed: {phone.Name}");
            }
            catch (Exception ex)
            {
                errors.Add($"Phone {phone.Name}: {ex.Message}");
            }
            current++;
            OnProgressPercent?.Invoke(current, total);
        }
        Report($"Phones done: {phonesOk}/{phones.Count}");

        // ── Phone Details ───────────────────────────────────────────────
        Report("Seeding phone details...");
        foreach (var phone in phones)
        {
            if (ct.IsCancellationRequested) break;
            try
            {
                var detail = MockPhoneData.BuildDetail(phone.Slug);
                var ok = await _firestore.SavePhoneDetailAsync(detail, "mock");
                if (ok) detailsOk++;
                else errors.Add($"Detail failed: {phone.Name}");
            }
            catch (Exception ex)
            {
                errors.Add($"Detail {phone.Name}: {ex.Message}");
            }
            current++;
            OnProgressPercent?.Invoke(current, total);
        }
        Report($"Details done: {detailsOk}/{phones.Count}");

        Report(ct.IsCancellationRequested
            ? "Seeding cancelled."
            : $"Seeding complete! {brandsOk} brands, {phonesOk} phones, {detailsOk} details.");

        return (brandsOk, phonesOk, detailsOk, errors);
    }

    private void Report(string msg)
    {
        System.Diagnostics.Debug.WriteLine($"[Seeder] {msg}");
        OnProgress?.Invoke(msg);
    }
}
