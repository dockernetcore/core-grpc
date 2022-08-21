using Grpc.Net.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Overt.Core.Grpc.H2
{
    /// <summary>
    /// 单例使用
    /// </summary>
    public interface IEndpointStrategy
    {
        Task<List<(string Address, int Port)>> FindServiceEndpoints(string serviceName);
    }
}