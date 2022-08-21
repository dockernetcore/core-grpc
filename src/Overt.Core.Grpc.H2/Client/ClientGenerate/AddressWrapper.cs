using Grpc.Core;
using Grpc.Net.Client.Balancer;
using System.Threading.Tasks;

namespace Overt.Core.Grpc.H2
{
    public class AddressWrapper
    {
        public AddressWrapper(string serviceId,BalancerAddress balancerAddress)
        {
            ServiceId = serviceId;
            Address= balancerAddress; 
        }

        /// <summary>
        /// 服务Id
        /// </summary>
        public string ServiceId { get; set; }

        public BalancerAddress Address { set; get; }


        public string Target
        {
            get
            {
                return $"{Address.EndPoint.Host}:{Address.EndPoint.Port}";
            }
        }
    }
}
