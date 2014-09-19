using System;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections.Generic;

namespace Print_Folder_Watcher_Common
{
	/// <summary>
	/// Summary description for Utils.
	/// </summary>
	public class clsUtils
	{
		private const string REMOTE_PRINTING_REG_KEY = "Software\\Meticulus\\Remote Printing";

		public clsUtils()
		{
        }

        #region Generic registry read access
        // To read REG_SZ into a string
        public string ReadRegistryValue(string key, string name, string defultVal)
        {
            string val = defultVal;
            RegistryKey rkBase = null;
            try
            {
                rkBase = Registry.LocalMachine.OpenSubKey(key);
                if (rkBase != null)
                {
                    object obj = rkBase.GetValue(name);
                    if (obj != null)
                    {
                        val = (string)obj;
                        if (val.ToLower() == "ml_default")
                        {
                            val = defultVal;
                        }
                    }
                }
            }
            finally
            {
                if (rkBase != null)
                {
                    rkBase.Close();
                }
            }
            return val;
        }

        // To read REG_MULTI_SZ into a list of strings
        public List<string> ReadRegistryValue(string key, string name, List<string> defultVal)
        {
            List<string> val = defultVal;
            RegistryKey rkBase = null;
            try
            {
                rkBase = Registry.LocalMachine.OpenSubKey(key);
                if (rkBase != null)
                {
                    object obj = rkBase.GetValue(name);
                    if (obj != null)
                    {
                        val = (List<string>)obj;
                    }
                }
            }
            finally
            {
                if (rkBase != null)
                {
                    rkBase.Close();
                }
            }
            return val;
        }
        // To read REG_SZ into a list of strings delimited by the passed delimiter
        // eg "Z@*;acr*;*.pdf;*.xfdf;"
        public List<string> ReadRegistryValue(string key, string name, List<string> defultVal, char delimiter)
        {
            List<string> val = defultVal;
            RegistryKey rkBase = null;
            try
            {
                rkBase = Registry.LocalMachine.OpenSubKey(key);
                if (rkBase != null)
                {
                    object obj = rkBase.GetValue(name);
                    if (obj != null)
                    {
                        string str = (string)obj;
                        if (str.ToLower() == "ml_default")
                        {
                            val = defultVal;
                        }
                        else
                        {
                            val = new List<string>(str.Split(delimiter));
                        }
                    }
                }
            }
            finally
            {
                if (rkBase != null)
                {
                    rkBase.Close();
                }
            }
            return val;
        }
        // To read REG_SZ into a bool value, expects true or false
        public bool ReadRegistryValue(string key, string name, bool defultVal)
        {
            bool val = defultVal;
            RegistryKey rkBase = null;
            try
            {
                rkBase = Registry.LocalMachine.OpenSubKey(key);
                if (rkBase != null)
                {
                    object obj = rkBase.GetValue(name);
                    if (obj != null)
                    {
                        val = Convert.ToBoolean(obj);
                    }
                }
            }
            finally
            {
                if (rkBase != null)
                {
                    rkBase.Close();
                }
            }
            return val;
        }
        // To read REG_DWORD into an int
        public int ReadRegistryValue(string key, string name, int defultVal)
        {
            int val = defultVal;
            RegistryKey rkBase = null;
            try
            {
                rkBase = Registry.LocalMachine.OpenSubKey(key);
                if (rkBase != null)
                {
                    object obj = rkBase.GetValue(name);
                    if (obj != null)
                    {
                        val = Convert.ToInt32(obj);
                    }
                }
            }
            finally
            {
                if (rkBase != null)
                {
                    rkBase.Close();
                }
            }
            return val;
        }
        #endregion Generic registry read access

        public string ReadCmdLine()
        {
            string strCmdLine = null;
            RegistryKey rkBase = null;
            try
            {

                rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY);
                if (rkBase != null)
                {
                    strCmdLine = (String)rkBase.GetValue("CmdLine");
                }
            }
            finally
            {
                if (rkBase != null)
                {
                    rkBase.Close();
                }
            }

            return strCmdLine;
        }

