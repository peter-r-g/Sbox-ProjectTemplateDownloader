using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using TemplateDownloader.Util;

namespace TemplateDownloader.Widgets;

/// <summary>
/// A <see cref="NavigationView"/> that supports paging.
/// </summary>
public class PaginatedNavigationView : Widget
{
	/// <summary>
	/// A readonly list of all options that are currently contained.
	/// </summary>
	public IReadOnlyList<VirtualOption> Options => options;
	private readonly List<VirtualOption> options = new();

	/// <summary>
	/// The page of the <see cref="CurrentOption"/>.
	/// </summary>
	public Widget? CurrentPage
	{
		get => currentPage;
		set
		{
			if ( currentPage == value )
				return;

			if ( currentPage.IsValid() )
				currentPage.Visible = false;

			currentPage = value;

			if ( currentPage.IsValid() )
			{
				PageContents.Add( currentPage );
				currentPage.Visible = true;
			}

			foreach ( var option in options )
			{
				if ( option.Page != currentPage )
				{
					option.Selected = false;
					continue;
				}

				CurrentOption = option;
			}

			Update();
		}
	}
	private Widget? currentPage;

	/// <summary>
	/// The current option that has been selected.
	/// </summary>
	public VirtualOption CurrentOption
	{
		get => currentOption;
		set
		{
			if ( currentOption == value )
				return;

			if ( currentOption is not null )
				currentOption.Selected = false;

			currentOption = value;

			if ( currentOption is not null )
			{
				currentOption.Selected = true;
				CurrentPage = CurrentOption.GetOrCreatePage();
			}
			else
				CurrentPage = null;

			Update();
		}
	}
	private VirtualOption currentOption = null!;

	/// <summary>
	/// The amount of pages that are in this view.
	/// </summary>
	public int AmountOfPages
	{
		get
		{
			var amount = MathX.CeilToInt( (float)options.Count / OptionsPerPage );
			return Math.Clamp( amount, 1, int.MaxValue );
		}
	}

	/// <summary>
	/// The current page number that is being viewed.
	/// </summary>
	public int PageNumber
	{
		get => pageNumber;
		set
		{
			if ( pageNumber < 1 || pageNumber > AmountOfPages )
				throw new ArgumentException( $"You cannot be on page {value}", nameof( value ) );

			if ( pageNumber == value )
				return;

			pageNumber = value;
			Rebuild();
		}
	}
	private int pageNumber = 1;

	/// <summary>
	/// The amount of options to display per page.
	/// </summary>
	/// <remarks>This is ignored when <see cref="FreezeOptionsPerPage"/> is true.</remarks>
	public int OptionsPerPage
	{
		get => optionsPerPage;
		set
		{
			if ( value < 1 )
				throw new ArgumentException( $"A page cannot have {value} items", nameof( value ) );

			if ( optionsPerPage == value )
				return;

			optionsPerPage = value;
			Rebuild();
		}
	}
	private int optionsPerPage = 1;

	/// <summary>
	/// Whether or not <see cref="OptionsPerPage"/> should not be changed according to the view menus size.
	/// </summary>
	public bool FreezeOptionsPerPage { get; set; } = false;

	/// <summary>
	/// A sequence of all options that are currently being displayed.
	/// </summary>
	public IEnumerable<VirtualOption> VisibleOptions
	{
		get
		{
			var page = PageNumber - 1;
			var startPoint = page * OptionsPerPage;
			var endPoint = page * OptionsPerPage + OptionsPerPage;

			for ( var i = startPoint; i < endPoint; i++ )
			{
				if ( i >= options.Count )
					yield break;

				yield return options[i];
			}
		}
	}

	/// <summary>
	/// The size that page buttons should be.
	/// </summary>
	public Vector2 PageButtonSize { get; set; } = new Vector2( 40, 24 );

	/// <summary>
	/// A method to override the default painting of options.
	/// </summary>
	public Action<VirtualOption>? OptionPaint { get; set; }

	/// <summary>
	/// The containing widget of the menu.
	/// </summary>
	public Widget MenuContainer { get; private init; }
	/// <summary>
	/// The containing widget of the page.
	/// </summary>
	public Widget PageContainer { get; private init; }

	/// <summary>
	/// Top of the menu on the left.
	/// </summary>
	public Layout MenuTop { get; private set; }

	/// <summary>
	/// Bottom of the menu.
	/// </summary>
	public Layout MenuBottom { get; private set; }

	/// <summary>
	/// The layout of the menus contents.
	/// </summary>
	public Layout MenuContents { get; private set; }

	/// <summary>
	/// The layout of the page panel.
	/// </summary>
	public Layout PageContents { get; private set; }

	/// <summary>
	/// The layout for the menu footer page buttons.
	/// </summary>
	public Layout PageButtons { get; private init; }
	/// <summary>
	/// The left page button.
	/// </summary>
	public Button? LeftPageButton { get; private set; }
	/// <summary>
	/// The layout for any specific page buttons.
	/// </summary>
	public Layout? SpecificPageButtonLayout { get; private set; }
	/// <summary>
	/// The right page button.
	/// </summary>
	public Button? RightPageButton { get; private set; }

