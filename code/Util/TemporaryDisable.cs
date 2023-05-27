using Editor;
using System;

namespace TemplateDownloader.Util;

/// <summary>
/// Disables input on the given widget till it is disposed.
/// </summary>
internal readonly struct TemporaryDisable : IDisposable
{
	private Widget DisabledWidget { get; }

	internal TemporaryDisable( Widget widget )
	{
		DisabledWidget = widget;
		widget.Enabled = false;
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		DisabledWidget.Enabled = true;
	}
}
