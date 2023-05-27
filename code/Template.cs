using System.IO;
using System.Threading.Tasks;
using System;
using TemplateDownloader.Util;
using Editor;
using System.Collections.Generic;
using TemplateDownloader.Util.Json;

namespace TemplateDownloader;

/// <summary>
/// Represents an external template that can be used in the <see cref="ProjectCreator"/>.
/// </summary>
internal sealed class Template
{
	/// <summary>
	/// The GitHub repository that hosts the template.
	/// </summary>
	internal Repository Repository { get; private set; }

	/// <summary>
	/// The path to the template that the <see cref="ProjectCreator"/> can see.
	/// </summary>
	internal string TemplatePath
	{
		get
		{
			var path = Path.Combine( Environment.CurrentDirectory,
				"templates",
				Repository.Id.ToString() );

			if ( SubDirectory is not null )
				path += '_' + SubDirectory;

			return path;
		}
	}

	/// <summary>
	/// The path to the templates GitHub repository.
	/// </summary>
	internal string CachePath
	{
		get
		{
			var path = Path.Combine( Environment.CurrentDirectory,
				"templates",
				"githubcache",
				Repository.Id.ToString() );

			return path;
		}
	}

	/// <summary>
	/// A list containing all of this templates siblings.
	/// </summary>
	internal List<Template> Siblings { get; } = new();

	/// <summary>
	/// The relative path from the GitHub repository that this template is stored in.
	/// </summary>
	internal string? SubDirectory { get; }

	internal Template( Repository gitHubRepository, string? subDirectory )
	{
		Repository = gitHubRepository;
		SubDirectory = subDirectory;
	}

	/// <summary>
	/// Returns whether or not the installation is corrupted.
	/// </summary>
	/// <remarks>
	/// This will return true even if the template is not installed.
	/// </remarks>
	/// <returns>Whether or not the installation is corrupted.</returns>
	internal bool IsCorrupted()
	{
		if ( !Directory.Exists( CachePath ) )
			return true;

		if ( !Directory.Exists( Path.Combine( CachePath, ".git" ) ) )
			return true;

		if ( !Directory.Exists( TemplatePath ) )
			return true;

		var updateFilePath = Path.Combine( CachePath, "update.txt" );
		if ( !File.Exists( updateFilePath ) )
			return true;

		if ( !DateTime.TryParse( File.ReadAllText( updateFilePath ), out var dateTime ) )
			return true;

		if ( dateTime == DateTime.MinValue )
			return true;

		return false;
	}

	/// <summary>
	/// Returns whether or not the template is installed.
	/// </summary>
	/// <remarks>
	/// This will return true even if the template is corrupted or partially installed.
	/// </remarks>
	/// <returns>Whether or not the template is installed.</returns>
	internal bool IsInstalled()
	{
		return Directory.Exists( CachePath ) || Directory.Exists( TemplatePath );
	}

	/// <summary>
	/// Returns the last time the template repository was updated.
	/// </summary>
	/// <returns>The last time the template repository was updated.</returns>
	internal DateTime GetLastVersionTime()
	{
		var path = Path.Combine( TemplatePath, "update.txt" );
		if ( !File.Exists( path ) )
			return DateTime.MinValue;

		return DateTime.Parse( File.ReadAllText( path ) );
	}

	/// <summary>
	/// Returns whether or not the templates repository is up to date.
	/// </summary>
	/// <remarks>
	/// This will test against the last received repository data. If you need to test against new data then use <see cref="IsUpToDateAsync"/>.
	/// </remarks>
	/// <returns>Whether or not the templates repository is up to date.</returns>
	internal bool IsUpToDate()
	{
		if ( !IsInstalled() )
			return false;

		return Repository.LastUpdate > GetLastVersionTime();
	}

	/// <summary>
	/// Returns whether or not the templates repository is up to date.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation. The result will either be a <see cref="bool"/> of if it is up to date or an error code.</returns>
	internal async ValueTask<Result<bool, int>> IsUpToDateAsync()
	{
		if ( !IsInstalled() )
			return false;

		var result = await GitHub.GetRepositoryAsync( Repository.Id );
		if ( result.IsError )
			return result.Error;

		Repository = result;
		return result.Value.LastUpdate > GetLastVersionTime();
	}

	/// <summary>
	/// Downloads the template repository.
	/// </summary>
	/// <remarks>
	/// If this template has siblings, this will also install them.
	/// </remarks>
	/// <returns>A task that represents the asynchronous operation.</returns>
	internal async Task DownloadAsync()
	{
		using var progress = Progress.Start( $"Downloading {Repository.FullName}" );

		Progress.Update( "Cloing repository...", 0, 100 );
		await RunGitCommandAsync( $"clone {Repository.CloneUrl} .", CachePath );

		Progress.Update( "Writing update time...", 80, 100 );
		File.WriteAllText( Path.Combine( CachePath, "update.txt" ), Repository.LastUpdate.ToString() );

		Progress.Update( "Copying files to template directory...", 90, 100 );
		CopyTemplateToTemplatesDirectory();

		foreach ( var sibling in Siblings )
			sibling.CopyTemplateToTemplatesDirectory();
	}

