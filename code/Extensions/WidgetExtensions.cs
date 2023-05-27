using Editor;
using System;
using TemplateDownloader.Util;

namespace TemplateDownloader.Extensions;

/// <summary>
/// Contains extension methods for <see cref="Widget"/>.
/// </summary>
internal static class WidgetExtensions
{
	/// <summary>
	/// Disables input on the widget till disposed.
	/// </summary>
	/// <param name="widget">The widget to disable input on.</param>
	/// <returns>An object that re-enables input when disposed.</returns>
	internal static IDisposable DisableTemporarily( this Widget widget )
	{
		return new TemporaryDisable( widget );
	}
}
