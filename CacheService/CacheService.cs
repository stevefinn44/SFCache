using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using CacheService.Models;
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

        public async Task<string> Get(string key)
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("cache");
            using (var tx = StateManager.CreateTransaction())
            {
                var result = await dictionary.TryGetValueAsync(tx, key);
                await tx.CommitAsync();
                return result.HasValue ? result.Value : string.Empty;
            }
        }

        public async Task Store(string key, string value)
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("cache");
            using (var tx = StateManager.CreateTransaction())
            {
                await dictionary.AddOrUpdateAsync(tx, key, value,
                    (key1, existing) => value);

                await tx.CommitAsync();

            }
        }
   

        public async Task Delete(string key)
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("cache");
            using (var tx = StateManager.CreateTransaction())
            {
                await dictionary.TryRemoveAsync(tx, key);

                await tx.CommitAsync();

            }
        }
    }
}
