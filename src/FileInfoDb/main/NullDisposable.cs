using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