	/// <summary>
	/// Updates the template repository
	/// </summary>
	/// <remarks>
	/// If this template has siblings, this will also update them.
	/// </remarks>
	/// <returns>A task that represents the asynchronous operation.</returns>
	/// <exception cref="InvalidOperationException">Thrown when this is called while the GitHub repository is not installed.</exception>
	internal async Task UpdateAsync()
	{
		if ( !Directory.Exists( CachePath ) )
			throw new InvalidOperationException( "The GitHub repository is not installed" );

		var upToDate = await IsUpToDateAsync();
		if ( upToDate.IsError )
			Log.Warning( "Failed to check for update, continuing anyway" );
		else if ( upToDate )
			return;

		using var progress = Progress.Start( $"Updating {Repository.FullName}" );

		Progress.Update( "Resetting repository...", 0, 100 );
		await RunGitCommandAsync( "reset --hard HEAD", CachePath );

		Progress.Update( "Pulling changes...", 30, 100 );
		await RunGitCommandAsync( "pull", CachePath );

		Progress.Update( $"Checking out {Repository.DefaultBranch}...", 60, 100 );
		await RunGitCommandAsync( $"checkout \"{Repository.DefaultBranch}\" --force", CachePath );

		Progress.Update( "Writing update time...", 80, 100 );
		File.WriteAllText( Path.Combine( CachePath, "update.txt" ), DateTime.UtcNow.ToString() );

		Progress.Update( "Copying files to template directory...", 90, 100 );
		CopyTemplateToTemplatesDirectory();

		foreach ( var sibling in Siblings )
			sibling.CopyTemplateToTemplatesDirectory();
	}

	/// <summary>
	/// Copies the necessary files from the GitHub repository to its destination template directory.
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown when this is called while the GitHub repository is not installed.</exception>
	private void CopyTemplateToTemplatesDirectory()
	{
		if ( !Directory.Exists( CachePath ) )
			throw new InvalidOperationException( "The GitHub repository is not installed" );

		if ( Directory.Exists( TemplatePath ) )
			Directory.Delete( TemplatePath, true );

		Directory.CreateDirectory( TemplatePath );

		var sourcePath = SubDirectory is not null
			? Path.Combine( CachePath, SubDirectory )
			: CachePath;
		foreach ( var file in Directory.EnumerateFiles( sourcePath, "*.*", SearchOption.AllDirectories ) )
		{
			if ( file.Contains( "/.git/" ) || file.Contains( "\\.git\\" ) )
				continue;

			var fileName = Path.GetFileName( file );
			if ( fileName == ".gitattributes" ||
				fileName == ".gitignore" ||
				fileName == "update.txt" ||
				fileName == "README" ||
				fileName == "README.md" )
				continue;

			var relative = Path.GetRelativePath( sourcePath, file );
			var directory = Path.GetDirectoryName( relative );

			if ( !string.IsNullOrEmpty( directory ) )
				Directory.CreateDirectory( Path.Combine( TemplatePath, directory ) );

			File.Copy( file, Path.Combine( TemplatePath, relative ) );
		}
	}

	/// <summary>
	/// Deletes any installed data for the template.
	/// </summary>
	/// <remarks>
	/// If this template has siblings, this will also delete them.
	/// </remarks>
	/// <exception cref="InvalidOperationException">Thrown when this is called while the GitHub repository is not installed.</exception>
	internal void Delete()
	{
		using var progress = Progress.Start( $"Deleting {Repository.FullName}" );

		try
		{
			Progress.Update( "Deleting template...", 0, 100 );
			RecursiveDeleteDirectory( TemplatePath );

			Progress.Update( "Deleting github cache...", 45, 100 );
			RecursiveDeleteDirectory( CachePath );

			foreach ( var sibling in Siblings )
				sibling.Delete();
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}
	}

	/// <summary>
	/// Recursively deletes all files in a directory.
	/// </summary>
	/// <remarks>
	/// This exists because Git repositories add some files that error the regular <see cref="Directory.Delete(string, bool)"/> calls.
	/// </remarks>
	/// <param name="directory">The directory to recursively delete.</param>
	private static void RecursiveDeleteDirectory( string directory )
	{
		if ( !Directory.Exists( directory ) )
			return;

		foreach ( string subdirectory in Directory.EnumerateDirectories( directory ) )
			RecursiveDeleteDirectory( subdirectory );

		foreach ( string fileName in Directory.EnumerateFiles( directory ) )
		{
			var fileInfo = new FileInfo( fileName )
			{
				Attributes = FileAttributes.Normal
			};

			fileInfo.Delete();
		}

		Directory.Delete( directory );
	}

	/// <summary>
	/// Runs a Git command.
	/// </summary>
	/// <param name="command">The command to run.</param>
	/// <param name="workingDirectory">The working directory for the command to execute in.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	private static async Task RunGitCommandAsync( string command, string? workingDirectory = null )
	{
		if ( workingDirectory is not null && !Directory.Exists( workingDirectory ) )
			Directory.CreateDirectory( workingDirectory );

		var process = new GitCliProcess( command, workingDirectory );

		try
		{
			process.Start();
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}

		await process.WaitForExitAsync();
	}
}
