using CommunityToolkit.Mvvm.ComponentModel;
using PhoneCompare.Models;

namespace PhoneCompare.Services;

public partial class CompareStateService : ObservableObject
{
    [ObservableProperty] private List<Phone> _compareList = [];

    public bool CanCompare => CompareList.Count == 2;
    public int CompareCount => CompareList.Count;

    public void Toggle(Phone phone)
    {
        if (phone == null) return;
        var list = new List<Phone>(CompareList);
        var existing = list.FirstOrDefault(p => p.Slug == phone.Slug);

        if (existing != null)
            list.Remove(existing);
        else if (list.Count < 2)
            list.Add(phone);
        else
        {
            list.RemoveAt(0);
            list.Add(phone);
        }

        CompareList = list;
        OnPropertyChanged(nameof(CanCompare));
        OnPropertyChanged(nameof(CompareCount));
    }

    public void Clear()
    {
        CompareList = [];
        OnPropertyChanged(nameof(CanCompare));
        OnPropertyChanged(nameof(CompareCount));
    }

    public bool Contains(string slug) =>
        CompareList.Any(p => p.Slug == slug);
}
