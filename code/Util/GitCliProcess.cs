using Sandbox.Diagnostics;
using System.Diagnostics;

namespace TemplateDownloader.Util;

/// <summary>
/// A wrapper for spawning a process that runs Git commands.
/// </summary>
internal sealed class GitCliProcess : Process
{
	/// <summary>
	/// A custom logger for the process.
	/// </summary>
	private readonly Logger logger;

	/// <summary>
	/// Initializes a new instance of <see cref="GitCliProcess"/>.
	/// </summary>
	/// <param name="command">The Git command to run.</param>
	/// <param name="workingDirectory">The working directory to run the command in.</param>
	internal GitCliProcess( string command, string? workingDirectory = null )
	{
		logger = new Logger( "git " + command );
		StartInfo = new ProcessStartInfo( "git", command )
		{
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		if ( workingDirectory is not null )
			StartInfo.WorkingDirectory = workingDirectory;

		OutputDataReceived += OutputDataReceiver;
		ErrorDataReceived += OutputErrorReceiver;
	}

	private void OutputDataReceiver( object sender, DataReceivedEventArgs e )
	{
		logger.Info( e.Data );
	}

	private void OutputErrorReceiver( object sender, DataReceivedEventArgs e )
	{
		logger.Warning( e.Data );
	}
}
