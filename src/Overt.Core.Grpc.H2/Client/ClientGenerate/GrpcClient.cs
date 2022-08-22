using Grpc.Core;
using System;
using System.Collections.Generic;

namespace Overt.Core.Grpc.H2
{
    /// <summary>
    /// GrpcClient管理类
    /// </summary>
    public class GrpcClient<T> : IGrpcClient<T> where T : ClientBase
    {
        readonly IGrpcClientFactory<T> _factory;
        public GrpcClient(IGrpcClientFactory<T> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// 获取
        /// </summary>
        public T Client
        {
            get
            {
                return _factory.Get();
            }
        }


    }
}