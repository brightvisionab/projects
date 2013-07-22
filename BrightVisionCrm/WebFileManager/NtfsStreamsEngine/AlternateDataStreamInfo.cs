using System;
using System.IO;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace WebFileManager.NtfsStreamsEngine
{
    /// <summary>
    /// Represents the details of an alternative data stream.
    /// </summary>
    internal sealed class AlternateDataStreamInfo
    {
        private readonly string fullPath;
        private readonly string filePath;
        private readonly string streamName;
        
        internal AlternateDataStreamInfo(string filePath, string streamName)
        {
            string path = SafeNativeMethods.BuildStreamPath(filePath, streamName);

            this.streamName = streamName;
            this.filePath = filePath;
            this.fullPath = path;
        }

        /// <summary>
        /// Gets full path of this stream.
        /// </summary>
        /// <value>
        /// The full path of this stream.
        /// </value>
        public string FullPath
        {
            get { return fullPath; }
        }
        
        /// <summary>
        /// Deletes this stream from the parent file.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the stream was deleted;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Delete()
        {
            const FileIOPermissionAccess permAccess = FileIOPermissionAccess.Write;
            new FileIOPermission(permAccess, filePath).Demand();
            return SafeNativeMethods.SafeDeleteFile(this.FullPath);
        }

        /// <summary>
        /// Calculates the access to demand.
        /// </summary>
        /// <param name="mode">
        /// The <see cref="FileMode"/>.
        /// </param>
        /// <param name="access">
        /// The <see cref="FileAccess"/>.
        /// </param>
        /// <returns>
        /// The <see cref="FileIOPermissionAccess"/>.
        /// </returns>
        private static FileIOPermissionAccess CalculateAccess(FileMode mode, FileAccess access)
        {
            FileIOPermissionAccess permAccess = FileIOPermissionAccess.NoAccess;
            switch (mode)
            {
                case FileMode.Append:
                    permAccess = FileIOPermissionAccess.Append;
                    break;

                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                    permAccess = FileIOPermissionAccess.Write;
                    break;

                case FileMode.Open:
                    permAccess = FileIOPermissionAccess.Read;
                    break;
            }

            switch (access)
            {
                case FileAccess.ReadWrite:
                    permAccess |= FileIOPermissionAccess.Write;
                    permAccess |= FileIOPermissionAccess.Read;
                    break;

                case FileAccess.Write:
                    permAccess |= FileIOPermissionAccess.Write;
                    break;

                case FileAccess.Read:
                    permAccess |= FileIOPermissionAccess.Read;
                    break;
            }

            return permAccess;
        }

        /// <summary>
        /// Opens this alternate data stream.
        /// </summary>
        /// <param name="mode">
        /// A <see cref="FileMode"/> value that specifies whether a stream is created if one does not exist, 
        /// and determines whether the contents of existing streams are retained or overwritten.
        /// </param>
        /// <param name="access">
        /// A <see cref="FileAccess"/> value that specifies the operations that can be performed on the stream. 
        /// </param>
        /// <param name="share">
        /// A <see cref="FileShare"/> value specifying the type of access other threads have to the file. 
        /// </param>
        /// <param name="bufferSize">
        /// The size of the buffer to use.
        /// </param>
        /// <param name="useAsync">
        /// <see langword="true"/> to enable async-IO;
        /// otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A <see cref="FileStream"/> for this alternate data stream.
        /// </returns>
        public FileStream Open(FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        {
            if (0 >= bufferSize) throw new ArgumentOutOfRangeException("bufferSize", bufferSize, null);

            FileIOPermissionAccess permAccess = CalculateAccess(mode, access);
            new FileIOPermission(permAccess, filePath).Demand();

            SafeNativeMethods.NativeFileFlags flags = useAsync ? SafeNativeMethods.NativeFileFlags.Overlapped : 0;
            SafeFileHandle handle = SafeNativeMethods.SafeCreateFile(
                this.FullPath,
                SafeNativeMethods.ToNative(access),
                share,
                IntPtr.Zero,
                mode,
                flags,
                IntPtr.Zero);

            if (handle.IsInvalid)
            {
                SafeNativeMethods.ThrowLastWin32Error();
            }

            return new FileStream(handle, access, bufferSize, useAsync);
        }

        /// <summary>
        /// Opens this alternate data stream.
        /// </summary>
        /// <param name="mode">
        /// A <see cref="FileMode"/> value that specifies whether a stream is created if one does not exist, 
        /// and determines whether the contents of existing streams are retained or overwritten.
        /// </param>
        /// <param name="access">
        /// A <see cref="FileAccess"/> value that specifies the operations that can be performed on the stream. 
        /// </param>
        /// <param name="share">
        /// A <see cref="FileShare"/> value specifying the type of access other threads have to the file. 
        /// </param>
        /// <returns>
        /// A <see cref="FileStream"/> for this alternate data stream.
        /// </returns>
        public FileStream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return this.Open(mode, access, share, SafeNativeMethods.DefaultBufferSize, false);
        }

        public bool StreamExists()
        {
            SafeNativeMethods.ValidateStreamName(streamName);

            string path = SafeNativeMethods.BuildStreamPath(this.filePath, streamName);
            return -1 != SafeNativeMethods.SafeGetFileAttributes(path);
        }
        
        /// <summary>
        /// Opens this stream for writing.
        /// </summary>
        /// <returns>
        /// A write-only <see cref="FileStream"/> for this stream.
        /// </returns>
        public FileStream OpenWrite()
        {
            return this.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
        }

        /// <summary>
        /// Opens this stream as a text file.
        /// </summary>
        /// <returns>
        /// A <see cref="StreamReader"/> which can be used to read the contents of this stream.
        /// </returns>
        public StreamReader OpenText()
        {
            Stream fileStream = this.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            return new StreamReader(fileStream);
        }
    }
}
