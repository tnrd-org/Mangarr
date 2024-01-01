﻿using Injectio.Attributes;

namespace Mangarr.Backend.Sources.Clients;

[RegisterTransient]
public class CloudflareHttpClient : CustomHttpClient
{
    protected override string ClientName => "Cloudflare";

    public CloudflareHttpClient(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory)
    {
    }
}
