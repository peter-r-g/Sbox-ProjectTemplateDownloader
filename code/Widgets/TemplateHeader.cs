using Editor;

namespace TemplateDownloader;

/// <summary>
/// A header for a <see cref="TemplatePage"/>.
/// </summary>
internal sealed class TemplateHeader : Widget
{
	/// <summary>
	/// The text to display in the header.
	/// </summary>
	internal string Title { get; set; }

	private const float HeaderHeight = 64 + 8;

	internal TemplateHeader( string title, Widget? parent = null, bool isDarkWindow = false ) : base( parent, isDarkWindow )
	{
		Title = title;
		Height = HeaderHeight;

		SetLayout( LayoutMode.TopToBottom );
	}

	/// <inheritdoc/>
	protected override void OnPaint()
	{
		Paint.SetBrushRadial( new Vector2( Width * 0.25f, 0 ), Width * 0.75f, Theme.Primary.WithAlpha( 0.2f ), Theme.Primary.WithAlpha( 0.01f ) );
		Paint.ClearPen();
		Paint.DrawRect( new Rect( new Vector2( 0, 0 ), new Vector2( Width, HeaderHeight ) ) );

		Paint.SetBrushRadial( 0, Width, Theme.White.WithAlpha( 0.2f ), Theme.Primary.WithAlpha( 0.0f ) );
		Paint.ClearPen();
		Paint.DrawRect( new Rect( new Vector2( 0, HeaderHeight - 26 ), new Vector2( Width, 26 ) ) );

		Paint.RenderMode = RenderMode.Screen;
		var pos = new Vector2( 24, 8 );
		Paint.SetPen( Theme.White );
		Paint.SetFont( "Poppins", 13, 450 );
		var r = Paint.DrawText( pos, Title );
		pos.y = r.Bottom;
	}
}
