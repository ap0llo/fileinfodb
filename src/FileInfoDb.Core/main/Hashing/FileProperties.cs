using System;
using System.IO;
using NodaTime;
using NodaTime.Extensions;
using static System.IO.Path;

namespace FileInfoDb.Core.Hashing
{
    public sealed class FileProperties : IEquatable<FileProperties>
    {
        public string Path { get; }

        public Instant LastWriteTime { get; }

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

        
        public static FileProperties FromFileInfo(FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            return new FileProperties(fileInfo.FullName, fileInfo.LastWriteTimeUtc.ToInstant(), fileInfo.Length);
        }


    }
}
