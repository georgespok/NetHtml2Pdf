namespace NetHtml2Pdf.Core.Models.Styles.Text
{
	public enum CssFontWeight { Normal, Bold }
	public enum CssFontStyle { Normal, Italic }

	public class TextStyle
	{
		public string? ColorHex { get; set; }
		public float? FontSize { get; set; }
		public float? LineHeight { get; set; }
		public CssFontWeight? Weight { get; set; }
		public CssFontStyle? Style { get; set; }
		public TextAlignment? TextAlign { get; set; }
	}
}

