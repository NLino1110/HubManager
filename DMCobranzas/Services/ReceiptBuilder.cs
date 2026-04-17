using System.Text;

namespace DMCobranzas.Services
{
    public class ReceiptBuilder
    {
        private readonly List<byte> _buffer = new();
        private readonly StringBuilder _preview = new();
        private readonly StringBuilder _htmlPreview = new();

        private readonly Encoding _encoding = Encoding.GetEncoding(437);

        private const int LineWidth = 32;
        private const int PreviewWidth = 45;

        private enum AlignMode
        {
            Left,
            Center,
            Right
        }

        private AlignMode _align = AlignMode.Left;

        private bool _bold = false;
        private string _fontSize = "14px";

        public ReceiptBuilder()
        {
            Reset();
        }

        public ReceiptBuilder Reset()
        {
            _buffer.Clear();
            _preview.Clear();
            _htmlPreview.Clear();

            _htmlPreview.AppendLine(
"<div style='font-family:Consolas,monospace; width:32ch; margin:auto; background:white; padding:8px; line-height:1.25;'>"
);

            _buffer.AddRange(new byte[] { 0x1B, 0x40 }); // ESC @

            return this;
        }

        public ReceiptBuilder Left()
        {
            _align = AlignMode.Left;
            _buffer.AddRange(new byte[] { 0x1B, 0x61, 0x00 });
            return this;
        }

        public ReceiptBuilder Center()
        {
            _align = AlignMode.Center;
            _buffer.AddRange(new byte[] { 0x1B, 0x61, 0x01 });
            return this;
        }

        public ReceiptBuilder Right()
        {
            _align = AlignMode.Right;
            _buffer.AddRange(new byte[] { 0x1B, 0x61, 0x02 });
            return this;
        }

        public ReceiptBuilder Bold(bool enabled = true)
        {
            _bold = enabled;
            _buffer.AddRange(new byte[] { 0x1B, 0x45, (byte)(enabled ? 1 : 0) });
            return this;
        }

        public ReceiptBuilder Large()
        {
            _fontSize = "18px";
            _buffer.AddRange(new byte[] { 0x1D, 0x21, 0x11 });
            return this;
        }

        public ReceiptBuilder Small()
        {
            _fontSize = "9px";
            _buffer.AddRange(new byte[] { 0x1B, 0x4D, 0x01 });
            return this;
        }

        public ReceiptBuilder Normal()
        {
            _fontSize = "14px";
            _buffer.AddRange(new byte[] { 0x1D, 0x21, 0x00 });
            return this;
        }

        public ReceiptBuilder ResetStyle()
        {
            _bold = false;
            _fontSize = "14px";

            _buffer.AddRange(new byte[] { 0x1B, 0x45, 0x00 });
            _buffer.AddRange(new byte[] { 0x1B, 0x4D, 0x00 });
            _buffer.AddRange(new byte[] { 0x1D, 0x21, 0x00 });

            return this;
        }

        public ReceiptBuilder Text(string text)
        {
            _buffer.AddRange(_encoding.GetBytes(text));
            return this;
        }

        public ReceiptBuilder Line(string text = "")
        {
            string formatted = FormatLine(text);
            string previewText = FormatPreviewImage(text);

            _buffer.AddRange(_encoding.GetBytes(formatted + "\n"));
            _preview.AppendLine(previewText);

            AddHtmlLine(text);

            return this;
        }

        public ReceiptBuilder Separator()
        {
            string printLine = new string('-', LineWidth);
            string previewLine = new string('-', PreviewWidth);

            _buffer.AddRange(_encoding.GetBytes(printLine + "\n"));
            _preview.AppendLine(previewLine);

            AddHtmlLine(printLine);

            return this;
        }

        public ReceiptBuilder SeparatorTop()
        {
            string printLine = new string('ˉ', LineWidth);
            string previewLine = new string('ˉ', PreviewWidth);

            _buffer.AddRange(_encoding.GetBytes(printLine + "\n"));
            _preview.AppendLine(previewLine);

            AddHtmlLine(printLine);

            return this;
        }

        public ReceiptBuilder SeparatorBottom()
        {
            string printLine = new string('ˍ', LineWidth);
            string previewLine = new string('ˍ', PreviewWidth);

            _buffer.AddRange(_encoding.GetBytes(printLine + "\n"));
            _preview.AppendLine(previewLine);

            AddHtmlLine(printLine);

            return this;
        }

        private string BuildColumns(string left, string right, int width)
        {
            int space = width - left.Length - right.Length;
            if (space < 1) space = 1;

            return left + new string(' ', space) + right;
        }

        public ReceiptBuilder Columns(string left, string right)
        {
            // línea para impresión y HTML (32)
            string linePrint = BuildColumns(left, right, LineWidth);

            // línea para preview imagen (45)
            string linePreview = BuildColumns(left, right, PreviewWidth);

            _buffer.AddRange(_encoding.GetBytes(linePrint + "\n"));
            _preview.AppendLine(linePreview);

            AddHtmlLine(linePrint);

            return this;
        }

        public ReceiptBuilder Feed(int lines = 3)
        {
            _buffer.AddRange(new byte[] { 0x1B, 0x64, (byte)lines });

            for (int i = 0; i < lines; i++)
            {
                _preview.AppendLine();
                _htmlPreview.AppendLine("<br>");
            }

            return this;
        }

        public ReceiptBuilder Cut()
        {
            _buffer.AddRange(new byte[] { 0x1D, 0x56, 0x00 });
            return this;
        }

        private IEnumerable<string> WrapText(string text)
        {
            var lines = new List<string>();

            while (text.Length > LineWidth)
            {
                lines.Add(text.Substring(0, LineWidth));
                text = text.Substring(LineWidth);
            }

            lines.Add(text);

            return lines;
        }

        private void AddHtmlLine(string text)
        {
            string weight = _bold ? "bold" : "normal";

            // aplicar alineación igual que el preview
            string aligned = FormatPreviewHtml(text);

            foreach (var line in WrapText(aligned))
            {
                _htmlPreview.AppendLine(
                    $"<div style='font-family:Consolas,monospace; font-weight:{weight}; font-size:{_fontSize}; white-space:pre;'>{System.Net.WebUtility.HtmlEncode(line)}</div>"
                );
            }
        }

        private string FormatPreviewHtml(string text)
        {
            if (text.Length >= LineWidth)
                return text;

            return _align switch
            {
                AlignMode.Center =>
                    new string(' ', (LineWidth - text.Length) / 2) + text,

                AlignMode.Right =>
                    new string(' ', LineWidth - text.Length) + text,

                _ => text
            };
        }

        private string FormatPreviewImage(string text)
        {
            if (text.Length >= PreviewWidth)
                return text;

            return _align switch
            {
                AlignMode.Center =>
                    new string(' ', (PreviewWidth - text.Length) / 2) + text,

                AlignMode.Right =>
                    new string(' ', PreviewWidth - text.Length) + text,

                _ => text
            };
        }

        private string FormatLine(string text)
        {
            return text;
        }

        public byte[] Build()
        {
            return _buffer.ToArray();
        }

        public string BuildPreview()
        {
            return _preview.ToString();
        }

        public string BuildPreviewHtml()
        {
            return _htmlPreview.ToString() + "</div>";
        }
    }
}