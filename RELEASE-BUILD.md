# Building and Releasing Simple Backup

This document explains how to create release builds and installers for Simple Backup.

## Prerequisites

- .NET 8 SDK installed
- PowerShell 5.0 or higher
- (Optional) InnoSetup 6 for creating Windows installers

## Quick Build

### Framework-Dependent Build (Recommended for most users)

```powershell
.\build-release.ps1
```

This creates a smaller, faster-to-download release that requires .NET 8 runtime to be installed on the target machine.

**Output:** `bin\Release\net8.0-windows\publish\`

### Self-Contained Build (Includes .NET Runtime)

```powershell
.\build-release.ps1 -SelfContained
```

This creates a standalone executable that doesn't require .NET 8 to be pre-installed. File size will be ~200-300 MB larger.

**Output:** `bin\Release\net8.0-windows\publish\`

## Creating an Installer

### Option 1: Using InnoSetup (Recommended)

1. **Install InnoSetup 6** from: https://jrsoftware.org/isdl.php

2. **Build with installer:**
   ```powershell
   .\build-release.ps1 -BuildInstaller
   ```
   Or for self-contained:
   ```powershell
   .\build-release.ps1 -SelfContained -BuildInstaller
   ```

3. **Output:** The installer `.exe` will be created in `.\installer\`

### Option 2: Manual Publishing

If you don't want an installer, simply distribute the contents of `bin\Release\net8.0-windows\publish\` to users. They can run `Simple Backup.exe` directly (requires .NET 8 runtime).

## Distribution Methods

### 1. Framework-Dependent Release (Recommended)
- **Size:** ~5-10 MB (just the app)
- **Requirements:** Users must have .NET 8 runtime installed
- **Installation:** Copy to Program Files, create shortcut, or use installer
- **Best for:** Professional distribution with installer

### 2. Self-Contained Release
- **Size:** ~200-300 MB (includes .NET runtime)
- **Requirements:** None (includes everything needed)
- **Installation:** Copy to Program Files, create shortcut, or use installer
- **Best for:** Users who may not have .NET 8 installed

### 3. Portable (No Installation)
- **Size:** 5-10 MB (framework-dependent) or 200-300 MB (self-contained)
- **Distribution:** ZIP file with contents of publish folder
- **Installation:** Extract ZIP and run
- **Best for:** Users who want no installation, or portable USB usage

## Installer Configuration

The installer is configured in `SimpleBackup.iss` (InnoSetup script):

- **App Name:** Simple Backup
- **Default Location:** `C:\Program Files\Simple Backup`
- **Shortcuts:** Start Menu and optional Desktop shortcut
- **Uninstaller:** Includes proper uninstall support
- **Icon:** Uses the backup-icon.ico from the project

To customize:
- Edit `SimpleBackup.iss`
- Change AppVersion, AppPublisher, etc. as needed
- Rebuild with `-BuildInstaller` flag

## Release Checklist

- [ ] Update version in `SimpleBackup.iss` (AppVersion field)
- [ ] Run `.\build-release.ps1 -BuildInstaller`
- [ ] Test the installer on a clean machine if possible
- [ ] Create GitHub Release with:
  - Release notes
  - Link to installer in `.\installer\`
  - Checksums for verification
  - Requirements (Framework-dependent vs Self-contained)

## Troubleshooting

### Build fails with "dotnet: not found"
- Ensure .NET 8 SDK is installed: `dotnet --version`
- Make sure you're running PowerShell and not Command Prompt

### InnoSetup not found when using -BuildInstaller
- Install InnoSetup 6 from https://jrsoftware.org/isdl.php
- Or manually run: `"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" SimpleBackup.iss`

### Installer is too large
- Use the framework-dependent build instead of self-contained
- Framework-dependent is recommended unless .NET 8 runtime is unreliable in your user base

## Publishing to GitHub Releases

```powershell
# Example: Upload the installer to GitHub releases
$version = "1.0.0"
$installerPath = ".\installer\SimpleBackup-$version-Setup.exe"

# You can use GitHub CLI: gh release create v$version $installerPath
```

See: https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository
