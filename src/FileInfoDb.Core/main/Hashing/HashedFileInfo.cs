using System;

namespace FileInfoDb.Core.Hashing
{
    /// <summary>
    /// Data-type used to provied the hash of a file
    /// </summary>
    public sealed class HashedFileInfo : IEquatable<HashedFileInfo>
    {
        /// <summary>
        /// The file that was hashed
        /// </summary>
        public FileProperties File { get; }

        /// <summary>
        /// The file's hash
        /// </summary>
        public HashValue Hash { get; }


        public HashedFileInfo(FileProperties file, HashValue hash)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }


        public override int GetHashCode() => File.GetHashCode();

        public override bool Equals(object obj) => Equals(obj as HashedFileInfo);

        public bool Equals(HashedFileInfo other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return File.Equals(other.File) && Hash.Equals(other.Hash);
        }
    }
}
