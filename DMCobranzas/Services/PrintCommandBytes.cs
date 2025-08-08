using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Services
{
    public class PrintCommandBytes
    {
        private const byte ESC = 0x1B;
        private const byte FS = 0x1C;
        private const byte GS = 0x1D;
        private const byte US = 0x1F;
        private const byte DLE = 0x10;
        private const byte DC4 = 0x14;
        private const byte DC1 = 0x11;
        private const byte SP = 0x20;
        private const byte NL = 0x0A;
        private const byte FF = 0x0C;
        public static readonly byte PIECE = (byte)0xFF;
        public static readonly byte NUL = (byte)0x00;
        

        // Inicialización de la impresora
        public static byte[] ESC_Init = new byte[] { ESC, (byte)'@' };

        /**
         * Comandos de impresión
         */
        // Imprimir y avanzar línea
        public static byte[] LF = new byte[] { NL };

        // Imprimir y avanzar papel
        public static byte[] ESC_J = new byte[] { ESC, (byte)'J', 0x00 };
        public static byte[] ESC_d = new byte[] { ESC, (byte)'d', 0x00 };

        // Imprimir página de autocomprobación
        public static byte[] US_vt_eot = new byte[] { US, DC1, 0x04 };

        // Comando de zumbador
        public static byte[] ESC_B_m_n = new byte[] { ESC, (byte)'B', 0x00, 0x00 };

        // Comando de corte
        public static byte[] GS_V_n = new byte[] { GS, (byte)'V', 0x00 };
        public static byte[] GS_V_m_n = new byte[] { GS, (byte)'V', (byte)'B', 0x00 };
        public static byte[] GS_i = new byte[] { ESC, (byte)'i' };
        public static byte[] GS_m = new byte[] { ESC, (byte)'m' };

        /**
         * Comandos de configuración de caracteres
         */
        // Configurar espacio derecho de caracteres
        public static byte[] ESC_SP = new byte[] { ESC, SP, 0x00 };

        // Configurar formato de fuente de caracteres
        public static byte[] ESC_ExclamationMark = new byte[] { ESC, (byte)'!', 0x00 };

        // Configurar tamaño de fuente doble alto y ancho
        public static byte[] GS_ExclamationMark = new byte[] { GS, (byte)'!', 0x00 };

        // Configurar impresión invertida
        public static byte[] GS_B = new byte[] { GS, (byte)'B', 0x00 };

        // Cancelar/seleccionar rotación de impresión de 90 grados
        public static byte[] ESC_V = new byte[] { ESC, (byte)'V', 0x00 };

        // Seleccionar tipo de fuente (principalmente para códigos ASCII)
        public static byte[] ESC_M = new byte[] { ESC, (byte)'M', 0x00 };

        // Seleccionar/cancelar negrita
        public static byte[] ESC_G = new byte[] { ESC, (byte)'G', 0x00 };
        public static byte[] ESC_E = new byte[] { ESC, (byte)'E', 0x00 };

        // Seleccionar/cancelar modo de impresión invertida
        public static byte[] ESC_LeftBrace = new byte[] { ESC, (byte)'{', 0x00 };

        // Configurar altura de puntos de subrayado (caracteres)
        public static byte[] ESC_Minus = new byte[] { ESC, (byte)'-', 0x00 };

        // Modo de caracteres
        public static byte[] FS_dot = new byte[] { FS, (byte)'.' };

        // Modo de caracteres chinos
        public static byte[] FS_and = new byte[] { FS, (byte)'&' };

        // Configurar modo de impresión de caracteres chinos
        public static byte[] FS_ExclamationMark = new byte[] { FS, (byte)'!', 0x00 };

        // Configurar altura de puntos de subrayado (caracteres chinos)
        public static byte[] FS_Minus = new byte[] { FS, (byte)'-', 0x00 };

        // Configurar espacio izquierdo y derecho de caracteres chinos
        public static byte[] FS_S = new byte[] { FS, (byte)'S', 0x00, 0x00 };

        // Seleccionar página de códigos de caracteres
        public static byte[] ESC_t = new byte[] { ESC, (byte)'t', 0x00 };

        /**
         * Comandos de configuración de formato
         */
        // Configurar espacio entre líneas por defecto
        public static byte[] ESC_Two = new byte[] { ESC, 50 };

        // Configurar espacio entre líneas
        public static byte[] ESC_Three = new byte[] { ESC, 51, 0x00 };

        // Configurar modo de alineación
        public static byte[] ESC_Align = new byte[] { ESC, (byte)'a', 0x00 };

        // Configurar margen izquierdo
        public static byte[] GS_LeftSp = new byte[] { GS, (byte)'L', 0x00, 0x00 };

        // Configurar posición de impresión absoluta
        // Establece la posición actual en nL + nH x 256 caracteres desde el principio de la línea.
        // Si la posición está fuera del área de impresión especificada, este comando se ignora.
        public static byte[] ESC_Relative = new byte[] { ESC, (byte)'$', 0x00, 0x00 };

        // Configurar posición de impresión relativa
        public static byte[] ESC_Absolute = new byte[] { ESC, (byte)92, 0x00, 0x00 };

        // Configurar ancho del área de impresión
        public static byte[] GS_W = new byte[] { GS, (byte)'W', 0x00, 0x00 };

        /**
         * Comandos de estado
         */
        // Comando de transmisión de estado en tiempo real
        public static byte[] DLE_eot = new byte[] { DLE, 0x04, 0x00 };

        // Comando de apertura de cajón de dinero en tiempo real
        public static byte[] DLE_DC4 = new byte[] { DLE, DC4, 0x00, 0x00, 0x00 };

        // Comando de apertura de cajón de dinero estándar
        public static byte[] ESC_p = new byte[] { ESC, (byte)'p', 0x00, 0x00, 0x00 };

        // Comando de lectura del estado del cajón de dinero
        public static byte[] DLE_DC4_n_m_t = new byte[] { DLE, DC4, (byte)'0', 0x00, 0x00, 0x00 };

        /**
         * Comandos de código de barras
         */
        // Imprimir código de barras
        public static byte[] GS_k_m_n = new byte[] { GS, (byte)'k', 0x00, 0x00, 0x00 };

        /**
         * Comandos de QR Code
         */
        // Imprimir QR Code
        public static byte[] GS_k_s_v_r_nL_nH = new byte[] { GS, (byte)'k', (byte)'Q', 0x00, 0x00, 0x00, 0x00, 0x00 };

    }
}
