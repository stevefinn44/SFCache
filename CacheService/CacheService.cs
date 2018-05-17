using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using CacheService.Model;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace CacheService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class CacheService : StatefulService, ICacheService
    {
        public CacheService(StatefulServiceContext context)
            : base(context)
        { }

       
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token)
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, CacheItem>>("cache");
            using (var tx = StateManager.CreateTransaction())
            {
                var result = await dictionary.TryGetValueAsync(tx, key);
                await tx.CommitAsync();
                return (result.HasValue)? result.Value.Value : new byte[0];
            }
        }

        public async Task SetAsync(string key, CacheItem value, CancellationToken token)
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, CacheItem>>("cache");
            using (var tx = StateManager.CreateTransaction())
            {
                await dictionary.AddOrUpdateAsync(tx, key, value,
                    (key1, existing) => value);

                await tx.CommitAsync();

            }
        }
   

        public async Task RemoveAsync(string key, CancellationToken token)
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, byte[]>>("cache");
            using (var tx = StateManager.CreateTransaction())
            {
                await dictionary.TryRemoveAsync(tx, key);

                await tx.CommitAsync();

            }
        }
    }
}
