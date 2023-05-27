using Sandbox;
namespace TemplateDownloader.Util;

/// <summary>
/// Represents a cached result of fetching an endpoint.
/// </summary>
internal readonly struct CachedEndpoint
{
	/// <summary>
	/// The time in seconds for the result to be cached.
	/// </summary>
	internal const float ExpiryTime = 30;

	/// <summary>
	/// The result of an endpoint.
	/// </summary>
	internal string Result { get; }
	/// <summary>
	/// The time in seconds till this result expires.
	/// </summary>
	internal RealTimeUntil ExpiresIn { get; }

	private CachedEndpoint( string result, float expiryTime = ExpiryTime )
	{
		Result = result;
		ExpiresIn = expiryTime;
	}

	public static implicit operator CachedEndpoint( string result ) => new( result );
	public static implicit operator string( in CachedEndpoint cachedEndpoint ) => cachedEndpoint.Result;
}
