using System;

namespace CacheService.Model
{
    public class CacheItem
    {
        public byte[] Value { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset LastReference { get; set; }
    }
}
