using Grpc.Core;
using System;
using System.Collections.Generic;

namespace Overt.Core.Grpc.H2
{
    /// <summary>
    /// 工厂类接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGrpcClientFactory<T> where T : ClientBase
    {
        /// <summary>
        /// 获取Client对象
        /// </summary>
        /// <returns></returns>
        T Get();

        /// <summary>
        /// 预热链接,预热负载均衡数据,避免不预热客户端起来流量太多流量起来比较慢甚至打挂
        /// </summary>
        void WarmConnect();
    }
}
