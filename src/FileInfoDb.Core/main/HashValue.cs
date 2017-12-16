using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FileInfoDb.Core
{
    public sealed class HashValue : IEquatable<HashValue>
    {
        static readonly Regex s_ValueRegex = new Regex("^[a-fA-F0-9]+$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Gets hash value of hex-encoded string
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the algorithm use to compute the hash
        /// </summary>
        public HashAlgorithm Algorithm { get; }


        public HashValue(string value, HashAlgorithm algorithm)
        {
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value must not be empty", nameof(value));

            if (!s_ValueRegex.IsMatch(value))
                throw new ArgumentException("Value must be a hex-string", nameof(value));

            if (!Enum.IsDefined(typeof(HashAlgorithm), algorithm))
                throw new ArgumentException($"{algorithm} is not within defined range of enum", nameof(algorithm));

            Value = value;
            Algorithm = algorithm;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(Value) * 397 ^ Algorithm.GetHashCode();                
            }
        }

        public override bool Equals(object obj) => Equals(obj as HashValue);

        public override string ToString() => $"{Algorithm}={Value}";

        public bool Equals(HashValue other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value) &&
                Algorithm == other.Algorithm;
        }

    }
}
