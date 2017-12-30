using System;

namespace FileInfoDb.Core
{
    public class DatabaseAccessDeniedException : DatabaseException
    {
        public DatabaseAccessDeniedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
