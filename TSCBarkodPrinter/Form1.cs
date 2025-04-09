using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSCBarkodPrinter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        private void button1_Click(object sender, EventArgs e)
        {
            string printerName = "TSC TTP-244CE"; // Yazıcının tam adı

            string tsplCommand = @"SIZE 100 mm, 50 mm
GAP 3 mm, 0 mm
DIRECTION 1
REFERENCE 0,0
CLS
TEXT 100,30,""3"",0,1,1,""Koçaklar Market""
TEXT 100,70,""3"",0,1,1,""Eski Fiyat: 10 TL""
TEXT 100,110,""3"",0,2,2,""Sikişirim senle""
PRINT 1
                        ";

            bool result = RawPrinterHelper.SendStringToPrinter(printerName, tsplCommand);

            Console.WriteLine(result ? "Yazdırma başarılı." : "Yazdırma başarısız!");
        }
        private static bool SendBytesToPrinter(string printerName, IntPtr pBytes, Int32 dwCount)
        {
            IntPtr hPrinter;
            DOCINFOA di = new DOCINFOA
            {
                pDocName = "TSC Raw Command",
                pDataType = "RAW"
            };

            if (!OpenPrinter(printerName.Normalize(), out hPrinter, IntPtr.Zero))
                return false;

            bool success = StartDocPrinter(hPrinter, 1, di) &&
                           StartPagePrinter(hPrinter) &&
                           WritePrinter(hPrinter, pBytes, dwCount, out _) &&
                           EndPagePrinter(hPrinter) &&
                           EndDocPrinter(hPrinter);

            ClosePrinter(hPrinter);
            return success;
        }

        public static bool SendStringToPrinter(string printerName, string command)
        {
            IntPtr pBytes;
            Int32 dwCount = command.Length;
            pBytes = Marshal.StringToCoTaskMemAnsi(command);

            bool success = SendBytesToPrinter(printerName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
            return success;
        }

    }

}
