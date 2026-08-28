using System.Net.Http.Json;
using System.Text.Json;
using WebAPI.Models.Common;

namespace Seeder;

public static class SeasonContentBlocksSeeder
{
	public const string HistoryHtml = "<p>History data</p>";

	public static async Task EnsureAsync(
		HttpClient http,
		JsonSerializerOptions jsonOptions,
		string routePrefix,
		Guid seasonId,
		IReadOnlyList<SeasonContentBlockSeed>? blocks,
		string seasonName)
	{
		bool hasSeedBlocks = blocks is { Count: > 0 };
		if (!hasSeedBlocks)
		{
			HttpResponseMessage get = await http.GetAsync(routePrefix + "/" + seasonId + "/content-blocks");
			if (get.IsSuccessStatusCode)
			{
				ApiResponse<SeasonContentBlocksResponse>? existing =
					await get.Content.ReadFromJsonAsync<ApiResponse<SeasonContentBlocksResponse>>(jsonOptions);
				if (existing?.Data?.Blocks is { Count: > 0 })
				{
					return;
				}
			}
		}

		List<SeasonContentBlockPutItem> items = ToItems(blocks, seasonName);
		HttpResponseMessage put = await http.PutAsJsonAsync(
			routePrefix + "/" + seasonId + "/content-blocks",
			new { items });
		await SeederHttp.EnsureSuccessWithBody(put, "Replace season content blocks");
		Console.WriteLine("  Content blocks for '" + seasonName + "': " + items.Count);
	}

	private static List<SeasonContentBlockPutItem> ToItems(
		IReadOnlyList<SeasonContentBlockSeed>? blocks,
		string seasonName)
	{
		if (blocks is { Count: > 0 })
		{
			return blocks
				.Select(block => new SeasonContentBlockPutItem
				{
					Title = block.Title,
					ContentHtml = block.ContentHtml,
				})
				.ToList();
		}

		return new List<SeasonContentBlockPutItem>
		{
			new SeasonContentBlockPutItem
			{
				Title = string.IsNullOrWhiteSpace(seasonName) ? "Season" : seasonName,
				ContentHtml = HistoryHtml,
			},
		};
	}

	private sealed class SeasonContentBlocksResponse
	{
		public List<SeasonContentBlockResponse>? Blocks { get; set; }
	}

	private sealed class SeasonContentBlockResponse
	{
		public Guid Id { get; set; }
	}

	private sealed class SeasonContentBlockPutItem
	{
		public string Title { get; set; } = "";
		public string ContentHtml { get; set; } = "";
	}
}
