
using SkiaSharp;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Zj.Com.Customize.Sdk
{
    public class Other
    {
        public byte[] buf;
        public int index;
        private static readonly int WIDTH_80 = 576;
        private static readonly int WIDTH_58 = 384;
        private static int[] p0 = new int[] { 0, 128 };
        private static int[] p1 = new int[] { 0, 64 };
        private static int[] p2 = new int[] { 0, 32 };
        private static int[] p3 = new int[] { 0, 16 };
        private static int[] p4 = new int[] { 0, 8 };
        private static int[] p5 = new int[] { 0, 4 };
        private static int[] p6 = new int[] { 0, 2 };

        public Other(int length)
        {
            buf = new byte[length];
            index = 0;
        }

        public static StringBuilder RemoveChar(string str, char c)
        {
            StringBuilder sb = new StringBuilder();
            int length = str.Length;
            for (int i = 0; i < length; i++)
            {
                char tmp = str[i];
                if (tmp != c)
                    sb.Append(tmp);
            }
            return sb;
        }

        public static bool IsHexChar(char c)
        {
            if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                return true;
            return false;
        }

        private static readonly byte[] chartobyte = new byte[] {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
            10, 11, 12, 13, 14, 15
        };

        public static byte HexCharsToByte(char ch, char cl)
        {
            byte b = (byte)((chartobyte[ch - 48] << 4) & 0xF0 | (chartobyte[cl - 48] & 0xF));
            return b;
        }

        public static byte[] HexStringToBytes(string str)
        {
            int count = str.Length;
            byte[] data = null;
            if (count % 2 == 0)
            {
                data = new byte[count / 2];
                for (int i = 0; i < count; i += 2)
                {
                    char ch = str[i];
                    char cl = str[i + 1];
                    if (IsHexChar(ch) && IsHexChar(cl))
                    {
                        if (ch >= 'a')
                            ch = (char)(ch - 32);
                        if (cl >= 'a')
                            cl = (char)(cl - 32);
                        data[i / 2] = HexCharsToByte(ch, cl);
                    }
                    else
                    {
                        data = null;
                        break;
                    }
                }
            }
            return data;
        }

        public static byte[] StringToGBK(string data)
        {
            byte[] buffer = null;
            try
            {
                buffer = Encoding.GetEncoding("GBK").GetBytes(data);
            }
            catch (Exception e)
            {
                //e.PrintStackTrace();
            }
            return buffer;
        }

        public static byte[] ByteArraysToBytes(byte[][] data)
        {
            int length = 0;
            for (int i = 0; i < data.Length; i++)
                length += data[i].Length;

            byte[] send = new byte[length];
            int k = 0;
            for (int j = 0; j < data.Length; j++)
            {
                for (int m = 0; m < data[j].Length; m++)
                    send[k++] = data[j][m];
            }
            return send;
        }


        public static SKBitmap ResizeBitmap(SKBitmap bitmap, int width, int height)
        {
            SKImageInfo info = new SKImageInfo(width, height);
            SKBitmap resizedBitmap = new SKBitmap(info);

            using (SKCanvas canvas = new SKCanvas(resizedBitmap))
            using (SKPaint paint = new SKPaint())
            {
                //canvas.Clear(SKColors.Transparent);
                canvas.DrawBitmap(bitmap, SKRect.Create(info.Width, info.Height), paint);
            }

            return resizedBitmap;
        }

        //public static SKBitmap ConvertToGrayscale(SKBitmap bitmap)
        //{
        //    SKBitmap grayscaleBitmap = new SKBitmap(bitmap.Width, bitmap.Height);

        //    using (SKCanvas canvas = new SKCanvas(grayscaleBitmap))
        //    {
        //        SKPaint paint = new SKPaint();
        //        float[] colorMatrix =
        //        {
        //            0.299f,
        //            0.587f,
        //            0.114f,
        //        };

        //        SKColorFilter filter = SKColorFilter.CreateColorMatrix(colorMatrix);
        //        paint.ColorFilter = filter;

        //        canvas.DrawBitmap(bitmap, 0, 0, paint);
        //    }

        //    return grayscaleBitmap;
        //}

        public static SKBitmap ConvertToGrayscale(SKBitmap originalBitmap)
        {
            // Crea la matriz de colores para desaturar la imagen
            float[] colorMatrixValues = new float[]
            {
        0.2126f, 0.2126f, 0.2126f, 0, 0,
        0.7152f, 0.7152f, 0.7152f, 0, 0,
        0.0722f, 0.0722f, 0.0722f, 0, 0,
        0, 0, 0, 1, 0
            };

            SKColorFilter colorFilter = SKColorFilter.CreateColorMatrix(colorMatrixValues);

            // Crea un nuevo bitmap para almacenar la imagen en escala de grises
            SKBitmap grayscaleBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);

            // Dibuja la imagen original en el nuevo bitmap aplicando la matriz de colores
            using (SKCanvas canvas = new SKCanvas(grayscaleBitmap))
            using (SKPaint paint = new SKPaint())
            {
                paint.ColorFilter = colorFilter;
                canvas.DrawBitmap(originalBitmap, 0, 0, paint);
            }

            return grayscaleBitmap;
        }

        public static byte[] EachLinePixToCmd(byte[] src, int nWidth, int nMode)
        {
            int nHeight = src.Length / nWidth;
            int nBytesPerLine = nWidth / 8;
            byte[] data = new byte[nHeight * (8 + nBytesPerLine)];
            int offset = 0;
            int k = 0;

            for (int i = 0; i < nHeight; i++)
            {
                offset = i * (8 + nBytesPerLine);
                data[offset + 0] = 29;
                data[offset + 1] = 118;
                data[offset + 2] = 48;
                data[offset + 3] = (byte)(nMode & 0x1);
                data[offset + 4] = (byte)(nBytesPerLine % 256);
                data[offset + 5] = (byte)(nBytesPerLine / 256);
                data[offset + 6] = 1;
                data[offset + 7] = 0;

                for (int j = 0; j < nBytesPerLine; j++)
                {
                    data[offset + 8 + j] =
                        (byte)(p0[src[k]] + p1[src[k + 1]] + p2[src[k + 2]] + p3[src[k + 3]] + p4[src[k + 4]] + p5[src[k + 5]] + p6[src[k + 6]] + src[k + 7]);
                    k += 8;
                }
            }

            return data;
        }

        public static byte[] ConvertSKBitmapToByteArray(SKBitmap skBitmap)
        {
            int byteCount = skBitmap.Width * skBitmap.Height * 4; // 4 bytes por píxel (RGBA)
            byte[] byteArray = new byte[byteCount];

            IntPtr ptr = skBitmap.GetPixels();
            Marshal.Copy(ptr, byteArray, 0, byteCount);

            return byteArray;
        }

        public static void GetPixels(SKBitmap bitmap, int[] pixels, int offset, int stride, int x, int y, int width, int height)
        {
            using (var pixmap = bitmap.PeekPixels())
            {
                var buffer = pixmap.GetPixels();
                var bytesPerPixel = pixmap.BytesPerPixel;
                var bytesPerRow = pixmap.RowBytes;

                for (var row = 0; row < height; row++)
                {
                    var srcRow = buffer + ((y + row) * bytesPerRow) + (x * bytesPerPixel);
                    var dstRow = offset + (row * stride);

                    Marshal.Copy(new IntPtr(srcRow), pixels, dstRow, width);
                }
            }
        }

        public static byte[] ThresholdToBWPic(SKBitmap mBitmap)
        {            
            int[] pixels = new int[mBitmap.Width * mBitmap.Height];
            byte[] data = new byte[mBitmap.Width * mBitmap.Height];            
            //Original Java
            //mBitmap.getPixels(pixels, 0, mBitmap.getWidth(), 0, 0, mBitmap.getWidth(), mBitmap.getHeight());
            GetPixels(mBitmap, pixels, 0, mBitmap.Width, 0, 0, mBitmap.Width, mBitmap.Height);
            FormatKThreshold(pixels, mBitmap.Width, mBitmap.Height, data);
            return data;
        }

        private static void FormatKThreshold(int[] orgPixels, int xSize, int ySize, byte[] desPixels)
        {
            int grayTotal = 0;
            int grayAve = 128;
            int k = 0;
            int i;

            for (i = 0; i < ySize; i++)
            {
                for (int j = 0; j < xSize; j++)
                {
                    int gray = orgPixels[k] & 0xFF;
                    grayTotal += gray;
                    k++;
                }
            }

            grayAve = grayTotal / ySize / xSize;
            k = 0;

            for (i = 0; i < ySize; i++)
            {
                for (int j = 0; j < xSize; j++)
                {
                    int gray = orgPixels[k] & 0xFF;
                    if (gray > grayAve)
                    {
                        desPixels[k] = 0;
                    }
                    else
                    {
                        desPixels[k] = 1;
                    }
                    k++;
                }
            }
        }

        public static byte[] byteArraysToBytes(byte[][] data)
        {
            int length = 0;

            for (int i = 0; i < data.Length; ++i)
            {
                length += data[i].Length;
            }

            byte[] send = new byte[length];
            int k = 0;

            for (int i = 0; i < data.Length; ++i)
            {
                for (int j = 0; j < data[i].Length; ++j)
                {
                    send[k++] = data[i][j];
                }
            }

            return send;
        }


    }
}