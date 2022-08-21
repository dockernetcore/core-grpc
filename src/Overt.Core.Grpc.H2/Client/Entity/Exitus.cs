namespace Overt.Core.Grpc.H2
{
    /// <summary>
    /// 出口类
    /// </summary>
    public class Exitus
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="endpointDiscovery"></param>
        public Exitus(string serviceName, IEndpointDiscovery endpointDiscovery)
        {
            ServiceName = serviceName;
            EndpointDiscovery = endpointDiscovery;
        }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 策略
        /// </summary>
        public IEndpointDiscovery EndpointDiscovery { get; set; }
    }
}
