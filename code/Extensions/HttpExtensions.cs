using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using TemplateDownloader.Util;

namespace TemplateDownloader.Extensions;

/// <summary>
/// Contains extension methods for HTTP related items.
/// </summary>
internal static class HttpExtensions
{
	/// <summary>
	/// Gets GitHub related rate limiting information from a <see cref="HttpResponseMessage"/>.
	/// </summary>
	/// <param name="message">The message to get rate limit info from.</param>
	/// <returns>The rate limit information relating to this message.</returns>
	internal static RateLimit GetRateLimitInfo( this HttpResponseMessage message )
	{
		var limit = message.Headers.GetRateLimitHeader( "limit" );
		var remaining = message.Headers.GetRateLimitHeader( "remaining" );
		var resetTime = message.Headers.GetRateLimitHeader( "reset" );
		var used = message.Headers.GetRateLimitHeader( "used" );

		return new RateLimit( limit, remaining, resetTime, used );
	}

	/// <summary>
	/// Gets a rate limit header and parses it.
	/// </summary>
	/// <param name="headers">The headers to search through.</param>
	/// <param name="headerName">The name of the header to find.</param>
	/// <returns>A parsed value of the header.</returns>
	private static int GetRateLimitHeader( this HttpResponseHeaders headers, string headerName )
	{
		if ( !headers.TryGetValues( "x-ratelimit-" + headerName, out var values ) )
			return 0;

		if ( !values.Any() )
			return 0;

		if ( int.TryParse( values.First(), out var value ) )
			return 0;

		return value;
	}
}
