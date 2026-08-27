using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Hockey.Seasons.DTOs;
using Domain.Entities.Common;
using Domain.Enums.Common;
using WebAPI.Models.Common;

namespace Seeder;

public static class SeasonContentBlockSeeder
{
	public static async Task SeedFloorballAsync(
		HttpClient http,
		JsonSerializerOptions jsonOptions,
		IReadOnlyList<FloorballSeasonDto> seasons)
	{
		FloorballSeasonDto? season = seasons.FirstOrDefault();
		if (season == null)
		{
			Console.WriteLine("No floorball season available; skipping content blocks.");
			return;
		}

		await SeedForSeasonAsync(
			http,
			jsonOptions,
			SportsCategory.Floorball,
			season.Id,
			SeasonYearLabel.FromDates(season.StartDate, season.EndDate),
			FloorballDefaults());
	}

	public static async Task SeedFootballAsync(
		HttpClient http,
		JsonSerializerOptions jsonOptions,
		IReadOnlyList<FootballSeasonDto> seasons)
	{
		FootballSeasonDto? season = seasons.FirstOrDefault();
		if (season == null)
		{
			Console.WriteLine("No football season available; skipping content blocks.");
			return;
		}

		await SeedForSeasonAsync(
			http,
			jsonOptions,
			SportsCategory.Football,
			season.Id,
			SeasonYearLabel.FromDates(season.StartDate, season.EndDate),
			FootballDefaults());
	}

	public static async Task SeedHockeyAsync(
		HttpClient http,
		JsonSerializerOptions jsonOptions,
		IReadOnlyList<HockeySeasonDto> seasons)
	{
		HockeySeasonDto? season = seasons.FirstOrDefault();
		if (season == null)
		{
			Console.WriteLine("No hockey season available; skipping content blocks.");
			return;
		}

		await SeedForSeasonAsync(
			http,
			jsonOptions,
			SportsCategory.Icehockey,
			season.Id,
			SeasonYearLabel.FromDates(season.StartDate, season.EndDate),
			HockeyDefaults());
	}

	private static async Task SeedForSeasonAsync(
		HttpClient http,
		JsonSerializerOptions jsonOptions,
		SportsCategory sport,
		Guid competitionId,
		string seasonYear,
		IReadOnlyList<(string Title, string ContentHtml)> blocks)
	{
		HttpResponseMessage listResp = await http.GetAsync(
			$"api/SeasonContentBlock?competitionId={competitionId}");
		if (listResp.IsSuccessStatusCode)
		{
			ApiResponse<List<SeasonContentBlockDto>>? existing =
				await listResp.Content.ReadFromJsonAsync<ApiResponse<List<SeasonContentBlockDto>>>(jsonOptions);
			if (existing?.Data != null && existing.Data.Count > 0)
			{
				Console.WriteLine(
					$"Season content blocks already exist for {sport} season {competitionId}, skipping.");
				return;
			}
		}

		for (int index = 0; index < blocks.Count; index++)
		{
			(string title, string contentHtml) = blocks[index];
			CreateSeasonContentBlockRequest request = new()
			{
				Sport = sport,
				CompetitionId = competitionId,
				SeasonYear = seasonYear,
				Title = title,
				ContentHtml = contentHtml,
				SortOrder = index,
			};

			HttpResponseMessage response = await http.PostAsJsonAsync("api/SeasonContentBlock", request, jsonOptions);
			await SeederHttp.EnsureSuccessWithBody(response, $"Create {sport} season content block");
			Console.WriteLine($"Created {sport} content block '{title}' ({seasonYear})");
		}
	}

