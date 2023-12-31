﻿using Anilist4Net;
using FluentResults;
using Mangarr.Backend.AniList;
using Mangarr.Shared.Models;
using Mangarr.Shared.Requests;
using Mangarr.Shared.Responses;

namespace Mangarr.Backend.Endpoints.Manga.Titles;

public class MangaTitlesEndpoint : Endpoint<MangaTitlesRequest, MangaTitlesResponse>
{
    private readonly AniListService _aniListService;

    public MangaTitlesEndpoint(AniListService aniListService) => _aniListService = aniListService;

    public override void Configure()
    {
        Get("manga/{id}/titles");
        AllowAnonymous();
    }

    public override async Task HandleAsync(MangaTitlesRequest req, CancellationToken ct)
    {
        Result<Media?> result = await _aniListService.GetMedia(req.Id);

        if (result.IsFailed)
        {
            Logger.LogError("Unable to get manga titles: {Id}; {Result}", req.Id, result.ToString());
            ThrowError("Unable to get manga titles");
        }

        if (result.Value == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        List<string> titles = new() { result.Value.EnglishTitle, result.Value.RomajiTitle, result.Value.NativeTitle };
        titles.AddRange(result.Value.Synonyms);

        await SendOkAsync(new MangaTitlesResponse { Data = new MangaTitlesModel { Titles = titles.ToArray() } }, ct);
    }
}
