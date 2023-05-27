using System.Text.Json.Serialization;

namespace TemplateDownloader.Util.Json;

/// <summary>
/// Contains necessary information about a new tree being created from a repository commit.
/// </summary>
public struct CommitTree
{
	/// <summary>
	/// The SHA1 of the new tree.
	/// </summary>
	[JsonPropertyName( "sha" )]
	public string Sha1 { get; set; }
}
