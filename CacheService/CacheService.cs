using System;
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
        private Timer DeleteTimer { get; set; } 
        public CacheService(StatefulServiceContext context)
            : base(context)
        { }

       
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }


        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Instanciate the DeleteTimer
            if (DeleteTimer == null)
            {
                DeleteTimer = new Timer(RunDeleteCheck, null, 0, 10000 );
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            await Task.Delay(10);
        }

        private  void RunDeleteCheck(object info)
        {
            Task.Run(EnumeratedictionaryAndDeleteStaleCacheItems);

        }

        private async Task EnumeratedictionaryAndDeleteStaleCacheItems()
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, CacheItem>>("cache");
            using (var tx = StateManager.CreateTransaction())
            {
                var enumerator = await dictionary.CreateEnumerableAsync(tx);
                var asyncEnumerator = enumerator.GetAsyncEnumerator();
                while (await asyncEnumerator.MoveNextAsync(new CancellationToken()))
                {
                    var cacheItem = asyncEnumerator.Current.Value;
                    
                    if (cacheItem.Deletable())
                    {
                        await dictionary.TryRemoveAsync(tx, asyncEnumerator.Current.Key);
                    }
                }

                await tx.CommitAsync();
            }

            
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

        public async Task RefreshAsync(string key, CancellationToken token)
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, CacheItem>>("cache");
            using (var tx = StateManager.CreateTransaction())
            {
                var existingItem = await dictionary.TryGetValueAsync(tx, key);
                if (existingItem.HasValue)
                {
                    var existingCacheItem = existingItem.Value;
                    var newItem = new CacheItem
                    {
                        Value = existingCacheItem.Value,
                        AbsoluteExpiration = existingCacheItem.AbsoluteExpiration,
                        SlidingExpiration = existingCacheItem.SlidingExpiration,
                        Created = DateTimeOffset.UtcNow
                    };
                    await dictionary.TryUpdateAsync(tx, key, newItem, existingCacheItem);
                    await tx.CommitAsync();
                }
               

            }
        }
    }
}
