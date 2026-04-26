# GitHub Free Deployment Guide

This guide walks you through publishing your PhoneCompare app so anyone can download and install it from GitHub for free.

## What You Will Get

- A public GitHub repository hosting your source code
- Automated builds that compile your app on every release
- A **Releases** page where users download:
  - `PhoneCompare-Android.apk` — Install on any Android device
  - `PhoneCompare-Windows.zip` — Extract and run on Windows 10/11

## Step 1 — Create a GitHub Repository

1. Go to [github.com/new](https://github.com/new)
2. Name it `PhoneCompare`
3. Choose **Public** (free)
4. Do NOT initialize with README, .gitignore, or license (we already have those files)
5. Click **Create repository**

Copy the repository URL shown after creation. It looks like:
```
https://github.com/YOUR_USERNAME/PhoneCompare.git
```

## Step 2 — Push Your Code

Open a terminal inside your project folder and run:

```bash
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/PhoneCompare.git
git push -u origin main
```

> Replace `YOUR_USERNAME` with your actual GitHub username.

## Step 3 — Update README Placeholder

Edit `README.md` in this project and replace every instance of `YOUR_USERNAME` with your actual GitHub username.

## Step 4 — Create a Release (Automated Builds)

After pushing, every time you push a tag starting with `v`, GitHub Actions will automatically:
1. Build an Android APK
2. Build a Windows MSIX
3. Attach both files to a new GitHub Release

To create your first release, run:

```bash
git tag -a v1.0.0 -m "First release"
git push origin v1.0.0
```

Then go to **GitHub → your repo → Actions** and watch the workflow run. After a few minutes, go to **Releases** and you will see both files ready for download.

## Step 5 — Share the Link

Your users can always grab the latest release at:

```
https://github.com/YOUR_USERNAME/PhoneCompare/releases/latest
```

## Manual Trigger (Optional)

You can also run the build workflow manually from GitHub:
1. Go to **Actions → Build & Release**
2. Click **Run workflow**
3. Choose the branch and click **Run**

> Note: Manual runs will build the artifacts but will NOT create a Release page. Releases are only created when you push a tag like `v1.0.0`.

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Android build fails | Check that `net10.0-android` is in your `.csproj` target frameworks |
| Windows build fails | Check that `net10.0-windows10.0.19041.0` is in your `.csproj` |
| APK not signed | Debug builds are auto-signed. For Play Store you need your own keystore |
| Windows SmartScreen warning | This is normal for unpackaged apps. Users can click **More info → Run anyway** |
