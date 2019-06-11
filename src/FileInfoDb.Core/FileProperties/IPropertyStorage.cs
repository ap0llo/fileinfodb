using System.Collections.Generic;
using FileInfoDb.Hashing;

namespace FileInfoDb.Core.FileProperties
{
    /// <summary>
    /// Defines the interface to add and query properties associated with files
    /// </summary>
    public interface IPropertyStorage
    {
        /// <summary>
        /// Gets the names of all properties stored in the database
        /// </summary>
        /// <returns>
        /// Returns the name of all properties associated with at least one file
        /// </returns>
        IEnumerable<string> GetPropertyNames();

        /// <summary>
        /// Gets a single property for a file
        /// </summary>
        /// <param name="fileHash">The hash of the file to retrieve properties for</param>
        /// <param name="name">The name of the property to retrieve</param>
        /// <returns>
        /// Returns the requested property or null if the file is unknown or the 
        /// property does not exist for the file
        /// </returns>
        Property GetProperty(HashValue fileHash, string name);

        /// <summary>
        /// Gets all properties associated with the specified file
        /// </summary>
        /// <param name="fileHash">The hash of the file to retrieve properties for</param>
        /// <returns>
        /// Returns all the file's properties or an empty enumberable
        /// if the file is unknown or has no properties
        /// </returns>
        IEnumerable<Property> GetProperties(HashValue fileHash);

        /// <summary>
        /// Associates the specified property with the specified file
        /// </summary>
        /// <param name="fileHash">The hash of the file to store the property for</param>
        /// <param name="property">The property to store</param>
        /// <param name="overwrite">
        /// When true overwrites an existing property with the same name 
        /// associated with the specified file if it exists
        /// </param>
        /// <exception cref="PropertyAlreadyExistsException">
        /// Thrown when a property with the specified name is already associated with the specified file,
        /// and <paramref name="overwrite"/> was false
        /// </exception>
        void SetProperty(HashValue fileHash, Property property, bool overwrite);
    }
}
