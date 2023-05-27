using System;
using System.Text.Json.Serialization;

namespace TemplateDownloader.Util.Json;

/// <summary>
/// Contains all necessary information for a GitHub repository.
/// </summary>
public struct Repository
{
	/// <summary>
	/// The unique ID given to the repository.
	/// </summary>
	[JsonPropertyName( "id" )]
	public int Id { get; set; }

	/// <summary>
	/// The name of the repository.
	/// </summary>
	[JsonPropertyName( "name" )]
	public string Name { get; set; }

	/// <summary>
	/// The owner plus <see cref="Name"/> of the repository concatanated with a '/'.
	/// </summary>
	[JsonPropertyName( "full_name" )]
	public string FullName { get; set; }

	/// <summary>
	/// The description given to the repository.
	/// </summary>
	[JsonPropertyName( "description" )]
	public string Description { get; set; }

	/// <summary>
	/// The user-facing URL of the repository.
	/// </summary>
	[JsonPropertyName( "html_url" )]
	public string Url { get; set; }

	/// <summary>
	/// The URL to clone the repository with.
	/// </summary>
	[JsonPropertyName( "clone_url" )]
	public string CloneUrl { get; set; }

	/// <summary>
	/// The default branch of this repository.
	/// </summary>
	[JsonPropertyName( "default_branch" )]
	public string DefaultBranch { get; set; }

	/// <summary>
	/// The last time the repository was updated.
	/// </summary>
	[JsonPropertyName( "updated_at" )]
	public DateTime LastUpdate { get; set; }
}
