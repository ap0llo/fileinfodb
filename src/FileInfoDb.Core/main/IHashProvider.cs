using System;
using System.Collections.Generic;
using System.Text;

namespace FileInfoDb.Core
{
    public interface IHashProvider
    {
        HashedFileInfo GetFileHash(string path);
    }
}
