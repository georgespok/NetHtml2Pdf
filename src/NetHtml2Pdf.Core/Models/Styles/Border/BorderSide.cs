namespace NetHtml2Pdf.Core.Models.Styles.Border
{
	public enum CssBorderLine { None, Solid, Dashed, Dotted }

	public class BorderSide
	{
		public float? Width { get; set; }
		public string? ColorHex { get; set; }
		public CssBorderLine Line { get; set; } = CssBorderLine.Solid;
	}
}

