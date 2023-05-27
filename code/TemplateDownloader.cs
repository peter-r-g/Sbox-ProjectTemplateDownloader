using Editor;
using System.Collections.Generic;
using System.Threading.Tasks;
using TemplateDownloader.Extensions;
using TemplateDownloader.Util;
using TemplateDownloader.Util.Json;

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
		if ( firstResult.IsError )
		{
			Log.Error( "Failed to search GitHub API for topic:sbox-template" );
			return;
		}

		var secondResult = await secondTask;
		if ( secondResult.IsError )
		{
			Log.Error( "Failed to search GitHub API for topic:sbox+template" );
			return;
		}

		// FIXME: Remove when FP add nice tags to their repo. This ID points to sboxgame/templates.
		var thirdResult = await GitHub.GetRepositoryAsync( 570947471 );
		if ( thirdResult.IsError )
		{
			Log.Error( "Failed to search GitHub API for sboxgame/templates repo" );
			return;
		}

		Progress.Update( "Populating list...", 90, 100 );
		var firstProcessTask = ProcessSearch( firstResult );
		var secondProcessTask = ProcessSearch( secondResult );
		var thirdProcessTask = ProcessRepository( thirdResult );

		await Task.WhenAll( firstProcessTask, secondProcessTask );

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
	/// <returns>A task that represents the asynchronous operation.</returns>
	private async Task ProcessSearch( Result<SearchResult, int> result )
	{
		if ( result.IsError )
		{
			Log.Error( "Failed to query GitHubs search API" );
			return;
		}

		foreach ( var gitHubRepository in result.Value.Repositories )
			await ProcessRepository( gitHubRepository );
	}

	/// <summary>
	/// Processes a GitHub repository for any templates contained inside.
	/// </summary>
	/// <param name="gitHubRepository">The repository to search.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	private async Task ProcessRepository( Repository gitHubRepository )
	{
		// Get the default branch.
		var branchResult = await GitHub.GetBranchAsync( gitHubRepository.FullName, gitHubRepository.DefaultBranch );
		if ( branchResult.IsError )
		{
			Log.Error( "Failed to get branch information from the API" );
			return;
		}

		// Get the root tree of the default branch.
		var rootTreeResult = await GitHub.GetTreeAsync( gitHubRepository.FullName, branchResult.Value.LatestCommit.CommitInformation.Tree.Sha1 );
		if ( rootTreeResult.IsError )
		{
			Log.Error( "Failed to get root tree information from the API" );
			return;
		}

		// Check if this repo is a single root template.
		var isRootTemplate = false;
		foreach ( var item in rootTreeResult.Value.Items )
		{
			if ( item.Path != ".addon" )
				continue;

			isRootTemplate = true;
			break;
		}

		if ( isRootTemplate )
		{
			AddTemplate( gitHubRepository, null );
			return;
		}

		// Check if the repo is a bunch of nested templates.
		var nestedTrees = new List<TreeItem>();

		foreach ( var item in rootTreeResult.Value.Items )
		{
			if ( item.Type != "tree" )
				continue;

			nestedTrees.Add( item );
		}

		// No nested trees, this is an invalid repo.
		if ( nestedTrees.Count == 0 )
			return;

		var siblingTemplates = new List<Template>();

		foreach ( var nestedTree in nestedTrees )
		{
			// Get nested tree.
			var nestedTreeResult = await GitHub.GetTreeAsync( gitHubRepository.FullName, nestedTree.Sha1 );
			if ( nestedTreeResult.IsError )
			{
				Log.Error( "Failed to get nested tree information from the API" );
				continue;
			}

			// Check nested tree for a template.
			var isNestedTemplate = false;
			foreach ( var nestedItem in nestedTreeResult.Value.Items )
			{
				if ( nestedItem.Path != ".addon" )
					continue;

				isNestedTemplate = true;
				break;
			}

			// Add the nested template and store it as a sibling.
			if ( isNestedTemplate )
				siblingTemplates.Add( AddTemplate( gitHubRepository, nestedTree.Path ) );
		}

		// Notify siblings of each other. 
		foreach ( var sibling in siblingTemplates )
		{
			foreach ( var otherSibling in siblingTemplates )
			{
				if ( sibling != otherSibling )
					sibling.Siblings.Add( otherSibling );
			}
		}
	}

	/// <summary>
	/// Adds a template to the <see cref="Templates"/> view.
	/// </summary>
	/// <param name="gitHubRepository">The repository that this template is a part of.</param>
	/// <param name="subDirectory">The sub-directory (if applicable) that the template is in.</param>
	/// <returns>The created template.</returns>
	private Template AddTemplate( Repository gitHubRepository, string? subDirectory )
	{
		var template = new Template( gitHubRepository, subDirectory );

		var icon = template.IsInstalled() switch
		{
			true when template.IsCorrupted() => MaterialIcon.BrokenImage,
			true when !template.IsUpToDate() => MaterialIcon.Update,
			true => MaterialIcon.DownloadDone,
			false => MaterialIcon.Download
		};

		var page = new TemplatePage( template );
		string optionText;
		if ( subDirectory is not null )
			optionText = subDirectory + " (" + gitHubRepository.FullName + ")";
		else
			optionText = gitHubRepository.FullName;

		var option = new NavigationView.Option( optionText, icon )
		{
			Page = page
		};

		Pages.Add( option, page );
		Templates.AddPage( option );

		return template;
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

			if ( option.Icon == icon )
				continue;

			if ( option != Templates.CurrentOption )
				((TemplatePage)option.Page).RefreshWindow();

			option.Icon = icon;
		}
	}
}
