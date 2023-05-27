namespace TemplateDownloader.Util;

/// <summary>
/// Contains all neccessary information about rate limiting provided by the GitHub API.
/// </summary>
internal readonly struct RateLimit
{
	/// <summary>
	/// The maximum number of requests you are permitted to make per hour.
	/// </summary>
	internal int Limit { get; }
	/// <summary>
	/// The number of requests remaining in the current rate limit window.
	/// </summary>
	internal int Remaining { get; }
	/// <summary>
	/// The time at which the current rate limit window resets in UTC epoch seconds.
	/// </summary>
	internal int ResetTime { get; }
	/// <summary>
	/// The number of requests you have made in the current rate limit window.
	/// </summary>
	internal int Used { get; }

	internal RateLimit( int limit = 0, int remaining = 0, int resetTime = 0, int used = 0 )
	{
		Limit = limit;
		Remaining = remaining;
		ResetTime = resetTime;
		Used = used;
	}
}
