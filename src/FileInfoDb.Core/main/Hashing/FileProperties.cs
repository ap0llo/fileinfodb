using System;
using System.IO;
using NodaTime;
using NodaTime.Extensions;
using static System.IO.Path;

namespace FileInfoDb.Core.Hashing
{
    /// <summary>
    /// Provides information about a file
    /// </summary>
    public sealed class FileProperties : IEquatable<FileProperties>
    {
        /// <summary>
        /// The file's full, absolute path 
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The time of the fil's last modification
        /// </summary>
        public Instant LastWriteTime { get; }

        /// <summary>
        /// The file's size in bytes
        /// </summary>
        public long Length { get; }
        

        public FileProperties(string path, Instant lastWriteTime, long length)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value must not be empty", nameof(path));

            if (!IsPathRooted(path))
                throw new ArgumentException("Value must be a rooted path", nameof(path));

            Path = GetFullPath(path);
            LastWriteTime = lastWriteTime;
            Length = length;
        }


        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Path);

        public override bool Equals(object obj) => Equals(obj as HashedFileInfo);

        public bool Equals(FileProperties other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return StringComparer.OrdinalIgnoreCase.Equals(Path, other.Path) &&
                LastWriteTime == other.LastWriteTime &&
                Length == other.Length;                
        }

        
        /// <summary>
        /// Creates a new instance of <see cref="FileProperties"/>
        /// from the specified <see cref="FileInfo"/>
        /// </summary>
        public static FileProperties FromFileInfo(FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            return new FileProperties(fileInfo.FullName, fileInfo.LastWriteTimeUtc.ToInstant(), fileInfo.Length);
        }
    }
}
