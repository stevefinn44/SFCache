using System;

namespace CacheService.Model
{
    public class CacheItem
    {
        public byte[] Value { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? LastReference { get; set; }

        public bool Deletable()
        {
            if (AbsoluteExpiration.HasValue)
            {
                if (DateTimeOffset.UtcNow > (DateTimeOffset) AbsoluteExpiration.Value)
                    return true;
            }

            if (SlidingExpiration.HasValue)
            {
                return DateTimeOffset.UtcNow > Created + (TimeSpan) SlidingExpiration;
            }

            return false;
        }
    }
}
