using NetHtml2Pdf.Core.Models.Styles.Box;
using NetHtml2Pdf.Core.Models.Styles.Border;
using NetHtml2Pdf.Core.Models.Styles.Background;
using NetHtml2Pdf.Core.Models.Styles.Text;

namespace NetHtml2Pdf.Core.Models
{
	public class NodeStyle
	{
		public BoxStyle Box { get; } = new();
		public BorderStyle Border { get; } = new();
		public BackgroundStyle Background { get; } = new();
		public TextStyle Text { get; } = new();

		public float Margins
		{
			get
			{
				var mt = Box.MarginTop ?? 0;
				var mr = Box.MarginRight ?? 0;
				var mb = Box.MarginBottom ?? 0;
				var ml = Box.MarginLeft ?? 0;
				return (Math.Abs(mt - mr) < 0.0001f && Math.Abs(mt - mb) < 0.0001f && Math.Abs(mt - ml) < 0.0001f) ? mt : 0f;
			}
			set { Box.MarginTop = Box.MarginRight = Box.MarginBottom = Box.MarginLeft = value; }
		}

		public float PaddingLeft { get => Box.PaddingLeft ?? 0; set => Box.PaddingLeft = value; }
		public float PaddingRight { get => Box.PaddingRight ?? 0; set => Box.PaddingRight = value; }
		public float PaddingTop { get => Box.PaddingTop ?? 0; set => Box.PaddingTop = value; }
		public float PaddingBottom { get => Box.PaddingBottom ?? 0; set => Box.PaddingBottom = value; }
		public TextAlignment Alignment { get => Text.TextAlign ?? TextAlignment.Left; set => Text.TextAlign = value; }
		public float? BorderWidth { get => Border.Left.Width; set { Border.Left.Width = Border.Top.Width = Border.Right.Width = Border.Bottom.Width = value; } }
		public string? BorderColor { get => Border.Left.ColorHex; set { Border.Left.ColorHex = Border.Top.ColorHex = Border.Right.ColorHex = Border.Bottom.ColorHex = value; } }
		public string? BackgroundColor { get => Background.ColorHex; set => Background.ColorHex = value; }
	}
}
