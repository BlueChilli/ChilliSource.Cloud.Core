using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud
{
    public class FileStorageFactory
    {
        public static IFileStorage Create(IRemoteStorage remoteStorage)
        {
            return new FileStorage(remoteStorage);
        }
    }
}