	private float selectY = -100;

	/// <summary>
	/// Initializes a new instance of <see cref="PaginatedNavigationView"/>.
	/// </summary>
	/// <param name="parent">The parent of this widget.</param>
	public PaginatedNavigationView( Widget? parent = null ) : base( parent )
	{
		SetLayout( LayoutMode.LeftToRight );

		MenuContainer = new Widget( this );
		MenuContainer.SetLayout( LayoutMode.TopToBottom );

		PageContainer = new Widget( this );
		PageContainer.SetLayout( LayoutMode.TopToBottom );

		PageContents = PageContainer.Layout.Add( LayoutMode.TopToBottom, 1 );
		PageContents.Margin = 0;

		MenuContainer.MinimumWidth = 200;

		Layout.Add( MenuContainer );
		Layout.Add( PageContainer, 1 );

		MenuContainer.Layout.Margin = 8;
		MenuTop = MenuContainer.Layout.Add( LayoutMode.TopToBottom );
		MenuContents = MenuContainer.Layout.Add( LayoutMode.TopToBottom );
		MenuContents.Spacing = 0;
		MenuContainer.Layout.AddStretchCell();
		MenuBottom = MenuContainer.Layout.Add( LayoutMode.BottomToTop );

		PageButtons = MenuBottom.AddRow();
		PageButtons.HorizontalSpacing = 4;

		Rebuild();
	}
	
	/// <summary>
	/// Adds a new page.
	/// </summary>
	/// <param name="name">The name of the page's option.</param>
	/// <param name="icon">The icon of the page's option/</param>
	/// <param name="page">The page for this option.</param>
	/// <returns>The option that was created for the page.</returns>
	public VirtualOption AddPage( string name, string icon, Widget? page = null )
	{
		if ( page is not null )
		{
			page.Parent = this;
			page.Visible = false;
		}

		return AddPage( new VirtualOption( name, icon ) { Page = page } );
	}

	/// <summary>
	/// Adds a new page.
	/// </summary>
	/// <param name="displayInfo">The display information that represents this page.</param>
	/// <param name="page">The page for this option.</param>
	/// <returns>The option that was created for the page.</returns>
	public VirtualOption AddPage( DisplayInfo displayInfo, Widget? page = null )
	{
		if ( page is not null )
		{
			page.Parent = this;
			page.Visible = false;
		}

		return AddPage( new VirtualOption( displayInfo.Name, displayInfo.Icon ) { Page = page } );
	}

	/// <summary>
	/// Adds a new page.
	/// </summary>
	/// <param name="tab">The option to add.</param>
	/// <returns>The same option.</returns>
	public VirtualOption AddPage( VirtualOption tab )
	{
		tab.Owner = this;
		options.Add( tab );
		Rebuild();

		return tab;
	}

	/// <summary>
	/// Clears all pages from this view.
	/// </summary>
	public void ClearPages()
	{
		PageContents.Clear( true );

		options.Clear();
		Rebuild();
	}

	/// <inheritdoc/>
	protected override void OnPaint()
	{
		if ( !FreezeOptionsPerPage && options.Any() )
			OptionsPerPage = (LocalRect.Height / (options.First().Rect.Height + 2)).FloorToInt() - 1;

		Paint.ClearPen();
		Paint.SetBrush( Theme.WidgetBackground );
		Paint.DrawRect( LocalRect.Shrink( 0 ), 4 );

		var sideMenurect = new Rect( 0, 0, MenuContainer.Width, Height );

		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground.WithAlpha( 0.9f ) );
		Paint.DrawRect( sideMenurect, 0 );

		Paint.SetPen( Theme.ControlBackground );
		Paint.DrawLine( sideMenurect.TopRight, sideMenurect.BottomRight );

