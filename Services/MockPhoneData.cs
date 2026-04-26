using PhoneCompare.Models;

namespace PhoneCompare.Services;

public static class MockPhoneData
{
    // ── Retailer Prices ─────────────────────────────────────────────────────
    public static List<RetailerPrice> GetRetailerPrices(string slug, decimal basePrice)
    {
        if (basePrice <= 0) return [];
        
        var random = new Random(slug.GetHashCode()); // Deterministic per phone
        
        var retailers = new List<RetailerPrice>
        {
            new()
            {
                Retailer = "Lazada",
                LogoEmoji = "🛒",
                Price = basePrice * (0.95m + (decimal)random.NextDouble() * 0.10m),
                Url = $"https://www.lazada.com.ph/catalog/?q={Uri.EscapeDataString(slug.Replace("-", " "))}",
                InStock = random.Next(100) > 15
            },
            new()
            {
                Retailer = "Shopee",
                LogoEmoji = "🧡",
                Price = basePrice * (0.93m + (decimal)random.NextDouble() * 0.12m),
                Url = $"https://shopee.ph/search?keyword={Uri.EscapeDataString(slug.Replace("-", " "))}",
                InStock = random.Next(100) > 10
            },
            new()
            {
                Retailer = "Official Store",
                LogoEmoji = "🏪",
                Price = basePrice,
                Url = $"https://www.{slug.Split('-')[0]}.com/ph/smartphones",
                InStock = true
            },
            new()
            {
                Retailer = "Abenson",
                LogoEmoji = "🏬",
                Price = basePrice * (0.98m + (decimal)random.NextDouble() * 0.05m),
                Url = $"https://www.abenson.com/catalogsearch/result/?q={Uri.EscapeDataString(slug.Replace("-", " "))}",
                InStock = random.Next(100) > 25
            }
        };
        
        // Round prices and mark best price
        foreach (var r in retailers)
            r.Price = Math.Round(r.Price / 100) * 100; // Round to nearest 100
        
        var minPrice = retailers.Where(r => r.InStock).Min(r => r.Price);
        foreach (var r in retailers.Where(r => r.InStock && r.Price == minPrice))
            r.IsBestPrice = true;
        
        return retailers.OrderBy(r => r.InStock ? 0 : 1).ThenBy(r => r.Price).ToList();
    }

    // ── Brands ───────────────────────────────────────────────────────────────
    public static List<Brand> GetBrands() =>
    [
        new() { Id = 1,  Name = "Samsung",  Slug = "samsung"  },
        new() { Id = 2,  Name = "Apple",    Slug = "apple"    },
        new() { Id = 3,  Name = "Google",   Slug = "google"   },
        new() { Id = 4,  Name = "OnePlus",  Slug = "oneplus"  },
        new() { Id = 5,  Name = "Xiaomi",   Slug = "xiaomi"   },
        new() { Id = 6,  Name = "Sony",     Slug = "sony"     },
        new() { Id = 7,  Name = "Motorola", Slug = "motorola" },
        new() { Id = 8,  Name = "Oppo",     Slug = "oppo"     },
        new() { Id = 9,  Name = "Vivo",     Slug = "vivo"     },
        new() { Id = 10, Name = "Realme",   Slug = "realme"   },
    ];

