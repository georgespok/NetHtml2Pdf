namespace NetHtml2Pdf.Core.Models
{
	/// <summary>
	/// Reusable style attributes shared by document nodes.
	/// Pure POCO with no external dependencies
	/// </summary>
	public class NodeStyle
	{
		public float Margins { get; set; } = 0;
		public float PaddingLeft { get; set; } = 0;
		public float PaddingRight { get; set; } = 0;
		public float PaddingTop { get; set; } = 0;
		public float PaddingBottom { get; set; } = 0;
		public TextAlignment Alignment { get; set; } = TextAlignment.Left;
		public string? BackgroundColor { get; set; }
		public float? BorderWidth { get; set; }
		public string? BorderColor { get; set; }
	}
}


