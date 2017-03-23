using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Data.Storage
{
    public class FileStorageFactory
    {
        public static IFileStorage Create(IRemoteStorage remoteStorage)
        {
            return new FileStorage(remoteStorage);
        }

        public static IFileStorage Create(IRemoteStorage remoteStorage, IStorageEncryptionKeys encryptionKeys)
        {
            return new FileStorage(remoteStorage, encryptionKeys);
        }
    }
}