    // ── Phone list ───────────────────────────────────────────────────────────
    public static List<Phone> GetPhones() =>
    [
        // Samsung
        new() { Id=1,  Name="Samsung Galaxy S24 Ultra",   Slug="samsung-galaxy-s24-ultra",   BrandName="Samsung",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/samsung-galaxy-s24-ultra-5g-sm-s928.jpg", PricePHP=84990 },
        new() { Id=2,  Name="Samsung Galaxy S24+",        Slug="samsung-galaxy-s24-plus",    BrandName="Samsung",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/samsung-galaxy-s24-plus-5g.jpg", PricePHP=62990 },
        new() { Id=3,  Name="Samsung Galaxy S24",         Slug="samsung-galaxy-s24",         BrandName="Samsung",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/samsung-galaxy-s24-5g.jpg", PricePHP=49990 },
        new() { Id=4,  Name="Samsung Galaxy Z Fold5",     Slug="samsung-galaxy-z-fold5",     BrandName="Samsung",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/samsung-galaxy-z-fold5.jpg", PricePHP=99990 },
        new() { Id=5,  Name="Samsung Galaxy Z Flip5",     Slug="samsung-galaxy-z-flip5",     BrandName="Samsung",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/samsung-galaxy-z-flip5.jpg", PricePHP=59990 },
        new() { Id=6,  Name="Samsung Galaxy A55",         Slug="samsung-galaxy-a55",         BrandName="Samsung",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/samsung-galaxy-a55.jpg", PricePHP=24990 },
        new() { Id=7,  Name="Samsung Galaxy A35",         Slug="samsung-galaxy-a35",         BrandName="Samsung",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/samsung-galaxy-a35.jpg", PricePHP=18990 },
        new() { Id=8,  Name="Samsung Galaxy A54",         Slug="samsung-galaxy-a54",         BrandName="Samsung",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/samsung-galaxy-a54.jpg", PricePHP=22990 },

        // Apple
        new() { Id=10, Name="Apple iPhone 15 Pro Max",    Slug="apple-iphone-15-pro-max",    BrandName="Apple",    ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/apple-iphone-15-pro-max.jpg", PricePHP=84990 },
        new() { Id=11, Name="Apple iPhone 15 Pro",        Slug="apple-iphone-15-pro",        BrandName="Apple",    ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/apple-iphone-15-pro.jpg", PricePHP=69990 },
        new() { Id=12, Name="Apple iPhone 15 Plus",       Slug="apple-iphone-15-plus",       BrandName="Apple",    ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/apple-iphone-15-plus.jpg", PricePHP=59990 },
        new() { Id=13, Name="Apple iPhone 15",            Slug="apple-iphone-15",            BrandName="Apple",    ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/apple-iphone-15.jpg", PricePHP=54990 },
        new() { Id=14, Name="Apple iPhone 14 Pro Max",    Slug="apple-iphone-14-pro-max",    BrandName="Apple",    ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/apple-iphone-14-pro-max.jpg", PricePHP=74990 },
        new() { Id=15, Name="Apple iPhone 14",            Slug="apple-iphone-14",            BrandName="Apple",    ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/apple-iphone-14.jpg", PricePHP=49990 },
        new() { Id=16, Name="Apple iPhone SE (2022)",     Slug="apple-iphone-se-2022",       BrandName="Apple",    ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/apple-iphone-se-2022.jpg", PricePHP=29990 },

        // Google
        new() { Id=20, Name="Google Pixel 9 Pro",         Slug="google-pixel-9-pro",         BrandName="Google",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/google-pixel-9-pro.jpg", PricePHP=59990 },
        new() { Id=21, Name="Google Pixel 9",             Slug="google-pixel-9",             BrandName="Google",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/google-pixel-9.jpg", PricePHP=49990 },
        new() { Id=22, Name="Google Pixel 8 Pro",         Slug="google-pixel-8-pro",         BrandName="Google",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/google-pixel-8-pro.jpg", PricePHP=54990 },
        new() { Id=23, Name="Google Pixel 8",             Slug="google-pixel-8",             BrandName="Google",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/google-pixel-8.jpg", PricePHP=39990 },
        new() { Id=24, Name="Google Pixel 8a",            Slug="google-pixel-8a",            BrandName="Google",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/google-pixel-8a.jpg", PricePHP=27990 },
        new() { Id=25, Name="Google Pixel Fold",          Slug="google-pixel-fold",          BrandName="Google",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/google-pixel-fold.jpg", PricePHP=94990 },

        // OnePlus
        new() { Id=30, Name="OnePlus 12",                 Slug="oneplus-12",                 BrandName="OnePlus",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/oneplus-12-5g.jpg", PricePHP=49990 },
        new() { Id=31, Name="OnePlus 12R",                Slug="oneplus-12r",                BrandName="OnePlus",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/oneplus-12r.jpg", PricePHP=29990 },
        new() { Id=32, Name="OnePlus 11",                 Slug="oneplus-11",                 BrandName="OnePlus",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/oneplus-11-5g.jpg", PricePHP=39990 },
        new() { Id=33, Name="OnePlus Open",               Slug="oneplus-open",               BrandName="OnePlus",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/oneplus-open.jpg", PricePHP=89990 },
        new() { Id=34, Name="OnePlus Nord 4",             Slug="oneplus-nord-4",             BrandName="OnePlus",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/oneplus-nord-4.jpg", PricePHP=24990 },
        new() { Id=35, Name="OnePlus Nord CE 3",          Slug="oneplus-nord-ce-3",          BrandName="OnePlus",  ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/oneplus-nord-ce-3-5g.jpg", PricePHP=18990 },

        // Xiaomi
        new() { Id=40, Name="Xiaomi 14 Ultra",            Slug="xiaomi-14-ultra",            BrandName="Xiaomi",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/xiaomi-14-ultra.jpg", PricePHP=69990 },
        new() { Id=41, Name="Xiaomi 14 Pro",              Slug="xiaomi-14-pro",              BrandName="Xiaomi",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/xiaomi-14-pro.jpg", PricePHP=49990 },
        new() { Id=42, Name="Xiaomi 14",                  Slug="xiaomi-14",                  BrandName="Xiaomi",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/xiaomi-14.jpg", PricePHP=39990 },
        new() { Id=43, Name="Xiaomi 13T Pro",             Slug="xiaomi-13t-pro",             BrandName="Xiaomi",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/xiaomi-13t-pro.jpg", PricePHP=29990 },
        new() { Id=44, Name="Xiaomi Redmi Note 13 Pro+",  Slug="xiaomi-redmi-note-13-pro-plus", BrandName="Xiaomi", ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/xiaomi-redmi-note-13-pro-plus-5g.jpg", PricePHP=18990 },
        new() { Id=45, Name="Xiaomi Poco X6 Pro",         Slug="xiaomi-poco-x6-pro",         BrandName="Xiaomi",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/xiaomi-poco-x6-pro.jpg", PricePHP=16990 },
        new() { Id=46, Name="Xiaomi Redmi 13C",           Slug="xiaomi-redmi-13c",           BrandName="Xiaomi",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/xiaomi-redmi-13c.jpg", PricePHP=6990 },

        // Sony
        new() { Id=50, Name="Sony Xperia 1 VI",           Slug="sony-xperia-1-vi",           BrandName="Sony",     ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/sony-xperia-1-vi.jpg", PricePHP=74990 },
        new() { Id=51, Name="Sony Xperia 1 V",            Slug="sony-xperia-1-v",            BrandName="Sony",     ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/sony-xperia-1-v.jpg", PricePHP=69990 },
        new() { Id=52, Name="Sony Xperia 5 V",            Slug="sony-xperia-5-v",            BrandName="Sony",     ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/sony-xperia-5-v.jpg", PricePHP=54990 },
        new() { Id=53, Name="Sony Xperia 10 VI",          Slug="sony-xperia-10-vi",          BrandName="Sony",     ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/sony-xperia-10-vi.jpg", PricePHP=24990 },

        // Motorola
        new() { Id=60, Name="Motorola Edge 50 Ultra",     Slug="motorola-edge-50-ultra",     BrandName="Motorola", ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/motorola-edge-50-ultra.jpg", PricePHP=44990 },
        new() { Id=61, Name="Motorola Edge 50 Pro",       Slug="motorola-edge-50-pro",       BrandName="Motorola", ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/motorola-edge-50-pro.jpg", PricePHP=32990 },
        new() { Id=62, Name="Motorola Razr 40 Ultra",     Slug="motorola-razr-40-ultra",     BrandName="Motorola", ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/motorola-razr-40-ultra.jpg", PricePHP=54990 },
        new() { Id=63, Name="Motorola Moto G85",          Slug="motorola-moto-g85",          BrandName="Motorola", ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/motorola-moto-g85.jpg", PricePHP=14990 },

        // Oppo
        new() { Id=70, Name="Oppo Find X7 Ultra",         Slug="oppo-find-x7-ultra",         BrandName="Oppo",     ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/oppo-find-x7-ultra.jpg", PricePHP=64990 },
        new() { Id=71, Name="Oppo Find X7",               Slug="oppo-find-x7",               BrandName="Oppo",     ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/oppo-find-x7.jpg", PricePHP=44990 },
        new() { Id=72, Name="Oppo Reno 12 Pro",           Slug="oppo-reno-12-pro",           BrandName="Oppo",     ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/oppo-reno-12-pro-5g.jpg", PricePHP=27990 },
        new() { Id=73, Name="Oppo A3 Pro",                Slug="oppo-a3-pro",                BrandName="Oppo",     ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/oppo-a3-pro.jpg", PricePHP=14990 },

        // Vivo
        new() { Id=80, Name="Vivo X100 Ultra",            Slug="vivo-x100-ultra",            BrandName="Vivo",     ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/vivo-x100-ultra.jpg", PricePHP=64990 },
        new() { Id=81, Name="Vivo X100 Pro",              Slug="vivo-x100-pro",              BrandName="Vivo",     ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/vivo-x100-pro.jpg", PricePHP=54990 },
        new() { Id=82, Name="Vivo V30 Pro",               Slug="vivo-v30-pro",               BrandName="Vivo",     ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/vivo-v30-pro.jpg", PricePHP=29990 },
        new() { Id=83, Name="Vivo Y100 Pro",              Slug="vivo-y100-pro",              BrandName="Vivo",     ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/vivo-y100-pro-5g.jpg", PricePHP=16990 },

        // Realme
        new() { Id=90, Name="Realme GT 6",                Slug="realme-gt-6",                BrandName="Realme",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/realme-gt-6.jpg", PricePHP=29990 },
        new() { Id=91, Name="Realme GT 5 Pro",            Slug="realme-gt-5-pro",            BrandName="Realme",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/realme-gt5-pro.jpg", PricePHP=34990 },
        new() { Id=92, Name="Realme 12 Pro+",             Slug="realme-12-pro-plus",         BrandName="Realme",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/realme-12-pro-plus-5g.jpg", PricePHP=22990 },
        new() { Id=93, Name="Realme Narzo 70 Pro",        Slug="realme-narzo-70-pro",        BrandName="Realme",   ImageUrl="https://fdn2.gsmarena.com/vv/bigpic/realme-narzo-70-pro-5g.jpg", PricePHP=13990 },
    ];

    // ── Detail builder ───────────────────────────────────────────────────────
    public static PhoneDetail BuildDetail(string slug)
    {
        var phone = GetPhones().FirstOrDefault(p => p.Slug == slug);
        var name  = phone?.Name ?? slug;
        var image = phone?.ImageUrl ?? "";

        return slug switch
        {
            "samsung-galaxy-s24-ultra"  => MakeDetail(slug, name, image, SamsungS24UltraSpecs()),
            "samsung-galaxy-s24-plus"   => MakeDetail(slug, name, image, SamsungS24PlusSpecs()),
            "samsung-galaxy-s24"        => MakeDetail(slug, name, image, SamsungS24Specs()),
            "samsung-galaxy-z-fold5"    => MakeDetail(slug, name, image, SamsungZFold5Specs()),
            "samsung-galaxy-z-flip5"    => MakeDetail(slug, name, image, SamsungZFlip5Specs()),
            "samsung-galaxy-a55"        => MakeDetail(slug, name, image, SamsungA55Specs()),
            "apple-iphone-15-pro-max"   => MakeDetail(slug, name, image, IPhone15ProMaxSpecs()),
            "apple-iphone-15-pro"       => MakeDetail(slug, name, image, IPhone15ProSpecs()),
            "apple-iphone-15-plus"      => MakeDetail(slug, name, image, IPhone15PlusSpecs()),
            "apple-iphone-15"           => MakeDetail(slug, name, image, IPhone15Specs()),
            "apple-iphone-14-pro-max"   => MakeDetail(slug, name, image, IPhone14ProMaxSpecs()),
            "google-pixel-9-pro"        => MakeDetail(slug, name, image, Pixel9ProSpecs()),
            "google-pixel-8-pro"        => MakeDetail(slug, name, image, Pixel8ProSpecs()),
            "google-pixel-8"            => MakeDetail(slug, name, image, Pixel8Specs()),
            "google-pixel-8a"           => MakeDetail(slug, name, image, Pixel8aSpecs()),
            "oneplus-12"                => MakeDetail(slug, name, image, OnePlus12Specs()),
            "oneplus-12r"               => MakeDetail(slug, name, image, OnePlus12RSpecs()),
            "oneplus-open"              => MakeDetail(slug, name, image, OnePlusOpenSpecs()),
            "xiaomi-14-ultra"           => MakeDetail(slug, name, image, Xiaomi14UltraSpecs()),
            "xiaomi-14-pro"             => MakeDetail(slug, name, image, Xiaomi14ProSpecs()),
            "xiaomi-14"                 => MakeDetail(slug, name, image, Xiaomi14Specs()),
            "sony-xperia-1-vi"          => MakeDetail(slug, name, image, SonyXperia1VISpecs()),
            "sony-xperia-5-v"           => MakeDetail(slug, name, image, SonyXperia5VSpecs()),
            "motorola-edge-50-ultra"    => MakeDetail(slug, name, image, MotoEdge50UltraSpecs()),
            "motorola-razr-40-ultra"    => MakeDetail(slug, name, image, MotoRazr40UltraSpecs()),
            "oppo-find-x7-ultra"        => MakeDetail(slug, name, image, OppoFindX7UltraSpecs()),
            "vivo-x100-pro"             => MakeDetail(slug, name, image, VivoX100ProSpecs()),
            "realme-gt-6"               => MakeDetail(slug, name, image, RealmeGT6Specs()),
            _                           => MakeDetail(slug, name, image, GenericSpecs(phone?.BrandName ?? ""))
        };
    }

    private static PhoneDetail MakeDetail(string slug, string name, string image, List<SpecGroup> specs) =>
        new() { Slug = slug, Name = name, ImageUrl = image, Specifications = specs };

    // ── Spec helpers ─────────────────────────────────────────────────────────
    private static SpecItem SI(string key, params string[] vals) =>
        new() { Key = key, Val = [.. vals] };

    private static SpecGroup SG(string title, params SpecItem[] specs) =>
        new() { Title = title, Specs = [.. specs] };

    // ── Samsung specs ────────────────────────────────────────────────────────
    private static List<SpecGroup> SamsungS24UltraSpecs() =>
    [
        SG("Display",      SI("Type","Dynamic AMOLED 2X, 120Hz, HDR10+, 2600 nits"), SI("Size","6.8 inches, 118.3 cm²"), SI("Resolution","1440 x 3120 pixels (~505 ppi)")),
        SG("Platform",     SI("OS","Android 14, One UI 6.1"), SI("Chipset","Snapdragon 8 Gen 3 (4 nm)"), SI("CPU","Octa-core 3.39+3.1+2.9+2.2 GHz"), SI("GPU","Adreno 750")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","256GB 12GB RAM / 512GB 12GB RAM / 1TB 12GB RAM")),
        SG("Main Camera",  SI("Quad","200MP f/1.7 (wide), 50MP f/3.4 periscope, 10MP f/2.4 telephoto, 12MP f/2.2 ultrawide"), SI("Video","8K@30fps, 4K@60fps, 1080p@240fps")),
        SG("Front Camera", SI("Single","12 MP, f/2.2, 26mm")),
        SG("Battery",      SI("Type","Li-Ion 5000 mAh, non-removable"), SI("Charging","45W wired, 15W wireless, 4.5W reverse")),
        SG("Connectivity", SI("5G","Yes"), SI("Wi-Fi","Wi-Fi 7"), SI("Bluetooth","5.3"), SI("NFC","Yes"), SI("USB","USB Type-C 3.2")),
        SG("Misc",         SI("Colors","Titanium Black, Gray, Violet, Yellow"), SI("Price","$1299 / €1449 / £1299")),
    ];

    private static List<SpecGroup> SamsungS24PlusSpecs() =>
    [
        SG("Display",      SI("Type","Dynamic AMOLED 2X, 120Hz, HDR10+"), SI("Size","6.7 inches"), SI("Resolution","1440 x 3088 pixels (~511 ppi)")),
        SG("Platform",     SI("OS","Android 14, One UI 6.1"), SI("Chipset","Snapdragon 8 Gen 3 (4 nm)"), SI("CPU","Octa-core"), SI("GPU","Adreno 750")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","256GB 12GB RAM / 512GB 12GB RAM")),
        SG("Main Camera",  SI("Triple","50MP f/1.8 (wide), 10MP f/2.4 telephoto, 12MP f/2.2 ultrawide"), SI("Video","8K@30fps, 4K@60fps")),
        SG("Front Camera", SI("Single","12 MP, f/2.2")),
        SG("Battery",      SI("Type","Li-Ion 4900 mAh"), SI("Charging","45W wired, 15W wireless")),
        SG("Misc",         SI("Colors","Cobalt Violet, Onyx Black, Jade Green, Sandstone Orange"), SI("Price","$999 / €1149 / £1049")),
    ];

    private static List<SpecGroup> SamsungS24Specs() =>
    [
        SG("Display",      SI("Type","Dynamic AMOLED 2X, 120Hz"), SI("Size","6.2 inches"), SI("Resolution","1080 x 2340 pixels (~416 ppi)")),
        SG("Platform",     SI("OS","Android 14, One UI 6.1"), SI("Chipset","Snapdragon 8 Gen 3 (4 nm)"), SI("GPU","Adreno 750")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","128GB 8GB RAM / 256GB 8GB RAM")),
        SG("Main Camera",  SI("Triple","50MP f/1.8 (wide), 10MP f/2.4, 12MP f/2.2 ultrawide"), SI("Video","8K@30fps, 4K@60fps")),
        SG("Battery",      SI("Type","Li-Ion 4000 mAh"), SI("Charging","25W wired, 15W wireless")),
        SG("Misc",         SI("Colors","Cobalt Violet, Onyx Black, Marble Gray, Amber Yellow"), SI("Price","$799 / €899 / £849")),
    ];

    private static List<SpecGroup> SamsungZFold5Specs() =>
    [
        SG("Display",      SI("Type","Foldable Dynamic AMOLED 2X, 120Hz"), SI("Main Size","7.6 inches"), SI("Cover Size","6.2 inches"), SI("Resolution","1812 x 2176 pixels (main)")),
        SG("Platform",     SI("OS","Android 13, One UI 5.1.1"), SI("Chipset","Snapdragon 8 Gen 2 (4 nm)"), SI("GPU","Adreno 740")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","256GB 12GB RAM / 512GB 12GB RAM / 1TB 12GB RAM")),
        SG("Main Camera",  SI("Triple","50MP f/1.8, 12MP f/2.2 ultrawide, 10MP f/2.4 telephoto"), SI("Video","4K@60fps")),
        SG("Battery",      SI("Type","Li-Ion 4400 mAh"), SI("Charging","25W wired, 15W wireless")),
        SG("Misc",         SI("Colors","Icy Blue, Phantom Black, Cream"), SI("Price","$1799 / €1999 / £1749")),
    ];

    private static List<SpecGroup> SamsungZFlip5Specs() =>
    [
        SG("Display",      SI("Type","Foldable Dynamic AMOLED 2X, 120Hz"), SI("Main Size","6.7 inches"), SI("Cover Size","3.4 inches")),
        SG("Platform",     SI("OS","Android 13, One UI 5.1.1"), SI("Chipset","Snapdragon 8 Gen 2 (4 nm)"), SI("GPU","Adreno 740")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","256GB 8GB RAM / 512GB 8GB RAM")),
        SG("Main Camera",  SI("Dual","12MP f/1.8 (wide), 12MP f/2.2 ultrawide"), SI("Video","4K@60fps")),
        SG("Battery",      SI("Type","Li-Ion 3700 mAh"), SI("Charging","25W wired, 15W wireless")),
        SG("Misc",         SI("Colors","Mint, Lavender, Graphite, Cream, Blue, Gray"), SI("Price","$999 / €1099 / £1049")),
    ];

    private static List<SpecGroup> SamsungA55Specs() =>
    [
        SG("Display",      SI("Type","Super AMOLED, 120Hz"), SI("Size","6.6 inches"), SI("Resolution","1080 x 2340 pixels")),
        SG("Platform",     SI("OS","Android 14, One UI 6.1"), SI("Chipset","Exynos 1480 (4 nm)"), SI("GPU","Xclipse 540")),
        SG("Memory",       SI("Card slot","Yes (microSDXC)"), SI("Internal","128GB 8GB RAM / 256GB 8GB RAM / 256GB 12GB RAM")),
        SG("Main Camera",  SI("Triple","50MP f/1.8, 12MP f/2.2 ultrawide, 5MP f/2.4 macro"), SI("Video","4K@30fps")),
        SG("Battery",      SI("Type","Li-Ion 5000 mAh"), SI("Charging","25W wired")),
        SG("Misc",         SI("Colors","Awesome Iceblue, Navy, Lilac, Lemon"), SI("Price","$449 / €499 / £429")),
    ];

    // ── Apple specs ──────────────────────────────────────────────────────────
    private static List<SpecGroup> IPhone15ProMaxSpecs() =>
    [
        SG("Display",      SI("Type","LTPO Super Retina XDR OLED, 120Hz, Dolby Vision"), SI("Size","6.7 inches"), SI("Resolution","1290 x 2796 pixels (~460 ppi)")),
        SG("Platform",     SI("OS","iOS 17, upgradable to iOS 18"), SI("Chipset","Apple A17 Pro (3 nm)"), SI("CPU","Hexa-core 3.78+2.11 GHz"), SI("GPU","Apple 6-core GPU")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","256GB 8GB RAM / 512GB 8GB RAM / 1TB 8GB RAM")),
        SG("Main Camera",  SI("Triple","48MP f/1.8 (wide), 12MP f/2.8 periscope telephoto 5x, 12MP f/2.2 ultrawide"), SI("Video","4K@120fps ProRes, 4K@60fps")),
        SG("Front Camera", SI("Single","12 MP, f/1.9, autofocus")),
        SG("Battery",      SI("Type","Li-Ion 4441 mAh"), SI("Charging","27W wired, 15W MagSafe, 7.5W Qi")),
        SG("Connectivity", SI("5G","Yes"), SI("Wi-Fi","Wi-Fi 6E"), SI("Bluetooth","5.3"), SI("NFC","Yes"), SI("USB","USB Type-C 3.2 Gen 2")),
        SG("Misc",         SI("Colors","Natural Titanium, Blue Titanium, White Titanium, Black Titanium"), SI("Price","$1199 / €1449 / £1199")),
    ];

    private static List<SpecGroup> IPhone15ProSpecs() =>
    [
        SG("Display",      SI("Type","LTPO Super Retina XDR OLED, 120Hz, Dolby Vision"), SI("Size","6.1 inches"), SI("Resolution","1179 x 2556 pixels (~460 ppi)")),
        SG("Platform",     SI("OS","iOS 17"), SI("Chipset","Apple A17 Pro (3 nm)"), SI("GPU","Apple 6-core GPU")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","128GB 8GB RAM / 256GB 8GB RAM / 512GB 8GB RAM / 1TB 8GB RAM")),
        SG("Main Camera",  SI("Triple","48MP f/1.78 (wide), 12MP f/2.8 telephoto 3x, 12MP f/2.2 ultrawide"), SI("Video","4K@60fps, ProRes")),
        SG("Battery",      SI("Type","Li-Ion 3274 mAh"), SI("Charging","27W wired, 15W MagSafe")),
        SG("Misc",         SI("Colors","Natural Titanium, Blue Titanium, White Titanium, Black Titanium"), SI("Price","$999 / €1229 / £999")),
    ];

    private static List<SpecGroup> IPhone15PlusSpecs() =>
    [
        SG("Display",      SI("Type","Super Retina XDR OLED, 60Hz"), SI("Size","6.7 inches"), SI("Resolution","1290 x 2796 pixels")),
        SG("Platform",     SI("OS","iOS 17"), SI("Chipset","Apple A16 Bionic (4 nm)"), SI("GPU","Apple 5-core GPU")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","128GB 6GB RAM / 256GB 6GB RAM / 512GB 6GB RAM")),
        SG("Main Camera",  SI("Dual","48MP f/1.6 (wide), 12MP f/2.4 ultrawide"), SI("Video","4K@60fps")),
        SG("Battery",      SI("Type","Li-Ion 4383 mAh"), SI("Charging","27W wired, 15W MagSafe")),
        SG("Misc",         SI("Colors","Black, Blue, Green, Yellow, Pink"), SI("Price","$899 / €1129 / £899")),
    ];

    private static List<SpecGroup> IPhone15Specs() =>
    [
        SG("Display",      SI("Type","Super Retina XDR OLED, 60Hz"), SI("Size","6.1 inches"), SI("Resolution","1179 x 2556 pixels")),
        SG("Platform",     SI("OS","iOS 17"), SI("Chipset","Apple A16 Bionic (4 nm)"), SI("GPU","Apple 5-core GPU")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","128GB 6GB RAM / 256GB 6GB RAM / 512GB 6GB RAM")),
        SG("Main Camera",  SI("Dual","48MP f/1.6 (wide), 12MP f/2.4 ultrawide"), SI("Video","4K@60fps")),
        SG("Battery",      SI("Type","Li-Ion 3349 mAh"), SI("Charging","27W wired, 15W MagSafe")),
        SG("Misc",         SI("Colors","Black, Blue, Green, Yellow, Pink"), SI("Price","$799 / €979 / £799")),
    ];

    private static List<SpecGroup> IPhone14ProMaxSpecs() =>
    [
        SG("Display",      SI("Type","LTPO Super Retina XDR OLED, 120Hz, Always-On"), SI("Size","6.7 inches"), SI("Resolution","1290 x 2796 pixels")),
        SG("Platform",     SI("OS","iOS 16, upgradable to iOS 18"), SI("Chipset","Apple A16 Bionic (4 nm)"), SI("GPU","Apple 5-core GPU")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","128GB 6GB RAM / 256GB 6GB RAM / 512GB 6GB RAM / 1TB 6GB RAM")),
        SG("Main Camera",  SI("Triple","48MP f/1.78 (wide), 12MP f/2.8 telephoto 3x, 12MP f/2.2 ultrawide"), SI("Video","4K@60fps, Action mode")),
        SG("Battery",      SI("Type","Li-Ion 4323 mAh"), SI("Charging","27W wired, 15W MagSafe")),
        SG("Misc",         SI("Colors","Space Black, Silver, Gold, Deep Purple"), SI("Price","From $1099 (refurb)")),
    ];

    // ── Google specs ─────────────────────────────────────────────────────────
    private static List<SpecGroup> Pixel9ProSpecs() =>
    [
        SG("Display",      SI("Type","LTPO OLED, 120Hz, HDR10+, 3000 nits"), SI("Size","6.3 inches"), SI("Resolution","1280 x 2856 pixels (~495 ppi)")),
        SG("Platform",     SI("OS","Android 14"), SI("Chipset","Google Tensor G4 (4 nm)"), SI("CPU","Nona-core"), SI("GPU","Immortalis-G715s MC10")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","128GB 16GB RAM / 256GB 16GB RAM / 512GB 16GB RAM / 1TB 16GB RAM")),
        SG("Main Camera",  SI("Triple","50MP f/1.7 (wide), 48MP f/2.8 periscope 5x, 48MP f/1.7 ultrawide"), SI("Video","4K@60fps, 1080p@240fps")),
        SG("Front Camera", SI("Single","10.5 MP, f/2.2")),
        SG("Battery",      SI("Type","Li-Ion 4700 mAh"), SI("Charging","37W wired, 23W wireless, 12W reverse")),
        SG("Misc",         SI("Colors","Obsidian, Porcelain, Hazel, Rose Quartz"), SI("Price","$999 / €1099 / £999")),
    ];

    private static List<SpecGroup> Pixel8ProSpecs() =>
    [
        SG("Display",      SI("Type","LTPO OLED, 120Hz, HDR10+"), SI("Size","6.7 inches"), SI("Resolution","1344 x 2992 pixels (~489 ppi)")),
        SG("Platform",     SI("OS","Android 14"), SI("Chipset","Google Tensor G3 (4 nm)"), SI("GPU","Immortalis-G715s MC10")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","128GB 12GB RAM / 256GB 12GB RAM / 512GB 12GB RAM / 1TB 12GB RAM")),
        SG("Main Camera",  SI("Triple","50MP f/1.7 (wide), 48MP f/2.8 telephoto 5x, 48MP f/2.0 ultrawide"), SI("Video","4K@60fps, 1080p@240fps")),
        SG("Battery",      SI("Type","Li-Ion 5050 mAh"), SI("Charging","30W wired, 23W wireless, 12W reverse")),
        SG("Misc",         SI("Colors","Obsidian, Porcelain, Bay"), SI("Price","$999 / €1099 / £999")),
    ];

    private static List<SpecGroup> Pixel8Specs() =>
    [
        SG("Display",      SI("Type","OLED, 120Hz, HDR10+"), SI("Size","6.2 inches"), SI("Resolution","1080 x 2400 pixels (~429 ppi)")),
        SG("Platform",     SI("OS","Android 14"), SI("Chipset","Google Tensor G3 (4 nm)"), SI("GPU","Immortalis-G715s MC10")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","128GB 8GB RAM / 256GB 8GB RAM")),
        SG("Main Camera",  SI("Dual","50MP f/1.7 (wide), 12MP f/2.2 ultrawide"), SI("Video","4K@60fps, 1080p@240fps")),
        SG("Battery",      SI("Type","Li-Ion 4575 mAh"), SI("Charging","27W wired, 18W wireless")),
        SG("Misc",         SI("Colors","Obsidian, Hazel, Rose"), SI("Price","$699 / €799 / £699")),
    ];

    private static List<SpecGroup> Pixel8aSpecs() =>
    [
        SG("Display",      SI("Type","OLED, 120Hz"), SI("Size","6.1 inches"), SI("Resolution","1080 x 2400 pixels")),
        SG("Platform",     SI("OS","Android 14"), SI("Chipset","Google Tensor G3 (4 nm)")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","128GB 8GB RAM / 256GB 8GB RAM")),
        SG("Main Camera",  SI("Dual","64MP f/1.9 (wide), 13MP f/2.2 ultrawide"), SI("Video","4K@60fps")),
        SG("Battery",      SI("Type","Li-Ion 4492 mAh"), SI("Charging","18W wired, 18W wireless")),
        SG("Misc",         SI("Colors","Obsidian, Porcelain, Aloe, Bay"), SI("Price","$499 / €549 / £499")),
    ];

    // ── OnePlus specs ────────────────────────────────────────────────────────
    private static List<SpecGroup> OnePlus12Specs() =>
    [
        SG("Display",      SI("Type","LTPO AMOLED, 1-120Hz, Dolby Vision, HDR10+, 4500 nits"), SI("Size","6.82 inches"), SI("Resolution","1440 x 3168 pixels (~510 ppi)")),
        SG("Platform",     SI("OS","Android 14, OxygenOS 14"), SI("Chipset","Snapdragon 8 Gen 3 (4 nm)"), SI("GPU","Adreno 750")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","256GB 12GB RAM / 512GB 16GB RAM")),
        SG("Main Camera",  SI("Triple","50MP f/1.6 (wide, Hasselblad), 64MP f/2.6 periscope, 48MP f/2.2 ultrawide"), SI("Video","8K@24fps, 4K@60fps")),
        SG("Front Camera", SI("Single","32 MP, f/2.4")),
        SG("Battery",      SI("Type","Li-Po 5400 mAh"), SI("Charging","100W wired, 50W wireless, 10W reverse")),
        SG("Connectivity", SI("5G","Yes"), SI("Wi-Fi","Wi-Fi 7"), SI("Bluetooth","5.4"), SI("NFC","Yes")),
        SG("Misc",         SI("Colors","Silky Black, Flowy Emerald"), SI("Price","$799 / €899 / £799")),
    ];

    private static List<SpecGroup> OnePlus12RSpecs() =>
    [
        SG("Display",      SI("Type","LTPO AMOLED, 1-120Hz"), SI("Size","6.78 inches"), SI("Resolution","1264 x 2780 pixels")),
        SG("Platform",     SI("OS","Android 14, OxygenOS 14"), SI("Chipset","Snapdragon 8 Gen 2 (4 nm)"), SI("GPU","Adreno 740")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","128GB 8GB RAM / 256GB 8GB RAM / 256GB 16GB RAM")),
        SG("Main Camera",  SI("Triple","50MP f/1.8 (wide), 8MP f/2.2 ultrawide, 2MP f/2.4 macro"), SI("Video","4K@60fps")),
        SG("Battery",      SI("Type","Li-Po 5500 mAh"), SI("Charging","100W wired")),
        SG("Misc",         SI("Colors","Iron Gray, Cool Blue"), SI("Price","$499 / €549")),
    ];

    private static List<SpecGroup> OnePlusOpenSpecs() =>
    [
        SG("Display",      SI("Type","Foldable LTPO AMOLED, 120Hz"), SI("Main Size","7.82 inches"), SI("Cover Size","6.31 inches")),
        SG("Platform",     SI("OS","Android 13, OxygenOS 13.2"), SI("Chipset","Snapdragon 8 Gen 2 (4 nm)"), SI("GPU","Adreno 740")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","512GB 16GB RAM / 1TB 16GB RAM")),
        SG("Main Camera",  SI("Triple","48MP f/1.7, 64MP f/2.6 periscope, 48MP f/2.2 ultrawide"), SI("Video","4K@60fps")),
        SG("Battery",      SI("Type","Li-Po 4805 mAh"), SI("Charging","67W wired")),
        SG("Misc",         SI("Colors","Emerald Dusk, Voyager Black"), SI("Price","$1699 / €1799")),
    ];

    // ── Xiaomi specs ─────────────────────────────────────────────────────────
    private static List<SpecGroup> Xiaomi14UltraSpecs() =>
    [
        SG("Display",      SI("Type","LTPO AMOLED, 1-120Hz, Dolby Vision, HDR10+, 3000 nits"), SI("Size","6.73 inches"), SI("Resolution","1440 x 3200 pixels (~522 ppi)")),
        SG("Platform",     SI("OS","Android 14, HyperOS"), SI("Chipset","Snapdragon 8 Gen 3 (4 nm)"), SI("GPU","Adreno 750")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","512GB 16GB RAM / 1TB 16GB RAM")),
        SG("Main Camera",  SI("Quad (Leica)","50MP f/1.6 (wide), 50MP f/1.8 periscope 5x, 50MP f/2.5 telephoto 3x, 50MP f/1.8 ultrawide"), SI("Video","8K@24fps, 4K@60fps")),
        SG("Front Camera", SI("Single","32 MP, f/2.0")),
        SG("Battery",      SI("Type","Li-Po 5000 mAh"), SI("Charging","90W wired, 80W wireless, 10W reverse")),
        SG("Misc",         SI("Colors","Black, White, Titanium Special Edition"), SI("Price","$1299 / €1499")),
    ];

    private static List<SpecGroup> Xiaomi14ProSpecs() =>
    [
        SG("Display",      SI("Type","LTPO AMOLED, 1-120Hz, Dolby Vision"), SI("Size","6.73 inches"), SI("Resolution","1440 x 3200 pixels")),
        SG("Platform",     SI("OS","Android 14, HyperOS"), SI("Chipset","Snapdragon 8 Gen 3 (4 nm)"), SI("GPU","Adreno 750")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","256GB 12GB RAM / 512GB 16GB RAM / 1TB 16GB RAM")),
        SG("Main Camera",  SI("Triple (Leica)","50MP f/1.4-f/4.0 (wide), 50MP f/2.0 ultrawide, 50MP f/2.0 periscope"), SI("Video","8K@24fps, 4K@60fps")),
        SG("Battery",      SI("Type","Li-Po 4880 mAh"), SI("Charging","120W wired, 50W wireless")),
        SG("Misc",         SI("Colors","Black, White, Rock Green"), SI("Price","$999 / €1099")),
    ];

    private static List<SpecGroup> Xiaomi14Specs() =>
    [
        SG("Display",      SI("Type","AMOLED, 120Hz, Dolby Vision"), SI("Size","6.36 inches"), SI("Resolution","1200 x 2670 pixels (~460 ppi)")),
        SG("Platform",     SI("OS","Android 14, HyperOS"), SI("Chipset","Snapdragon 8 Gen 3 (4 nm)"), SI("GPU","Adreno 750")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","256GB 12GB RAM / 512GB 12GB RAM")),
        SG("Main Camera",  SI("Triple (Leica)","50MP f/1.6 (wide), 50MP f/2.5 telephoto, 50MP f/2.0 ultrawide"), SI("Video","8K@24fps, 4K@60fps")),
        SG("Battery",      SI("Type","Li-Po 4610 mAh"), SI("Charging","90W wired, 50W wireless")),
        SG("Misc",         SI("Colors","Black, White, Jade Green, Pink"), SI("Price","$699 / €799")),
    ];

    // ── Sony specs ───────────────────────────────────────────────────────────
    private static List<SpecGroup> SonyXperia1VISpecs() =>
    [
        SG("Display",      SI("Type","OLED, 1-120Hz, HDR, 1500 nits"), SI("Size","6.5 inches"), SI("Resolution","1080 x 2340 pixels")),
        SG("Platform",     SI("OS","Android 14"), SI("Chipset","Snapdragon 8 Gen 3 (4 nm)"), SI("GPU","Adreno 750")),
        SG("Memory",       SI("Card slot","Yes (microSDXC)"), SI("Internal","256GB 12GB RAM / 512GB 12GB RAM")),
        SG("Main Camera",  SI("Triple (Zeiss)","48MP f/1.9 (wide), 12MP f/2.3 ultrawide, 12MP f/2.3-3.5 telephoto"), SI("Video","4K@120fps")),
        SG("Battery",      SI("Type","Li-Ion 5000 mAh"), SI("Charging","30W wired, Qi wireless")),
        SG("Misc",         SI("Colors","Black, Platinum Silver, Khaki Green"), SI("Price","$1299 / €1399")),
    ];

    private static List<SpecGroup> SonyXperia5VSpecs() =>
    [
        SG("Display",      SI("Type","OLED, 120Hz, HDR"), SI("Size","6.1 inches"), SI("Resolution","1080 x 2520 pixels (~449 ppi)")),
        SG("Platform",     SI("OS","Android 13"), SI("Chipset","Snapdragon 8 Gen 2 (4 nm)"), SI("GPU","Adreno 740")),
        SG("Memory",       SI("Card slot","Yes (microSDXC)"), SI("Internal","256GB 8GB RAM")),
        SG("Main Camera",  SI("Triple (Zeiss)","52MP f/1.9, 12MP f/2.2 ultrawide, 10MP f/2.5 telephoto"), SI("Video","4K@120fps")),
        SG("Battery",      SI("Type","Li-Ion 5000 mAh"), SI("Charging","30W wired")),
        SG("Misc",         SI("Colors","Black, Platinum Silver, Sage Green"), SI("Price","$999 / €1049")),
    ];

    // ── Motorola specs ───────────────────────────────────────────────────────
    private static List<SpecGroup> MotoEdge50UltraSpecs() =>
    [
        SG("Display",      SI("Type","pOLED, 165Hz, HDR10+"), SI("Size","6.7 inches"), SI("Resolution","1080 x 2400 pixels")),
        SG("Platform",     SI("OS","Android 14"), SI("Chipset","Snapdragon 8s Gen 3 (4 nm)"), SI("GPU","Adreno 735")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","256GB 12GB RAM / 512GB 12GB RAM")),
        SG("Main Camera",  SI("Triple","50MP f/1.8 (wide, OIS), 50MP f/2.0 telephoto 2x, 50MP f/2.2 ultrawide"), SI("Video","4K@60fps")),
        SG("Battery",      SI("Type","Li-Po 4500 mAh"), SI("Charging","125W wired, 50W wireless")),
        SG("Misc",         SI("Colors","Nordic Wood, Peach Fuzz, Dark Grey"), SI("Price","$699 / €799")),
    ];

    private static List<SpecGroup> MotoRazr40UltraSpecs() =>
    [
        SG("Display",      SI("Type","Foldable pOLED, 165Hz"), SI("Main Size","6.9 inches"), SI("Cover Size","3.6 inches, 144Hz")),
        SG("Platform",     SI("OS","Android 13"), SI("Chipset","Snapdragon 8+ Gen 1 (4 nm)"), SI("GPU","Adreno 730")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","256GB 8GB RAM / 512GB 12GB RAM")),
        SG("Main Camera",  SI("Dual","12MP f/1.7 (wide), 13MP f/2.2 ultrawide"), SI("Video","4K@60fps")),
        SG("Battery",      SI("Type","Li-Po 3800 mAh"), SI("Charging","30W wired, 5W wireless")),
        SG("Misc",         SI("Colors","Infinite Black, Glacier Blue, Peach Fuzz, Vanilla Cream"), SI("Price","$999 / €1199")),
    ];

    // ── Oppo specs ───────────────────────────────────────────────────────────
    private static List<SpecGroup> OppoFindX7UltraSpecs() =>
    [
        SG("Display",      SI("Type","LTPO AMOLED, 1-120Hz, Dolby Vision, 2500 nits"), SI("Size","6.82 inches"), SI("Resolution","1440 x 3168 pixels")),
        SG("Platform",     SI("OS","Android 14, ColorOS 14"), SI("Chipset","Snapdragon 8 Gen 3 (4 nm)"), SI("GPU","Adreno 750")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","256GB 12GB RAM / 512GB 16GB RAM")),
        SG("Main Camera",  SI("Quad (Hasselblad)","50MP f/1.8 (wide), 50MP f/2.6 periscope 6x, 50MP f/2.6 periscope 3x, 50MP f/2.0 ultrawide"), SI("Video","4K@60fps")),
        SG("Battery",      SI("Type","Li-Po 5000 mAh"), SI("Charging","100W wired, 50W wireless")),
        SG("Misc",         SI("Colors","Black, White, Brown"), SI("Price","$1299 / €1499")),
    ];

    // ── Vivo specs ───────────────────────────────────────────────────────────
    private static List<SpecGroup> VivoX100ProSpecs() =>
    [
        SG("Display",      SI("Type","LTPO AMOLED, 1-120Hz, 3000 nits"), SI("Size","6.78 inches"), SI("Resolution","1260 x 2800 pixels")),
        SG("Platform",     SI("OS","Android 14, OriginOS 4"), SI("Chipset","Dimensity 9300 (4 nm)"), SI("GPU","Immortalis-G720 MC12")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","256GB 16GB RAM / 512GB 16GB RAM / 1TB 16GB RAM")),
        SG("Main Camera",  SI("Triple (Zeiss)","50MP f/1.6 (wide), 50MP f/2.0 periscope 4.3x, 50MP f/2.0 ultrawide"), SI("Video","4K@60fps")),
        SG("Battery",      SI("Type","Li-Po 5400 mAh"), SI("Charging","100W wired, 50W wireless")),
        SG("Misc",         SI("Colors","Starlite Blue, Black, White"), SI("Price","$799 / €899")),
    ];

    // ── Realme specs ─────────────────────────────────────────────────────────
    private static List<SpecGroup> RealmeGT6Specs() =>
    [
        SG("Display",      SI("Type","LTPO AMOLED, 1-120Hz, 6000 nits peak"), SI("Size","6.78 inches"), SI("Resolution","1264 x 2780 pixels")),
        SG("Platform",     SI("OS","Android 14, Realme UI 5.0"), SI("Chipset","Snapdragon 8s Gen 3 (4 nm)"), SI("GPU","Adreno 735")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","256GB 12GB RAM / 512GB 12GB RAM / 512GB 16GB RAM")),
        SG("Main Camera",  SI("Triple","50MP f/1.8 (wide, OIS), 8MP f/2.2 ultrawide, 50MP f/2.0 telephoto 3x"), SI("Video","4K@60fps")),
        SG("Battery",      SI("Type","Li-Po 5500 mAh"), SI("Charging","120W wired")),
        SG("Misc",         SI("Colors","Fluid Silver, Razor Green"), SI("Price","$499 / €549")),
    ];

    // ── Generic fallback ─────────────────────────────────────────────────────
    private static List<SpecGroup> GenericSpecs(string brand) =>
    [
        SG("Display",      SI("Type","AMOLED, 120Hz"), SI("Size","6.5 inches"), SI("Resolution","1080 x 2400 pixels")),
        SG("Platform",     SI("OS","Android 14"), SI("Chipset","Snapdragon 8 Gen 2 / Dimensity 9200"), SI("GPU","Adreno 740")),
        SG("Memory",       SI("Card slot","No"), SI("Internal","128GB 8GB RAM / 256GB 8GB RAM")),
        SG("Main Camera",  SI("Triple","50MP f/1.8 (wide), 12MP ultrawide, 5MP macro"), SI("Video","4K@60fps")),
        SG("Battery",      SI("Type","Li-Po 5000 mAh"), SI("Charging","67W wired")),
        SG("Misc",         SI("Brand", brand), SI("Price","$499 - $799")),
    ];
}
