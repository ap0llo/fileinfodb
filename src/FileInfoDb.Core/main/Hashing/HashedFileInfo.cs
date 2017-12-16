using System;
using NodaTime;
using static System.IO.Path;

namespace FileInfoDb.Core.Hashing
{
    public sealed class HashedFileInfo : IEquatable<HashedFileInfo>
    {
        public string Path { get; }

        public Instant LastWriteTime { get; }

        public long Length { get; }

        public HashValue Hash { get; }


        public HashedFileInfo(string path, Instant lastWriteTime, long length, HashValue hash)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value must not be empty", nameof(path));

            if (!IsPathRooted(path))
                throw new ArgumentException("Value must be a rooted path", nameof(path));

            Path = GetFullPath(path);
            LastWriteTime = lastWriteTime;
            Length = length;
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }


        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Path);

        public override bool Equals(object obj) => Equals(obj as HashedFileInfo);

        public bool Equals(HashedFileInfo other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return StringComparer.OrdinalIgnoreCase.Equals(Path, other.Path) &&
                LastWriteTime == other.LastWriteTime &&
                Length == other.Length &&
                Hash.Equals(other.Hash);
        }
    }
}
