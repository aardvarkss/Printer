using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.IO;
using System.Drawing.Printing;
using Print_Folder_Watcher_Common;
using Print_Folder_Watcher_Engine;
using System.Runtime.InteropServices;

namespace Print_Folder_Watcher_Manager
{
	public class FrmManager : System.Windows.Forms.Form
	{
		#region Form variables
		private System.ComponentModel.IContainer components;
		private clsNotifyBalloon niconNotifyBalloon;
		private System.Windows.Forms.ContextMenu cmenuNotify;

		private MenuItem mitemExit;
		private MenuItem mitemFailedJobs;
		private MenuItem mitemPrintedJobs;
		private MenuItem mitemPDFJobs;
		private MenuItem mitemConfigure;
		private MenuItem mitemStartService;
		private MenuItem mitemStopService;
		private MenuItem mitemSeperatorOne;
		private MenuItem mitemSeperatorTwo;
		private MenuItem mitemSeperatorThree;
		private System.Windows.Forms.TabPage tabFailedPrintsPage;
		private TreeView trvFailedPrints;
		private System.Windows.Forms.TabControl tabControlDisplay;
		private System.Windows.Forms.TabPage tabPrintedPage;
		private MultiSelectTreeView trvPrinted;
		private System.Windows.Forms.TabPage tabPDFPage;
		private MultiSelectTreeView trvPDFs;
		private System.Windows.Forms.TabPage tabOptionsPage;
		private System.Windows.Forms.Label lblMonFolder;
		private System.Windows.Forms.Label lblPDFFolder;
		private System.Windows.Forms.TextBox txtMonFolder;
		private System.Windows.Forms.TextBox txtPDFFolder;
		private System.Windows.Forms.Button btnMonFolder;
		private System.Windows.Forms.Button btnPDFFolder;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.Button btnPurge;
		private System.Windows.Forms.Label lblPrinter;
		private System.Windows.Forms.ComboBox cmbPrinters;
		private System.Windows.Forms.CheckBox cbStopServiceOnError;
		private System.Windows.Forms.Label lblStopServiceOnError;
		private System.Windows.Forms.Label labelTimeOut;
		private System.Windows.Forms.NumericUpDown numericUpDownTimeOut;
		private System.Windows.Forms.ComboBox cmbPrintBothSides;
		private System.Windows.Forms.Label lblPrintBothSides;
        private System.Windows.Forms.Button btnOpenFolder;
        #endregion

		private const int						WM_QUERYENDSESSION = 0x0011;
		private const string					EVENT_LOG_SOURCE_MANAGER = "Print Folder Watcher Manager";

		private clsUtils						m_Utils;
		private PFWEngine						m_pfwEngine;

		private EventLog						m_eventLog;

		private DateTime						m_dtLastTimeRun;

		private Icon							m_icAllFine;
		private Icon							m_icServiceStopped;
		private Icon							m_icError;

		private bool							m_bSessionEnd;

		private string							m_strPrintFolder;
		private string							m_strPDFFolder;
		private string							m_strPrinterName;
		private bool							m_bStopServiceOnError;
		private int								m_timeoutInMinutes;
		private int								m_duplexOption;//0 - Default, 1 - Simplex, 2 - Long Edge, 3 - Short Edge
		private bool							m_bInteractiveMode;
		private bool							m_bRunAsService;

		private bool							m_bPdfFolderModified;
		private bool							m_bPrintFolderModified;
		private bool							m_bPrinterModified;
		private bool							m_bStopServiceOnErrorModified;
		private bool							m_bPrinterAdded;

