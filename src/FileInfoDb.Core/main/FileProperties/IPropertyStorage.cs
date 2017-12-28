using System.Collections.Generic;
using FileInfoDb.Core.Hashing;

namespace FileInfoDb.Core.FileProperties
{
    public interface IPropertyStorage
    {
        IEnumerable<string> GetPropertyNames();

        IEnumerable<Property> GetProperties(HashValue fileHash);

        void SetProperty(HashValue fileHash, Property property, bool overwrite);
    }
}
