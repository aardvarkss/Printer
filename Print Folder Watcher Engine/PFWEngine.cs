using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.IO;
using System.Xml;

using Print_Folder_Watcher_Common;

namespace Print_Folder_Watcher_Engine
{
	/// <summary>
	/// Manages two threads:
	///		1. Handles monitoring the folders for new files,
	///		2. Prints files using dpp_print.exe.
	/// </summary>
	public class PFWEngine
	{
		private ArrayList			m_alQueue;
		private ArrayList			m_printQueue;
		private string				m_currentPrintFile;
		private DateTime			m_timeLastFileAddedToQueue;
		private Thread				m_trdWatcher;
		private Thread				m_trdPrint;
        private Timer               m_purgeTmpFilesTimer;
		private FileSystemWatcher	m_fswWatcher;
		private ManualResetEvent	m_printShutdownEvent;
		private ManualResetEvent	m_watcherShutdownEvent;
		private clsUtils			m_Utils;

		private Process				m_processAdobe;
		private Process				m_processDppPrint;
        private Process             m_processCmdLine;
        private bool                m_processAborted;

		private const double		MILLISECONDS_DELAY_BEFORE_BATCH_PROCESSING = 2000;
		private const int			MILLISECONDS_BEFORE_RESUBMIT = 2 * 60 * 1000;//Two minutes.

		private bool				m_isStarted;

		private string				m_strPFWDirectory;
		private string				m_strPDFDirectory;
		private string				m_strPrintDirectory;
		private string				m_strDppPrintExePath;
		private const string		DPP_PRINT_EXE_NAME = "dpp_print.exe";
        private string              m_strAdobeReaderExePath;
        private string              m_strAdobeReaderVersion;

		private const int			FILE_NOT_IN_USE = 0;
		private const int			FILE_IN_USE = 1;
		private const int			FILE_DOES_NOT_EXIST = -1;

        private object              m_bprintExpandToFit;
        private object              m_iprintScaling;
        private object              m_bprintAutoRotate;

        private bool logAllEvents = false;

		public PFWEngine()
		{
			m_alQueue = new ArrayList();
			m_printQueue = ArrayList.Synchronized(m_alQueue);

			m_Utils = new clsUtils();
			m_Utils.CreateEventLog("Print Folder Watcher Service");

			m_currentPrintFile = null;

			Initialise();

			m_isStarted = false;
		}

		private void Initialise()
		{
			m_strPFWDirectory = m_Utils.ReadPrintDirectory();
			m_strPDFDirectory = m_Utils.ReadPDFDirectory();

            m_strAdobeReaderVersion = m_Utils.ReadAdobeReaderVersion();
            string strDirectory = m_Utils.ReadAdobeReaderInstallPath(m_strAdobeReaderVersion);

            if (strDirectory == null)
            {
                throw new ApplicationException("Adobe Reader install path not found in Registry.\nPlease check Adobe Reader version 5, 6, 7 or 8 is installed.");
            }
            m_strAdobeReaderExePath = strDirectory + "\\AcroRd32.exe";


            string strInstallDirectory = m_Utils.ReadInstallDirectory();

            m_strDppPrintExePath = strInstallDirectory + DPP_PRINT_EXE_NAME;
            m_strPrintDirectory = strInstallDirectory + "Print Folder";

            logAllEvents = m_Utils.ReadLogAllEvents();
		}

		/// <summary>
		/// TODO: Should inherit from IDispose.
		/// </summary>
		public void Dispose()
		{
			DisposeWatcher();

			if (m_printShutdownEvent != null)
			{
				m_printShutdownEvent.Close();
				m_printShutdownEvent = null;
			}

		}

		private void DisposeWatcher()
		{
			if (m_fswWatcher != null)
			{
				m_fswWatcher.Dispose();
				m_fswWatcher = null;
			}

			if (m_watcherShutdownEvent != null)
			{
				m_watcherShutdownEvent.Close();
				m_watcherShutdownEvent = null;
			}
		}

		public bool IsStarted
		{
			get
			{
				return m_isStarted;
			}
		}

		public void Start()
		{
			try
			{
                StartPurgeTmpFilesTimer();
				StartWatcherThread();
				StartPrintThread();
			}
			catch(Exception ex)
			{
				string strMessage = string.Format("Unexpected error.\n    {0}\n    {1}", ex.Message, ex.StackTrace);
				m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
				throw ex;
			}

			m_isStarted = true;

			m_Utils.WriteEventLogEntry(Utils.SERVICE_STARTED, EventLogEntryType.Information, Utils.EVENT_LOG_SOURCE);
		}
 
		public void Stop(bool waitForPrintThreadToExit)
		{
			try
			{
                StopPurgeTmpFilesTimer();
				StopWatcherThread();
				StopPrintThread(waitForPrintThreadToExit);
			}
			catch(Exception ex)
			{
				string strMessage = string.Format("Unexpected error.\n    {0}\n    {1}", ex.Message, ex.StackTrace);
				m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
				throw ex;
			}

			m_isStarted = false;

			m_Utils.WriteEventLogEntry(Utils.SERVICE_STOPPED, EventLogEntryType.Warning, Utils.EVENT_LOG_SOURCE);
		}

