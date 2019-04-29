using EquipmentData.Globals;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EquipmentWeb
{
    public interface ICommon
    {
        StatelessServiceContext GetServiceContext();

        HttpClient GetHttpClient();

        FabricClient GetFabricClient();

        Uri GetEquipmentDataServiceName();

        Uri GetProxyAddress();

        string GetInvoicesProxyGetUrl(Partition partition, bool downloadLastInvoice = true);

        string GetEquipmentsProxyGetUrl(Partition partition);

        string GetBasketProxyGetUrl(Partition partition);

        string GetBasketProxyPutUrl(string name, int days = Constants.MinimumOrderDays);

        string GetBasketProxyDeleteUrl(Guid guid);

        string GetBasketProxyCloseUrl();
    }
}
