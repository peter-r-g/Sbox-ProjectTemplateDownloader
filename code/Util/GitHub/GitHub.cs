using Sandbox;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TemplateDownloader.Extensions;

namespace TemplateDownloader.Util;

/// <summary>
/// Contains helper methods for working with the GitHub API.
/// </summary>
internal static class GitHub
{
	/// <summary>
	/// A dedicated client for interacting with the GitHub API.
	/// </summary>
	private static HttpClient Client { get; set; }
	/// <summary>
	/// The time in seconds until the API can be used again.
	/// </summary>
	private static RealTimeUntil TimeUntilCanUseApi { get; set; }

	/// <summary>
	/// A thread-safe dictionary containing all of the currently cached endpoints.
	/// </summary>
	private static ConcurrentDictionary<string, CachedEndpoint> CachedEndpoints { get; } = new();

	static GitHub()
	{
		Client = new();
		Client.DefaultRequestHeaders.UserAgent.ParseAdd( "s&box" );
	}

	/// <summary>
	/// GETs an endpoint in the GitHub API.
	/// </summary>
	/// <param name="url">The URL to GET.</param>
	/// <returns>A task that represents the asynchronous operation. The result will either be the result of the endpoint or an error code.</returns>
	/// <exception cref="ArgumentException">Thrown when attempting to get an endpoint that is not GitHubs.</exception>
	private static async ValueTask<Result<string, int>> GetEndpointAsync( string url )
	{
		if ( !url.StartsWith( "https://api.github.com/" ) )
			throw new ArgumentException( "The URL must be to api.github.com", nameof( url ) );

		// Check if we've already cached this recently.
		if ( CachedEndpoints.TryGetValue( url, out var cachedEndpoint ) && cachedEndpoint.ExpiresIn > 0 )
			return cachedEndpoint.Result;

		// Check if we're rate limited.
		if ( TimeUntilCanUseApi > 0 )
			return 1;

		var response = await Client.GetAsync( url );
		var content = await response.Content.ReadAsStringAsync();

		// Update rate limit info.
		var rateLimit = response.GetRateLimitInfo();
		if ( rateLimit.Remaining <= 0 )
		{
			var timestamp = DateTime.UnixEpoch.AddSeconds( rateLimit.Remaining );
			TimeUntilCanUseApi = (float)(timestamp - DateTime.Now).TotalSeconds;
		}

		// Add endpoint to cache and return.
		CachedEndpoints.AddOrUpdate( url, content, ( _, _ ) => content );
		return content;
	}

	/// <summary>
	/// Gets information about a repository.
	/// </summary>
	/// <param name="id">The unique ID of the repository.</param>
	/// <returns>A task that represents the asynchronous operation. The result will either be the found <see cref="GitHubRepository"/> or an error code.</returns>
	internal static async ValueTask<Result<GitHubRepository, int>> GetRepositoryAsync( int id )
	{
		var result = await GetEndpointAsync( "https://api.github.com/repositories/" + id );
		if ( result.IsError )
			return result.Error;

		return JsonSerializer.Deserialize<GitHubRepository>( result );
	}

	/// <summary>
	/// Searches GitHub for repositories that fit the query provided.
	/// </summary>
	/// <param name="query">The search query to provide to the endpoint.</param>
	/// <returns>A task that represents the asynchronous operation. The result will either be a <see cref="SearchResult"/> or an error code.</returns>
	internal static async ValueTask<Result<SearchResult, int>> SearchAsync( string query )
	{
		var result = await GetEndpointAsync( "https://api.github.com/search/repositories?q=" + query );
		if ( result.IsError )
			return result.Error;

		return JsonSerializer.Deserialize<SearchResult>( result );
	}
}
