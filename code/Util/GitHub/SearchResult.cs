using System.Text.Json.Serialization;

namespace TemplateDownloader;

/// <summary>
/// Contains all necessary information for a GitHub search result.
/// </summary>
public struct SearchResult
{
	/// <summary>
	/// The number of entries in the <see cref="Repositories"/> array.
	/// </summary>
	[JsonPropertyName( "total_count" )]
	public int Count { get; set; }

	/// <summary>
	/// An array containing all of the repositories found in the search.
	/// </summary>
	[JsonPropertyName( "items" )]
	public GitHubRepository[] Repositories { get; set; }
}
