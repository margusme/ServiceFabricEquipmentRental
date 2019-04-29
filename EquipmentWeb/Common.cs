using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EquipmentWeb
{
    /// <summary>
    /// Class for storing different objects used to connecto to stateful service
    /// </summary>
    public class Common : ICommon
    {
        protected readonly StatelessServiceContext _context;
        protected readonly HttpClient _httpClient;
        protected readonly FabricClient _fabricClient;

        public Common(StatelessServiceContext context, HttpClient httpClient, FabricClient fabricClient)
        {
            _context = context;
            _httpClient = httpClient;
            _fabricClient = fabricClient;
        }

        public StatelessServiceContext GetServiceContext()
        {
            return _context;
        }

        public Uri GetEquipmentDataServiceName()
        {
            return new Uri($"{_context.CodePackageActivationContext.ApplicationName}/EquipmentData");
        }

        /// <summary>
        /// Constructs a reverse proxy URL for a given service.
        /// Example: http://localhost:19081/EquipmentApplication/EquipmentData/
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public Uri GetProxyAddress()
        {
            var serviceName = GetEquipmentDataServiceName();
            //http://localhost:19081
            return new Uri($"http://localhost:19081{serviceName.AbsolutePath}");
        }

        public string GetInvoicesProxyGetUrl(Partition partition, bool downloadLastInvoice = true)
        {
            Uri proxyAddress = GetProxyAddress();

            return $"{proxyAddress}/api/InvoicesData/{(downloadLastInvoice ? "/Last" : "")}?PartitionKey={((Int64RangePartitionInformation)partition.PartitionInformation).LowKey}&PartitionKind=Int64Range";
        }

        public string GetEquipmentsProxyGetUrl(Partition partition)
        {
            Uri proxyAddress = GetProxyAddress();

            return $"{proxyAddress}/api/EquipmentData?PartitionKey={((Int64RangePartitionInformation)partition.PartitionInformation).LowKey}&PartitionKind=Int64Range";
        }


        public string GetBasketProxyGetUrl(Partition partition)
        {
            return $"{GetBasketProxyBaseUrl()}?PartitionKey={((Int64RangePartitionInformation)partition.PartitionInformation).LowKey}&PartitionKind=Int64Range";
        }

        public string GetBasketProxyPutUrl(string name, int days = 1)
        {
            long partitionKey = GetPartitionKey(name);
            return $"{GetBasketProxyBaseUrl()}/{name}/{days}?PartitionKey={partitionKey}&PartitionKind=Int64Range";
        }

        public string GetBasketProxyDeleteUrl(Guid guid)
        {
            long partitionKey = this.GetPartitionKey(guid.ToString());
            return $"{GetBasketProxyBaseUrl()}/{guid}?PartitionKey={partitionKey}&PartitionKind=Int64Range";
        }

        public string GetBasketProxyCloseUrl()
        {
            long partitionKey = this.GetPartitionKeyRandomly();
            return $"{GetBasketProxyBaseUrl()}/close?PartitionKey={partitionKey}&PartitionKind=Int64Range";
        }

        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }

        public FabricClient GetFabricClient()
        {
            return _fabricClient;
        }

        protected string GetBasketProxyBaseUrl()
        {
            Uri proxyAddress = GetProxyAddress();

            return $"{proxyAddress}/api/BasketData";
        }

        /// <summary>
        /// Creates a partition key from the given name.
        /// Uses the zero-based numeric position in the alphabet of the first letter of the name (0-25).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected long GetPartitionKey(string name)
        {
            return Char.ToUpper(name.First()) - 'A';
        }

        /// <summary>
        /// Get partition key from random number between 0 and 25
        /// </summary>
        /// <returns></returns>
        protected long GetPartitionKeyRandomly()
        {
            return new Random().Next(0, 25);
        }
    }
}
