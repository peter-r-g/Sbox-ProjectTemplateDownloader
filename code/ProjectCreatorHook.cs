using Editor;
using System.Linq;
using TemplateDownloader.Util;

namespace TemplateDownloader;

/// <summary>
/// Contains a hook to add a quick bar option to the <see cref="ProjectCreator"/> to open the <see cref="TemplateDownloader"/>.
/// </summary>
file static class ProjectCreatorHook
{
	/// <summary>
	/// The last <see cref="ProjectCreator"/> that was edited.
	/// </summary>
	private static Window? HookedCreator { get; set; } = null;

	/// <summary>
	/// Searches for a <see cref="ProjectCreator"/> and adds the quick bar option to open the <see cref="TemplateDownloader"/>.
	/// </summary>
	[EditorEvent.Frame]
	private static void Frame()
	{
		var createProjectWindow = EditorMainWindow.All.FirstOrDefault( window => window.Title == "Create New Project" );
		if ( createProjectWindow is null )
			return;

		if ( ReferenceEquals( HookedCreator, createProjectWindow ) )
			return;

		createProjectWindow.MenuBar.AddMenu( "Templates" );
		createProjectWindow.MenuBar.AddOption( "Templates/Open Template Downloader", MaterialIcon.Storage, () => _ = new TemplateDownloader() );
		HookedCreator = createProjectWindow;
	}
}
