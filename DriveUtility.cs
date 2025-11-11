using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Simple_Backup
{
    public static class DriveUtility
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        private const uint IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;
        private const uint IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS = 0x560000;

        /// <summary>
        /// Detects if a drive is removable storage (USB, external drive, etc.)
        /// </summary>
        public static bool IsRemovableDrive(string drivePath)
        {
            try
            {
                // Get the drive letter from the path
                string drive = Path.GetPathRoot(drivePath);
                if (string.IsNullOrEmpty(drive))
                    return false;

                // Use DriveInfo to check drive type
                DriveInfo driveInfo = new DriveInfo(drive.TrimEnd('\\'));
                
                // DriveType.Removable indicates USB drives, external drives, etc.
                if (driveInfo.DriveType == DriveType.Removable)
                {
                    System.Diagnostics.Debug.WriteLine($"Drive {drive} is removable");
                    return true;
                }

                System.Diagnostics.Debug.WriteLine($"Drive {drive} is not removable (type: {driveInfo.DriveType})");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking if drive is removable: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ejects a removable drive with multiple strategies
        /// </summary>
        public static bool EjectDrive(string drivePath)
        {
            try
            {
                string drive = Path.GetPathRoot(drivePath);
                if (string.IsNullOrEmpty(drive))
                    return false;

                drive = drive.TrimEnd('\\');

                System.Diagnostics.Debug.WriteLine($"Attempting to eject drive: {drive}");

                // Try Windows API first
                if (TryEjectWithWinAPI(drive))
                {
                    System.Diagnostics.Debug.WriteLine($"Successfully ejected drive using Windows API: {drive}");
                    return true;
                }

                System.Diagnostics.Debug.WriteLine("Windows API eject failed, trying command-line methods...");

                // Fall back to command-line methods
                bool ejected = TryEjectWithCommand(drive);

                if (ejected)
                {
                    System.Diagnostics.Debug.WriteLine($"Successfully ejected drive using command-line: {drive}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to eject drive: {drive}");
                }

                return ejected;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ejecting drive: {ex.Message}");
                return false;
            }
        }

        private static bool TryEjectWithWinAPI(string driveLetter)
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                // Open the volume
                string volumePath = $"\\\\.\\{driveLetter}";
                handle = CreateFile(
                    volumePath,
                    GENERIC_READ | GENERIC_WRITE,
                    FILE_SHARE_READ | FILE_SHARE_WRITE,
                    IntPtr.Zero,
                    OPEN_EXISTING,
                    FILE_ATTRIBUTE_NORMAL,
                    IntPtr.Zero);

                if (handle == IntPtr.Zero || handle.ToInt64() == -1)
                {
                    System.Diagnostics.Debug.WriteLine($"Could not open volume {driveLetter}: {Marshal.GetLastWin32Error()}");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Successfully opened volume {driveLetter}");

                // Try to eject the media
                uint bytesReturned = 0;
                bool result = DeviceIoControl(
                    handle,
                    IOCTL_STORAGE_EJECT_MEDIA,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    0,
                    out bytesReturned,
                    IntPtr.Zero);

                if (result)
                {
                    System.Diagnostics.Debug.WriteLine($"DeviceIoControl succeeded for {driveLetter}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"DeviceIoControl failed for {driveLetter}: {Marshal.GetLastWin32Error()}");
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in TryEjectWithWinAPI: {ex.Message}");
                return false;
            }
            finally
            {
                if (handle != IntPtr.Zero && handle.ToInt64() != -1)
                {
                    CloseHandle(handle);
                }
            }
        }

        private static bool TryEjectWithCommand(string driveLetter)
        {
            try
            {
                // Use mountvol to dismount the drive
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c mountvol {driveLetter} /d",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process?.WaitForExit(5000);
                    bool success = process?.ExitCode == 0;

                    System.Diagnostics.Debug.WriteLine($"mountvol exit code: {process?.ExitCode}");

                    if (success)
                        return true;
                }

                // If mountvol failed, try the eject command (if available)
                psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c eject {driveLetter}:",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process?.WaitForExit(5000);
                    bool success = process?.ExitCode == 0;

                    System.Diagnostics.Debug.WriteLine($"eject command exit code: {process?.ExitCode}");
                    return success;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TryEjectWithCommand: {ex.Message}");
                return false;
            }
        }
    }
}
