using System;

namespace FileInfoDb.Core
{
    /// <summary>
    /// Occurs when database operation fails because the user does not have sufficient rghts
    /// </summary>
    public class DatabaseAccessDeniedException : DatabaseException
    {
        public DatabaseAccessDeniedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
