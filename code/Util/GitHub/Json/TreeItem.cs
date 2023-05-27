using System.Text.Json.Serialization;

namespace TemplateDownloader.Util.Json;

/// <summary>
/// Contains necessary information about an item in a repository tree.
/// </summary>
public struct TreeItem
{
	/// <summary>
	/// The file path of the item.
	/// </summary>
	[JsonPropertyName( "path" )]
	public string Path { get; set; }

	/// <summary>
	/// The type of item this is.
	/// </summary>
	[JsonPropertyName( "type" )]
	public string Type { get; set; }

	/// <summary>
	/// The SHA1 of the item.
	/// </summary>
	[JsonPropertyName( "sha" )]
	public string Sha1 { get; set; }
}
