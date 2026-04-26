# PhoneCompare

A .NET MAUI cross-platform mobile app for comparing phones.

## Download & Install

Get the latest version from the **Releases** page on the right side of this repository.

### Android
1. Download `PhoneCompare-Android.apk` from the latest release
2. Open the APK on your Android device
3. If prompted, allow installation from unknown sources
4. Install and open the app

### Windows
1. Download `PhoneCompare-Windows.zip` from the latest release
2. Extract the ZIP to a folder
3. Open the folder and double-click `PhoneCompare.exe`
4. If Windows SmartScreen appears, click **More info → Run anyway**

## Features

- Phone comparison
- Favorites
- Search
- User authentication
- Quiz mode

## Development

Built with .NET MAUI, targeting:
- Android
- iOS
- macOS (Mac Catalyst)
- Windows 10/11

## Building from Source

```bash
# Clone the repository
git clone https://github.com/YOUR_GITHUB_USERNAME/PhoneCompare.git
cd PhoneCompare

# Restore dependencies
dotnet restore

# Build for Android
dotnet build -f net10.0-android -c Release

# Build for Windows
dotnet build -f net10.0-windows10.0.19041.0 -c Release
```

## License

MIT License
