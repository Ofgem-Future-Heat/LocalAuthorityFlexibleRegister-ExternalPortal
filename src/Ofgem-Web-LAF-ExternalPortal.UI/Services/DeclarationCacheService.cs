using Ofgem_Web_LAF_ExternalPortal.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Services
{
    public static class DeclarationCacheService
    {
        private static readonly Dictionary<Guid, CachedDeclaration> CachedData = [];

        private const int TimeOutInMinutes = 20;

        /// <summary>
        /// Gets the matching entry from the declaration cache
        /// </summary>
        /// <param name="declarationId">guid</param>
        /// <returns>Declaration or null</returns>
        public static Declaration? Get(Guid declarationId)
        {
            CleanCache();

            return CachedData.TryGetValue(declarationId, out var value)
                ? value.Declaration
                : null;
        }

        /// <summary>
        /// Adds the given declaration with a time stamp into the cache
        /// </summary>
        /// <param name="declaration"></param>
        public static void Add(Declaration? declaration)
        {
            if (declaration is null) return;

            var source = new CachedDeclaration
            {
                Declaration = declaration,
                AddedToCache = DateTime.UtcNow
            };

            CachedData.Add(declaration.DeclarationId, source);
        }

        public static void Update(Declaration? declaration)
        {
            if (declaration is null) return;

            var source = CachedData.TryGetValue(declaration.DeclarationId, out var value)
                ? value
                : null;

            if (source is null)
            {
                Add(declaration);
                return;
            }

            source.AddedToCache = DateTime.UtcNow;
            source.Declaration = declaration;

            CachedData[declaration.DeclarationId] = source;
        }

        public static int Count()
        {
            return CachedData.Count;
        }

        public static void Clear()
        {
            CachedData.Clear();
        }

        public static void Remove(Guid declarationId)
        {
            CachedData.Remove(declarationId);
        }

        private static void CleanCache()
        {
            foreach (KeyValuePair<Guid, CachedDeclaration> cachedDeclaration in CachedData)
            {
                if (DateTime.UtcNow > cachedDeclaration.Value.AddedToCache.AddMinutes(TimeOutInMinutes))
                    CachedData.Remove(cachedDeclaration.Key);
            }
        }

        private sealed class CachedDeclaration
        {
            public DateTime AddedToCache { get; set; }
            public Declaration? Declaration { get; set; }
        }
    }
}
