# Simple Backup

A modern, user-friendly backup application for Windows built with WPF and .NET 10. Simple Backup provides an intuitive interface for backing up files from a source directory to a destination directory with real-time progress tracking and conflict resolution.

## Features

### Core Functionality
- **Directory Selection**: Choose source and destination directories with an easy-to-use folder browser
- **Persistent Configuration**: Backup and restore directory paths are automatically saved to a local JSON configuration file
- **Two-Phase Backup Process**:
  - **Research Phase**: Scans all files and directories to determine what will be backed up
  - **Copy Phase**: Performs the actual file copy operation with real-time progress tracking

### Progress Tracking
- **Research Progress**: Shows file count and total data size to be backed up
- **Copy Progress**: Displays individual file copying progress with bytes transferred
- **Status Messages**: Real-time status updates showing the current operation

### File Conflict Resolution
When backing up files that already exist in the destination:
- **Skip**: Skip just this file
- **Skip All**: Skip all future conflicting files
- **Overwrite**: Overwrite just this file
- **Overwrite All**: Overwrite all future conflicting files

### Directory Structure Preservation
- Empty directories are created in the destination
- Full directory hierarchy is replicated
- Subdirectories are properly created during the copy process

### Cancellation Support
- **Cancel Button**: Request cancellation of the backup operation
- **Confirmation Dialog**: Prompts user to confirm cancellation with the option to continue
- **X Button Handling**: 
  - First click shows confirmation dialog
  - After cancellation is requested, subsequent clicks close the window immediately
- **Graceful Shutdown**: Cancellation is properly handled during both research and copy phases

### Dark Theme UI
- Modern dark theme throughout the entire application
- Consistent styling for all dialogs and controls
- Clear visual feedback for button states (enabled/disabled)
- Responsive layout that adapts to content

## Installation

### Option 1: Using the Installer (Recommended)

