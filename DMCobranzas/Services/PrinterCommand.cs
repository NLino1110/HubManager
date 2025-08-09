using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zj.Com.Customize.Sdk;

namespace DMCobranzas.Services
{
    public class PrinterCommand
    {
        /**
     * Impresión de comandos
     * Inicialización de la impresora
     * @return
     */
        public static byte[] POS_Set_PrtInit()
        {
            byte[] data = Other.byteArraysToBytes(new byte[][] {
            PrintCommandBytes.ESC_Init
        });
            return data;
        }

        /**
         * Impresión de comandos
         * Imprimir y avanzar una línea
         * @return
         */
        public static byte[] POS_Set_LF()
        {
            byte[] data = Other.byteArraysToBytes(new byte[][] {
            PrintCommandBytes.LF
        });
            return data;
        }

        /**
         * Impresión de comandos
         * Imprimir y avanzar papel (0~255)
         * @param feed
         * @return
         */
        public static byte[] POS_Set_PrtAndFeedPaper(int feed)
        {
            if (feed > 255 || feed < 0)
                return null;

            PrintCommandBytes.ESC_J[2] = (byte)feed;

            byte[] data = Other.byteArraysToBytes(new byte[][] {
            PrintCommandBytes.ESC_J
        });
            return data;
        }

        /**
         * Impresión de comandos
         * Imprimir autoprueba
         * @return
         */
        public static byte[] POS_Set_PrtSelfTest()
        {
            byte[] data = Other.byteArraysToBytes(new byte[][]{
            PrintCommandBytes.US_vt_eot
        });
            return data;
        }

        /**
         * Impresión de comandos
         * Comando de zumbador
         * @param m  Número de zumbidos
         * @param t  Duración de cada zumbido
         * @return
         */
        public static byte[] POS_Set_Beep(int m, int t)
        {
            if ((m < 1 || m > 9) || (t < 1 || t > 9))
                return null;

            PrintCommandBytes.ESC_B_m_n[2] = (byte)m;
            PrintCommandBytes.ESC_B_m_n[3] = (byte)t;

            byte[] data = Other.byteArraysToBytes(new byte[][]{
            PrintCommandBytes.ESC_B_m_n
        });
            return data;
        }

        /**
         * Impresión de comandos
         * Comando de corte (avanzar papel y cortar)
         * @param cut  0~255
         * @return
         */
        public static byte[] POS_Set_Cut(int cut)
        {
            if (cut > 255 || cut < 0)
                return null;

            PrintCommandBytes.GS_V_m_n[3] = (byte)cut;
            byte[] data = Other.byteArraysToBytes(new byte[][]{
            PrintCommandBytes.GS_V_m_n
        });
            return data;
        }

        public static byte[] POS_Set_Cashbox(int nMode, int nTime1, int nTime2)
        {
            if ((nMode < 0 || nMode > 1) | nTime1 < 0 | nTime1 > 255 | nTime2 < 0 | nTime2 > 255)
                return null;
            PrintCommandBytes.ESC_p[2] = (byte)nMode;
            PrintCommandBytes.ESC_p[3] = (byte)nTime1;
            PrintCommandBytes.ESC_p[4] = (byte)nTime2;

            byte[] data = Other.byteArraysToBytes(new byte[][]{
        PrintCommandBytes.ESC_p
    });
            return data;
        }

        public static byte[] POS_Set_Absolute(int absolute)
        {
            if (absolute > 65535 | absolute < 0)
                return null;

            PrintCommandBytes.ESC_Relative[2] = (byte)(absolute % 0x100);
            PrintCommandBytes.ESC_Relative[3] = (byte)(absolute / 0x100);

            byte[] data = Other.byteArraysToBytes(new byte[][]{
        PrintCommandBytes.ESC_Relative
    });
            return data;
        }

        public static byte[] POS_Set_Relative(int relative)
        {
            if (relative < 0 | relative > 65535)
                return null;

            PrintCommandBytes.ESC_Absolute[2] = (byte)(relative % 0x100);
            PrintCommandBytes.ESC_Absolute[3] = (byte)(relative / 0x100);

            byte[] data = Other.byteArraysToBytes(new byte[][]{
        PrintCommandBytes.ESC_Absolute
    });
            return data;
        }

        public static byte[] POS_Set_LeftSP(int left)
        {
            if (left > 255 | left < 0)
                return null;

            PrintCommandBytes.GS_LeftSp[2] = (byte)(left % 100);
            PrintCommandBytes.GS_LeftSp[3] = (byte)(left / 100);

            byte[] data = Other.byteArraysToBytes(new byte[][]{
        PrintCommandBytes.GS_LeftSp
    });
            return data;
        }

