using Editor;
using System.Threading.Tasks;
using TemplateDownloader.Extensions;
using TemplateDownloader.Util;

namespace TemplateDownloader;

/// <summary>
/// Displays a template that can be installed, updated, and deleted.
/// </summary>
internal class TemplatePage : Widget
{
	/// <summary>
	/// The template that this page is displaying.
	/// </summary>
	internal Template Template { get; }

	/// <summary>
	/// The header of the page.
	/// </summary>
	private TemplateHeader Header { get; set; } = null!;
	/// <summary>
	/// The tool bar under the <see cref="Header"/>.
	/// </summary>
	private ToolBar ToolBar { get; set; } = null!;
	/// <summary>
	/// Container for the action buttons on the bottom of the page.
	/// </summary>
	private Layout ButtonDrawer { get; set; } = null!;

	internal TemplatePage( Template template, Widget? parent = null, bool isDarkWindow = false )
		: base( parent, isDarkWindow )
	{
		Template = template;

		SetupWindow();
	}

	/// <inheritdoc/>
	protected override void DoLayout()
	{
		base.DoLayout();

		Header.Position = 0;
		Header.Height = 512;

		ToolBar.Position = new Vector2( 16, 44 );
	}

	/// <summary>
	/// Creates the elements for this page.
	/// </summary>
	private void SetupWindow()
	{
		// Setup.
		{
			SetLayout( LayoutMode.TopToBottom );

			Layout.Spacing = 8;
			Layout.Margin = 24;

			DestroyChildren();
		}

		// Header/toolbar.
		{
			Header = Layout.Add( new TemplateHeader( Template.Repository.Name ) );
			Layout.AddSpacingCell( 8 );

			RefreshToolBar();

			Layout.AddSpacingCell( 8 );
		}

		// Description.
		{
			var description = new Label( Template.Repository.Description )
			{
				WordWrap = true
			};
			Layout.Add( description );
		}

		// Fill space between description and button drawer.
		Layout.AddStretchCell();

		RefreshButtonDrawer();
	}

	/// <summary>
	/// Refreshes the tool bar and action buttons on the page.
	/// </summary>
	private void RefreshWindow()
	{
		RefreshToolBar();
		RefreshButtonDrawer();
	}

	/// <summary>
	/// Refreshes the tool bar buttons.
	/// </summary>
	private void RefreshToolBar()
	{
		if ( ToolBar is null )
			ToolBar = Layout.Add( new ToolBar( this ) );
		else
			ToolBar.Clear();

		ToolBar.SetIconSize( 16 );

		ToolBar.AddOption( "Open on GitHub", MaterialIcon.OpenInBrowser, OpenGitHubPage );
		if ( !Template.IsInstalled() || Template.IsCorrupted() )
			return;

		ToolBar.AddOption( "Open in Explorer", MaterialIcon.Folder, OpenTemplateInExplorer );
		ToolBar.AddOption( "Open local repository in Explorer", MaterialIcon.FolderSpecial, OpenCachedRepoInExplorer );
	}

	/// <summary>
	/// Refreshes the action buttons on the bottom of the page.
	/// </summary>
	private void RefreshButtonDrawer()
	{
		if ( ButtonDrawer is null )
			ButtonDrawer = Layout.AddRow();
		else
			ButtonDrawer.Clear( true );

		ButtonDrawer.Spacing = 8;

		if ( Template.IsInstalled() && Template.IsCorrupted() )
		{
			var corruptedLabel = new Label()
			{
				WordWrap = true,
				Alignment = TextFlag.Center,
				Text = "This installation is corrupted, you will need to delete the files and re-install."
			};
			Layout.Add( corruptedLabel );
		}

		var mainButton = Template.IsInstalled() switch
		{
			true => new Button.Primary( "Delete", MaterialIcon.Delete )
			{
				ButtonType = "danger",
				Clicked = DeleteTemplate
			},
			false => new Button.Primary( "Download", MaterialIcon.Download )
			{
				Clicked = () => _ = DownloadTemplateAsync()
			}
		};
		ButtonDrawer.Add( mainButton );

		if ( Template.IsInstalled() && !Template.IsCorrupted() )
		{
			var secondaryButton = Template.IsUpToDate() switch
			{
				true => new Button( "Check For Updates", MaterialIcon.BrowserUpdated )
				{
					Clicked = () => _ = CheckForUpdatesAsync()
				},
				false => new Button( "Update", MaterialIcon.Update )
				{
					Clicked = () => _ = UpdateTemplateAsync()
				}
			};
			ButtonDrawer.Add( secondaryButton );
		}
	}

	/// <summary>
	/// Action to open the GitHub repository.
	/// </summary>
	private void OpenGitHubPage()
	{
		Utility.OpenFolder( Template.Repository.Url );
	}

	/// <summary>
	/// Action to open the template directory.
	/// </summary>
	private void OpenTemplateInExplorer()
	{
		Utility.OpenFolder( Template.TemplatePath );
	}

	/// <summary>
	/// Action to open the local GitHub repository.
	/// </summary>
	private void OpenCachedRepoInExplorer()
	{
		Utility.OpenFolder( Template.CachePath );
	}

	/// <summary>
	/// Action to download the template.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	private async Task DownloadTemplateAsync()
	{
		using var _ = TemplateDownloader.Instance?.DisableTemporarily();
		await Template.DownloadAsync();
		RefreshWindow();
	}

	/// <summary>
	/// Action to check for updates.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	private async Task CheckForUpdatesAsync()
	{
		using var _ = TemplateDownloader.Instance?.DisableTemporarily();
		using var progress = Progress.Start( "Checking For Updates" );

		var isUpToDate = await Template.IsUpToDateAsync();
		if ( isUpToDate.IsError )
		{
			Log.Error( "Failed to check for updates" );
			return;
		}

		if ( !isUpToDate )
			RefreshWindow();
	}

	/// <summary>
	/// Action to update the template.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	private async Task UpdateTemplateAsync()
	{
		using var _ = TemplateDownloader.Instance?.DisableTemporarily();
		await Template.UpdateAsync();
		RefreshWindow();
	}

	/// <summary>
	/// Action to delete the template.
	/// </summary>
	private void DeleteTemplate()
	{
		using var __ = TemplateDownloader.Instance?.DisableTemporarily();
		Template.Delete();
		RefreshWindow();
	}
}