        public long ReadIgnorePrintJobStatus()
        {
			long ignorePrintJobStatus = 0;
			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY);
				if(rkBase != null)
				{
					object value = rkBase.GetValue("IgnorePrintJobStatus");
					if(value != null)
					{
						ignorePrintJobStatus = Convert.ToInt32(value);
					}
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
			return ignorePrintJobStatus;
        }
		
		public bool ReadRunAsService()
		{
			bool bRunAsService = false;
			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY);
				if(rkBase != null)
				{
					object RunAsService = rkBase.GetValue("RunAsService");
					if(RunAsService != null)
					{
						bRunAsService = Convert.ToBoolean(RunAsService);
					}
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
			return bRunAsService;
		}

		public void WriteRunAsService(bool bRunAsService) 
		{
			RegistryKey rkBase = null;
			try
			{
				string strRunAsService;

				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY, true);
				if(rkBase != null)
				{
					strRunAsService = bRunAsService.ToString();
					rkBase.SetValue("RunAsService", strRunAsService);
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
		}

		public void WriteInteractiveMode(bool bInteractiveMode) 
		{
			RegistryKey rkBase = null;
			try
			{
				string strInteractiveMode;

				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY, true);
				if(rkBase != null)
				{
					strInteractiveMode = bInteractiveMode.ToString();
					rkBase.SetValue("InteractiveMode", strInteractiveMode);
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
		}

		public bool ReadStopServiceOnPrinterError()
		{
			bool bStopServiceOnPrinterError = false;
			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY);
				if(rkBase != null)
				{
					object stopServiceOnPrinterError = rkBase.GetValue("StopServiceOnPrinterError");
					if(stopServiceOnPrinterError != null)
					{
						bStopServiceOnPrinterError = Convert.ToBoolean(stopServiceOnPrinterError);
					}
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
			return bStopServiceOnPrinterError;
		}

		public void ReadPrinterAdded(ref bool bPrinterAdded)
		{
			RegistryKey rkBase = null;
			try
			{
				string strPrinterAdded = "";
				bPrinterAdded = false;

				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY);
				if(rkBase != null){
					strPrinterAdded = (String)rkBase.GetValue("Printer Added");
					if(strPrinterAdded != null && (string.Compare(strPrinterAdded,"True")==0 || string.Compare(strPrinterAdded,"False")==0)){
						bPrinterAdded = Convert.ToBoolean(strPrinterAdded);
					}
				}
			}
			finally
			{
				if (rkBase != null) {
					rkBase.Close();
				}
			}
		}

		public void WritePrinterAdded(bool bPrinterAdded) {
			RegistryKey rkBase = null;
			try
			{
				string strPrinterAdded;

				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY, true);
				if(rkBase != null){
					strPrinterAdded = bPrinterAdded.ToString();
					rkBase.SetValue("Printer Added", strPrinterAdded);
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
		}

		public void WriteStopServiceOnPrinterError(bool bStopServiceOnPrinterError) 
		{
			RegistryKey rkBase = null;
			try
			{
				string strStopServiceOnPrinterError;

				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY, true);
				if(rkBase != null)
				{
					strStopServiceOnPrinterError = bStopServiceOnPrinterError.ToString();
					rkBase.SetValue("StopServiceOnPrinterError", strStopServiceOnPrinterError);
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
		}

		public int ReadTimeout() 
		{
			int iTimeoutInMinutes = 10;//Default value.
			RegistryKey rkBase = null;
			try
			{
				string strTimeoutInMinutes;
				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY);
				if(rkBase != null)
				{
					strTimeoutInMinutes = (String)rkBase.GetValue("TimeoutInMinutes");
					if(strTimeoutInMinutes != null)
					{
						iTimeoutInMinutes = Convert.ToInt32(strTimeoutInMinutes);
					}
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
			return iTimeoutInMinutes;
		}

		public void WriteTimeOut(int iTimeoutInMinutes) 
		{
			RegistryKey rkBase = null;
			try
			{
				string strTimeoutInMinutes;

				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY, true);
				if(rkBase != null)
				{
					strTimeoutInMinutes = iTimeoutInMinutes.ToString();
					rkBase.SetValue("TimeoutInMinutes", strTimeoutInMinutes);
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
		}

		public bool ReadLogAllEvents() 
		{
			bool bLogAllEvents = false;
			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY);
				if (rkBase != null)
				{
					string strLogAllEvents = (String)rkBase.GetValue("LogAllEvents");
					if (strLogAllEvents != null)
					{
						bLogAllEvents = Convert.ToBoolean(strLogAllEvents);
					}
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
			return bLogAllEvents;
		}

        public string ReadAdobeReaderVersion()
        {
            if (ReadAdobeReaderInstallPath("5.0") != null)
            {
                return "5.0";
            }
            else if (ReadAdobeReaderInstallPath("6.0") != null)
            {
                return "6.0";
            }
            else if (ReadAdobeReaderInstallPath("7.0") != null)
            {
                return "7.0";
            }
            else if (ReadAdobeReaderInstallPath("8.0") != null)
            {
                return "8.0";
            }
            else if (ReadAdobeReaderInstallPath("9.0") != null)
            {
                return "9.0";
            }

            throw new ApplicationException("No compatible Adobe Reader version was found in Registry.\nPlease check Adobe Reader version 5, 6, 7 or 8 is installed.");
        }

		public string ReadAdobeReaderInstallPath(string versionNumber)
		{
			string strDirectory = null;

            if (versionNumber != null)
            {
			    string registryKey = string.Format("SOFTWARE\\Adobe\\Acrobat Reader\\{0}\\InstallPath", versionNumber);

			    RegistryKey rkBase = null;
			    try
			    {
				    rkBase = Registry.LocalMachine.OpenSubKey(registryKey);
				    if(rkBase != null)
				    {
					    strDirectory = (string)rkBase.GetValue("");//read default value
				    }
			    }
			    finally
			    {
				    if (rkBase != null) 
				    {
					    rkBase.Close();
				    }
			    }
            }
             
			return strDirectory;
		}

		public string ReadInstallDirectory()
		{
			string strDirectory = null;
			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY);
				strDirectory = "";
				if(rkBase != null)
				{
					string dir = (String)rkBase.GetValue("Install Directory");
					if (dir != null) 
					{
						strDirectory = dir;
					}
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}

			return strDirectory;
		}

		public string ReadPrintDirectory()
		{
			string strDirectory = null;
			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY);
				strDirectory = "";
				if(rkBase != null){
					string dir = (String)rkBase.GetValue("Print Directory");
					if (dir != null) {
						strDirectory = dir;
					}
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}

			return strDirectory;
		}

		public void WritePrintDirectory(string strDirectory){
			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY, true);
				if(rkBase != null){
					rkBase.SetValue("Print Directory", strDirectory);
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
		}

		public string ReadPDFDirectory(){
			string strPDFDirectory = null;
			RegistryKey rkBase = null;
			try
			{
				strPDFDirectory = "";
				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY);
				if(rkBase != null){
					string dir = (String)rkBase.GetValue("PDF Directory");
					if (dir != null) {
						strPDFDirectory = dir;
					}
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
			return strPDFDirectory;
		}

		public void WritePDFDirectory(string strPDFDirectory){
			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY, true);
				if(rkBase != null){
					rkBase.SetValue("PDF Directory", strPDFDirectory);
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
		}
        public object GetAdobeAcrobatReaderDefault(string strAdobeAcrobatReaderVersion, string key)
        {
            string registryKey = string.Format("SOFTWARE\\Adobe\\Acrobat Reader\\{0}\\AVGeneral", strAdobeAcrobatReaderVersion);
            RegistryKey rkBase = null;
            object keyObj = null;
            try
            {
                rkBase = Registry.CurrentUser.OpenSubKey(registryKey);
                if (rkBase != null)
                {
                    keyObj = rkBase.GetValue(key);
                }
            }
            finally
            {
                if (rkBase != null)
                {
                    rkBase.Close();
                }
            }
            return keyObj;
        }
        public void SetAdobeAcrobatReaderDefault(string strAdobeAcrobatReaderVersion, string key, object value)
        {
            if (value != null)
            {
                string registryKey = string.Format("SOFTWARE\\Adobe\\Acrobat Reader\\{0}\\AVGeneral", strAdobeAcrobatReaderVersion);
                RegistryKey rkBase = null;
                try
                {
                    rkBase = Registry.CurrentUser.OpenSubKey(registryKey, true);
                    if (rkBase != null)
                    {
                        rkBase.SetValue(key, value);
                    }
                }
                finally
                {
                    if (rkBase != null)
                    {
                        rkBase.Close();
                    }
                }
            }
        }
		public void ReadLastPollDate(ref DateTime dtPollDate){
			RegistryKey rkBase = null;
			try
			{
				string strPollDate;
				long lTemp;

				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY);
				if(rkBase != null){
					strPollDate = (String)rkBase.GetValue("LastPollDate");
					if((strPollDate == null) || (strPollDate.Length == 0)){
						dtPollDate = DateTime.MinValue;
					}else{
						lTemp = Convert.ToInt64(strPollDate);
						dtPollDate = new DateTime(lTemp);
					}
				}else{
					dtPollDate = DateTime.MinValue;
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
		}

		public void ReadLastPollDate(ref string strPollDate){
			DateTime dtTemp;

			dtTemp = new DateTime();
			ReadLastPollDate(ref dtTemp);
			strPollDate = Convert.ToString(dtTemp);
		}

		public void WriteLastPollDate(string strPollDate){
			WriteLastPollDate(Convert.ToDateTime(strPollDate));
		}

		public void WriteLastPollDate(DateTime dtPollDate){
			WriteLastPollDate(dtPollDate.Ticks);
		}

		public void WriteLastPollDate(long lTicks){
			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY, true);
				if (rkBase != null)
				{
					rkBase.SetValue("LastPollDate", Convert.ToString(lTicks));
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
		}

		public void MakeSystemPrinter(string strPrinterName)
		{
			WriteMeticulusPrinterName(strPrinterName);
			
			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\Devices", false);
				if (rkBase != null)
				{
					string strPrinterDevice = (String)rkBase.GetValue(strPrinterName);
					if (strPrinterDevice != null)
					{
						rkBase = Registry.Users.OpenSubKey(".Default\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Devices", true);
						rkBase.SetValue(strPrinterName, strPrinterDevice);
					}
				}

				rkBase = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\PrinterPorts", false);
				if(rkBase != null)
				{
					string strPrinterPorts = (String)rkBase.GetValue(strPrinterName);
					if (strPrinterPorts != null)
					{
						rkBase = Registry.Users.OpenSubKey(".Default\\Software\\Microsoft\\Windows NT\\CurrentVersion\\PrinterPorts", true);
						rkBase.SetValue(strPrinterName, strPrinterPorts);
					}
				}
			}
			catch (Exception)
			{
				//Ignore any exceptions here. PFW Manager process probably does not have the correct priviledges.
				//The CPAT (pfw_print.exe) process may though, if it is running as a service.
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
		}

        public short ReadPrintOnBothSides(FileManifest fileManifest) 
		{
			short iPrintOnBothSides = 0;//Default value.

            if (fileManifest != null && fileManifest.DuplexMode >= 0)
            {
                iPrintOnBothSides = fileManifest.DuplexMode;
            }
            else
            {
			    RegistryKey rkBase = null;
			    try
			    {
				    string strPrintOnBothSides;
				    rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY);
				    if (rkBase != null)
				    {
					    strPrintOnBothSides = (String)rkBase.GetValue("PrintOnBothSides");
					    if (strPrintOnBothSides != null)
					    {
						    iPrintOnBothSides = Convert.ToInt16(strPrintOnBothSides);
					    }
				    }
			    }
			    finally
			    {
				    if (rkBase != null) 
				    {
					    rkBase.Close();
				    }
			    }
            }

			return iPrintOnBothSides;
		}

		public void WritePrintOnBothSides(int iPrintOnBothSides) 
		{
			RegistryKey rkBase = null;
			try
			{
				string strPrintOnBothSides;

				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY, true);
				if (rkBase != null)
				{
					strPrintOnBothSides = iPrintOnBothSides.ToString();
					rkBase.SetValue("PrintOnBothSides", strPrintOnBothSides);
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
		}

		// Find out the name of the printer we are going to use. We pass this into CPAT, which sets
		// USER.Default\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Windows\\device
		// Doing it this way round we know we have control over the printer used rather than relying on
		// the above key which could be blatted by any other bit of software on the system.
        public string ReadPrinterName(FileManifest fileManifest)
		{
			string strPrinterName;

            if (fileManifest != null && string.IsNullOrEmpty(fileManifest.PrinterName) == false)
            {
                strPrinterName = fileManifest.PrinterName;
            }
            else
            { 
			    strPrinterName = ReadMeticulusPrinterName();

                if (!IsPrinterInitialized(strPrinterName))
                {
				    // If we can't use the meticulus printer at least have a go at using the default.
				    strPrinterName = ReadSystemPrinterName();
			    } 
            }

			return strPrinterName != null ? strPrinterName : "";
		}

        public bool IsPrinterInitialized(string strPrinterName)
        {
            return (strPrinterName != "" &&
                    strPrinterName.ToLower() != "defaultprinter" &&
                    strPrinterName.ToLower() != "select printer");
        }

		private string ReadSystemPrinterName()
		{
			RegistryKey rkBase = null;
			string strPrinterName = "";
			try
			{
				string strPrinterString = "";

				rkBase = Registry.Users.OpenSubKey(".Default\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Windows", true);
				if(rkBase != null) 
				{
					strPrinterString = (String)rkBase.GetValue("device");
				}

				if (strPrinterString != null && strPrinterString != "") 
				{
					string [] split = strPrinterString.Split(',');

					if (split.Length > 0) 
					{
						strPrinterName = split[0];
					}
				}
			}
			catch (Exception)
			{
				//Ignore problems here. User will need to set PFW printer manually.
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}


			return strPrinterName != null ? strPrinterName : "";
		}

		public string ReadMeticulusPrinterName()
		{
			string strPrinterName = "";
			RegistryKey rkBase = null;
			try
			{

				rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY);
				if(rkBase != null){
					strPrinterName = (String)rkBase.GetValue("device");
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}

			return strPrinterName != null ? strPrinterName : "";
		}

		private void WriteMeticulusPrinterName(string strPrinterName)
		{
			RegistryKey rkBase = null;
			try
			{
				// Set up the default printer under Meticulus registry key area. This is later passed to CPAT, which 
				// blats the contents of USERS.Default\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Windows\\device
				if (strPrinterName != "")
				{
					rkBase = Registry.LocalMachine.OpenSubKey(REMOTE_PRINTING_REG_KEY, true);
					rkBase.SetValue("device", strPrinterName);
				}
			}
			finally
			{
				if (rkBase != null) 
				{
					rkBase.Close();
				}
			}
		}

		public void CreateEventLog(string source)
		{
			if(!EventLog.SourceExists(source)){
				EventLog.CreateEventSource(source, "Meticulus Log");
				//Need to set HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Eventlog\Meticulus Log 
				//MaxSize and Rentention
				RegistryKey rkBase = null;
				try
				{
					rkBase = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Eventlog\Meticulus Log", true);
					if(rkBase != null)
					{
						rkBase.SetValue("MaxSize", 0x00100000);//1 Mb max value
						rkBase.SetValue("Retention", 0x00000000);//Overwite as required.
					}
				}
				finally
				{
					if (rkBase != null) 
					{
						rkBase.Close();
					}
				}
			}
		}
		public void WriteEventLogEntry(string message, EventLogEntryType entryType, string source)
		{
			EventLog eventLog = null;
   
			try
			{
				eventLog = new EventLog();
				eventLog.Source = source;
				eventLog.WriteEntry(message, entryType, 0);
			}
			finally
			{
				if (eventLog != null)
				{
					eventLog.Close();
				}
			}
		}
	}
}