        public static byte[] POS_S_Align(int align)
        {
            if ((align < 0 || align > 2) | (align < 48 || align > 50))
                return null;

            byte[] data = PrintCommandBytes.ESC_Align;
            data[2] = (byte)align;
            return data;
        }

        public static byte[] POS_Set_PrintWidth(int width)
        {
            if (width < 0 | width > 255)
                return null;

            PrintCommandBytes.GS_W[2] = (byte)(width % 100);
            PrintCommandBytes.GS_W[3] = (byte)(width / 100);

            byte[] data = Other.byteArraysToBytes(new byte[][]{
        PrintCommandBytes.GS_W
    });
            return data;
        }

        public static byte[] POS_Set_DefLineSpace()
        {
            byte[] data = PrintCommandBytes.ESC_Two;
            return data;
        }

        public static byte[] POS_Set_LineSpace(int space)
        {
            if (space < 0 | space > 255)
                return null;

            PrintCommandBytes.ESC_Three[2] = (byte)space;

            byte[] data = Other.byteArraysToBytes(new byte[][]{
        PrintCommandBytes.ESC_Three
    });
            return data;
        }

        /**
 * Seleccione la página de códigos de caracteres
 * @param page
 * @return
 */
        public static byte[] POS_Set_CodePage(int page)
        {
            if (page > 255)
                return null;

            PrintCommandBytes.ESC_t[2] = (byte)page;

            byte[] data = Other.byteArraysToBytes(new byte[][]{
        PrintCommandBytes.ESC_t
    });

            return data;
        }

        /**
         * Imprimir documento de texto
         * @param pszString     Cadena de texto a imprimir
         * @param encoding      Codificación de caracteres para imprimir
         * @param codepage      Establecer página de códigos (0--255)
         * @param nWidthTimes   Aumento del ancho (0--4)
         * @param nHeightTimes  Aumento del alto (0--4)
         * @param nFontType     Tipo de fuente (solo válido para el código ASCII) (0,1 48,49)
         */
        public static byte[] POS_Print_Text(String pszString, String encoding, int codepage,
                                            int nWidthTimes, int nHeightTimes, int nFontType)
        {

            if (codepage < 0 || codepage > 255 || pszString == null || "".Equals(pszString) || pszString.Length < 1)
            {
                return null;
            }

            byte[] pbString = null;
            try
            {
                //pbString = pszString.getBytes(encoding);
                pbString = Encoding.UTF8.GetBytes(pszString);
            }
            //catch (UnsupportedEncodingException e)
            catch (Exception e)
            {
                return null;
            }

            byte[] intToWidth = { 0x00, 0x10, 0x20, 0x30 };
            byte[] intToHeight = { 0x00, 0x01, 0x02, 0x03 };
            PrintCommandBytes.GS_ExclamationMark[2] = (byte)(intToWidth[nWidthTimes] + intToHeight[nHeightTimes]);

            PrintCommandBytes.ESC_t[2] = (byte)codepage;

            PrintCommandBytes.ESC_M[2] = (byte)nFontType;

            if (codepage == 0)
            {
                byte[] data = Other.byteArraysToBytes(new byte[][] {
            PrintCommandBytes.GS_ExclamationMark, PrintCommandBytes.ESC_t, PrintCommandBytes.FS_and, PrintCommandBytes.ESC_M, pbString });

                return data;
            }
            else
            {
                byte[] data = Other.byteArraysToBytes(new byte[][] {
            PrintCommandBytes.GS_ExclamationMark, PrintCommandBytes.ESC_t, PrintCommandBytes.FS_dot, PrintCommandBytes.ESC_M, pbString });

                return data;
            }
        }

        /**
         * Instrucción de negrita (bit más bajo válido)
         * @param bold
         * @return
         */
        public static byte[] POS_Set_Bold(int bold)
        {

            PrintCommandBytes.ESC_E[2] = (byte)bold;
            PrintCommandBytes.ESC_G[2] = (byte)bold;

            byte[] data = Other.byteArraysToBytes(new byte[][]{
        PrintCommandBytes.ESC_E, PrintCommandBytes.ESC_G
    });
            return data;
        }

