using System;
using System.Diagnostics.CodeAnalysis;

namespace TemplateDownloader.Util;

/// <summary>
/// A wrapper for a <see ref="TValue"/> that can also be a <see ref="TError"/>.
/// </summary>
/// <typeparam name="TValue">The type for the successful value.</typeparam>
/// <typeparam name="TError">The type for the erroneous value.</typeparam>
internal sealed class Result<TValue, TError>
{
	/// <summary>
	/// The value.
	/// </summary>
	internal TValue? Value { get; }
	/// <summary>
	/// The error.
	/// </summary>
	internal TError? Error { get; }

	/// <summary>
	/// Returns whether or not this result is a value.
	/// </summary>
	[MemberNotNullWhen( true, nameof( Value ) )]
	internal bool HasValue => Value is not null;
	/// <summary>
	/// Returns whether or not this result is an error.
	/// </summary>
	[MemberNotNullWhen( true, nameof( Error ) )]
	internal bool IsError => !HasValue;

	/// <summary>
	/// Creates a new <see cref="Result{TValue, TError}"/> from a successful value.
	/// </summary>
	/// <param name="value">The successful value to store.</param>
	internal Result( TValue value )
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="Result{TValue, TError}"/> from an erroneous value.
	/// </summary>
	/// <param name="error">The erroneous value to store.</param>
	internal Result( TError error )
	{
		Error = error;
	}

	public static implicit operator TValue( Result<TValue, TError> result )
	{
		if ( result.IsError )
			throw new InvalidOperationException( "Tried to retrieve a successful value when this was an errored result" );

		return result.Value!;
	}

	public static implicit operator TError( Result<TValue, TError> result )
	{
		if ( result.HasValue )
			throw new InvalidOperationException( "Tried to retrieve an erroneous value when this was a successful result" );

		return result.Error!;
	}

	public static implicit operator Result<TValue, TError>( TValue value ) => new( value );
	public static implicit operator Result<TValue, TError>( TError error ) => new( error );
}
