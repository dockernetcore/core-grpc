﻿using System;

namespace Overt.Core.Grpc.H2
{
    internal static class ClientTimespan
    {
        /// <summary>
        /// 重置服务时间 15s
        /// </summary>
        public static readonly TimeSpan ResetInterval = TimeSpan.FromSeconds(15);

        /// <summary>
        /// 黑名单时效 2m
        /// </summary>
        public static readonly TimeSpan BlacklistPeriod = TimeSpan.FromMinutes(2);
    }
}
