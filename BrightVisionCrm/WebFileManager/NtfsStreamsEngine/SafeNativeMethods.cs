using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace WebFileManager.NtfsStreamsEngine
{
    /// <summary>
    /// Safe native methods.
    /// </summary>
    internal static class SafeNativeMethods
    {
        public const int MaxPath = 256;
        private const string longPathPrefix = @"\\?\";
        public const char StreamSeparator = ':';
        public const int DefaultBufferSize = 0x1000;

        private const int errorFileNotFound = 2;

        [Flags]
        public enum NativeFileFlags : uint
        {
            WriteThrough     = 0x80000000,
            Overlapped       = 0x40000000,
            NoBuffering      = 0x20000000,
            RandomAccess     = 0x10000000,
            SequentialScan   = 0x8000000,
            DeleteOnClose    = 0x4000000,
            BackupSemantics  = 0x2000000,
            PosixSemantics   = 0x1000000,
            OpenReparsePoint = 0x200000,
            OpenNoRecall     = 0x100000
        }

        [Flags]
        public enum NativeFileAccess : uint
        {
            GenericRead    = 0x80000000,
            GenericWrite   = 0x40000000,
            WriteAttributes = 0x100
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetFileAttributes(string fileName);

        [DllImport("kernel32.dll")]
        private static extern int GetFileType(SafeFileHandle handle);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(
            string name,
            NativeFileAccess access,
            FileShare share,
            IntPtr security,
            FileMode mode,
            NativeFileFlags flags,
            IntPtr template);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        [DllImport("kernel32.dll")]
        private static extern int SetEndOfFile(SafeFileHandle handle);

        [DllImport("kernel32.dll")]
        private static extern bool SetFileValidData(SafeFileHandle handle, long dataLength);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpaceEx(
            string lpDirectoryName,
            out long lpFreeBytesAvailable,
            out long lpTotalNumberOfBytes,
            out long lpTotalNumberOfFreeBytes);

        public static void ThrowLastWin32Error()
        {
            int error = Marshal.GetLastWin32Error();
            if (0 != error)
            {
                int hr = Marshal.GetHRForLastWin32Error();
                switch (error)
                {
                    case 0x20:
                        throw new IOException("Sharing violation");

                    case 2:
                        throw new FileNotFoundException();

                    case 3:
                        throw new DirectoryNotFoundException();

                    case 5:
                        throw new UnauthorizedAccessException();

                    case 15:
                        throw new DriveNotFoundException();

                    case 0x57:
                        throw new IOException();

                    case 0xb7:
                        break;

                    case 0xce:
                        throw new PathTooLongException("Path too long");

                    case 0x3e3:
                        throw new OperationCanceledException();
                }

                throw new IOException();

                //if (0 > hr) Marshal.ThrowExceptionForHR(error);
                //throw new Win32Exception(error);
            }
        }

        public static NativeFileAccess ToNative(FileAccess access)
        {
            NativeFileAccess result = 0;
            if (FileAccess.Read == (FileAccess.Read & access))
            {
                result |= NativeFileAccess.GenericRead;
            }

            if (FileAccess.Write == (FileAccess.Write & access))
            {
                result |= NativeFileAccess.GenericWrite;
            }

            return result;
        }

        public static string BuildStreamPath(string filePath, string streamName)
        {
            string result = filePath.TrimEnd('\\');
            if (!string.IsNullOrEmpty(filePath))
            {
                if (1 == result.Length)
                {
                    result = ".\\" + result;
                }

                result += StreamSeparator + streamName;

                if (MaxPath <= result.Length)
                {
                    result = longPathPrefix + result;
                }
            }

            return result;
        }

        public static bool SafeDeleteFile(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            bool result = DeleteFile(name);
            if (!result)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorFileNotFound != errorCode)
                {
                    ThrowLastWin32Error();
                }
            }

            return result;
        }

        public static SafeFileHandle SafeCreateFile(
            string path,
            NativeFileAccess access,
            FileShare share,
            IntPtr security,
            FileMode mode,
            NativeFileFlags flags,
            IntPtr template)
        {
            SafeFileHandle result = CreateFile(path, access, share, security, mode, flags, template);
            if (!result.IsInvalid && 1 != GetFileType(result))
            {
                result.Dispose();
                throw new NotSupportedException(
                    string.Format("The specified file name '{0}' is not a disk-based file.", path));
            }

            return result;
        }
        
        public static void TruncateFile(FileInfo file)
        {
            // There is no another way to truncate file stream
            using (FileStream fs = file.OpenWrite())
            {
                SetFileValidData(fs.SafeFileHandle, 0);
                SetEndOfFile(fs.SafeFileHandle);
            }
        }

        public static void ValidateStreamName(string streamName)
        {
            if (!string.IsNullOrEmpty(streamName) &&
                -1 != streamName.IndexOfAny(Path.GetInvalidFileNameChars()))
            {
                throw new ArgumentException("Name of the stream contains invalid characters.");
            }
        }

        public static int SafeGetFileAttributes(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            int result = GetFileAttributes(name);
            if (-1 == result)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorFileNotFound != errorCode)
                {
                    ThrowLastWin32Error();
                }
            }

            return result;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FILE_TIME
        {
            public FILE_TIME(long fileTime)
            {
                ftTimeLow = (uint)fileTime;
                ftTimeHigh = (uint)(fileTime >> 32);
            }

            internal uint ftTimeLow;
            internal uint ftTimeHigh;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetFileTime(
            SafeFileHandle hFile,
            IntPtr creationTime,
            IntPtr lastAccessTime,
            ref FILE_TIME lastWriteTime);

        public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            using (var handle = SafeCreateFile(
                path,
                NativeFileAccess.WriteAttributes,
                FileShare.Read,
                IntPtr.Zero,
                FileMode.Open,
                0,
                IntPtr.Zero))
            {
                var fileTime = new FILE_TIME(lastWriteTimeUtc.ToFileTimeUtc());
                if (!SetFileTime(handle, IntPtr.Zero, IntPtr.Zero, ref fileTime))
                {
                    ThrowLastWin32Error();
                }
            }
        }
    }
}
