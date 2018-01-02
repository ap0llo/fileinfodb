using System.Collections.Generic;
using FileInfoDb.Core.Hashing;

namespace FileInfoDb.Core.FileProperties
{
    /// <summary>
    /// Defines the interface to add and query properties associated with files
    /// </summary>
    public interface IPropertyStorage
    {
        IEnumerable<string> GetPropertyNames();

        IEnumerable<Property> GetProperties(HashValue fileHash);

        void SetProperty(HashValue fileHash, Property property, bool overwrite);
    }
}
