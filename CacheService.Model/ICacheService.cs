using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;

[assembly: FabricTransportServiceRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]

namespace CacheService.Model
{

    public interface ICacheService: IService
    {
        Task<byte[]> GetAsync(string key, CancellationToken token);

        Task SetAsync(string key, CacheItem value, CancellationToken token);
        Task RemoveAsync(string key, CancellationToken token);

        Task RefreshAsync(string key, CancellationToken token);
    }
}
