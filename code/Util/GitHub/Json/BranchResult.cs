using System.Text.Json.Serialization;

namespace TemplateDownloader.Util.Json;

/// <summary>
/// Contains all necesary information for a GitHub branch.
/// </summary>
public struct BranchResult
{
	/// <summary>
	/// The latest commit that was made to the branch.
	/// </summary>
	[JsonPropertyName( "commit" )]
	public Commit LatestCommit { get; set; }
}
