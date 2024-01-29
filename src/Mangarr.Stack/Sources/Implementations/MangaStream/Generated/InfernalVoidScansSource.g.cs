// Auto generated

using Injectio.Attributes;
using Mangarr.Stack.Caching;
using Mangarr.Stack.Sources;
using Mangarr.Stack.Sources.Clients;
using Mangarr.Stack.Sources.Implementations.MangaStream;

namespace Mangarr.Backend.Sources.Implementations.MangaStream;

[RegisterSingleton<ISource>(Duplicate = DuplicateStrategy.Append)]
internal class InfernalVoidScansSource : MangaStreamSourceBase
{
    protected override string Id => "voidscans";
    protected override string Name => "Infernal Void Scans";
    protected override string Url => "https://void-scans.com";
    protected override bool HasCloudflareProtection => false;

    public InfernalVoidScansSource(
        GenericHttpClient genericHttpClient,
        CloudflareHttpClient cloudflareHttpClient,
        ILoggerFactory loggerFactory,
        CachingService cachingService
    )
        : base(genericHttpClient, cloudflareHttpClient, loggerFactory, cachingService)
    {
    }
}
