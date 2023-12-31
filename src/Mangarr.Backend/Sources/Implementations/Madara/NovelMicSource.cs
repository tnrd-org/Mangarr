﻿using Injectio.Attributes;
using Mangarr.Backend.Sources.Clients;

namespace Mangarr.Backend.Sources.Implementations.Madara;

[RegisterSingleton<ISource>(Duplicate = DuplicateStrategy.Append)]
internal class NovelMicSource : MadaraSourceBase
{
    protected override string Id => "novelmic";
    protected override string Name => "Novel Mic";
    protected override string Url => "https://novelmic.com";
    protected override bool UseIdChapterListMethod => true;

    public NovelMicSource(GenericHttpClient genericHttpClient, CloudflareHttpClient cloudflareHttpClient)
        : base(genericHttpClient, cloudflareHttpClient)
    {
    }
}