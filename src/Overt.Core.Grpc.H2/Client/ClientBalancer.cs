using System;

namespace Overt.Core.Grpc.H2
{
    internal static class ClientBalancer
    {
        /// <summary>
        /// 随机策略
        /// </summary>
        public static readonly string Random = "random";

    }
}