        /**
         * Establecer el modo de impresión invertida (solo válido cuando el bit más bajo es 1)
         * @param brace
         * @return
         */
        public static byte[] POS_Set_LeftBrace(int brace)
        {

            PrintCommandBytes.ESC_LeftBrace[2] = (byte)brace;
            byte[] data = Other.byteArraysToBytes(new byte[][]{
        PrintCommandBytes.ESC_LeftBrace
    });
            return data;
        }


        /**
 * Establecer subrayado
 * @param line
 * @return
 */
        public static byte[] POS_Set_UnderLine(int line)
        {

            if ((line < 0 || line > 2))
                return null;

            PrintCommandBytes.ESC_Minus[2] = (byte)line;
            PrintCommandBytes.FS_Minus[2] = (byte)line;

            byte[] data = Other.byteArraysToBytes(new byte[][]{
        PrintCommandBytes.ESC_Minus, PrintCommandBytes.FS_Minus
    });
            return data;
        }

        /**
         * Seleccione el tamaño de fuente (alto y ancho)
         * @return
         */
        public static byte[] POS_Set_FontSize(int size1, int size2)
        {
            if (size1 < 0 | size1 > 7 | size2 < 0 | size2 > 7)
                return null;

            byte[] intToWidth = { 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70 };
            byte[] intToHeight = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
            PrintCommandBytes.GS_ExclamationMark[2] = (byte)(intToWidth[size1] + intToHeight[size2]);
            byte[] data = Other.byteArraysToBytes(new byte[][]{
        PrintCommandBytes.GS_ExclamationMark
    });
            return data;
        }

        /**
         * Establecer impresión invertida
         * @param inverse
         * @return
         */
        public static byte[] POS_Set_Inverse(int inverse)
        {

            PrintCommandBytes.GS_B[2] = (byte)inverse;

            byte[] data = Other.byteArraysToBytes(new byte[][]{
        PrintCommandBytes.GS_B
    });

            return data;
        }

        /**
 * Seleccione el tipo de fuente
 * @param font
 * @return
 */
        public static byte[] POS_Set_ChoseFont(int font)
        {
            if (font > 1 | font < 0)
                return null;

            PrintCommandBytes.ESC_M[2] = (byte)font;
            byte[] data = Other.byteArraysToBytes(new byte[][]{
        PrintCommandBytes.ESC_M
    });
            return data;
        }



        public static byte[] GetBarCommand(string str,
            int nVersion,
            int nErrorCorrectionLevel,
            int nMagnification)
        {
            if (nVersion < 0 || nVersion > 19 || nErrorCorrectionLevel < 0 || nErrorCorrectionLevel > 3 || nMagnification < 1 || nMagnification > 8)
            {
                return null;
            }

            byte[] bCodeData = null;
            try
            {
                bCodeData = Encoding.UTF8.GetBytes(str);
            }
            catch (NotSupportedException e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }

            byte[] command = new byte[bCodeData.Length + 7];

            command[0] = 27;
            command[1] = 90;
            command[2] = (byte)nVersion;
            command[3] = (byte)nErrorCorrectionLevel;
            command[4] = (byte)nMagnification;
            command[5] = (byte)(bCodeData.Length & 0xff);
            command[6] = (byte)((bCodeData.Length & 0xff00) >> 8);
            Array.Copy(bCodeData, 0, command, 7, bCodeData.Length);

            return command;
        }

        public static byte[] GetCodeBarCommand(string str, int nType, int nWidthX, int nHeight, int nHriFontType, int nHriFontPosition)
        {
            if (nType < 0x41 || nType > 0x49 || nWidthX < 2 || nWidthX > 6 || nHeight < 1 || nHeight > 255 || str.Length == 0)
            {
                return null;
            }

            byte[] bCodeData = null;
            try
            {
                //GBK not supporter
                //bCodeData = Encoding.GetEncoding("GBK").GetBytes(str);
                bCodeData = Encoding.UTF8.GetBytes(str);
            }
            catch (NotSupportedException e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }

            byte[] command = new byte[bCodeData.Length + 16];

            command[0] = 29;
            command[1] = 119;
            command[2] = (byte)nWidthX;
            command[3] = 29;
            command[4] = 104;
            command[5] = (byte)nHeight;
            command[6] = 29;
            command[7] = 102;
            command[8] = (byte)(nHriFontType & 0x01);
            command[9] = 29;
            command[10] = 72;
            command[11] = (byte)(nHriFontPosition & 0x03);
            command[12] = 29;
            command[13] = 107;
            command[14] = (byte)nType;
            command[15] = (byte)bCodeData.Length;
            Array.Copy(bCodeData, 0, command, 16, bCodeData.Length);

            return command;
        }


    }
}