		foreach ( var visualWidget in VisibleOptions )
		{
			if ( OptionPaint is not null )
				OptionPaint( visualWidget );
			else
				DefaultOptionPaint( visualWidget );
		}
	}

	/// <summary>
	/// Rebuilds the elements of this view.
	/// </summary>
	protected virtual void Rebuild()
	{
		if ( !VisibleOptions.Contains( CurrentOption ) )
			CurrentOption = VisibleOptions.FirstOrDefault()!;

		RebuildVisibleOptions();
		RebuildPageButtons();
	}

	/// <summary>
	/// Rebuilds all of the visible options for drawing.
	/// </summary>
	private void RebuildVisibleOptions()
	{
		var i = -1;
		foreach ( var option in VisibleOptions )
		{
			i++;

			option.Rect = new Rect( new Vector2( ContentRect.Left, ContentRect.Top + 32 * i ), new Vector2( MenuContainer.Width, 30 ) );
			option.Column = 1;
			option.Row = i + 1;
			option.Selected = CurrentOption == option;
		}

		Update();
	}

	/// <summary>
	/// Rebuilds the page buttons.
	/// </summary>
	private void RebuildPageButtons()
	{
		PageButtons.Clear( true );

		if ( AmountOfPages == 1 )
			return;

		LeftPageButton = new Button.Primary( "<" )
		{
			MinimumSize = PageButtonSize,
			MaximumSize = PageButtonSize,
			Clicked = () => PageNumber--
		};
		PageButtons.Add( LeftPageButton );

		SpecificPageButtonLayout = PageButtons.AddRow();
		SpecificPageButtonLayout.HorizontalSpacing = 2;

		// Handle special cases so that we can try to always have 3 page buttons visible.
		int startPoint = PageNumber switch
		{
			1 => 0,
			_ when PageNumber == AmountOfPages => -2,
			_ => -1
		};
		int endPoint = PageNumber switch
		{
			1 => 2,
			_ when PageNumber == AmountOfPages => 0,
			_ => 1
		};

		for ( var i = startPoint; i <= endPoint; i++ )
		{
			var pageNumber = PageNumber + i;
			if ( pageNumber < 1 || pageNumber > AmountOfPages )
				continue;

			var pageButton = new Button.Primary( pageNumber.ToString() )
			{
				Enabled = pageNumber != PageNumber && pageNumber >= 1 && pageNumber <= AmountOfPages,
				MinimumSize = PageButtonSize,
				MaximumSize = PageButtonSize,
				Clicked = () => PageNumber = pageNumber
			};
			SpecificPageButtonLayout.Add( pageButton );
		}

		RightPageButton = new Button.Primary( ">" )
		{
			MaximumSize = PageButtonSize,
			MinimumSize = PageButtonSize,
			Clicked = () => PageNumber++
		};
		PageButtons.Add( RightPageButton );

		if ( PageNumber == 1 )
			LeftPageButton.Enabled = false;
		if ( PageNumber == AmountOfPages )
			RightPageButton.Enabled = false;

		Update();
	}

	private void DefaultOptionPaint( VirtualOption virtualOption )
	{
		if ( virtualOption is null )
			return;

		var fg = Theme.White.WithAlpha( 0.5f );

		if ( virtualOption.Selected )
		{
			fg = Theme.White;

			if ( selectY != -100 )
			{
				selectY = MathX.Lerp( selectY, virtualOption.Rect.Position.y, 80.0f * RealTime.Delta );

				// redraw again next frame if we're not there yet
				if ( !selectY.AlmostEqual( virtualOption.Rect.Position.y ) ) Update();
			}
			else
				selectY = virtualOption.Rect.Position.y;

			var sideMenurect = new Rect( 0, 0, MenuContainer.Width, Height );
			var activeRect = new Rect( sideMenurect.Left + 12, selectY, sideMenurect.Width - 12 * 2, virtualOption.Rect.Height );

			Paint.ClearPen();
			Paint.SetBrush( Theme.Primary );
			Paint.DrawRect( activeRect, 4 );
		}

		Paint.ClearPen();
		Paint.SetBrush( Theme.WidgetBackground.WithAlpha( 0.0f ) );

		if ( virtualOption.Hovered )
			fg = Theme.White.WithAlpha( 0.8f );

		Paint.TextAntialiasing = true;
		Paint.Antialiasing = true;

		Paint.DrawRect( virtualOption.Rect.Shrink( 0 ) );

		var inner = virtualOption.Rect.Shrink( 8, 0, 0, 0 );
		var iconRect = inner;
		iconRect.Width = iconRect.Height;

		Paint.SetPen( fg );
		Paint.DrawIcon( iconRect, virtualOption.Icon, 14, TextFlag.Center );

		inner.Left += iconRect.Width + 4;

		Paint.SetPen( fg.WithAlphaMultiplied( 0.8f ) );
		Paint.SetFont( "Poppins", 8, 440 );

		Paint.DrawText( inner, virtualOption.Title, TextFlag.LeftCenter );
	}

	[EditorEvent.Frame]
	private void CheckVisibleOptions()
	{
		var cursorPos = Application.CursorPosition;
		var screenRect = ScreenRect;
		if ( !screenRect.IsInside( cursorPos ) )
			return;

		var shouldUpdate = false;
		foreach ( var option in VisibleOptions )
		{
			var virtualScreenPos = screenRect.Position + option.Rect.Position;
			var virtualScreenRect = new Rect( virtualScreenPos, option.Rect.Size );

			var wasHovered = option.Hovered;
			if ( !virtualScreenRect.IsInside( cursorPos ) )
			{
				option.Hovered = false;
				if ( wasHovered != option.Hovered )
					shouldUpdate = true;

				continue;
			}

			option.Hovered = true;
			var wasSelected = option.Selected;
			if ( MouseUtil.IsPressed( MouseButtons.Left ) )
			{
				CurrentOption.Selected = false;
				CurrentOption = option;
				CurrentOption.Selected = true;
			}

			if ( wasHovered != option.Hovered || wasSelected != option.Selected )
				shouldUpdate = true;
		}

		if ( shouldUpdate )
			Update();
	}
}
