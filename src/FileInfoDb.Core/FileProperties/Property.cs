using System;

namespace FileInfoDb.Core.FileProperties
{
    public sealed class Property : IEquatable<Property>
    {
        public string Name { get; }

        public string Value { get; }

        
        public Property(string name, string value)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value must not be empty", nameof(name));

            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value must not be empty", nameof(value));

            Name = name;
            Value = value;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
                hash *= 397;
                hash ^= StringComparer.Ordinal.GetHashCode(Value);
                return hash;
            }
        }

        public override bool Equals(object obj) => Equals(obj as Property);

        public bool Equals(Property other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return StringComparer.OrdinalIgnoreCase.Equals(Name, other.Name) &&
                StringComparer.Ordinal.Equals(Value, other.Value);
        }

        public override string ToString() => $"({Name} : {Value})";
    }
}
