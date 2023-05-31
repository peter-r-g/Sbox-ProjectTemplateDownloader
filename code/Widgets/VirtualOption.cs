using Editor;
using Sandbox;
using System;

namespace TemplateDownloader.Widgets;

/// <summary>
/// Represents an option in a <see cref="PaginatedNavigationView"/>.
/// </summary>
public class VirtualOption : VirtualWidget
{
	/// <summary>
	/// The <see cref="PaginatedNavigationView"/> that owns this option.
	/// </summary>
	internal PaginatedNavigationView? Owner { get; set; }

	/// <summary>
	/// The title of the option.
	/// </summary>
	public string Title { get; set; }
	/// <summary>
	/// The material icon placed next to the title.
	/// </summary>
	public string Icon { get; set; }
	/// <summary>
	/// The page to display in the <see cref="PaginatedNavigationView"/>.
	/// </summary>
	public Widget? Page { get; set; }

	/// <summary>
	/// This options rect in screen coordinates.
	/// </summary>
	public Rect ScreenRect
	{
		get
		{
			if ( Owner is null )
				throw new InvalidOperationException( $"This option is not a part of a {nameof( PaginatedNavigationView )}" );

			var pos = Owner.ScreenRect.Position + Rect.Position;
			return new Rect( pos, Rect.Size );
		}
	}

	/// <summary>
	/// A callback to do context menu behavior for this option.
	/// </summary>
	public Action? OpenContextMenu { get; set; }
	/// <summary>
	/// A callback to create a new page for this option.
	/// </summary>
	public Func<Widget>? CreatePage { get; set; }

	/// <summary>
	/// Initializes a new instance of <see cref="VirtualOption"/>.
	/// </summary>
	/// <param name="title">The title to set on the option.</param>
	/// <param name="icon">The material icon to set on the option.</param>
	public VirtualOption( string title, string icon )
	{
		Title = title;
		Icon = icon;
	}

	/// <summary>
	/// Gets or creates the page for this option.
	/// </summary>
	/// <returns>The page for this option.</returns>
	public Widget? GetOrCreatePage()
	{
		if ( Page is null && CreatePage is not null )
			Page = CreatePage();

		return Page;
	}
}
