using System;
using System.Collections.Generic;
using System.Text;

namespace FileInfoDb.Core
{
    /// <summary>
    /// Generic exception class for unhandled errors that occurred when acessing a SQL database.
    /// Also serves as base class for all more specific exceptions thrown in the data access component
    /// </summary>
    [Serializable]
    public class DatabaseException : Exception
    {
        public DatabaseException()
        {
        }

        public DatabaseException(string message) : base(message)
        {
        }

        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
