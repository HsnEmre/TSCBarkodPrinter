using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TSCBarkodPrinter
{
    public class RawPrinterHelper
    {
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

        public static bool SendStringToPrinter(string printerName, string command)
        {
            IntPtr pBytes;
            Int32 dwCount = command.Length;
            pBytes = Marshal.StringToCoTaskMemAnsi(command);

            bool success = SendBytesToPrinter(printerName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
            return success;
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

            //ClosePrinter(hPrinter);
            return success;
        }
    }
}
