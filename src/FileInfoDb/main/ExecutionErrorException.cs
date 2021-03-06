﻿using System;

namespace FileInfoDb
{
    /// <summary>
    /// Indicates that execution of the console application failed.
    /// The error message should be displayed to the user and the application should terminate
    /// </summary>
    [Serializable]
    class ExecutionErrorException : Exception
    {
        public ExecutionErrorException(string message) : base(message)
        {
        }
    }
}