	private static IReadOnlyList<(string Title, string ContentHtml)> FloorballDefaults()
	{
		return
		[
			(
				"MAHL Salibandy 2025–2026 | Saimaanportti",
				Paragraphs(
					"Ole nopea ja varaa paikkasi jo hyvissä ajoin kaupungin kovimmassa salibandysarjassa. Kausi 2025-2026 pelataan samalla kaavalla kun pari edellistäkin kautta eli kaikki lisenssipelaajat ovat vapautettuja sarjatasosta riippumatta! Joukkueissa voi olla rajaton määrä lisenssipelaajia, mutta pelikohtaisesti maksimissaan kolme (3). Kausi aloitetaan tasonmittauspeleillä, jonka jälkeen pelataan sekä liigan että divarin mestaruudesta!",
					"Jokaisen joukkueen tulee nimetä 2 tuomaria, jotka sitoutuvat viheltämään osan kauden otteluista. Tuomareille maksetaan peleistä normaalikorvaus (10e/ottelu). Tarvittaessa MAHL kouluttaa tuomarit syksyllä.",
					"Sarjamaksuun sisältyy myös osallistumisoikeus PMT Festival salibandyturnaukseen, joka järjestetään keväällä 2026 Saimaa Stadiumilla.")
			),
			(
				"Sarjainfo",
				Paragraphs(
					"Tavoitellaan mukaan 12-16 joukkuetta. Kauden alkuun tasonmittauspelit, jonka jälkeen jakaannutaan liigaan ja divariin. Sarjakausi huipentuu loppupeleihin. Pelit perjantai-iltaisin klo 18-22, ottelut pelataan uudella Saimaan Portin koululla, jossa on mattoalusta.",
					"Lopullinen sarjamuoto sekä mitallipelien pelaamismuoto määräytyy joukkuemäärän mukaan. Otteluita joukkueelle kertyy kauden aikana n. 16-24 kappaletta.",
					"Lisenssipelaajat sallittuja sarjatasosta riippumatta. Joukkueessa voi olla rajaton määrä lisenssipelaajia, mutta pelikohtaisesti maksimissaan kolme (3). Maalivahteja EI LASKETA lisenssipelaajiksi. Alle 18 vuotiaita ei lasketa lisenssipelaajiksi, vaikka pelaisivatkin korkeammalla tasolla. Myöskään naisia tai 1986 syntyneitä miehiä ei lasketa lisenssipelaajiksi. Lainavahdin käyttö MAHL salibandyssä on sallittua.",
					"Salibandyn PMT turnaus sisältyy joukkuemaksuun! Joukkuemaksu sisältää osallistumisoikeuden toukokuussa järjestettävään PMT festival turnaukseen. Turnaus järjestetään Saimaa Stadiumilla toukokuussa 2026. Lisäinfoa turnauksesta keväällä.")
			),
			(
				"Lainapelaajat",
				Paragraphs(
					"Kaudelle 2025-2026 stuntataan lainapelaajien pankkia. Mikäli pelipäivänä joukkueella on pulaa pelaajista voi joukkue täydentää rosteriaan lainapelaajista siten, että pelaamassa olevan rosterin koko on 1+10 pelaajaa. Homma toimii siten, että JoJo ilmoittaa Mahl:lle tarvittavan lainapelaajien määrän ja MAHL hoitaa lainapelaajat otteluihin. MAHL laskuttaa lainapelaajat. Tästä vielä lisäinfoa ennen kauden alkua.")
			),
			(
				"Joukkuemaksu",
				Paragraphs(
					"Osallistumismaksu 1200-2000 €, alv. 0% (Lopullinen hinta selviää joukkuemäärän ja sarjamuodon varmistuttua).",
					"Puolet joukkuemaksusta tulee olla hoidettuna lokakuun 2025 loppuun mennessä. Loput joukkuemaksusta tulee olla hoidettuna tammikuun 2026 loppuun mennessä.",
					"Joukkuemaksu sisältää osallistumisoikeuden PMT festival salibandyturnaukseen. Joukkueiden laskutus tapahtuu suomisportin kautta. Jojoihin ollaan yhteydessä laskutuksesta kauden alussa.")
			),
			(
				"Lisätietoja salibandykaudesta antaa",
				"<p>Mikko Luukkonen<br>mikko(at)mahl.fi<br>044 209 9199</p>"
			),
		];
	}

	private static IReadOnlyList<(string Title, string ContentHtml)> FootballDefaults()
	{
		return
		[
			(
				"MAHL Jalkapallo",
				Paragraphs(
					"Seuraa MAHL:n jalkapallosarjoja, sarjataulukoita, otteluohjelmia ja tuloksia.",
					"Valitse kausi yläreunasta, niin näet kyseisen vuoden kilpailut ja sisältöblokit.",
					"Ottelusivuilla näkyvät maalit, kortit, vaihdot ja kokoonpanot.")
			),
			(
				"Sarjainfo",
				Paragraphs(
					"Jalkapallosarjoissa käytetään voitto–tasapeli–tappio-pistelaskua ja maalieroa.",
					"Sarjataulukot ja pelaajatilastot päivittyvät otteluiden valmistuttua.",
					"Turnauksissa on lohkotaulukot ja tarvittaessa pudotuspelikaavio.",
					"Kausivalitsimella vaihdat nykyisen kauden ja arkiston välillä.")
			),
			(
				"Lisätietoja jalkapallokaudesta antaa",
				"<p>Mikko Luukkonen<br>mikko(at)mahl.fi<br>044 209 9199</p>"
			),
		];
	}

	private static IReadOnlyList<(string Title, string ContentHtml)> HockeyDefaults()
	{
		return
		[
			(
				"MAHL Jääkiekko",
				Paragraphs(
					"MAHL:n jääkiekkosarjat tarjoavat harrastekiekkoa eri tasoille. Kausi pelataan liigassa ja muissa sarjoissa, ja ottelut sekä tilastot päivittyvät sivustolle.",
					"Valitse kausi yläreunasta nähdäksesi kyseisen vuoden sarjat, sarjataulukot ja kausikohtaiset infot.")
			),
			(
				"Sarjainfo",
				Paragraphs(
					"Ottelut pelataan kauden aikana ja sarjataulukko muodostuu voittojen, jatkoaikavoittojen ja maalien perusteella.",
					"Joukkueiden tilastot, pelaajatilastot ja otteluohjelma löytyvät sarjasivulta.",
					"Turnaukset ja cupit julkaistaan omilla sivuillaan, kun ne on avattu.")
			),
			(
				"Lisätietoja jääkiekkokaudesta antaa",
				"<p>Mikko Luukkonen<br>mikko(at)mahl.fi<br>044 209 9199</p>"
			),
		];
	}

	private static string Paragraphs(params string[] paragraphs)
	{
		return string.Concat(paragraphs.Select(paragraph => $"<p>{WebUtility.HtmlEncode(paragraph)}</p>"));
	}
}
