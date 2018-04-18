using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;

[assembly: FabricTransportServiceRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]

namespace CacheService.Models
{

    public interface ICacheService: IService
    {
        Task<string> Get(string key);

        Task Store(string key, string value);
        Task Delete(string key);
    }
}