1. Download `SimpleBackup-1.0.0-Setup.exe` from the [Releases](https://github.com/unhuman/Simple-Backup/releases) page
2. Run the installer
3. Follow the installation wizard
4. Choose installation location (default: `C:\Program Files\Simple Backup`)
5. Optionally create a Desktop shortcut
6. Click Install and launch the application

**Requirements**: .NET 10 Runtime (installer will guide you to install if needed)

### Option 2: Portable (Framework-Dependent)

1. Download the published files from releases or build locally
2. Extract to your preferred location
3. Run `Simple Backup.exe`

**Requirements**: .NET 10 Runtime must be pre-installed

### Option 3: Self-Contained Portable

1. Build using: `.\build-release.ps1 -SelfContained`
2. Extract `bin\Release\net10.0-windows\publish\` to your preferred location
3. Run `Simple Backup.exe`

**Advantages**: No .NET 10 runtime dependency  
**Disadvantages**: Larger file size (~300 MB)

## Building the Installer

### Prerequisites

- Visual Studio 2026 or .NET 10 SDK
- InnoSetup 6 ([Download here](https://jrsoftware.org/isdl.php))
- PowerShell 5.0 or later

### Build Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/unhuman/Simple-Backup
   cd Simple\ Backup
   ```

2. **Build the installer**
   ```powershell
   .\build-release.ps1 -BuildInstaller
   ```

   This script will:
   - Clean previous build artifacts
   - Build the release version for .NET 10
   - Publish the application
   - Create the Windows installer

3. **Find the installer**
   - Installer location: `installer\SimpleBackup-1.0.0-Setup.exe`
   - Size: ~2.17 MB

### Build Script Options

```powershell
# Build framework-dependent release (requires .NET 10 runtime)
.\build-release.ps1

# Build self-contained release (includes .NET 10 runtime)
.\build-release.ps1 -SelfContained

# Create Windows installer (requires InnoSetup)
.\build-release.ps1 -BuildInstaller

# Build self-contained installer
.\build-release.ps1 -SelfContained -BuildInstaller
```

### Output Locations

- **Framework-dependent build**: `bin\Release\net10.0-windows\publish\` (~0.71 MB)
- **Self-contained build**: `bin\Release\net10.0-windows\publish\` (~200-300 MB)
- **Installer**: `installer\SimpleBackup-1.0.0-Setup.exe`

## User Interface

### Main Window
- **Title**: Simple Backup Tool
- **Backup Directory Field**: Shows selected backup source directory
- **Restore Directory Field**: Shows selected backup destination directory
- **Browse Buttons**: "..." buttons to select directories with folder browser
- **Backup Button**: Initiates the backup process
- **Exit Button**: Closes the application (saves current configuration)

### Backup Progress Dialog
- **Phase 1 Progress**: Research phase with file enumeration progress
- **Phase 2 Progress**: Copy phase with detailed file-by-file progress
- **Overall Status**: Current operation status message
- **Cancel Button**: Request backup cancellation (enabled during operation)
- **Close Button**: Close the dialog (enabled only after operation completes)

### Conflict Resolution Dialog
- **File Name Display**: Shows the name of the conflicting file
- **Four Action Buttons**: Skip, Skip All, Overwrite, Overwrite All
- **Responsive Dialog**: Centered and properly sized for readability

### Cancellation Confirmation Dialog
- **Clear Message**: Confirms operation is in progress
- **Two Options**: Continue (keep backup running) or Cancel Backup (request cancellation)
- **Consistent Styling**: Matches the overall application theme

## Configuration

Configuration is stored in:
```
%APPDATA%\SimpleBackup\backup_config.json
```

Example configuration file:
```json
{
  "BackupDirectory": "C:\\Users\\YourUsername\\Documents\\MyFiles",
  "RestoreDirectory": "D:\\Backups\\MyFilesBackup"
}
```

## Technical Details

### Architecture
- **WPF Application**: Built with Windows Presentation Foundation for modern UI
- **Async/Await Pattern**: Non-blocking operations for responsive UI
- **Background Threading**: Research phase runs on background thread for performance
- **UI Thread Operations**: File copying runs on UI thread to enable proper dialog interactions

### Technologies
- **.NET 10**: Latest .NET runtime
- **C# 12**: Modern language features
- **WPF**: Desktop application framework
- **JSON Configuration**: Simple, human-readable settings storage
- **Ookii.Dialogs.Wpf**: Native folder browser dialog

### Key Components

#### MainWindow.xaml / MainWindow.xaml.cs
Main application window with:
- Directory selection controls
- Configuration management
- Backup operation initiation
- Folder browser integration

#### BackupProgressDialog.xaml / BackupProgressDialog.xaml.cs
Progress tracking window with:
- Two-phase backup process execution
- Real-time UI updates
- File enumeration and copying
- Conflict detection and resolution
- Cancellation handling

#### ConflictResolutionDialog.xaml / ConflictResolutionDialog.xaml.cs
File conflict handling dialog with:
- Skip/Skip All/Overwrite/Overwrite All options
- File name display
- User preference persistence

#### CancellationConfirmDialog.xaml / CancellationConfirmDialog.xaml.cs
Cancellation confirmation dialog with:
- Operation status confirmation
- Continue or Cancel options

#### App.xaml / App.xaml.cs
Application-wide resources with:
- Dark theme color definitions
- Button styling with hover effects
- TextBox and Label styling
- Disabled state styling

## Usage

1. **Select Backup Directory**: Click the "..." button next to "Backup Directory" to choose the folder to backup
2. **Select Restore Directory**: Click the "..." button next to "Restore Directory" to choose where to backup to
3. **Start Backup**: Click the "Backup" button to begin
4. **Monitor Progress**: 
   - Watch the research phase complete
   - Monitor file-by-file copying in the copy phase
5. **Handle Conflicts**: If files already exist in the destination, respond to the conflict dialog
6. **View Results**: Once complete, you can close the progress dialog or start another backup
7. **Exit**: Click "Exit" to close the application (your directory selections are saved)

## Debug Information

The application includes comprehensive debug logging. View debug output in Visual Studio:
- **Debug ? Windows ? Output** or **Ctrl+Alt+O**
- Shows file enumeration progress
- Logs conflict detection and resolution
- Tracks file copy operations
- Displays cancellation requests

## Error Handling

- **Directory Not Found**: Validates that directories exist before starting backup
- **Access Denied**: Skips inaccessible directories and logs the error
- **File Copy Errors**: Logs individual file copy errors and continues with next file
- **Configuration Errors**: Creates configuration directory if it doesn't exist

## Performance

- **Research Phase**: Optimized for fast directory enumeration
- **Async Operations**: Non-blocking UI during all operations
- **Memory Efficient**: Streams file operations rather than loading into memory
- **Configurable Delay**: Test build includes 1-second delay between file copies (easily removed)

## Building and Running

### Requirements
- Windows 10 or later
- .NET 10 Runtime or SDK
- Visual Studio 2026 (optional, for development)

### Building from Source
```bash
dotnet build
```

### Running
```bash
dotnet run
```

### Publishing
For development/testing:
```bash
dotnet publish -c Debug
```

For release distribution:
```powershell
.\build-release.ps1 -BuildInstaller
```

## Future Enhancements

Potential features for future versions:
- Backup scheduling
- Differential backups (only changed files)
- Compression support
- Network destination support
- Backup verification
- File filtering by type or date
- Backup history and restoration from previous backups

## License

[Add your license information here]

## Support

For issues, questions, or suggestions, please open an issue on [GitHub](https://github.com/unhuman/Simple-Backup/issues).

---

**Version**: 1.0  
**Last Updated**: 2025  
**Target Framework**: .NET 10  
**Language**: C# 12  
**License**: [Your License]
