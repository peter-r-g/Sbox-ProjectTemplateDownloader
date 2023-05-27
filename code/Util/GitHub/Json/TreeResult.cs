using System.Text.Json.Serialization;

namespace TemplateDownloader.Util.Json;

/// <summary>
/// Contains necessary information about a repository tree.
/// </summary>
public struct TreeResult
{
	/// <summary>
	/// The items that are contained in the tree.
	/// </summary>
	[JsonPropertyName( "tree" )]
	public TreeItem[] Items { get; set; }
}
