using System;

namespace FileInfoDb
{
    class NullDisposable : IDisposable
    {
        public static NullDisposable Instance = new NullDisposable();

        private NullDisposable()
        {
        }

        public void Dispose()
        {            
        }
    }
}
