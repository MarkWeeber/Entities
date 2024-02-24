using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace CustomUtils
{
    public static class BlobAssetUtil
    {
        public static BlobAssetReference<T> CreateBlobAsset<T>(T value) where T : unmanaged
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref T data = ref builder.ConstructRoot<T>();
            data = value;
            var result = builder.CreateBlobAssetReference<T>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }
    }
}
