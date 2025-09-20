namespace NetHtml2Pdf.Core.Models.Styles.Border
{
	public class BorderStyle
	{
		public BorderSide Top { get; } = new();
		public BorderSide Right { get; } = new();
		public BorderSide Bottom { get; } = new();
		public BorderSide Left { get; } = new();
	}
}