		private const int						FAILED_PRINTS = 0;
		private const int						PDFS = 1;
        private const int                       PRINTED = 2;
        private const int OPTIONS = 3;


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			FrmManager form = null;
			try
			{
				if (!IsAlreadyRunning())
				{
					form = new FrmManager();
					Application.Run(form);
				}
				else
				{
					MessageBox.Show("The Meticulus Print Folder Watcher Manager is already running.", 
						"Meticulus Print Folder Watcher Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				clsUtils utils = new clsUtils();
				utils.CreateEventLog(EVENT_LOG_SOURCE_MANAGER);

				// Write an entry to the event log.
				if (utils != null)
				{
					string strMessage = string.Format("Unexpected error.\n\t{0}\n\t{1}", ex.Message, ex.StackTrace);
					if (utils != null)
					{
						utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, EVENT_LOG_SOURCE_MANAGER);
					}
				}

				MessageBox.Show("A severe error has occurred causing the Meticulus Print Folder Watcher Manager to close.\n\n" + ex.Message, 
					"Meticulus Print Folder Watcher Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static bool IsAlreadyRunning()
		{
			string processFullName = Application.ExecutablePath;
			string processName = Path.GetFileNameWithoutExtension(processFullName);
			ProcessMonitor processMonitor = new ProcessMonitor(processName);
			if (processMonitor.IsProcessRunning(processName))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public FrmManager()
		{
			m_Utils = new clsUtils();
			m_Utils.CreateEventLog(EVENT_LOG_SOURCE_MANAGER);

            // Read the registry very early on to see if the printer has been set. If not we will try to select
            // the default printer and then show the user a warning balloon and then display the manager so 
            // that they can verify that it is correct.
            bool printerInitializedAtStartUp = m_Utils.IsPrinterInitialized(m_Utils.ReadMeticulusPrinterName());

			InitializeComponent();
			Initialize();
            InitIcons();

            if (!m_bRunAsService)
            {
                m_pfwEngine = new PFWEngine();
                StartEngine();
            }

            m_bSessionEnd = false;

			ResetOptionsModifiedFlags();
			
			m_eventLog = new EventLog();
			m_eventLog.Log = "Meticulus Log";                      
        
			m_eventLog.EntryWritten += new EntryWrittenEventHandler(OnEventWritten);
			m_eventLog.EnableRaisingEvents = true;

			if (IsServiceOrEngineStopped())
			{
				SetServiceStopped();
			}
			else
			{
				SetServiceStarted();
			}

			CheckPastErrorEvents();

            if (m_bRunAsService && printerInitializedAtStartUp == false)
            {
                ShowPrinterWasNotSet();
            }
		}

		#region Form display function
		protected override void Dispose( bool disposing ){
			if( disposing )
			{
				if (components != null)
					components.Dispose();

				if (m_eventLog != null)
				{
					m_eventLog.Close();
					m_eventLog = null;
				}
				if (m_pfwEngine != null)
				{
					m_pfwEngine.Dispose();
					m_pfwEngine = null;
				}
			}
			base.Dispose( disposing );
		}

		private void Initialize()
		{
			InitializeState();
			InitializeUI();
		}

		private void InitializeState()
		{
			//Do these first, since in particular ReadPrinterAdded() relies on this flag.
			m_bRunAsService = m_Utils.ReadRunAsService();
            m_bInteractiveMode = !m_bRunAsService;

			m_Utils.ReadLastPollDate(ref m_dtLastTimeRun);
			m_strPrintFolder = m_Utils.ReadPrintDirectory();
			m_strPDFFolder = m_Utils.ReadPDFDirectory();
			m_Utils.ReadPrinterAdded(ref m_bPrinterAdded);
			m_strPrinterName = this.ReadPrinterName();
			m_bStopServiceOnError = m_Utils.ReadStopServiceOnPrinterError();
			m_timeoutInMinutes = m_Utils.ReadTimeout();
            m_duplexOption = m_Utils.ReadPrintOnBothSides(null);// null=> no specific file manifest at this point
		}

		private void InitializeUI()
		{
			this.components = new System.ComponentModel.Container();
			this.cmenuNotify = new System.Windows.Forms.ContextMenu();

			this.mitemExit = new MenuItem();
			this.mitemFailedJobs = new MenuItem();
			this.mitemPrintedJobs = new MenuItem();
			this.mitemPDFJobs = new MenuItem();
			this.mitemConfigure = new MenuItem();
			this.mitemSeperatorOne = new MenuItem();
			this.mitemSeperatorTwo = new MenuItem();
			this.mitemSeperatorThree = new MenuItem();

			this.mitemStartService = new MenuItem();
			this.mitemStopService = new MenuItem();

			int index = 0;

            this.cmenuNotify.MenuItems.Add(index++, this.mitemFailedJobs);
			this.mitemFailedJobs.Text = "View Failed Prints";
			this.mitemFailedJobs.Click += new System.EventHandler(this.mitemFailedJobs_Click);

            this.cmenuNotify.MenuItems.Add(index++, this.mitemPrintedJobs);
			this.mitemPrintedJobs.Text = "View Printed Jobs";
			this.mitemPrintedJobs.Click += new System.EventHandler(this.mitemPrintedJobs_Click);

            this.cmenuNotify.MenuItems.Add(index++, this.mitemPDFJobs);
			this.mitemPDFJobs.Text = "View PDF Documents";
			this.mitemPDFJobs.Click += new System.EventHandler(this.mitemPDFJobs_Click);

            this.cmenuNotify.MenuItems.Add(index++, this.mitemSeperatorTwo);
			this.mitemSeperatorTwo.Text = "-";

            this.cmenuNotify.MenuItems.Add(index++, this.mitemConfigure);
			this.mitemConfigure.Text = "Configure";
			this.mitemConfigure.Click += new System.EventHandler(this.mitemConfigure_Click);

            if (m_bRunAsService)
            {
                this.cmenuNotify.MenuItems.Add(index++, this.mitemStartService);
			    this.mitemStartService.Text = "Start Service";
			    this.mitemStartService.Click += new System.EventHandler(this.mitemStartService_Click);

                this.cmenuNotify.MenuItems.Add(index++, this.mitemStopService);
			    this.mitemStopService.Text = "Stop Service";
			    this.mitemStopService.Click += new System.EventHandler(this.mitemStopService_Click);
            }


            this.cmenuNotify.MenuItems.Add(index++, this.mitemSeperatorThree);
			this.mitemSeperatorThree.Text = "-";

            this.cmenuNotify.MenuItems.Add(index++, this.mitemExit);
			this.mitemExit.Text = "E&xit";
			this.mitemExit.Click += new System.EventHandler(this.mitemExit_Click);

			// Set up how the form should be displayed.
			this.Text = "Meticulus Print Folder Watcher";
			this.ShowInTaskbar = false;
			this.WindowState = FormWindowState.Minimized;
			ShowHide(false);
      
			m_icAllFine = new Icon(GetType(), "Start.ico");
			m_icServiceStopped = new Icon(GetType(), "Stopped.ico");
			m_icError = new Icon(GetType(), "Error.ico");
			
			if(niconNotifyBalloon == null)
			{
				niconNotifyBalloon = new clsNotifyBalloon();
				niconNotifyBalloon.Text = "Meticulus Print Folder Watcher Manager";
				niconNotifyBalloon.Icon = m_icAllFine;
				niconNotifyBalloon.Visible = true;
				niconNotifyBalloon.ContextMenu = this.cmenuNotify;

				niconNotifyBalloon.BalloonClick += new EventHandler(OnClickBalloon);
				niconNotifyBalloon.DoubleClick += new EventHandler(OnDoubleClickIcon);
			}
			
		}

		private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmManager));
            this.tabControlDisplay = new System.Windows.Forms.TabControl();
            this.tabFailedPrintsPage = new System.Windows.Forms.TabPage();
            this.trvFailedPrints = new System.Windows.Forms.TreeView();
            this.tabPDFPage = new System.Windows.Forms.TabPage();
            this.trvPDFs = new Print_Folder_Watcher_Common.MultiSelectTreeView();
            this.tabPrintedPage = new System.Windows.Forms.TabPage();
            this.trvPrinted = new Print_Folder_Watcher_Common.MultiSelectTreeView();
            this.tabOptionsPage = new System.Windows.Forms.TabPage();
            this.cmbPrintBothSides = new System.Windows.Forms.ComboBox();
            this.lblPrintBothSides = new System.Windows.Forms.Label();
            this.numericUpDownTimeOut = new System.Windows.Forms.NumericUpDown();
            this.labelTimeOut = new System.Windows.Forms.Label();
            this.lblStopServiceOnError = new System.Windows.Forms.Label();
            this.cbStopServiceOnError = new System.Windows.Forms.CheckBox();
            this.cmbPrinters = new System.Windows.Forms.ComboBox();
            this.lblPrinter = new System.Windows.Forms.Label();
            this.btnPDFFolder = new System.Windows.Forms.Button();
            this.btnMonFolder = new System.Windows.Forms.Button();
            this.txtPDFFolder = new System.Windows.Forms.TextBox();
            this.txtMonFolder = new System.Windows.Forms.TextBox();
            this.lblPDFFolder = new System.Windows.Forms.Label();
            this.lblMonFolder = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnPurge = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.tabControlDisplay.SuspendLayout();
            this.tabFailedPrintsPage.SuspendLayout();
            this.tabPDFPage.SuspendLayout();
            this.tabPrintedPage.SuspendLayout();
            this.tabOptionsPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeOut)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlDisplay
            // 
            this.tabControlDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlDisplay.Controls.Add(this.tabFailedPrintsPage);
            this.tabControlDisplay.Controls.Add(this.tabPDFPage);
            this.tabControlDisplay.Controls.Add(this.tabPrintedPage);
            this.tabControlDisplay.Controls.Add(this.tabOptionsPage);
            this.tabControlDisplay.Location = new System.Drawing.Point(4, 5);
            this.tabControlDisplay.Name = "tabControlDisplay";
            this.tabControlDisplay.SelectedIndex = 0;
            this.tabControlDisplay.Size = new System.Drawing.Size(477, 192);
            this.tabControlDisplay.TabIndex = 0;
            this.tabControlDisplay.SelectedIndexChanged += new System.EventHandler(this.tabControlDisplay_SelectedIndexChanged);
            // 
            // tabFailedPrintsPage
            // 
            this.tabFailedPrintsPage.Controls.Add(this.trvFailedPrints);
            this.tabFailedPrintsPage.Location = new System.Drawing.Point(4, 22);
            this.tabFailedPrintsPage.Name = "tabFailedPrintsPage";
            this.tabFailedPrintsPage.Size = new System.Drawing.Size(469, 166);
            this.tabFailedPrintsPage.TabIndex = 0;
            this.tabFailedPrintsPage.Text = "Failed Prints";
            // 
            // trvFailedPrints
            // 
            this.trvFailedPrints.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.trvFailedPrints.Location = new System.Drawing.Point(6, 5);
            this.trvFailedPrints.Name = "trvFailedPrints";
            this.trvFailedPrints.Size = new System.Drawing.Size(456, 152);
            this.trvFailedPrints.TabIndex = 0;
            this.trvFailedPrints.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trvFailedPrints_AfterSelect);
            // 
            // tabPDFPage
            // 
            this.tabPDFPage.Controls.Add(this.trvPDFs);
            this.tabPDFPage.Location = new System.Drawing.Point(4, 22);
            this.tabPDFPage.Name = "tabPDFPage";
            this.tabPDFPage.Size = new System.Drawing.Size(469, 166);
            this.tabPDFPage.TabIndex = 1;
            this.tabPDFPage.Text = "PDF documents";
            // 
            // trvPDFs
            // 
            this.trvPDFs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.trvPDFs.Location = new System.Drawing.Point(1, 3);
            this.trvPDFs.Name = "trvPDFs";
            this.trvPDFs.SelectedNodes = ((System.Collections.ArrayList)(resources.GetObject("trvPDFs.SelectedNodes")));
            this.trvPDFs.Size = new System.Drawing.Size(733, 227);
            this.trvPDFs.TabIndex = 0;
            this.trvPDFs.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trvPDFs_AfterSelect);
            // 
            // tabPrintedPage
            // 
            this.tabPrintedPage.Controls.Add(this.trvPrinted);
            this.tabPrintedPage.Location = new System.Drawing.Point(4, 22);
            this.tabPrintedPage.Name = "tabPrintedPage";
            this.tabPrintedPage.Size = new System.Drawing.Size(469, 166);
            this.tabPrintedPage.TabIndex = 2;
            this.tabPrintedPage.Text = "Printed documents";
            // 
            // trvPrinted
            // 
            this.trvPrinted.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.trvPrinted.Location = new System.Drawing.Point(6, 6);
            this.trvPrinted.Name = "trvPrinted";
            this.trvPrinted.SelectedNodes = ((System.Collections.ArrayList)(resources.GetObject("trvPrinted.SelectedNodes")));
            this.trvPrinted.Size = new System.Drawing.Size(727, 221);
            this.trvPrinted.TabIndex = 0;
            this.trvPrinted.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trvPrinted_AfterSelect);
            // 
            // tabOptionsPage
            // 
            this.tabOptionsPage.Controls.Add(this.cmbPrintBothSides);
            this.tabOptionsPage.Controls.Add(this.lblPrintBothSides);
            this.tabOptionsPage.Controls.Add(this.numericUpDownTimeOut);
            this.tabOptionsPage.Controls.Add(this.labelTimeOut);
            this.tabOptionsPage.Controls.Add(this.lblStopServiceOnError);
            this.tabOptionsPage.Controls.Add(this.cbStopServiceOnError);
            this.tabOptionsPage.Controls.Add(this.cmbPrinters);
            this.tabOptionsPage.Controls.Add(this.lblPrinter);
            this.tabOptionsPage.Controls.Add(this.btnPDFFolder);
            this.tabOptionsPage.Controls.Add(this.btnMonFolder);
            this.tabOptionsPage.Controls.Add(this.txtPDFFolder);
            this.tabOptionsPage.Controls.Add(this.txtMonFolder);
            this.tabOptionsPage.Controls.Add(this.lblPDFFolder);
            this.tabOptionsPage.Controls.Add(this.lblMonFolder);
            this.tabOptionsPage.Location = new System.Drawing.Point(4, 22);
            this.tabOptionsPage.Name = "tabOptionsPage";
            this.tabOptionsPage.Size = new System.Drawing.Size(469, 166);
            this.tabOptionsPage.TabIndex = 3;
            this.tabOptionsPage.Text = "Options";
            // 
            // cmbPrintBothSides
            // 
            this.cmbPrintBothSides.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPrintBothSides.Location = new System.Drawing.Point(105, 108);
            this.cmbPrintBothSides.Name = "cmbPrintBothSides";
            this.cmbPrintBothSides.Size = new System.Drawing.Size(361, 21);
            this.cmbPrintBothSides.TabIndex = 3;
            this.cmbPrintBothSides.SelectionChangeCommitted += new System.EventHandler(this.cmbPrintBothSides_SelectedChangeCommitted);
            // 
            // lblPrintBothSides
            // 
            this.lblPrintBothSides.Location = new System.Drawing.Point(9, 111);
            this.lblPrintBothSides.Name = "lblPrintBothSides";
            this.lblPrintBothSides.Size = new System.Drawing.Size(91, 14);
            this.lblPrintBothSides.TabIndex = 12;
            this.lblPrintBothSides.Text = "Print both sides";
            // 
            // numericUpDownTimeOut
            // 
            this.numericUpDownTimeOut.Location = new System.Drawing.Point(287, 139);
            this.numericUpDownTimeOut.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTimeOut.Name = "numericUpDownTimeOut";
            this.numericUpDownTimeOut.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownTimeOut.TabIndex = 6;
            this.numericUpDownTimeOut.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTimeOut.ValueChanged += new System.EventHandler(this.numericUpDownTimeOut_ValueChanged);
            // 
            // labelTimeOut
            // 
            this.labelTimeOut.Location = new System.Drawing.Point(181, 139);
            this.labelTimeOut.Name = "labelTimeOut";
            this.labelTimeOut.Size = new System.Drawing.Size(105, 21);
            this.labelTimeOut.TabIndex = 10;
            this.labelTimeOut.Text = "Time out (minutes) :";
            this.labelTimeOut.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStopServiceOnError
            // 
            this.lblStopServiceOnError.Location = new System.Drawing.Point(9, 139);
            this.lblStopServiceOnError.Name = "lblStopServiceOnError";
            this.lblStopServiceOnError.Size = new System.Drawing.Size(150, 21);
            this.lblStopServiceOnError.TabIndex = 4;
            this.lblStopServiceOnError.Text = "Stop service on printer error?";
            this.lblStopServiceOnError.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbStopServiceOnError
            // 
            this.cbStopServiceOnError.Location = new System.Drawing.Point(167, 137);
            this.cbStopServiceOnError.Name = "cbStopServiceOnError";
            this.cbStopServiceOnError.Size = new System.Drawing.Size(14, 24);
            this.cbStopServiceOnError.TabIndex = 5;
            this.cbStopServiceOnError.CheckedChanged += new System.EventHandler(this.cbStopServiceOnError_CheckedChanged);
            // 
            // cmbPrinters
            // 
            this.cmbPrinters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPrinters.Location = new System.Drawing.Point(105, 78);
            this.cmbPrinters.Name = "cmbPrinters";
            this.cmbPrinters.Size = new System.Drawing.Size(361, 21);
            this.cmbPrinters.TabIndex = 2;
            this.cmbPrinters.SelectionChangeCommitted += new System.EventHandler(this.cmbPrinters_SelectedChangeCommitted);
            // 
            // lblPrinter
            // 
            this.lblPrinter.Location = new System.Drawing.Point(9, 80);
            this.lblPrinter.Name = "lblPrinter";
            this.lblPrinter.Size = new System.Drawing.Size(91, 14);
            this.lblPrinter.TabIndex = 11;
            this.lblPrinter.Text = "Printer";
            // 
            // btnPDFFolder
            // 
            this.btnPDFFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPDFFolder.Location = new System.Drawing.Point(382, 46);
            this.btnPDFFolder.Name = "btnPDFFolder";
            this.btnPDFFolder.Size = new System.Drawing.Size(81, 24);
            this.btnPDFFolder.TabIndex = 1;
            this.btnPDFFolder.Text = "Browse";
            this.btnPDFFolder.Click += new System.EventHandler(this.btnPDFFolder_Click);
            // 
            // btnMonFolder
            // 
            this.btnMonFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMonFolder.Location = new System.Drawing.Point(382, 15);
            this.btnMonFolder.Name = "btnMonFolder";
            this.btnMonFolder.Size = new System.Drawing.Size(81, 24);
            this.btnMonFolder.TabIndex = 0;
            this.btnMonFolder.Text = "Browse";
            this.btnMonFolder.Click += new System.EventHandler(this.btnMonFolder_Click);
            // 
            // txtPDFFolder
            // 
            this.txtPDFFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPDFFolder.Location = new System.Drawing.Point(105, 47);
            this.txtPDFFolder.Name = "txtPDFFolder";
            this.txtPDFFolder.ReadOnly = true;
            this.txtPDFFolder.Size = new System.Drawing.Size(267, 20);
            this.txtPDFFolder.TabIndex = 10;
            this.txtPDFFolder.WordWrap = false;
            // 
            // txtMonFolder
            // 
            this.txtMonFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMonFolder.Location = new System.Drawing.Point(105, 16);
            this.txtMonFolder.Name = "txtMonFolder";
            this.txtMonFolder.ReadOnly = true;
            this.txtMonFolder.Size = new System.Drawing.Size(267, 20);
            this.txtMonFolder.TabIndex = 8;
            this.txtMonFolder.WordWrap = false;
            // 
            // lblPDFFolder
            // 
            this.lblPDFFolder.Location = new System.Drawing.Point(9, 49);
            this.lblPDFFolder.Name = "lblPDFFolder";
            this.lblPDFFolder.Size = new System.Drawing.Size(91, 14);
            this.lblPDFFolder.TabIndex = 9;
            this.lblPDFFolder.Text = "PDF Folder";
            // 
            // lblMonFolder
            // 
            this.lblMonFolder.Location = new System.Drawing.Point(9, 18);
            this.lblMonFolder.Name = "lblMonFolder";
            this.lblMonFolder.Size = new System.Drawing.Size(91, 14);
            this.lblMonFolder.TabIndex = 7;
            this.lblMonFolder.Text = "Monitored Folder";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(399, 206);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(81, 24);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(311, 206);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(81, 24);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Enabled = false;
            this.btnApply.Location = new System.Drawing.Point(223, 206);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(81, 24);
            this.btnApply.TabIndex = 8;
            this.btnApply.Text = "Apply";
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnPurge
            // 
            this.btnPurge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPurge.Enabled = false;
            this.btnPurge.Location = new System.Drawing.Point(135, 206);
            this.btnPurge.Name = "btnPurge";
            this.btnPurge.Size = new System.Drawing.Size(81, 24);
            this.btnPurge.TabIndex = 9;
            this.btnPurge.Text = "Purge All";
            this.btnPurge.Click += new System.EventHandler(this.btnPurge_Click);
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOpenFolder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnOpenFolder.Location = new System.Drawing.Point(14, 206);
            this.btnOpenFolder.Margin = new System.Windows.Forms.Padding(0);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(24, 24);
            this.btnOpenFolder.TabIndex = 10;
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.OpenFolder_Click);
            // 
            // FrmManager
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(487, 236);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnPurge);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.tabControlDisplay);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(364, 176);
            this.Name = "FrmManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.tabControlDisplay.ResumeLayout(false);
            this.tabFailedPrintsPage.ResumeLayout(false);
            this.tabPDFPage.ResumeLayout(false);
            this.tabPrintedPage.ResumeLayout(false);
            this.tabOptionsPage.ResumeLayout(false);
            this.tabOptionsPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeOut)).EndInit();
            this.ResumeLayout(false);

		}

		private void ShowHide(bool bVisible){
			if (bVisible)
			{
				this.Show();
			}
			else
			{
				this.Hide();
			}
		}


		#endregion

		bool IsServiceOrEngineStopped()
		{
			if (m_bRunAsService)
			{
				return IsWindowsServiceStopped();
			}
			else
			{
				return IsEngineStopped();
			}
		}

		private bool IsEngineStopped()
		{
			return !IsEngineStarted();
		}

		private bool IsEngineStarted()
		{
			return m_pfwEngine.IsStarted;
		}

		private bool IsWindowsServiceStopped()
		{
			bool isServiceStopped;
			using (ServiceController serviceController = new ServiceController(Utils.SERVICE_NAME))
			{
				serviceController.Refresh();
				isServiceStopped = serviceController.Status == ServiceControllerStatus.Stopped;
			}

			return isServiceStopped;
		}

		void RestartService()
		{
			try
			{
				using (ServiceController serviceController = new ServiceController(Utils.SERVICE_NAME))
				{
					serviceController.Refresh();
					if (serviceController.Status != ServiceControllerStatus.Stopped)
					{
						serviceController.Stop();
						serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
					}

					serviceController.Start();
				}
			}
			catch (Exception ex)
			{
				string error = "Warning: failed to restart Meticulus Print Folder Watcher Service.\n" + ex.Message;
				m_Utils.WriteEventLogEntry(error, EventLogEntryType.Warning, EVENT_LOG_SOURCE_MANAGER);
			}
		}

		#region Tab control functions
		private void DisplayOptions() {
			String strInstalledPrinters;
			int iCounter;

			this.btnPurge.Visible = false;
			
			this.btnApply.Text = "Apply";
			this.btnApply.Visible = false;

			this.txtMonFolder.Text = m_strPrintFolder;
			this.txtPDFFolder.Text = m_strPDFFolder;
			this.cbStopServiceOnError.Checked = m_bStopServiceOnError;
			this.numericUpDownTimeOut.Value = m_timeoutInMinutes;

			this.cmbPrinters.Items.Clear();
			for (iCounter = 0; iCounter < PrinterSettings.InstalledPrinters.Count; iCounter++){
				strInstalledPrinters = PrinterSettings.InstalledPrinters[iCounter];
				this.cmbPrinters.Items.Add(strInstalledPrinters);
			}

			this.cmbPrinters.SelectedItem = null;
			this.cmbPrinters.SelectedIndex = -1;
			this.cmbPrinters.SelectedText = "";
			this.cmbPrinters.SelectedValue = "";
			this.cmbPrinters.Text = m_strPrinterName;

			this.cmbPrintBothSides.Items.Clear();
			this.cmbPrintBothSides.Items.Add("Duplex Mode Not Supported");
			this.cmbPrintBothSides.Items.Add("Single Side");
			this.cmbPrintBothSides.Items.Add("Flip Pages Left to Right");
			this.cmbPrintBothSides.Items.Add("Flip Pages Up");

			this.cmbPrintBothSides.SelectedItem = null;
			this.cmbPrintBothSides.SelectedIndex = -1;
			this.cmbPrintBothSides.SelectedText = "";
			this.cmbPrintBothSides.SelectedValue = "";
			this.cmbPrintBothSides.Text = (string)this.cmbPrintBothSides.Items[m_duplexOption];

			this.btnApply.Enabled = OptionsHaveBeenModified();
			SetOptionsMode();
		}

		private void SetOptionsMode()
		{
			if (m_bInteractiveMode)
			{
				this.cmbPrinters.Enabled = false;
				this.cmbPrintBothSides.Enabled = false;
				this.cbStopServiceOnError.Enabled = false;
				this.numericUpDownTimeOut.Enabled = false;
			}
			else
			{
				this.cmbPrinters.Enabled = true;
				this.cmbPrintBothSides.Enabled = true;
				this.cbStopServiceOnError.Enabled = true;
				this.numericUpDownTimeOut.Enabled = true;
			}
		}

		private void DisplayPDFs() {
			this.btnPurge.Enabled = false;
			this.btnPurge.Visible = true;
			this.btnPurge.Text = "Purge";

			this.btnApply.Enabled = false;
			this.btnApply.Text = "Resubmit";
			this.btnApply.Visible = true;

			this.trvPDFs.Nodes.Clear();
			try {
				string[] straFiles = Directory.GetFiles(m_strPDFFolder, "*.pdf");
				foreach (string strFile in straFiles) {
					this.trvPDFs.Nodes.Add(Path.GetFileName(strFile));
				}
			} 
			catch (Exception e) {
				string strDelimiters = "\n";
				string [] strSplit = null;
				char [] caDelimiters = strDelimiters.ToCharArray();
				int iCounter;
				int iIndex;

				strSplit = e.Message.Split(caDelimiters, 5);
				iIndex = this.trvPDFs.Nodes.Add(new TreeNode("Following error occurred:"));
				for(iCounter=0;iCounter<strSplit.Length;iCounter++){
					this.trvPDFs.Nodes[iIndex].Nodes.Add(strSplit[iCounter]);
				}
			}
		}

		private void DisplayPrinted() {
			this.btnPurge.Enabled = false;
			this.btnPurge.Visible = true;
			this.btnPurge.Text = "Purge";
			
			this.btnApply.Enabled = false;
			this.btnApply.Visible = true;
			this.btnApply.Text = "Resubmit";

			this.trvPrinted.Nodes.Clear();

			if (!Directory.Exists(m_strPrintFolder + "\\Printed Documents"))
			{
				//Nothing printed yet...
				return;
			}
			
			try {
				string[] straFiles = Directory.GetFiles(m_strPrintFolder + "\\Printed Documents", "*");
				foreach (string strFile in straFiles) {
					this.trvPrinted.Nodes.Add(Path.GetFileName(strFile)); // + " (" + File.GetCreationTime(strFile).ToString() + ")");
				}
			} 
			catch (Exception e) {
				string strDelimiters = "\n";
				string [] strSplit = null;
				char [] caDelimiters = strDelimiters.ToCharArray();
				int iCounter;
				int iIndex;

				strSplit = e.Message.Split(caDelimiters, 5);
				iIndex = this.trvPrinted.Nodes.Add(new TreeNode("Following error occurred:"));
				for(iCounter=0;iCounter<strSplit.Length;iCounter++){
					this.trvPrinted.Nodes[iIndex].Nodes.Add(strSplit[iCounter]);
				}
			}
		}

		private void DisplayErrors() {
			string strDelimiters = "\n\t";
			string [] strSplit = null;
			char [] caDelimiters = strDelimiters.ToCharArray();
			int iCounter;

			this.trvFailedPrints.Nodes.Clear();
			
			foreach (EventLogEntry eleEntry in m_eventLog.Entries )
			{
				if (string.Compare(eleEntry.Source, Utils.EVENT_LOG_SOURCE, true) == 0)
				{
					if (eleEntry.EntryType == EventLogEntryType.Error)
					{
						strSplit = eleEntry.Message.Split(caDelimiters, 10);
						this.trvFailedPrints.Nodes.Insert(0, new TreeNode(strSplit[0]));
						this.trvFailedPrints.Nodes[0].Nodes.Add("Date: " + Convert.ToString(eleEntry.TimeGenerated));
						for(iCounter=1;iCounter<strSplit.Length;iCounter++)
						{
							// Add the lines to the tree view making sure not to add blank lines.
							string curLine = strSplit[iCounter].Trim();
							if (curLine.Length > 0) {
								this.trvFailedPrints.Nodes[0].Nodes.Add(curLine);
							}
						}
					}
				}
			}
			if (m_eventLog.Entries.Count > 0)
			{
				this.btnPurge.Enabled = true;
			}
			else
			{
				this.btnPurge.Enabled = false;
			}
			this.btnPurge.Text = "Purge All";
			this.btnPurge.Visible = true;

			this.btnApply.Enabled = false;
			this.btnApply.Text = "Resubmit";
			this.btnApply.Visible = true;
		}

		#endregion

		#region Message Handlers

		#region ContextMenu
		private void mitemExit_Click(object Sender, EventArgs e) 
		{
			this.Close();
		}

        private void mitemFailedJobs_Click(object Sender, EventArgs e) 
		{
			ShowHide(true);
			this.WindowState = FormWindowState.Normal;
			this.tabControlDisplay.SelectedIndex = FAILED_PRINTS;
			ClearErrorState();			
			DisplayErrors();
		}

		private void mitemPrintedJobs_Click(object Sender, EventArgs e) 
		{
			ShowHide(true);
			this.WindowState = FormWindowState.Normal;
			this.tabControlDisplay.SelectedIndex = PRINTED;
			ClearErrorState();			
			DisplayPrinted();
		}

		private void mitemPDFJobs_Click(object Sender, EventArgs e) 
		{
			ShowHide(true);
			this.WindowState = FormWindowState.Normal;
			this.tabControlDisplay.SelectedIndex = PDFS;
			ClearErrorState();			
			DisplayPDFs();
		}

		private void mitemConfigure_Click(object Sender, EventArgs e) 
		{
            ShowDialogAndOpenConfigPage();
			ClearErrorState();			
		}
        private void ShowDialogAndOpenConfigPage()
        {
			ShowHide(true);
			this.WindowState = FormWindowState.Normal;
			this.tabControlDisplay.SelectedIndex = OPTIONS;
        }

		private void mitemStartService_Click(object Sender, EventArgs e) 
		{
			StartServiceOrEngine();
		}

		private void StartServiceOrEngine()
		{
			if (m_bRunAsService)
			{
				StartWindowsService();
			}
			else
			{
				StartEngine();
			}
		}

		private void StartEngine()
		{
			m_pfwEngine.Start();
			SetServiceStarted();
		}

		private void StartWindowsService()
		{
			bool isServiceStopped = true;
			using (ServiceController serviceController = new ServiceController(Utils.SERVICE_NAME))
			{
				serviceController.Refresh();
				isServiceStopped = serviceController.Status == ServiceControllerStatus.Stopped;
				if (isServiceStopped)
				{
					try
					{
						serviceController.Start();

						Thread.Sleep(Utils.MILLISECONDS_SHORT_TIME_DELAY * 2);
							
						serviceController.Refresh();
						isServiceStopped = serviceController.Status == ServiceControllerStatus.Stopped;
					}
					catch (Exception ex) 
					{
						//Ignore any failure here. 
						string error = "Warning: failed to start Meticulus Print Folder Watcher Service.\n" + ex.Message;
						m_Utils.WriteEventLogEntry(error, EventLogEntryType.Warning, EVENT_LOG_SOURCE_MANAGER);
					}
				}

				if (!isServiceStopped)
				{
					SetServiceStarted();
				}
			}
		}

		private void AbortEngine()
		{
            if (m_pfwEngine != null)
            {
                m_pfwEngine.Abort();
                SetServiceStopped();
            }
		}

		private void mitemStopService_Click(object Sender, EventArgs e) 
		{
			if (Confirm("Stopping the Print Folder Watcher Service may cause problems printing digital forms.\n\nAre you sure you want to stop the service?"))
			{
				StopServiceOrEngine();
			}
		}

		private void StopServiceOrEngine()
		{
			if (m_bRunAsService)
			{
				StopWindowsService();
			}
			else
			{
				StopEngine();
			}
		}

		private void StopEngine()
		{
			try
			{
                if (m_pfwEngine != null)
                {
                    m_pfwEngine.Stop(false);
                    SetServiceStopped();
                }
			}
			catch(Exception)
			{
			}
		}

		private void StopWindowsService()
		{
			using (ServiceController serviceController = new ServiceController(Utils.SERVICE_NAME))
			{
				serviceController.Refresh();
				bool isServiceStopped = serviceController.Status == ServiceControllerStatus.Stopped;
				if (!isServiceStopped)
				{
					try
					{
						serviceController.Stop();

						Thread.Sleep(Utils.MILLISECONDS_SHORT_TIME_DELAY * 2);
						
						serviceController.Refresh();
						
						isServiceStopped = serviceController.Status == ServiceControllerStatus.Stopped;
					}
					catch (Exception ex) 
					{
						//Ignore any failure here.
						string error = "Warning: failed to stop Meticulus Print Folder Watcher Service.\n" + ex.Message;
						m_Utils.WriteEventLogEntry(error, EventLogEntryType.Warning, EVENT_LOG_SOURCE_MANAGER);
					}
				}

				if (isServiceStopped) 
				{
					SetServiceStopped();
				}
			}
		}
		#endregion

		#region System Messages
		protected override void WndProc(ref Message msg) {
			switch(msg.Msg){
                case WM_QUERYENDSESSION: //RTFM: This means System Shutdown OR User Logout!!!
					m_bSessionEnd = true;
					msg.Result = (IntPtr)1; // 1 means happy to allow session end
					break;
			} 
			base.WndProc(ref msg);
		}

		protected override void OnClosing(CancelEventArgs e) 
		{
			if (m_bSessionEnd == true)
			{
                AbortEngine();
				return;
			}

			mitemExit.Enabled = false;
			if (!Confirm("Exiting the Print Folder Watcher Manager may cause problems printing digital forms.\n\nAre you sure you want to exit?"))
			{
				e.Cancel = true;
				mitemExit.Enabled = true;
				return;
			}

			StopEngine();

			if (m_bInteractiveMode)
			{
				string message = "If a form is open in Adobe Reader, you should print it now and close Adobe Reader.\n\n" +
								  "                                             Click OK to exit.";
				MessageBox.Show(message, "Meticulus Print Folder Watcher Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			AbortEngine();
		}

		private void OnClickBalloon(object sender, EventArgs e)
		{
			bool showErrorTab = true;
			ShowDlg(showErrorTab);
			ClearErrorState();
		}

		private void OnDoubleClickIcon(object sender, EventArgs e)
		{
			ShowDlg(false);
		}

		private void ShowDlg(bool showErrorTab)
		{
			ShowHide(true);
			this.WindowState = FormWindowState.Normal;
			Activate();
			ShowHide(true);
			if (!m_bPrinterAdded && m_bRunAsService)
			{
				this.tabControlDisplay.SelectedIndex = OPTIONS;
			}
			else if(showErrorTab == true)
			{
				this.tabControlDisplay.SelectedIndex = FAILED_PRINTS;
				DisplayErrors();
			}
			else
			{
				this.tabControlDisplay.SelectedIndex = PDFS;
			}
		}

		private void tabControlDisplay_SelectedIndexChanged(object sender, System.EventArgs e) 
		{
			switch(this.tabControlDisplay.SelectedIndex){
				case FAILED_PRINTS:
					DisplayErrors();
					break;
				case PDFS:
					DisplayPDFs();
					break;
				case PRINTED:
					DisplayPrinted();
					break;
				case OPTIONS:
					DisplayOptions();
					break;
				default:
					break;
			}
		}
		#endregion

		#region Form buttons
		private void btnCancel_Click(object sender, System.EventArgs e) 
		{
			if (this.tabControlDisplay.SelectedIndex == OPTIONS)
			{
				ResetOptionsModifiedFlags();
				m_strPrintFolder = m_Utils.ReadPrintDirectory();
				m_strPDFFolder = m_Utils.ReadPDFDirectory();
				m_bStopServiceOnError = m_Utils.ReadStopServiceOnPrinterError();
				m_strPrinterName = this.ReadPrinterName();
                m_duplexOption = ReadPrintOnBothSides();
			}
			this.WindowState = FormWindowState.Minimized;
			ShowHide(false);
		}

		private void btnOK_Click(object sender, System.EventArgs e) 
		{
			if(this.tabControlDisplay.SelectedIndex == OPTIONS)
			{
				UpdateOptions();
			}
			this.WindowState = FormWindowState.Minimized;
			ShowHide(false);
		}

		private void btnPurge_Click(object sender, System.EventArgs e) 
		{
			if (this.tabControlDisplay.SelectedIndex == FAILED_PRINTS)
			{
				PurgeFailedJobs();
			}
			else if(this.tabControlDisplay.SelectedIndex == PDFS)
			{
				PurgePDFFiles();
			}
			else if(this.tabControlDisplay.SelectedIndex == PRINTED)
			{
				PurgePrintedJobs();
			}
		}

		private void btnApply_Click(object sender, System.EventArgs e) {
			if (this.tabControlDisplay.SelectedIndex == OPTIONS)
			{
				UpdateOptions();
			}
			else if(this.tabControlDisplay.SelectedIndex == FAILED_PRINTS)
			{
				PrintFailedDocument();
			}
			else if(this.tabControlDisplay.SelectedIndex == PDFS)
			{
				PrintPDF();
			}
			else if(this.tabControlDisplay.SelectedIndex == PRINTED)
			{
				ReprintDocument();
			}
		}
		#endregion

		#region Tree View Events
		private void trvFailedPrints_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e) 
		{
			if (IsExistingFile(e.Node.Text))
			{
				this.btnApply.Enabled = true;
			}
			else
			{
				this.btnApply.Enabled = false;
			}
		}

		private void trvPrinted_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e) {
			this.btnApply.Enabled = true;
			this.btnPurge.Enabled = true;
		}

		private void trvPDFs_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e) 
		{
			this.btnApply.Enabled = true;
			this.btnPurge.Enabled = true;
		}
		#endregion

		#region Options Tab
		private void btnMonFolder_Click(object sender, System.EventArgs e) {
			FolderBrowserDialog dlgBrowse;

			dlgBrowse = new FolderBrowserDialog();

			dlgBrowse.Description = "Browse for Monitoring Folder";
			dlgBrowse.SelectedPath = m_strPrintFolder;
			dlgBrowse.ShowNewFolderButton = false;

			if (dlgBrowse.ShowDialog() == DialogResult.OK){
				m_strPrintFolder = dlgBrowse.SelectedPath;
				this.txtMonFolder.Text = m_strPrintFolder;
				this.btnApply.Enabled = true;
				m_bPrintFolderModified = true;
			}
		}

		private void btnPDFFolder_Click(object sender, System.EventArgs e) {
			FolderBrowserDialog dlgBrowse;

			dlgBrowse = new FolderBrowserDialog();

			dlgBrowse.Description = "Browse for Monitoring Folder";
			dlgBrowse.SelectedPath = m_strPDFFolder;
			dlgBrowse.ShowNewFolderButton = false;

			if (dlgBrowse.ShowDialog() == DialogResult.OK){
				m_strPDFFolder = dlgBrowse.SelectedPath;
				this.txtPDFFolder.Text = m_strPDFFolder;
				this.btnApply.Enabled = true;
				m_bPdfFolderModified = true;
			}
		}

		private void SetPrinter(string strPrinterName) 
		{
			//In interactive mode:
			//	a) we are not interested in setting the printer since the user does this in Adobe
			//	b) the user may not have admin rights to set the system printer.
			if (m_bInteractiveMode)
				return;

			m_Utils.MakeSystemPrinter(strPrinterName);
			m_bPrinterAdded = true;
			m_Utils.WritePrinterAdded(m_bPrinterAdded);
		}

		private void cmbPrinters_SelectedChangeCommitted(object sender, System.EventArgs e)
		{
			m_strPrinterName = this.cmbPrinters.SelectedItem.ToString();
			this.btnApply.Enabled = true;
			m_bPrinterModified = true;
		}

		private void cmbPrintBothSides_SelectedChangeCommitted(object sender, System.EventArgs e)
		{
			m_duplexOption = this.cmbPrintBothSides.SelectedIndex;
		}

		private void cbStopServiceOnError_CheckedChanged(object sender, System.EventArgs e)
		{
			m_bStopServiceOnError = cbStopServiceOnError.Checked;		
			this.btnApply.Enabled = true;
			m_bStopServiceOnErrorModified = true;
		}	
		#endregion

		#endregion

		#region Print Documents/Apply
		private void PrintPDF()
		{
			if (IsServiceOrEngineStopped())
			{
				MessageBox.Show("Please start the Meticulus Print Service first.", "Meticulus Print Folder Watcher Manager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			ArrayList alSelected;
			string strOrigin;
			string strDestination;

			try{
				alSelected = this.trvPDFs.SelectedNodes;
				foreach ( TreeNode tnCountNode in alSelected ){
					strOrigin = m_strPDFFolder+ "\\" + tnCountNode.Text;
					strDestination = m_strPrintFolder + "\\" + tnCountNode.Text;

					File.Copy(strOrigin, strDestination, true);
				}
			}
			catch(IOException exError){
				MessageBox.Show(exError.Message,"Meticulus Print Folder Watcher Manager",MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void ReprintDocument(){
			if (IsServiceOrEngineStopped())
			{
				MessageBox.Show("Please start the Meticulus Print Service first.", "Meticulus Print Folder Watcher Manager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				ArrayList alSelected = this.trvPrinted.SelectedNodes;
				foreach ( TreeNode tnCountNode in alSelected )
				{
					string strOrigin = m_strPrintFolder+ "\\Printed Documents\\" + tnCountNode.Text;
					string strDestination = m_strPrintFolder + "\\" + tnCountNode.Text;

					File.Copy(strOrigin, strDestination, true);
				}
			}
			catch(Exception exError)
			{
				MessageBox.Show(exError.Message,"Meticulus Print Folder Watcher Manager",MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void PrintFailedDocument(){
			if (IsServiceOrEngineStopped())
			{
				MessageBox.Show("Please start the Meticulus Print Service first.", "Meticulus Print Folder Watcher Manager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				TreeNode tnCountNode = this.trvFailedPrints.SelectedNode;
				if (IsExistingFile(tnCountNode.Text))
				{
					string strOrigin = m_strPrintFolder+ "\\Failed Documents\\" + tnCountNode.Text;
					string strDestination = m_strPrintFolder + "\\" + tnCountNode.Text;

					//Note: User may click Print on same node more than once.
					File.Move(strOrigin, strDestination);
				}
			}
			catch(Exception exError)
			{
				MessageBox.Show(exError.Message,"Meticulus Print Folder Watcher Manager",MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private bool IsExistingFile(string text)
		{
			bool fileExists = false;
			string strOrigin = m_strPrintFolder+ "\\Failed Documents\\" + text;
			try
			{
				fileExists = File.Exists(strOrigin);
			}
			catch(Exception)
			{
			}
			return fileExists;
		}

		private void UpdateOptions(){
			// If PrintFolder or PDFFolder or Stop service on error flag have been changed the service needs to be restarted  
			// because these values are read from the regisrty when the service is started and cached thereafter. 
			bool bRestartService = false;
			if (m_bPdfFolderModified || m_bPrintFolderModified) 
			{
				bRestartService = true;
			}

			if (m_strPrintFolder != "") 
				{
				m_Utils.WritePrintDirectory(m_strPrintFolder);
				m_bPdfFolderModified = false;
			}

			if (m_strPDFFolder != "") {
				m_Utils.WritePDFDirectory(m_strPDFFolder);
				m_bPrintFolderModified = false;
			}

			if (m_strPrinterName != "") {
				SetPrinter(m_strPrinterName);
				m_bPrinterModified = false;
			}

			m_Utils.WriteStopServiceOnPrinterError(m_bStopServiceOnError);
			m_bStopServiceOnErrorModified = false;

			//Always update the timeout value here, since we cannot detect Value Changed events on the text field itself.
			m_timeoutInMinutes = (int)numericUpDownTimeOut.Value;
			m_Utils.WriteTimeOut(m_timeoutInMinutes);

			m_duplexOption = this.cmbPrintBothSides.SelectedIndex;
			m_Utils.WritePrintOnBothSides(m_duplexOption);

			if (bRestartService) 
			{
				RestartService();
			}

			this.btnApply.Enabled = false;
		}

		#endregion

		#region Purge functions
		private void PurgeFailedJobs()
		{
			if (Confirm("Purging all the failed jobs will permanently delete all the files from the failed jobs folder. \n\nDo you want to continue with the purge?"))
			{
				PurgeFailedFiles();
				PurgeEventLog();
				this.trvFailedPrints.Nodes.Clear();
				this.btnPurge.Enabled = false;
				this.btnApply.Enabled = false;
				ClearErrorState();
			}
		}

		private void PurgeEventLog()
		{
			try
			{
				if (m_eventLog != null)
				{
					m_eventLog.Clear();
				}
			}
			catch(Exception exError)
			{
				MessageBox.Show(exError.Message,"Meticulus Print Folder Watcher Manager",MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void PurgeFailedFiles()
		{
			try
			{
				TreeNodeCollection alNodes = this.trvFailedPrints.Nodes;
				for (int i = 0; i < alNodes.Count; i++)
				{
					TreeNode tnCountNode = (TreeNode)alNodes[i];
					string strLocation = m_strPrintFolder + "\\Failed Documents\\" + tnCountNode.Text;
					if (File.Exists(strLocation))
					{
						File.Delete(strLocation);
                        FileManifest.PurgeMatchingManifestFile(m_strPrintFolder, strLocation);
					}
				}
			}
			catch(Exception exError)
			{
				MessageBox.Show(exError.Message,"Meticulus Print Folder Watcher Manager",MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

			//Refresh display.
			DisplayErrors();
		}

		private void PurgePrintedJobs()
		{
			if (Confirm("Purging the selected printed jobs will permanently delete all the selected files. \n\nDo you want to continue with the purge?"))
			{
				ArrayList alSelected;
				string strLocation;

				try{
					alSelected = this.trvPrinted.SelectedNodes;
					for (int i = 0; i < alSelected.Count; i++)
					{
						TreeNode tnCountNode = (TreeNode)alSelected[i];
						strLocation = m_strPrintFolder+ "\\Printed Documents\\" + tnCountNode.Text;
						File.Delete(strLocation);
                        FileManifest.PurgeMatchingManifestFile(m_strPrintFolder, strLocation);
					}
				}
				catch(IOException exError){
					MessageBox.Show(exError.Message,"Meticulus Print Folder Watcher Manager",MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}

				DisplayPrinted();
			}
		}

		private void PurgePDFFiles(){
			if (Confirm("Purging the selected PDF files will permanently delete all the selected files. \n\nDo you want to continue with the purge?"))
			{
				try
				{
					ArrayList alSelected = this.trvPDFs.SelectedNodes;
					for (int i = 0; i < alSelected.Count; i++)
					{
						TreeNode tnCountNode = (TreeNode)alSelected[i];
						string fullPath = m_strPDFFolder+ "\\" + tnCountNode.Text;

						FileAttributes fileAttributes = File.GetAttributes(fullPath);
						if ((fileAttributes & FileAttributes.ReadOnly) != 0)
						{
							fileAttributes = fileAttributes & ~FileAttributes.ReadOnly;
							File.SetAttributes(fullPath, fileAttributes);
						}

						File.Delete(fullPath);
                        FileManifest.PurgeMatchingManifestFile(m_strPrintFolder, fullPath);
					}
				}
				catch(IOException exError)
				{
					MessageBox.Show(exError.Message,"Meticulus Print Folder Watcher Manager",MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}

				DisplayPDFs();
			}
		}
		#endregion

		private void ShowPrinterWasNotSet()
        {
            // If the user has not specified a printer yet then we will have tried to set  
            // up the default printer so:
            // 1) Let the user know what printer was selected.
            // 2) Show the config dialog so that they can check and change it if necessary.
            // 3) Make sure that we stop on an error because the default printer that we may have selected
            //    may not be correct.
            m_bStopServiceOnError = true;
            m_Utils.WriteStopServiceOnPrinterError(m_bStopServiceOnError);

            string strPrinterName = m_Utils.ReadMeticulusPrinterName();
            string msg;
            if (m_Utils.IsPrinterInitialized(strPrinterName))
            {
                msg = string.Format("The {0} has selected the following printer.\n\n{1}", Utils.SERVICE_NAME, strPrinterName);
            }
            else
            {
                msg = string.Format("The {0} could not select a printer.", Utils.SERVICE_NAME);
            }

            DisplayInfoBalloon(msg);
            ShowDialogAndOpenConfigPage();
        }

		private void ResetOptionsModifiedFlags() 
		{
			m_bPdfFolderModified = false;
			m_bPrintFolderModified = false;
			m_bPrinterModified = false;
			m_bStopServiceOnErrorModified = false;
		}

		private bool OptionsHaveBeenModified() 
		{
			return (m_bPdfFolderModified || m_bPrintFolderModified || m_bPrinterModified || m_bStopServiceOnErrorModified);
		}

		private string ReadPrinterName()
		{
            string strPrinterName = m_Utils.ReadPrinterName(null);// null=> no specific file manifest at this point
			if (strPrinterName != "")
			{
				SetPrinter(strPrinterName);
				return strPrinterName;
			}
			else
			{
				return "Select Printer";
			}
		}	

		private int ReadPrintOnBothSides()
		{
            int duplexOption = m_Utils.ReadPrintOnBothSides(null);// null=> no specific file manifest at this point
			cmbPrintBothSides.SelectedIndex = duplexOption;
			return duplexOption;
		}	

		private bool Confirm(string msg)
		{
			return MessageBox.Show(msg, "Meticulus Print Folder Watcher Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
		}

		private void numericUpDownTimeOut_ValueChanged(object sender, System.EventArgs e)
		{
			m_timeoutInMinutes = (int)numericUpDownTimeOut.Value;
			this.btnApply.Enabled = true;
		}

		private void OnEventWritten(object source, EntryWrittenEventArgs e) {
			EventLogEntry eleEntry = e.Entry;
		
			if (eleEntry.Source != Utils.EVENT_LOG_SOURCE)
				return;

			if (eleEntry.EntryType == EventLogEntryType.Error)
			{
				if (eleEntry.Category != "(null)" || m_bInteractiveMode)
				{		
					HandleError();
				}
			}
			else if (eleEntry.Message == Utils.SERVICE_STARTED)
			{
				SetServiceStarted();
			}
			else if (eleEntry.Message == Utils.SERVICE_STOPPED)
			{
				SetServiceStopped();
			}
		}

		private void DisplayErrorBalloon(string error)
		{
			niconNotifyBalloon.ShowBalloon("Meticulus FAS", error, clsNotifyBalloon.NotifyInfoFlags.Error);
		}
        private void DisplayInfoBalloon(string msg)
        {
            niconNotifyBalloon.ShowBalloon("Meticulus FAS", msg, clsNotifyBalloon.NotifyInfoFlags.Info);
        }

		private void HandleError()
		{
			string error = "A printing error has occurred.\n\nClick here for more details.";
			DisplayErrorBalloon(error);

			m_dtLastTimeRun = DateTime.Now;
			m_Utils.WriteLastPollDate(m_dtLastTimeRun);

			SetErrorState();
		}

		private void SetErrorState()
		{
			bool isServiceStopped = IsServiceOrEngineStopped();
			if (isServiceStopped) 
			{
				SetServiceStopped();
			}	
			else 
			{
				SetServiceError();
			}
		}

		private void ClearErrorState()
		{
			if (IsServiceOrEngineStopped())
			{
				SetServiceStopped();
			}
			else
			{
				SetServiceStarted();
			}
		}

		private void SetServiceStopped()
		{
			niconNotifyBalloon.Icon = m_icServiceStopped;
			niconNotifyBalloon.Text = "Meticulus Print Service Stopped";

			EnableStartServiceMenu();
		}

		private void SetServiceError()
		{
			niconNotifyBalloon.Icon = m_icError;
			niconNotifyBalloon.Text = "Meticulus Print Service Error";

			DisableStartServiceMenu();
		}

		private void SetServiceStarted()
		{
			niconNotifyBalloon.Icon = m_icAllFine;
			niconNotifyBalloon.Text = "Meticulus Print Folder Watcher Manager";

			DisableStartServiceMenu();
		}

		private void EnableStartServiceMenu()
		{
			mitemStartService.Enabled = true;
			mitemStopService.Enabled = false;
		}

		private void DisableStartServiceMenu()
		{
			mitemStartService.Enabled = false;
			mitemStopService.Enabled = true;
		}

		private void CheckPastErrorEvents() 
		{
			bool bErrorFound = false;
			for (int iCounter = m_eventLog.Entries.Count - 1; iCounter >= 0 && !bErrorFound; iCounter--)
			{
				EventLogEntry eleEntry = m_eventLog.Entries[iCounter];
				if (DateTime.Compare(eleEntry.TimeWritten, m_dtLastTimeRun) > 0)
				{
					if (eleEntry.Source == Utils.EVENT_LOG_SOURCE && eleEntry.EntryType == EventLogEntryType.Error)
					{
						bErrorFound = true;
						HandleError();
					}
				}
			}
		}

        private void OpenFolder_Click(object sender, EventArgs e)
        {
            string path = "";
            try
            {
                switch (this.tabControlDisplay.SelectedIndex)
                {
                    case FAILED_PRINTS:
                        path = m_strPrintFolder + "\\Failed Documents";
                        break;
                    case PDFS:
                        path = m_strPDFFolder;
                        break;
                    case PRINTED:
                        path = m_strPrintFolder + "\\Printed Documents";
                        break;
                    default:
                        path = m_strPrintFolder;
                        break;
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                System.Diagnostics.Process.Start(path);
            }
            catch (Exception ex)
            {
                string message = "Error opening folder.\n\n" + path + "\n\n" + ex.Message;
                MessageBox.Show(message, "Unable to open folder.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        [DllImport("Shell32.dll")]
        public extern static int ExtractIconEx( string libName, int iconIndex,IntPtr[] largeIcon, IntPtr[] smallIcon, int nIcons );
        private void InitIcons()
        {
            // Use icons in shell32.dll, extract only the open folder one for now.
            int numIcons = 1;

            IntPtr[] largeIcon = new IntPtr[numIcons];
            IntPtr[] smallIcon = new IntPtr[numIcons];

            ExtractIconEx("shell32.dll", 4, largeIcon, smallIcon, numIcons);
            Icon smallIco = Icon.FromHandle(smallIcon[0]);

            btnOpenFolder.Image = smallIco.ToBitmap();
        }
	}
}
