using iiSUMediaScraper.Models.Scraping.Igdb.Converters;
using System.Text.Json.Serialization;

namespace iiSUMediaScraper.Models.Scraping.Igdb;

public class Game
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("age_ratings")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<AgeRating>))]
    public IEnumerable<AgeRating> AgeRatings { get; set; } = [];

    [JsonPropertyName("alternative_names")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<AlternativeName>))]
    public IEnumerable<AlternativeName> AlternativeNames { get; set; } = [];

    [JsonPropertyName("cover")]
    [JsonConverter(typeof(IdOnlyConverter<Cover>))]
    public Cover Cover { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("external_games")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<ExternalGame>))]
    public IEnumerable<ExternalGame> ExternalGames { get; set; } = [];

    [JsonPropertyName("first_release_date")]
    public long? FirstReleaseDate { get; set; }

    [JsonPropertyName("franchises")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<Franchise>))]
    public IEnumerable<Franchise> Franchises { get; set; }

    [JsonPropertyName("game_modes")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<GameMode>))]
    public IEnumerable<GameMode> GameModes { get; set; } = [];

    [JsonPropertyName("genres")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<Genre>))]
    public IEnumerable<Genre> Genres { get; set; } = [];

    [JsonPropertyName("involved_companies")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<InvolvedCompany>))]
    public IEnumerable<InvolvedCompany> InvolvedCompanies { get; set; } = [];

    [JsonPropertyName("keywords")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<Keyword>))]
    public IEnumerable<Keyword> Keywords { get; set; } = [];

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("parent_game")]
    public int? ParentGame { get; set; }

    [JsonPropertyName("platforms")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<Platform>))]
    public IEnumerable<Platform> Platforms { get; set; } = [];

    [JsonPropertyName("player_perspectives")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<PlayerPerspective>))]
    public IEnumerable<PlayerPerspective> PlayerPerspectives { get; set; } = [];

    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("rating_count")]
    public int? RatingCount { get; set; }

    [JsonPropertyName("release_dates")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<ReleaseDate>))]
    public IEnumerable<ReleaseDate> ReleaseDates { get; set; } = [];

    [JsonPropertyName("screenshots")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<Screenshot>))]
    public IEnumerable<Screenshot> Screenshots { get; set; } = [];

    [JsonPropertyName("similar_games")]
    public IEnumerable<int> SimilarGames { get; set; } = [];

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    [JsonPropertyName("tags")]
    public IEnumerable<int> Tags { get; set; } = [];

    [JsonPropertyName("themes")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<Theme>))]
    public IEnumerable<Theme> Themes { get; set; } = [];

    [JsonPropertyName("total_rating")]
    public double? TotalRating { get; set; }

    [JsonPropertyName("total_rating_count")]
    public int? TotalRatingCount { get; set; }

    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("videos")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<IgdbVideo>))]
    public IEnumerable<IgdbVideo> Videos { get; set; } = [];

    [JsonPropertyName("websites")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<Website>))]
    public IEnumerable<Website> Websites { get; set; } = [];

    [JsonPropertyName("checksum")]
    public string Checksum { get; set; }

    [JsonPropertyName("language_supports")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<LanguageSupport>))]
    public IEnumerable<LanguageSupport> LanguageSupports { get; set; } = [];

    [JsonPropertyName("game_localizations")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<GameLocalization>))]
    public IEnumerable<GameLocalization> GameLocalizations { get; set; } = [];

    [JsonPropertyName("collections")]
    [JsonConverter(typeof(IdOnlyEnumerableConverter<Collection>))]
    public IEnumerable<Collection> Collections { get; set; } = [];

    [JsonPropertyName("game_type")]
    public int GameType { get; set; }
}
