using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WPF_LA_OFRENDA_V1.Domain.Peripherals
{
    public class PrintService
    {

        public static PrintCommands PrintCommands;
        private static PrinterCommandBuilder _printerCommandBuilder;
        private static bool builderStarter = false;
        
        static PrintService()
        {
            PrintCommands = new PrintCommands(DefaultMarginSize.Default);
        }
        public static void StartBuilder()
        {
            _printerCommandBuilder = new PrinterCommandBuilder();
            builderStarter = true;
        }
        public static void RegisterInstruction(Func<int> instruction)
        {
            if (builderStarter)
            {
                _printerCommandBuilder.AddCommand(instruction);
            }
            else throw new Exception("Exectute <StartBuilder> before Registing an instruction ");
        }
        public static void ExecuteBuilder()
        {
            if (builderStarter)
            {
                PrintCommands.ExecuteGroupOfOrders(_printerCommandBuilder.Execute);
            }

        }
        public static DefaultPrinterStatus PrintBitmap(string path)
        {
            return PrintCommands.PrintBitmap(path);
        }

        public static DefaultPrinterStatus SetPrintMark(int value)
        {
            return PrintCommands.SetBlackmark(value);
        }

        public static DefaultPrinterStatus SetMargin()
        {
            return PrintCommands.SetMargin();
        }

        public static DefaultPrinterStatus PrinterQRFlow()
        {
            return PrintCommands.PrinterQRFlow();
        }

        public static DefaultPrinterStatus ConsultStatus()
        {
            //Console.WriteLine($"Estado de la impresora: {PrintCommands.CheckPrinterStatus()}");
            return PrintCommands.CheckPrinterStatus();
        }

        public static int ConsultDeepStatus()
        {
            //Console.WriteLine($"Estado de la impresora: {PrintCommands.CheckPrinterDeepStatus()}");
            return PrintCommands.CheckPrinterDeepStatus();
        }
        public static DefaultPrinterStatus PrintBlackmarkPosition(string data)
        {
            //Console.WriteLine($"Estado de la impresora: {PrintCommands.CheckPrinterDeepStatus()}");
            return PrintCommands.PrintBlackmarkPosition(data);
        }
        public static DefaultPrinterStatus PrintTable(string[] colNames, string[][] values, int totalCol)
        {
            //Console.WriteLine($"Estado de la impresora: {PrintCommands.CheckPrinterDeepStatus()}");
            return PrintCommands.PrintTable(colNames, values, totalCol);
        }
        public static DefaultPrinterStatus CutPrinting()
        {
            //Console.WriteLine($"Estado de la impresora: {PrintCommands.CheckPrinterDeepStatus()}");
            return PrintCommands.CutPrinting();
        }
        public static DefaultPrinterStatus PrintInCenter(string message)
        {
            //Console.WriteLine($"Estado de la impresora: {PrintCommands.CheckPrinterDeepStatus()}");
            return PrintCommands.PrintOnDirection(1, message);
        }
        public static DefaultPrinterStatus PrintWithSize(int size, string message)
        {
            //Console.WriteLine($"Estado de la impresora: {PrintCommands.CheckPrinterDeepStatus()}");
            return PrintCommands.PrintWithSize(size, message);
        }
        public static DefaultPrinterStatus PrintInRight(string message)
        {
            //Console.WriteLine($"Estado de la impresora: {PrintCommands.CheckPrinterDeepStatus()}");
            return PrintCommands.PrintOnDirection(2, message);
        }
        public static DefaultPrinterStatus PrintInLeft(string message)
        {
            //Console.WriteLine($"Estado de la impresora: {PrintCommands.CheckPrinterDeepStatus()}");
            return PrintCommands.PrintOnDirection(0, message);
        }

        public static string BuildPrint(Dictionary<string, string?> header, Dictionary<string, string?> body, Dictionary<string, string?> footer)
        {
            string result = "";
            foreach (KeyValuePair<string, string> pair in header)
            {
                result += pair.Key + ":" + pair.Value + "\n";
            }
            foreach (KeyValuePair<string, string> pair in body)
            {
                result += pair.Key + ":" + pair.Value + "\n";
            }
            foreach (KeyValuePair<string, string> pair in footer)
            {
                result += pair.Key + ":" + pair.Value + "\n";
            }
            return result;
        }

        public static string EvaluateStatus(int status)
        {
            if (!Enum.IsDefined(typeof(DefaultPrinterStatus), status)) return "El error no se puede traducir. La impresora respondio con un valor no definido en la documentacion";
            DefaultPrinterStatus internalStatus = (DefaultPrinterStatus)status;
            switch (internalStatus)
            {
                case DefaultPrinterStatus.PrinterIsOk:
                    return "La impresora se encuentra lista";
                case DefaultPrinterStatus.PrinterIsOffline:
                    return "La impresora no se encuentra en linea, o no esta encedida";
                case DefaultPrinterStatus.PrinterCalledUnMatchedLibrary:
                    return "La impresora llamo una libreria que no se encuentra";
                case DefaultPrinterStatus.PrinterHeadIsOpened:
                    return "El cabezal de la impresora se encuentra abierto";
                case DefaultPrinterStatus.CutterIsNotReset:
                    return "Cutter is not reset";
                case DefaultPrinterStatus.PrinterHeadTemperatureIsAbnormal:
                    return "La temperatura del cabezal es anormal. Muy caliente o muy fria";
                case DefaultPrinterStatus.PrinterDoesNotDetectBlackmark:
                    return "Printer does not detect blackmark";
                case DefaultPrinterStatus.PaperOut:
                    return "El papel se encuentra por fuera";
                case DefaultPrinterStatus.PaperLow:
                    return "Hay poco papel";
                case DefaultPrinterStatus.CantConnectToPrinter:
                    return "No se pudo conectar a la impresora";
                case DefaultPrinterStatus.UndefinedInternalError:
                    return "El error no se puede traducir. La impresora respondio con un valor no definido en la documentacion";
                case DefaultPrinterStatus.ErrorWhenExecutingDllCommand:
                    return "La ejecucion de un metodo interno de la Dll repsondio con error";
                default:
                    return "Error no registrado por el fabricante";
            }
        }

        public static string PrepareRow(string[] values, int numberChars)
        {
            int numItems = values.Length;
            int maxStringSize = numberChars / numItems;
            var stringRow = new StringBuilder();
            for (int i = 0; i < numItems; i++)
            {
                string valueToInsert = values[i].Substring(0, (maxStringSize - 2 > values[i].Length ? values[i].Length : maxStringSize - 2));
                while (valueToInsert.Length < maxStringSize - 2)
                {
                    valueToInsert = $" {valueToInsert}";
                }
                stringRow.Append(valueToInsert);
            }
            return stringRow.ToString();
        }

        public static string ConfigurePairValues(string leftString, string rightString, int totalWidth)
        {
            if (leftString.Length + rightString.Length > totalWidth)
            {
                throw new ArgumentException("The combined length of the two strings exceeds the total width.");
            }

            // Calculate the remaining space between the two strings
            int spaceBetween = totalWidth - (leftString.Length + rightString.Length);

            // Return the formatted string with left and right aligned strings
            return $"{leftString}{new string(' ', spaceBetween)}{rightString}";
        }

        public static string AlignStrings(string leftString, string centerString, string rightString, int totalWidth)
        {
            // Ensure totalWidth is at least as large as the combined lengths of all strings
            if (leftString.Length + centerString.Length + rightString.Length > totalWidth)
            {
                throw new ArgumentException("The combined length of the three strings exceeds the total width.");
            }

            // Calculate remaining space after placing the left, center, and right strings
            int remainingSpace = totalWidth - (leftString.Length + centerString.Length + rightString.Length);

            // Calculate spaces on either side of the center string
            int spaceBeforeCenter = remainingSpace / 2;
            int spaceAfterCenter = remainingSpace - spaceBeforeCenter;

            // Construct the final aligned string
            return $"{leftString}{new string(' ', spaceBeforeCenter)}{centerString}{new string(' ', spaceAfterCenter)}{rightString}";
        }
    }

    public class PrintCommands
    {
        #region Dll Import

        [DllImport("kernel32.dll", EntryPoint = "GetSystemDefaultLCID", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetSystemDefaultLCID();

        [DllImport("Msprintsdk.dll", EntryPoint = "SetInit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetInit();

        [DllImport("Msprintsdk.dll", EntryPoint = "SetUsbportauto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetUsbportauto();

        [DllImport("Msprintsdk.dll", EntryPoint = "SetClean", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetClean();

        [DllImport("Msprintsdk.dll", EntryPoint = "SetClose", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetClose();

        [DllImport("Msprintsdk.dll", EntryPoint = "SetCodepage", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetCodepage(int country, int CPnumber);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetAlignment", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetAlignment(int iAlignment);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetBold", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetBold(int iBold);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetCommmandmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetCommmandmode(int iMode);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetLinespace", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetLinespace(int iLinespace);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetPrintport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetPrintport(StringBuilder strPort, int iBaudrate);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintString", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintString(StringBuilder strData, int iImme);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintSelfcheck", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintSelfcheck();

        [DllImport("Msprintsdk.dll", EntryPoint = "GetStatus", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int GetStatus();
        [DllImport("Msprintsdk.dll", EntryPoint = "GetStatusspecial", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int GetStatusspecial();

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintFeedline", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintFeedline(int iLine);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintCutpaper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintCutpaper(int iMode);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetSizetext", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetSizetext(int iHeight, int iWidth);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetSizechinese", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetSizechinese(int iHeight, int iWidth, int iUnderline, int iChinesetype);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetItalic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetItalic(int iItalic);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintDiskbmpfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintDiskbmpfile(StringBuilder strData);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintDiskimgfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintDiskimgfile(StringBuilder strData);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintQrcode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintQrcode(StringBuilder strData, int iLmargin, int iMside, int iRound);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintRemainQR", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintRemainQR();

        [DllImport("Msprintsdk.dll", EntryPoint = "SetLeftmargin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetLeftmargin(int iLmargin);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintMarkposition", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintMarkposition();

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintMarkcutpaper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintMarkcutpaper(int IMode);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetMarkoffsetprint", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetMarkoffsetprint(int iOffset);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetMarkoffsetcut", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetMarkoffsetcut(int iOffset);

        [DllImport("Msprintsdk.dll", EntryPoint = "GetProductinformation", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int GetProductinformation(int Fstype, StringBuilder FIDdata);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintTransmit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintTransmit(byte[] strCmd, int iLength);

        [DllImport("Msprintsdk.dll", EntryPoint = "GetTransmit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int GetTransmit(string strCmd, int iLength, StringBuilder strRecv, int iRelen);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetHTseat", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetHTseat(StringBuilder bHTseat, int iLength);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintNextHT", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintNextHT();
        #endregion

        int m_iInit = -1;
        int m_iStatus = -1;
        int m_lcLanguage = 0;
        private string _portName;
        private int _baudrate;
        private DefaultMarginSize _leftMarginValue;
        public PrintCommands(DefaultMarginSize marginType, string portName = "", int baudrate = 0)
        {
            _portName = portName;
            _baudrate = baudrate;
            _leftMarginValue = marginType;
        }
        public bool IsPrinterReady()
        {
            var isConnected = ConfigurePrinter();
            if (!isConnected) return false;
            SetClose();
            return true;
        }
        public DefaultPrinterStatus CheckPrinterStatus()
        {
            var isConnected = ConfigurePrinter();
            if (!isConnected) return DefaultPrinterStatus.CantConnectToPrinter;
            var status = PrintCommands.GetStatus();
            SetClose();
            if (!Enum.IsDefined(typeof(DefaultPrinterStatus), status)) return DefaultPrinterStatus.UndefinedInternalError;
            return (DefaultPrinterStatus)status;
        }
        public DefaultPrinterStatus PrintBlackmarkPosition(string data)
        {
            var isConnected = ConfigurePrinter();
            if (!isConnected) return DefaultPrinterStatus.CantConnectToPrinter;
            StringBuilder sData = new StringBuilder(data, data.Length);
            var statusMargin = SetLeftmargin((int)_leftMarginValue);
            int status = PrintString(sData, 300);
            //PrintMarkposition();
            //PrintMarkcutpaper(1);
            SetClose();
            if (status == 1) return DefaultPrinterStatus.ErrorWhenExecutingDllCommand;
            if (status == 0) return DefaultPrinterStatus.PrinterIsOk;
            return (DefaultPrinterStatus)status;
        }
        public DefaultPrinterStatus PrintBitmap(string path)
        {
            var isConnected = ConfigurePrinter();
            if (!isConnected) return DefaultPrinterStatus.CantConnectToPrinter;
            StringBuilder sPath = new StringBuilder(path, path.Length);
            int status = PrintDiskbmpfile(sPath);
            //PrintMarkposition();
            //PrintMarkcutpaper(1);
            SetClose();
            if (status == 1) return DefaultPrinterStatus.ErrorWhenExecutingDllCommand;
            if (status == 0) return DefaultPrinterStatus.PrinterIsOk;
            return (DefaultPrinterStatus)status;
        }
        public DefaultPrinterStatus PrintTable(string[] colNames, string[][] values, int totalCol)
        {
            var isConnected = ConfigurePrinter();
            if (!isConnected) return DefaultPrinterStatus.CantConnectToPrinter;
            var statusAlignment = SetAlignment(2);

            int[] cSeat = new int[] { 7, 15, 23, 30 };

            StringBuilder sb = new StringBuilder();
            foreach (int seat in cSeat)
            {
                char asciiChar = Convert.ToChar(seat);
                sb.Append(asciiChar);
            }

            int status = 0;
            if (status == 0)
            {
                SetHTseat(sb, totalCol - 1);
                for (int i = 0; i < totalCol; i++)
                {
                    PrintString(new StringBuilder(colNames[i], colNames[i].Length), (i == totalCol - 1 ? 0 : 1));
                    if (i == totalCol - 1) continue;
                    PrintNextHT();
                }
                foreach (var subArray in values)
                {
                    for (int i = 0; i < totalCol; i++)
                    {
                        PrintString(new StringBuilder(subArray[i], subArray[i].Length), (i == totalCol - 1 ? 0 : 1));
                        if (i == totalCol - 1) continue;
                        PrintNextHT();
                    }
                }
            }
            //PrintMarkposition();
            //PrintMarkcutpaper(1);
            SetClose();
            if (status == 1) return DefaultPrinterStatus.ErrorWhenExecutingDllCommand;
            if (status == 0) return DefaultPrinterStatus.PrinterIsOk;
            return (DefaultPrinterStatus)status;
        }
        public DefaultPrinterStatus CutPrinting()
        {
            var isConnected = ConfigurePrinter();
            if (!isConnected) return DefaultPrinterStatus.CantConnectToPrinter;
            var status = PrintMarkcutpaper(0);
            SetClose();
            if (status == 1) return DefaultPrinterStatus.ErrorWhenExecutingDllCommand;
            if (status == 0) return DefaultPrinterStatus.PrinterIsOk;
            return (DefaultPrinterStatus)status;
        }
        /// <summary>
        /// Solo usar cuando se esta configurando el Blackmark de la impresora
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public DefaultPrinterStatus SetBlackmark(int value)
        {
            var isConnected = ConfigurePrinter();
            if (!isConnected) return DefaultPrinterStatus.CantConnectToPrinter;
            var status = SetMarkoffsetprint(250);//VALOR IMPORTANTE PARA CONFIGURAR LUGAR DE CORTE
            //var status2 = SetMarkoffsetcut(1600);
            SetClose();
            if (status == 1) return DefaultPrinterStatus.ErrorWhenExecutingDllCommand;
            if (status == 0) return DefaultPrinterStatus.PrinterIsOk;
            return (DefaultPrinterStatus)status;
        }
        public DefaultPrinterStatus SetMargin()
        {
            var isConnected = ConfigurePrinter();
            if (!isConnected) return DefaultPrinterStatus.CantConnectToPrinter;
            var status = SetLeftmargin((int)_leftMarginValue);
            string message = "\n\n";
            for (int i = 0; i < 100; i++)
            {
                message = message + $"_{i}";
            }
            StringBuilder sData = new StringBuilder(message, message.Length);
            int status2 = PrintString(sData, 300);
            SetClose();
            if (status == 1) return DefaultPrinterStatus.ErrorWhenExecutingDllCommand;
            if (status == 0) return DefaultPrinterStatus.PrinterIsOk;
            return (DefaultPrinterStatus)status;
        }
        public DefaultPrinterStatus PrinterQRFlow()
        {
            var isConnected = ConfigurePrinter();
            if (!isConnected) return DefaultPrinterStatus.CantConnectToPrinter;
            SetAlignment(1);

            string message = "\n\n";
            StringBuilder sData = new StringBuilder(message, message.Length);
            int status2 = PrintString(sData, 0);
            StringBuilder QRData = new StringBuilder("Martillo", "Martillo".Length);
            int resultQR = PrintQrcode(QRData, 27, 8, 0);// 1 en el ultimo valor para darle redondeo
            int resultRemainingQR = PrintRemainQR();
            PrintString(new StringBuilder("Martillo", "Martillo".Length), 0);

            SetClose();
            if (status2 == 1) return DefaultPrinterStatus.ErrorWhenExecutingDllCommand;
            if (status2 == 0) return DefaultPrinterStatus.PrinterIsOk;
            return (DefaultPrinterStatus)status2;
        }
        public DefaultPrinterStatus PrintOnDirection(int alignment, string message = "")
        {
            var isConnected = ConfigurePrinter();
            if (!isConnected) return DefaultPrinterStatus.CantConnectToPrinter;
            var statusMargin = SetLeftmargin((int)_leftMarginValue);
            var status = SetAlignment(alignment);
            StringBuilder sData = new StringBuilder(message, message.Length);
            int status2 = PrintString(sData, 0);
            SetClose();
            if (status == 1) return DefaultPrinterStatus.ErrorWhenExecutingDllCommand;
            if (status == 0) return DefaultPrinterStatus.PrinterIsOk;
            return (DefaultPrinterStatus)status;
        }
        public int ExecuteGroupOfOrders(Func<int> groupOfOrders)
        {
            var isConnected = ConfigurePrinter();
            if (!isConnected) return 1;

            var status = groupOfOrders.Invoke();

            SetClose();
            return status;
        }
        public static StringBuilder ConfigureStringToSend(string message)
        {
            return new StringBuilder(message, message.Length);
        }
        public DefaultPrinterStatus PrintWithSize(int size, string message = "")
        {
            var isConnected = ConfigurePrinter();
            if (!isConnected) return DefaultPrinterStatus.CantConnectToPrinter;
            var statusMargin = SetLeftmargin((int)_leftMarginValue);
            var status = SetSizetext(size, size);
            StringBuilder sData = new StringBuilder(message, message.Length);
            int status2 = PrintString(sData, 0);
            SetClose();
            if (status == 1) return DefaultPrinterStatus.ErrorWhenExecutingDllCommand;
            if (status == 0) return DefaultPrinterStatus.PrinterIsOk;
            return (DefaultPrinterStatus)status;
        }
        public int CheckPrinterDeepStatus()
        {
            var isConnected = ConfigurePrinter();
            if (!isConnected) return 0;
            var status = PrintCommands.GetStatusspecial();
            SetClose();
            return status;
        }
        /// <summary>
        /// Usar solo cuando la comunicacion de la impresora es por medio de comunicacion USB
        /// </summary>
        /// <returns></returns>
        private bool ConfigurePrinter()
        {
            try
            {
                int countIntent = 0;
                m_lcLanguage = GetSystemDefaultLCID();
                SetUsbportauto();
                while (countIntent < 3)
                {
                    m_iInit = SetInit();
                    if (m_iInit == 0)
                    {
                        return true;
                    }
                    else
                    {
                        countIntent++;
                    }
                }
                return false;
            }
            catch (Exception )
            {
                return false;
            }
        }
        private bool ConfigurePrinter(string portName, int baudrate)
        {
            try
            {
                int countIntent = 0;
                m_lcLanguage = GetSystemDefaultLCID();
                StringBuilder sPort = new StringBuilder(portName, portName.Length);
                int iBaudrate = baudrate;
                SetPrintport(sPort, iBaudrate);
                while (countIntent < 3)
                {
                    m_iInit = SetInit();
                    if (m_iInit == 0)
                    {
                        return true;
                    }
                    else
                    {
                        countIntent++;
                    }
                }
                return false;
            }
            catch (Exception )
            {
                return false;
            }
        }
    }

    public class PrinterCommandBuilder
    {
        private readonly List<Func<int>> _commands = new List<Func<int>>();

        public PrinterCommandBuilder AddCommand(Func<int> instruction)
        {
            _commands.Add(instruction);
            return this;
        }

        public int Execute()
        {
            int status = 1;
            foreach (var command in _commands)
            {
                status = command.Invoke();
                if (status == 1) break;
            }

            return status;
        }
    }

    public enum DefaultPrinterStatus
    {
        PrinterIsOk,
        PrinterIsOffline,
        PrinterCalledUnMatchedLibrary,
        PrinterHeadIsOpened,
        CutterIsNotReset,
        PrinterHeadTemperatureIsAbnormal,
        PrinterDoesNotDetectBlackmark,
        PaperOut,
        PaperLow,
        CantConnectToPrinter = 30,
        UndefinedInternalError,
        ErrorWhenExecutingDllCommand,
        PrintingSuccess,
        PrintingTimeOutError
    }

    public enum DefaultMarginSize
    {
        Apostar = 190,
        Default = 0
    }

}
