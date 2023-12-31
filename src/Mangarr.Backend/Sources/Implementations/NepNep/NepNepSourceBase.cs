﻿using System.Globalization;
using System.Text.RegularExpressions;
using FluentResults;
using Mangarr.Backend.Sources.Clients;
using Mangarr.Backend.Sources.Extensions;
using Mangarr.Backend.Sources.Implementations.NepNep.Data;
using Mangarr.Backend.Sources.Models.Chapter;
using Mangarr.Backend.Sources.Models.Page;
using Mangarr.Backend.Sources.Models.Search;
using Newtonsoft.Json;
using Group = System.Text.RegularExpressions.Group;

namespace Mangarr.Backend.Sources.Implementations.NepNep;

internal abstract class NepNepSourceBase : SourceBase
{
    protected NepNepSourceBase(
        GenericHttpClient genericHttpClient,
        CloudflareHttpClient cloudflareHttpClient,
        ILoggerFactory loggerFactory
    )
        : base(genericHttpClient, cloudflareHttpClient, loggerFactory)
    {
    }

    protected override Task<Result> Initialize() => Task.FromResult(Result.Ok());

    protected override Task<Result> Cache() => Task.FromResult(Result.Ok());

    protected override Task<Result<string>> Status() => Task.FromResult(Result.Ok("OK"));

    protected sealed override async Task<Result<SearchResult>> Search(string query)
    {
        Result<string> result = await GetHttpClient().Get($"{Url}/search/");

        if (result.IsFailed)
        {
            return result.ToResult();
        }

        MangaData.Item[] items;

        try
        {
            Match match = Regex.Match(result.Value, @"vm.Directory\s*=\s*(.*?]);");

            if (!match.Success)
            {
                return Result.Fail("Unable to find directory");
            }

            Group? group = match.Groups.Values.FirstOrDefault(x => x is not Match);

            if (group == null)
            {
                return Result.Fail("Unable to find directory");
            }

            string json = group.Value;

            items = JsonConvert.DeserializeObject<MangaData.Item[]>(json)!;
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }

        items = items.Where(x => x.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase)).ToArray();

        string coverUrl = string.Empty;

        try
        {
            Match match = Regex.Match(result.Value,
                @"<a[^>]+class=['""]SeriesName['""][^>]*>\s*<img[^>]+src=['""]([^'""]+)['""]");

            if (!match.Success)
            {
                return Result.Fail("Unable to find cover url");
            }

            Group? group = match.Groups.Values.FirstOrDefault(x => x is not Match);

            if (group == null)
            {
                return Result.Fail("Unable to find cover url");
            }

            coverUrl = group.Value;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Result.Ok(
            new SearchResult(
                items.Select(x =>
                        new SearchResultItem(
                            ConstructId($"{Url}/manga/{x.Slug}", x.Slug),
                            x.Title,
                            coverUrl.Replace("{{Series.i}}", x.Slug)))
                    .ToList()));
    }

    protected sealed override async Task<Result<ChapterList>> GetChapterList(string mangaId)
    {
        DeconstructId(mangaId, out string mangaUrl, out string[] args);
        string slug = args[0];

        Result<string> result = await GetHttpClient().Get(mangaUrl);

        if (result.IsFailed)
        {
            return result.ToResult();
        }

        ChapterData[] data;

        try
        {
            string json = result.Value;
            json = json[json.IndexOf("vm.Chapters = ", StringComparison.InvariantCultureIgnoreCase)..];
            json = json[..json.IndexOf("];", StringComparison.InvariantCultureIgnoreCase)];
            json = json.Replace("vm.Chapters = ", string.Empty) + "]";

            data = JsonConvert.DeserializeObject<ChapterData[]>(json)!;
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }

        List<ChapterListItem> items = new();

        foreach (ChapterData chapter in data)
        {
            // Thanks Netsky https://github.com/TheNetsky/extensions-generic-0.8/blob/54ef11e6ea4c89bf7423eba72ab31f7651080de2/src/NepNepParser.ts#L102-L110
            string chapterCode = chapter.Chapter;
            int volume = int.Parse(chapterCode[..1]);
            string index = volume != 1 ? "-index-" + volume : string.Empty;
            int n = int.Parse(chapterCode[1..^1]);
            int a = int.Parse(chapterCode[^1].ToString());
            string m = a != 0 ? "." + a : string.Empty;
            string id = slug + "-chapter-" + n + m + index + ".html";
            double chapterNumber = n + a * 0.1;
            string chapterUrl = Url + "/read-online/" + id;

            items.Add(
                new ChapterListItem(
                    ConstructId(chapterUrl, slug),
                    chapter.Type + " - " + chapterNumber.ToString(CultureInfo.InvariantCulture),
                    chapterNumber,
                    DateTime.Parse(chapter.Date).Date,
                    chapterUrl));
        }

        return Result.Ok(new ChapterList(items));
    }

    protected sealed override async Task<Result<PageList>> GetPageList(string chapterId)
    {
        DeconstructId(chapterId, out string chapterUrl, out string[] args);
        string slug = args[0];

        Result<string> result = await GetHttpClient().Get(chapterUrl);

        if (result.IsFailed)
        {
            return result.ToResult();
        }

        PageData data;

        try
        {
            string json = result.Value;
            json = json[json.IndexOf("vm.CurChapter = ", StringComparison.InvariantCultureIgnoreCase)..];
            json = json[..json.IndexOf("};", StringComparison.InvariantCultureIgnoreCase)];
            json = json.Replace("vm.CurChapter = ", string.Empty) + "}";

            data = JsonConvert.DeserializeObject<PageData>(json)!;
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }

        if (!int.TryParse(data.Page, out int pageCount))
        {
            return Result.Fail("Unable to parse page count");
        }

        List<PageListItem> items = new();

        string chapterString = GetChapterString(data);
        string pathName = GetPathName(result.Value);
        string directory = string.IsNullOrEmpty(data.Directory) ? string.Empty : data.Directory + "/";

        for (int i = 0; i < pageCount; i++)
        {
            string s = "000" + (i + 1);
            string page = s[^3..];
            string url = $"https://{pathName}/manga/{slug}/{directory}{chapterString}-{page}.png";

            items.Add(new PageListItem(ConstructId(url), url));
        }

        return Result.Ok(new PageList(items));
    }

    protected sealed override Task<Result<byte[]>> GetImage(string pageId) =>
        GetHttpClient().GetBuffer(pageId.FromBase64());

    private static string GetChapterString(PageData data)
    {
        string chapterString = data.Chapter[1..^1];

        if (data.Chapter[^1] != '0')
        {
            chapterString += $".{data.Chapter[^1]}";
        }

        return chapterString;
    }

    private static string GetPathName(string body)
    {
        string pathName = body;
        pathName = pathName[pathName.IndexOf("vm.CurPathName = ", StringComparison.InvariantCultureIgnoreCase)..];
        pathName = pathName[..pathName.IndexOf(";", StringComparison.InvariantCultureIgnoreCase)];
        pathName = pathName.Replace("vm.CurPathName = ", string.Empty).Trim('"');
        return pathName;
    }
}
