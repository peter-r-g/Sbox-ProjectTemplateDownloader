using Editor;
using System.Collections.Generic;
using System.Threading.Tasks;
using TemplateDownloader.Extensions;
using TemplateDownloader.Util;

namespace TemplateDownloader;

/// <summary>
/// A tool for downloading custom templates from GitHub.
/// </summary>
[Tool( "Template Downloader", MaterialIcon.Storage, "Downloads templates from GitHub to use in the create project window." )]
public sealed class TemplateDownloader : BaseWindow
{
	/// <summary>
	/// The only instance of this tool in existance.
	/// </summary>
	internal static TemplateDownloader? Instance { get; private set; }

	private NavigationView Templates { get; set; } = null!;
	private Dictionary<NavigationView.Option, TemplatePage> Pages { get; set; } = new();

	public TemplateDownloader()
	{
		Instance = this;

		MinimumSize = new Vector2( 700, 400 );
		WindowTitle = "Template Downloader";
		SetWindowIcon( MaterialIcon.Storage );

		SetLayout( LayoutMode.LeftToRight );
		SetupWindow();
		Show();

		_ = RefreshTemplatesAsync();
	}

	/// <summary>
	/// Sets up the tool window.
	/// </summary>
	private void SetupWindow()
	{
		// FIXME: NavigationView does not have any kind of scrolling behavior.
		Templates = Layout.Add( new NavigationView( this ) );

		var footer = Templates.MenuBottom.AddRow();
		footer.Spacing = 4;

		var refreshButton = new Button.Primary( "Refresh", MaterialIcon.Refresh )
		{
			Clicked = () => _ = RefreshTemplatesAsync()
		};
		footer.Add( refreshButton, 1 );
	}

	/// <summary>
	/// Refreshes the template list.
	/// </summary>
	/// <returns>A task that represents the asynchronous task.</returns>
	private async Task RefreshTemplatesAsync()
	{
		using var _ = this.DisableTemporarily();
		using var progress = Progress.Start( "Searching For Templates" );

		var currentOptionName = Templates.CurrentOption?.Title;

		Progress.Update( "Setting up...", 1, 100 );
		Templates.ClearPages();
		Pages.Clear();

		Progress.Update( "Searching for repositories with \"sbox-template\" or \"sbox\" and \"template\" topics...", 10, 100 );
		var firstTask = GitHub.SearchAsync( "topic:sbox-template" );
		var secondTask = GitHub.SearchAsync( "topic:sbox+template" );

		var firstResult = await firstTask;
		var secondResult = await secondTask;

		Progress.Update( "Populating list...", 90, 100 );
		ProcessSearch( firstResult );
		ProcessSearch( secondResult );

		if ( Pages.Count == 0 )
		{
			Templates.AddPage( "No Templates Found!", MaterialIcon.Error );
			return;
		}

		if ( string.IsNullOrEmpty( currentOptionName ) )
			return;

		foreach ( var (option, page) in Pages )
		{
			if ( option.Title != currentOptionName )
				continue;

			Templates.CurrentOption = option;
			Templates.CurrentPage = page;
		}
	}

	/// <summary>
	/// Processes a search result.
	/// </summary>
	/// <param name="result">The result to process.</param>
	private void ProcessSearch( Result<SearchResult, int> result )
	{
		if ( result.IsError )
		{
			Log.Error( "Failed to query GitHubs search API" );
			return;
		}

		foreach ( var gitHubRepository in result.Value.Repositories )
		{
			var template = new Template( gitHubRepository );

			var icon = template.IsInstalled() switch
			{
				true when template.IsCorrupted() => MaterialIcon.BrokenImage,
				true when !template.IsUpToDate() => MaterialIcon.Update,
				true => MaterialIcon.DownloadDone,
				false => MaterialIcon.Download
			};

			var page = new TemplatePage( template );
			var option = new NavigationView.Option( gitHubRepository.FullName, icon )
			{
				Page = page
			};

			Pages.Add( option, page );
			Templates.AddPage( option );
		}
	}

	/// <summary>
	/// Updates the icons on navigation options.
	/// </summary>
	[EditorEvent.Frame]
	private void UpdateIcons()
	{
		foreach ( var (option, templatePage) in Pages )
		{
			var template = templatePage.Template;

			var icon = template.IsInstalled() switch
			{
				true when template.IsCorrupted() => MaterialIcon.BrokenImage,
				true when !template.IsUpToDate() => MaterialIcon.Update,
				true => MaterialIcon.DownloadDone,
				false => MaterialIcon.Download
			};

			if ( option.Icon != icon )
				option.Icon = icon;
		}
	}
}