		public void Abort()
		{
            lock (this)
            {
                if (m_processCmdLine != null)
                {
                    AbortProcess(m_processCmdLine);
                }
            }
            
            lock (this)
			{
				if (m_processAdobe != null)
				{
					AbortProcess(m_processAdobe);
				}
			}

			lock(this)
			{
				if (m_processDppPrint != null)
				{
					AbortProcess(m_processDppPrint);
				}
			}
		}

		private void AbortProcess(Process process)
		{
			try
			{
				if (!process.CloseMainWindow())
				{
					process.Kill();
				}

				m_processAborted = true;
			}
			catch(Win32Exception ex)
			{
				string strMessage = string.Format("Unexpected error. The PFW process could not be terminated.\n    {0}\n    {1}", ex.Message, ex.StackTrace);
				m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
			}
			catch(InvalidOperationException ex)
			{
				string strMessage = string.Format("The PFW process has already exited and cannot be terminated.\n    {0}\n    {1}", ex.Message, ex.StackTrace);
				m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Information, Utils.EVENT_LOG_SOURCE);
			}
			catch(Exception ex)
			{
				string strMessage = string.Format("Unexpected error.\n    {0}\n    {1}", ex.Message, ex.StackTrace);
				m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Information, Utils.EVENT_LOG_SOURCE);
			}
		}

		private void StartWatcherThread()
		{
			m_timeLastFileAddedToQueue = DateTime.Now;
			ThreadStart tsWatcher = new ThreadStart( this.WatcherThread );
			m_watcherShutdownEvent = new ManualResetEvent(false);

			m_trdWatcher = new Thread(tsWatcher);
			m_trdWatcher.Start();
		}

		private void StopWatcherThread()
		{
			m_watcherShutdownEvent.Set();

			if (!m_trdWatcher.Join(Utils.MILLISECONDS_SHORT_TIME_DELAY))
			{
				string strMessage = string.Format("Watcher thread failed to exit after {0} ms.\n", Utils.MILLISECONDS_SHORT_TIME_DELAY * 2);
				m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
			}
		}

		private void StartPrintThread()
		{
			ThreadStart tsPrint = new ThreadStart( this.PrintThread );

			m_printShutdownEvent = new ManualResetEvent(false);
			m_trdPrint = new Thread( tsPrint );

			m_trdPrint.Start();
		}

		private void StopPrintThread(bool waitForPrintThreadToExit)
		{
			m_printShutdownEvent.Set();

			if (waitForPrintThreadToExit)
			{
				//Wait forever for the print thread to exit. If we don't we risk printing a file twice,
				//since a file may have been submitted to CPAT for printing, and this takes minutes, not seconds.
				m_trdPrint.Join();
			}
		}
		protected void WatcherThread() 
		{

			//Load any files already in the folder.
			InitialiseQueue();

			m_fswWatcher = new FileSystemWatcher();
			m_fswWatcher.Path = m_strPFWDirectory;
			m_fswWatcher.Filter = "";

			try
			{
				m_fswWatcher.Created += new FileSystemEventHandler(OnCreated);
				m_fswWatcher.Error += new ErrorEventHandler(WatcherError);

				m_fswWatcher.EnableRaisingEvents = true;

				//m_Utils.WriteEventLogEntry(string.Format("Watcher Thread started (Thread ID {0}).\n", AppDomain.GetCurrentThreadId()), EventLogEntryType.Information, Utils.EVENT_LOG_SOURCE);

				//Wait until process shutdown.
				m_watcherShutdownEvent.WaitOne();
			}
			catch(Exception ex)
			{
				string strMessage = string.Format("Unexpected error.\n    {0}\n    {1}", ex.Message, ex.StackTrace);
				m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
				throw ex;
			}
		}

        private string GetDppPrintArguments(string fullPath, string printerName, short duplexMode)
		{
            long ignorePrintJobStatus = m_Utils.ReadIgnorePrintJobStatus();
            string args = string.Format("/f \"{0}\" /p \"{1}\" /ipjs {2} /evtl \"{3}\" /evts \"{4}\"", 
				fullPath, printerName, ignorePrintJobStatus, Utils.EVENT_LOG, Utils.EVENT_LOG_SOURCE); 

			if (duplexMode > 0)
			{
				args += " /du " + duplexMode.ToString();
			}

			if (!logAllEvents)
			{
				//Only log errors from CPAT.
				args += " /error";
			}

			return args;
		}

		private void PrintThread() 
		{
			ArrayList queueSorted = new ArrayList();


			while(!IsShutdown())
			{
				string fullPath = null;
                FileManifest fileManifest = null;
				try
				{
					if (queueSorted.Count == 0)
					{
						//Check if any new files are waiting to be printed.
						//This only happesn at start up or once the 
						//previous batch of queued files has been processed.
						AddFilesToSortedQueue(ref queueSorted);
					}

					bool firstAttempt = true;
					int iCounter = 0;
					while (iCounter < queueSorted.Count && !IsShutdown())
					{
						fullPath = (string)queueSorted[iCounter];
                        fileManifest = new FileManifest(m_strPFWDirectory, fullPath);

						int fileState = IsFileOpen(fullPath);
						if (fileState == FILE_IN_USE)
						{
							//File in use. Leave for next time round queue.
							iCounter++;
						}
						else if (fileState == FILE_NOT_IN_USE)
						{
							PrintFile(fullPath, queueSorted, ref firstAttempt, fileManifest);
						}
						else if (fileState == FILE_DOES_NOT_EXIST)
						{
							RemoveDocumentFromQueues(fullPath, queueSorted);
						}
					}
				}
				catch(Exception e)
				{
					string strMessage = null;
					if (fullPath != null)
					{
						strMessage = string.Format("{0}\n    Unexpected error: {1}\n    {2} was not printed.", 
							Path.GetFileName(fullPath), e.ToString(), fullPath);
					}
					else
					{
						strMessage = string.Format("Unexpected error.\n    {0}\n    {1}", e.Message, e.StackTrace);
					}

					m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);


                    if (m_Utils.ReadStopServiceOnPrinterError())
                    {
                        StopPrinting();
                    }
                    else if (fullPath != null)
                    {
                        MoveDocumentToFailed(fullPath, fileManifest);
                        RemoveDocumentFromQueues(fullPath, queueSorted);
                    }
                }
			}
		}

		private void SetCurrentPrintFile(string fullPath)
		{
			lock(this)
			{
				m_currentPrintFile = fullPath;
			}
		}

		private void PrintFile(string fullPath, ArrayList queueSorted, ref bool firstAttempt, FileManifest fileManifest)
		{
			SetCurrentPrintFile(fullPath);

			AlterXFDF(fullPath);

            string printerName = m_Utils.ReadPrinterName(fileManifest);
            short duplexMode = m_Utils.ReadPrintOnBothSides(fileManifest);
            bool runAsService = m_Utils.ReadRunAsService();

            string cmdLineString = m_Utils.ReadCmdLine();
            CmdLine cmdLine = null;
            if (!string.IsNullOrEmpty(cmdLineString))
            {
                cmdLine = ParseCmdLine(fullPath, printerName, duplexMode, cmdLineString);
            }

            if (cmdLine != null && cmdLine.RunAsFireAndForget)
            {
                PrintOrOtherwiseHandleFileUsingFireAndForgetCmdLine(cmdLine, runAsService, fullPath, queueSorted, fileManifest);
            }
            else
            {
                SetAdobeAcrobatReaderDefaults();
                if (runAsService)
                {
                    PrintFileUsingDppPrint(fullPath, printerName, duplexMode, queueSorted, ref firstAttempt, fileManifest);
                }
                else
                {
                    PrintFileUsingAdobeReader(fullPath, queueSorted, fileManifest);
                }

                ReSetAdobeAcrobatReaderDefaults();
            }

			SetCurrentPrintFile(null);
		}

        private CmdLine ParseCmdLine(string fullPath, string printerName, short duplexMode, string cmdLineString)
        {
            try
            {
                CmdLineParser cmdLineParser = new CmdLineParser(cmdLineString);
                return cmdLineParser.Parse(fullPath, printerName, duplexMode);
            }
            catch (Exception ex)
            {
                string message = string.Format("Problem parsing command line: {0}\n{1}", cmdLineString, ex.ToString());
                m_Utils.WriteEventLogEntry(message, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
                throw ex;
            }
        }

		//TODO: Refactor handling of processes into a ProntProcess class
        private void PrintFileUsingAdobeReader(string fullPath, ArrayList queueSorted, FileManifest fileManifest)
		{
			try
			{
				lock(this)
				{
					m_processAdobe = new Process();

					m_processAdobe.StartInfo.FileName =  m_strAdobeReaderExePath;

					m_processAdobe.StartInfo.UseShellExecute = true;
                    m_processAdobe.StartInfo.Arguments = Path.GetFileName(fullPath);
					m_processAdobe.StartInfo.WorkingDirectory = m_strPrintDirectory;
					m_processAdobe.StartInfo.CreateNoWindow = true;
					m_processAdobe.StartInfo.ErrorDialog = true;

                    if (logAllEvents)
                    {
                        string message = string.Format("Launching Adobe: {0} {1} in folder {2}", 
                            m_processAdobe.StartInfo.FileName, m_processAdobe.StartInfo.Arguments, m_strPrintDirectory);
                        m_Utils.WriteEventLogEntry(message, EventLogEntryType.Information, Utils.EVENT_LOG_SOURCE);

                    }

					m_processAdobe.Start();
					m_processAborted = false;
				}

				m_processAdobe.WaitForExit();
			}
			finally
			{
				lock(this)
				{
					if (m_processAdobe != null)
					{
						m_processAdobe.Dispose();
						m_processAdobe = null;
					}
				}
			}

			lock(this)
			{
				if (!m_processAborted)
				{
					HandleAdobe(fullPath, queueSorted, fileManifest);
				}
			}
		}

        private void HandleAdobe(string fullPath, ArrayList queueSorted, FileManifest fileManifest)
		{
			//We cannot tell whether Adobe Reader has been successfully used to print the form or not, 
			//since the process exit code is not informative (seems to always be 0x1) so we have to assume 
			//it worked OK. Don't log anything, partly because of this, and partly because we are in 
			//interactive mode anyway.
			MoveDocumentToPrinted(fullPath, fileManifest);
			RemoveDocumentFromQueues(fullPath, queueSorted);
		}

        private void PrintOrOtherwiseHandleFileUsingFireAndForgetCmdLine(CmdLine cmdLine, bool runAsService, string fullPath, ArrayList queueSorted, FileManifest fileManifest)
        {
            int timeoutInMinutes = m_Utils.ReadTimeout();
            bool timedOut = false;
            bool showErrorDialog = !runAsService;
            int exitCode = 0;
            try
            {
                lock (this)
                {
                    m_processCmdLine = new Process();

                    m_processCmdLine.StartInfo.FileName = cmdLine.ExecutableFullName;
                    m_processCmdLine.StartInfo.Arguments = cmdLine.Arguments;

                    m_processCmdLine.StartInfo.UseShellExecute = true;
                    m_processCmdLine.StartInfo.WorkingDirectory = m_strPrintDirectory;
                    m_processCmdLine.StartInfo.CreateNoWindow = true;
                    m_processCmdLine.StartInfo.ErrorDialog = showErrorDialog;

                    if (logAllEvents)
                    {
                        string message = string.Format("Launching command line: {0} {1} in folder {2}",
                            m_processCmdLine.StartInfo.FileName, m_processCmdLine.StartInfo.Arguments, m_strPrintDirectory);
                        m_Utils.WriteEventLogEntry(message, EventLogEntryType.Information, Utils.EVENT_LOG_SOURCE);
                    }

                    m_processCmdLine.Start();
                    m_processAborted = false;
                }

                if (timeoutInMinutes == 0)
                {
                    //No timeout.
                    m_processCmdLine.WaitForExit();
                    exitCode = m_processCmdLine.ExitCode;
                }
                else if (!m_processCmdLine.WaitForExit(timeoutInMinutes * 60000))
                {
                    timedOut = true;
                    m_processCmdLine.Kill();
                    m_processCmdLine.WaitForExit(timeoutInMinutes * 60000);
                }
                else
                {
                    exitCode = m_processCmdLine.ExitCode;
                }

                if (!m_processAborted)
                {
                    if (timedOut)
                    {
                        HandleCmdLineTimedOut(fullPath, queueSorted, cmdLine, timeoutInMinutes, fileManifest);
                    }
                    else
                    {
                        HandleCmdLine(fullPath, queueSorted, cmdLine, exitCode, fileManifest);
                    }
                }
            }
            finally
            {
                lock (this)
                {
                    if (m_processCmdLine != null)
                    {
                        m_processCmdLine.Dispose();
                        m_processCmdLine = null;
                    }
                }
            }
        }

        private void HandleCmdLine(string fullPath, ArrayList queueSorted, CmdLine cmdLine, int exitCode, FileManifest fileManifest)
        {
            bool handleAsSuccess = !cmdLine.SuccessExitCodeSpecified || exitCode == cmdLine.SuccessExitCode;

            if (handleAsSuccess)
            {
                MoveDocumentToPrinted(fullPath, fileManifest);
                RemoveDocumentFromQueues(fullPath, queueSorted);
            }
            else
            {
                MoveDocumentToFailed(fullPath, fileManifest);
                RemoveDocumentFromQueues(fullPath, queueSorted);

                string message = string.Format("{0} returned exit code {1} (success exit code is specified as {2})\nDocument {3} has been moved to the failed documents folder",
                    cmdLine.ExecutableFullName, exitCode.ToString(), cmdLine.SuccessExitCode.ToString(), fullPath);
                m_Utils.WriteEventLogEntry(message, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
            }
        }

        private void HandleCmdLineTimedOut(string fullPath, ArrayList queueSorted, CmdLine cmdLine, int timeoutInMinutes, FileManifest fileManifest)
        {
            MoveDocumentToFailed(fullPath, fileManifest);
            RemoveDocumentFromQueues(fullPath, queueSorted);

            string message = string.Format("{0} timed out after {1} minutes.\nDocument {2} has been moved to the failed documents folder",
                cmdLine.ExecutableFullName, timeoutInMinutes, fullPath);
            m_Utils.WriteEventLogEntry(message, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
        }

        //TODO: Refactor handling of processes into a ProntProcess class
        private void PrintFileUsingDppPrint(string fullPath, string printerName, short duplexMode, ArrayList queueSorted, ref bool firstAttempt, FileManifest fileManifest)
		{
			//Call CPAT via the dpp_print process.
			int timeoutInMinutes = m_Utils.ReadTimeout() ;
			bool timedOut = false;
			int dppPrintExitCode = 0;
			try
			{
				lock(this)
				{
					m_processDppPrint = new Process();

					m_processDppPrint.StartInfo.FileName =  m_strDppPrintExePath;
					m_processDppPrint.StartInfo.UseShellExecute = false;
                    m_processDppPrint.StartInfo.Arguments = GetDppPrintArguments(fullPath, printerName, duplexMode);
                    m_processDppPrint.StartInfo.WorkingDirectory = m_Utils.ReadInstallDirectory();
					m_processDppPrint.StartInfo.CreateNoWindow = true;

                    if (logAllEvents)
                    {
                        string message = string.Format("Launching CPAT: {0} {1} in folder {2}",
                            m_processDppPrint.StartInfo.FileName, m_processDppPrint.StartInfo.Arguments, m_strPrintDirectory);
                        m_Utils.WriteEventLogEntry(message, EventLogEntryType.Information, Utils.EVENT_LOG_SOURCE);

                    }

					m_processDppPrint.Start();
					m_processAborted = false;
				}

				if (timeoutInMinutes == 0)
				{
					//No timeout.
					m_processDppPrint.WaitForExit();
				}
				else if (!m_processDppPrint.WaitForExit(timeoutInMinutes * 60000))
				{
					timedOut = true;
                    dppPrintExitCode = -1; // Not a dppPrint print error.
                    HandleTimeout(dppPrintExitCode, fullPath, queueSorted, ref firstAttempt, timeoutInMinutes, fileManifest);
				}

				if (!timedOut)
				{
					lock(this)
					{
						if (!m_processAborted)
						{
							dppPrintExitCode = m_processDppPrint.ExitCode;
							HandleDppPrintExitCode(dppPrintExitCode, fullPath, queueSorted, ref firstAttempt, fileManifest);
						}
					}
				}
			}
			finally
			{
				lock(this)
				{
					if (m_processDppPrint != null)
					{
						m_processDppPrint.Dispose();
						m_processDppPrint = null;
					}
				}
			}
		}

        private void HandleDppPrintExitCode(long dppPrintExitCode, string fullPath, ArrayList queueSorted, ref bool firstAttempt, FileManifest fileManifest)
		{
			long bitmask = 0xffffffffL << 32;
			dppPrintExitCode = dppPrintExitCode & ~bitmask;
			if (Utils.ExitCodeIsSuccess(dppPrintExitCode))
			{
                HandleSuccess(fullPath, queueSorted, fileManifest);
			}
			else if (Utils.ExitCodeIsFatalError(dppPrintExitCode))
			{
                HandleFatalError(dppPrintExitCode, fullPath, queueSorted, ref firstAttempt, fileManifest);
			}
			else if (Utils.ExitCodeIsPrinterError(dppPrintExitCode))
			{
                HandlePrinterError(dppPrintExitCode, fullPath, queueSorted, ref firstAttempt, fileManifest);
			}
			else if (Utils.ExitCodeIsDataError(dppPrintExitCode))
			{
                HandleDataError(dppPrintExitCode, fullPath, queueSorted, ref firstAttempt, fileManifest);
			}
			else if (Utils.ExitCodeIsParameterError(dppPrintExitCode))
			{
                HandleParameterError(dppPrintExitCode, fullPath, queueSorted, ref firstAttempt, fileManifest);
			}
			else if (Utils.ExitCodeIsUnknownError(dppPrintExitCode) || Utils.ExitCodeIsNotUsed(dppPrintExitCode))
			{
                HandleUnknownError(dppPrintExitCode, fullPath, queueSorted, ref firstAttempt, fileManifest);
			}
			else
			{
                HandleUnexpectedError(dppPrintExitCode, fullPath, queueSorted, ref firstAttempt, fileManifest);
			}
		}

		private void HandleSuccess(string fullPath, ArrayList queueSorted, FileManifest fileManifest)
		{
			MoveDocumentToPrinted(fullPath, fileManifest);
			RemoveDocumentFromQueues(fullPath, queueSorted);

			m_Utils.WriteEventLogEntry(string.Format("{0}\n    Document printed successfully.", Path.GetFileName(fullPath)), 
				EventLogEntryType.Information, Utils.EVENT_LOG_SOURCE);
		}
        private void HandleError(
            long dppPrintExitCode,
            string typeMsg, 
            string fullPath, 
            ArrayList queueSorted,
            bool doRetry ,
            ref bool firstAttempt,
            FileManifest fileManifest)
        {
            string description = "The attempt to print has failed (see previous error message for details).";

            if (m_Utils.ReadStopServiceOnPrinterError())
            {
                StopPrinting();

                description += "\nPrinting has been stopped.";
                description += "\nPlease resolve the problem and check whether the file prints OK.";
                description += "\nThen restart the " + (m_Utils.ReadRunAsService() ? Utils.SERVICE_NAME : Utils.MANAGER_NAME);

                firstAttempt = true;
            }
            else
            {
                if (doRetry && firstAttempt)
                {
                    description += "\nResubmitting job.";
                    firstAttempt = false;
                }
                else
                {
                    MoveDocumentToFailed(fullPath, fileManifest);
                    RemoveDocumentFromQueues(fullPath, queueSorted);

                    description += "\nPlease resolve the problem and check whether the file prints OK.";
                    description += "\nIf the file does not print resubmit the print from the failed print tab in the Print Folder Manager.";

                    firstAttempt = true;
                }
            }

            // File name MUST always be first so that it appears correctly in the failed prints tab
            string message = Path.GetFileName(fullPath);
            message += "\n" + description;
            message += "\n" + typeMsg;

            if (dppPrintExitCode > 0)
            {
                message += "\nError code: " + dppPrintExitCode.ToString() + " (" + EvtMsg.GetShortDescriptionFromCode(dppPrintExitCode) + ")";
            }

            message.Replace("\n", "\n    ");

            m_Utils.WriteEventLogEntry(message, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
        }
        private void HandleTimeout(long dppPrintExitCode, string fullPath, ArrayList queueSorted, ref bool firstAttempt, int timeoutInMinutes, FileManifest fileManifest)
		{
            string message = string.Format("Timed out after {0} minutes", timeoutInMinutes);

            bool doRetry = true;
            HandleError(dppPrintExitCode, message, fullPath, queueSorted, doRetry, ref firstAttempt, fileManifest);
		}
        private void HandleDataError(long dppPrintExitCode, string fullPath, ArrayList queueSorted, ref bool firstAttempt, FileManifest fileManifest)
		{
			//Duff Xfdf or Pdf.
            bool doRetry = false;
            HandleError(dppPrintExitCode, "Data format error", fullPath, queueSorted, doRetry, ref firstAttempt, fileManifest);
		}
        private void HandleFatalError(long dppPrintExitCode, string fullPath, ArrayList queueSorted, ref bool firstAttempt, FileManifest fileManifest)
		{
            bool doRetry = true;
            HandleError(dppPrintExitCode, "Fatal configuration error", fullPath, queueSorted, doRetry, ref firstAttempt, fileManifest);
		}
        private void HandlePrinterError(long dppPrintExitCode, string fullPath, ArrayList queueSorted, ref bool firstAttempt, FileManifest fileManifest)
		{
            bool doRetry = false;
            HandleError(dppPrintExitCode, "Printer error", fullPath, queueSorted, doRetry, ref firstAttempt, fileManifest);
		}
        private void HandleParameterError(long dppPrintExitCode, string fullPath, ArrayList queueSorted, ref bool firstAttempt, FileManifest fileManifest)
		{
            bool doRetry = false;
            HandleError(dppPrintExitCode, "Invalid parameter error", fullPath, queueSorted, doRetry, ref firstAttempt, fileManifest);
		}
        private void HandleUnknownError(long dppPrintExitCode, string fullPath, ArrayList queueSorted, ref bool firstAttempt, FileManifest fileManifest)
		{
            bool doRetry = true;
            HandleError(dppPrintExitCode, "Unknown error", fullPath, queueSorted, doRetry, ref firstAttempt, fileManifest);
		}
        private void HandleUnexpectedError(long dppPrintExitCode, string fullPath, ArrayList queueSorted, ref bool firstAttempt, FileManifest fileManifest)
		{
            bool doRetry = true;
            HandleError(dppPrintExitCode, "Unexpected error", fullPath, queueSorted, doRetry, ref firstAttempt, fileManifest);
		}

		private void StopPrinting()
		{
			m_printShutdownEvent.Set();

            if (!m_Utils.ReadRunAsService())
                return;

            try
			{
				using (ServiceController serviceController = new ServiceController(Utils.SERVICE_NAME))
				{
					serviceController.Refresh();
					if (serviceController.Status != ServiceControllerStatus.Stopped && serviceController.Status != ServiceControllerStatus.StopPending)
					{ 
						serviceController.Stop();
					}
				}
			}
			catch(Exception ex)
			{
				string error = string.Format("Failed to stop service.\n{0}.", ex.ToString());
				m_Utils.WriteEventLogEntry(error, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
			}
		}

		private bool IsShutdown()
		{
			if (m_printShutdownEvent == null)
			{
				return true;
			}

			return m_printShutdownEvent.WaitOne(Utils.MILLISECONDS_SHORT_TIME_DELAY, true);
		}

		/// <summary>
		/// Determines whether the file is in use.
		/// Return values:
		/// 0		- File not in use
		/// 1		- File in use
		/// -1		- File does not exist
		/// </summary>
		private int IsFileOpen(String fullPath)
		{
			FileStream fsFile;

			try
			{
				if (File.Exists(fullPath))
				{
					//Make sure the file is not read-only.
					FileAttributes fileAttributes = File.GetAttributes(fullPath);
					if ((fileAttributes & FileAttributes.ReadOnly) != 0)
					{
						fileAttributes = fileAttributes & ~FileAttributes.ReadOnly;
						File.SetAttributes(fullPath, fileAttributes);
					}

					fsFile = File.Open(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
					if (fsFile != null)
					{
						fsFile.Close();
					}
					return FILE_NOT_IN_USE;
				}
				else
				{
					return FILE_DOES_NOT_EXIST;
				}
			}
			catch(IOException)
			{
				return FILE_IN_USE;
			}
		}

		private void OnCreated(object source, FileSystemEventArgs e) 
		{
			try
			{
				AddFileToQueue(e.FullPath);
			}
			catch (Exception ex)
			{
				//Make sure an exception does not get thrown here, since it stops the FielSystemWatcher from raising any new events.
				string strMessage = string.Format("Unexpected error.\n    {0}\n    {1}", ex.Message, ex.StackTrace);
				m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
			}
		}

		private void WatcherError(object source, ErrorEventArgs e) 
		{
			try
			{
				Exception watchException = e.GetException();
				m_Utils.WriteEventLogEntry(string.Format("PFWService","A FileSystemWatcher error has occurred: {0}. Thread ID {1}.", 
					watchException.Message, Thread.CurrentThread.ManagedThreadId), EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);

				// We probably need to create new version of the object because the old one is probably now corrupted. MSDN is unclear on this point.
				RestartWatcher();
			}
			catch (Exception ex)
			{
				string strMessage = string.Format("Unexpected error.\n    {0}\n    {1}", ex.Message, ex.StackTrace);
				m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
			}
		}

		public void RestartWatcher()
		{
			StopWatcherThread();
			DisposeWatcher();
			StartWatcherThread();
		}

		private void InitialiseQueue()
		{
			string[] strFiles;
			
			try
			{
				if (!Directory.Exists(m_strPFWDirectory))
				{
					Directory.CreateDirectory(m_strPFWDirectory);
				}

				if (Directory.Exists(m_strPFWDirectory))
				{
					strFiles = Directory.GetFiles(m_strPFWDirectory);

					foreach( string strFile in strFiles )
					{
						AddFileToQueue(strFile);
					}
				}
			}
			catch (Exception e) 
			{
				string strMessage = string.Format("Unexpected error.\n    {0}\n    {1}", e.Message, e.StackTrace);
				m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
			}
		}

		private void AddFileToQueue(string fullPath)
		{
			lock(this)
			{
			    m_timeLastFileAddedToQueue = DateTime.Now;

			    bool bFileIsPrinting = m_currentPrintFile != null && string.Compare(m_currentPrintFile, fullPath, true) == 0;
			    if (!bFileIsPrinting)
			    {
				    m_printQueue.Add(fullPath);
			    }
			}
		}

		private void AlterXFDF(string fullPath)
		{
			string strExtension = Path.GetExtension(fullPath);

			if (string.Compare(strExtension, ".xfdf", true) == 0)
			{
				try
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.PreserveWhitespace = true;
					xmlDocument.Load(fullPath);

					XmlNodeList xmlElementList = xmlDocument.GetElementsByTagName("f");
					XmlNode xmlNode = xmlElementList[0];

					string strXFDFFile = Path.GetFileName(xmlNode.Attributes.GetNamedItem("href").Value);
					int pathDelimiter = strXFDFFile.LastIndexOf(@"\");
					if (pathDelimiter > -1)
					{
						strXFDFFile = strXFDFFile.Substring(pathDelimiter + 1);
					}

					xmlNode.Attributes.GetNamedItem("href").Value = m_strPDFDirectory + "\\" + strXFDFFile;

					xmlDocument.Save(fullPath);
				}
				catch(XmlException ex)
				{
					//Ignore strange Xml formats...
					string strMessage = string.Format("Unexpected XFDF file format.\n    {0}\n    {1}", ex.Message, ex.StackTrace);
					m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Warning, Utils.EVENT_LOG_SOURCE);
				}
			}
		}

		private void AddFilesToSortedQueue(ref ArrayList queueSorted)
		{
			if (queueSorted.Count > 0)
			{
				throw new InvalidOperationException("queueSorted.Count should equal zero.");
			}

			if (m_printQueue.Count > 0)
			{
				//Check that the timestamp of the last file added to the queue is old enough.
				//The idea here is to enable sorting of a batch of files dropped into the 
				//print folder within a short time interval.
				bool copyToSortedQueue = false;
                lock (this)
                {
				    copyToSortedQueue = ((TimeSpan)(DateTime.Now - m_timeLastFileAddedToQueue)).TotalMilliseconds > MILLISECONDS_DELAY_BEFORE_BATCH_PROCESSING;

				    if (copyToSortedQueue)
				    {
					    queueSorted = (ArrayList)m_printQueue.Clone();
					    queueSorted.Sort();
				    }
                }
			}
		}

		private void RemoveDocumentFromQueues(string fullPath, ArrayList queueSorted)
		{
			queueSorted.Remove(fullPath);
			m_printQueue.Remove(fullPath);
		}

        private void MoveDocumentToFailed(string fullPath, FileManifest fileManifest)
		{
			bool success = false;
			MoveDocument(fullPath, success, fileManifest);
		}

        private void MoveDocumentToPrinted(string fullPath, FileManifest fileManifest)
		{
			bool success = true;
			MoveDocument(fullPath, success, fileManifest);
		}

        private void MoveDocument(string fullPath, bool success, FileManifest fileManifest)
		{
            try
            {
			    string strParentDirectory = Directory.GetParent(fullPath).FullName;
			    if (success)
			    {
				    strParentDirectory += "\\Printed Documents";
			    }
			    else
			    {
				    strParentDirectory += "\\Failed Documents";
			    }

			    if (!Directory.Exists(strParentDirectory))
			    {
				    Directory.CreateDirectory(strParentDirectory);
			    }

			    if (Directory.Exists(strParentDirectory))
			    {
				    string destFullName = strParentDirectory + "\\" + Path.GetFileName(fullPath);
				    if (File.Exists(destFullName))
				    {
					    File.Delete(destFullName);
				    }

				    File.Move(fullPath, destFullName);
			    }
            }
            catch (Exception ex)
            {
                //Catch and log errors.
                string strMessage = string.Format("Unexpected error moving document.\n    {0}\n    {1}", ex.Message, ex.StackTrace);
                m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
            }
		}
        public void SetAdobeAcrobatReaderDefaults()
        {
            // This method sets the necessary defaults to enable Acrobat reader to print pattern 
            // correctly without scaling etc making a note of what the user had to start with 
            // so that it can be reset.

            try
            {
                m_bprintExpandToFit = m_Utils.GetAdobeAcrobatReaderDefault(m_strAdobeReaderVersion, "bprintExpandToFit");
                m_Utils.SetAdobeAcrobatReaderDefault(m_strAdobeReaderVersion, "bprintExpandToFit", 0);

                m_iprintScaling = m_Utils.GetAdobeAcrobatReaderDefault(m_strAdobeReaderVersion, "iprintScaling");
                m_Utils.SetAdobeAcrobatReaderDefault(m_strAdobeReaderVersion, "iprintScaling", 1);

                m_bprintAutoRotate = m_Utils.GetAdobeAcrobatReaderDefault(m_strAdobeReaderVersion, "bprintAutoRotate");
                m_Utils.SetAdobeAcrobatReaderDefault(m_strAdobeReaderVersion, "bprintAutoRotate", 0);
            }
            catch (Exception ex)
            {
                // Consume the exception because we dont want to have this stop the printing process.
                // It's a nice to have if it is possible.
                string strMessage = string.Format("Unable to Set Adobe Acrobat Reader Defaults.\n    {0}\n    {1}", ex.Message, ex.StackTrace);
                m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Warning, Utils.EVENT_LOG_SOURCE);
            }
        }
        public void ReSetAdobeAcrobatReaderDefaults()
        {
            try
            {
                m_Utils.SetAdobeAcrobatReaderDefault(m_strAdobeReaderVersion, "bprintExpandToFit", m_bprintExpandToFit);
                m_Utils.SetAdobeAcrobatReaderDefault(m_strAdobeReaderVersion, "iprintScaling", m_iprintScaling);
                m_Utils.SetAdobeAcrobatReaderDefault(m_strAdobeReaderVersion, "bprintAutoRotate", m_bprintAutoRotate);
            }
            catch (Exception ex)
            {
                // Consume the exception because we dont want to have this stop the printing process.
                // It's a nice to have if it is possible.
                string strMessage = string.Format("Unable to ReSet Adobe Acrobat Reader Defaults.\n    {0}\n    {1}", ex.Message, ex.StackTrace);
                m_Utils.WriteEventLogEntry(strMessage, EventLogEntryType.Warning, Utils.EVENT_LOG_SOURCE);
            }
        }
        #region Purge tmp files timer
        private void StartPurgeTmpFilesTimer()
        {
            TempFilesManager man = new TempFilesManager();
            TimerCallback timerDelegate = new TimerCallback(this.PurgeTmpFilesTimer);
            m_purgeTmpFilesTimer = new Timer(timerDelegate, null, 0, man.FrequencyMinutes*60*1000);
        }
        private void StopPurgeTmpFilesTimer()
        {
            m_purgeTmpFilesTimer.Dispose();
            m_purgeTmpFilesTimer = null;
        }
        private void PurgeTmpFilesTimer(object state)
        {
            //Purge any old acrobat files left in the temp folders
            //to avoid a slow deteriation of performance. Executed on a timer.
            TempFilesManager tempFilesManager = new TempFilesManager();
            tempFilesManager.PurgeAll();
        }
        #endregion Purge tmp files timer
    }
}
