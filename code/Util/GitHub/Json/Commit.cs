using System.Text.Json.Serialization;

namespace TemplateDownloader.Util.Json;

/// <summary>
/// Contains all necessary information about a commit to a repository branch.
/// </summary>
public struct Commit
{
	/// <summary>
	/// Contains meta data about the commit.
	/// </summary>
	[JsonPropertyName( "commit" )]
	public CommitInformation CommitInformation { get; set; }
}
