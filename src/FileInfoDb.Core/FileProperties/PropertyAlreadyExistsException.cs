using System;
using System.Collections.Generic;
using System.Text;

namespace FileInfoDb.Core.FileProperties
{
    [Serializable]
    public class PropertyAlreadyExistsException : Exception
    {
        public PropertyAlreadyExistsException()
        {
        }

        public PropertyAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
