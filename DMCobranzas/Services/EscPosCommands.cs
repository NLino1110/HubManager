using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCobranzas.Services
{
    public static class EscPosCommands
    {
        private const byte ESC = 0x1B;
        private const byte GS = 0x1D;

        public static byte[] Join(params byte[][] arrays)
        {
            return arrays.SelectMany(a => a).ToArray();
        }

        public static byte[] Init()
            => new byte[] { ESC, (byte)'@' };

        public static byte[] LineFeed()
            => new byte[] { 0x0A };

        public static byte[] Feed(int n)
            => new byte[] { ESC, (byte)'J', (byte)n };

        public static byte[] AlignLeft()
            => new byte[] { ESC, (byte)'a', 0 };

        public static byte[] AlignCenter()
            => new byte[] { ESC, (byte)'a', 1 };

        public static byte[] AlignRight()
            => new byte[] { ESC, (byte)'a', 2 };

        public static byte[] Bold(bool enabled)
            => new byte[] { ESC, (byte)'E', (byte)(enabled ? 1 : 0) };

        public static byte[] FontSize(int width, int height)
        {
            byte n = (byte)((width << 4) | height);
            return new byte[] { GS, (byte)'!', n };
        }

        public static byte[] Cut()
            => new byte[] { GS, (byte)'V', 0 };

        public static byte[] Text(string text, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return encoding.GetBytes(text);
        }

        public static byte[] Barcode(string data, int type = 73) // CODE128 default
        {
            var bytes = Encoding.UTF8.GetBytes(data);

            return Join(
                new byte[] { GS, (byte)'w', 3 }, // width
                new byte[] { GS, (byte)'h', 100 }, // height
                new byte[] { GS, (byte)'k', (byte)type, (byte)bytes.Length },
                bytes
            );
        }

        public static byte[] BarcodeFull(int which, string str)
        {
            if (string.IsNullOrEmpty(str))
                return Array.Empty<byte>();

            byte type = 0;
            string label = "";

            switch (which)
            {
                case 4: // CODE39
                    type = 69;
                    label = "CODE39\n";
                    break;

                case 8: // CODE128
                    type = 73;
                    label = "CODE128\n";
                    break;

                case 5: // ITF
                    type = 70;
                    label = "ITF\n";
                    break;

                case 6: // CODABAR
                    type = 71;
                    label = "CODABAR\n";
                    break;

                case 7: // CODE93
                    type = 72;
                    label = "CODE93\n";
                    break;

                default:
                    return Array.Empty<byte>();
            }

            var code = RawBarcode(str, type, 3, 168, 1, 2);

            return Join(
                AlignLeft(),
                Text(label),
                code
            );
        }

        public static byte[] QR(string data, byte version = 0, byte error = 3, byte size = 6)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            int len = bytes.Length;

            byte nL = (byte)(len & 0xFF);
            byte nH = (byte)((len >> 8) & 0xFF);

            return Join(
                new byte[]
                {
            0x1B, 0x5A, // ESC Z
            version,    // antes: 0
            error,      // antes: 3
            size,       // antes: 6
            nL,
            nH
                },
                bytes
            );
        }

        public static byte[] RawBarcode(
            string data,
            byte type,
            byte width,
            byte height,
            byte hri,
            byte font)
        {
            var bytes = Encoding.ASCII.GetBytes(data);

            return Join(
                new byte[] { 0x1D, 0x77, width },     // GS w → ancho
                new byte[] { 0x1D, 0x68, height },    // GS h → alto
                new byte[] { 0x1D, 0x48, hri },       // GS H → texto visible
                new byte[] { 0x1D, 0x66, font },      // GS f → fuente

                new byte[] { 0x1D, 0x6B, type, (byte)bytes.Length }, // GS k
                bytes
            );
        }
    }
}
