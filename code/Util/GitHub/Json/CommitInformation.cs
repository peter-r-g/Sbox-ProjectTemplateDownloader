using System.Text.Json.Serialization;

namespace TemplateDownloader.Util.Json;

/// <summary>
/// Contains necessary meta data about a repository commit.
/// </summary>
public struct CommitInformation
{
	/// <summary>
	/// Information about the updated tree that was created from the commit.
	/// </summary>
	[JsonPropertyName( "tree" )]
	public CommitTree Tree { get; set; }
}
