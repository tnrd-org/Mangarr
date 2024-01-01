﻿using FluentResults;

namespace Mangarr.Backend.Sources.FluentResults;

public class UrlReason : IReason
{
    public string Url { get; }

    public UrlReason(string url)
    {
        Url = url;
        Message = $"Url: {url}";
    }

    public string Message { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
