using Editor;
using Sandbox;
using System;

namespace TemplateDownloader.Util;

/// <summary>
/// Contains utility methods for checking mouse state.
/// </summary>
internal static class MouseUtil
{
	private static MouseButtons pressedButtons;
	private static MouseButtons downButtons;
	private static MouseButtons upButtons;

	/// <summary>
	/// Returns whether or not a mouse button has been pressed this frame.
	/// </summary>
	/// <param name="button">The mouse button to check.</param>
	/// <returns>Whether or not the mouse button was pressed this frame.</returns>
	internal static bool IsPressed( MouseButtons button )
	{
		return pressedButtons.HasFlag( button );
	}

	/// <summary>
	/// Returns whether or not a mouse button is being pressed down.
	/// </summary>
	/// <param name="button">The mouse button to check.</param>
	/// <returns>Whether or not the mouse button is being pressed down.</returns>
	internal static bool IsDown( MouseButtons button )
	{
		return pressedButtons.HasFlag( button ) || downButtons.HasFlag( button );
	}

	/// <summary>
	/// Returns whether or not a mouse button is not being pressed down.
	/// </summary>
	/// <param name="button">The mouse button to check.</param>
	/// <returns>Whether or not the mouse button is not being pressed down.</returns>
	internal static bool IsReleased( MouseButtons button )
	{
		return upButtons.HasFlag( button );
	}

	[EditorEvent.Frame]
	private static void CheckButtons()
	{
		var currentButtons = Application.MouseButtons;
		foreach ( var button in Enum.GetValues<MouseButtons>() )
		{
			// Button is being pressed down.
			if ( currentButtons.HasFlag( button ) )
			{
				// Button was previously pressed down, mark it as down.
				if ( pressedButtons.HasFlag( button ) )
				{
					pressedButtons &= ~button;
					downButtons |= button;
				}
				// Button was not previously pressed down, mark it as pressed.
				else if ( upButtons.HasFlag( button ) )
				{
					pressedButtons |= button;
					upButtons &= ~button;
				}
			}
			// Button is not being pressed down.
			else
			{
				pressedButtons &= ~button;
				downButtons &= ~button;
				upButtons |= button;
			}
		}
	}
}
