﻿using Grpc.Core;
using Grpc.Net.Client;

namespace Overt.Core.Grpc.H2
{
    /// <summary>
    /// 单服务客户端配置
    /// </summary>
    public class GrpcClientOptions
    {
        /// <summary>
        /// Json文件
        /// 如果指定了ServiceName 怎不适用默认的ConfigPath
        /// defaultValue: dllconfigs/{namespace}.dll.json 
        /// </summary>
        public string ConfigPath { get; set; }

        /// <summary>
        /// 服务名称 
        /// 手动配置优先
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 配置GrpcChannelOptions 
        /// 手动配置优先
        /// </summary>
        public GrpcChannelOptions GrpcChannelOptions { set; get; }
    }

    /// <summary>
    /// 单服务客户端配置
    /// </summary>
    public class GrpcClientOptions<T> : GrpcClientOptions where T : ClientBase
    {
        
    }
}
