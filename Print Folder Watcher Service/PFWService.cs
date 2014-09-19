using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.IO;
using System.Xml;

using Print_Folder_Watcher_Service;
using Print_Folder_Watcher_Common;
using Print_Folder_Watcher_Engine;

namespace Print_Folder_Watcher_Service
{
	public class PFWService : System.ServiceProcess.ServiceBase
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private PFWEngine m_pfwEngine = null;
		private clsUtils  m_utils;
		
		// The main entry point for the process
		static void Main() 
		{
			System.ServiceProcess.ServiceBase[] ServicesToRun;
	
			ServicesToRun = new System.ServiceProcess.ServiceBase[] { new PFWService() };

			System.ServiceProcess.ServiceBase.Run(ServicesToRun);
		}

		public PFWService()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();

			CanShutdown = true;

			m_utils = new clsUtils();
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ServiceName = "";
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing ){
				if (components != null)
				{
					components.Dispose();
				}
			}

			DoDispose();
			base.Dispose( disposing );
		}

		private void DoDispose()
		{
			if (m_pfwEngine != null)
			{
				m_pfwEngine.Dispose();//TODO Should inherit fom IDispose.
				m_pfwEngine = null;
			}
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			try
			{
				if (m_pfwEngine == null)
				{
					m_pfwEngine = new PFWEngine();
				}

				m_pfwEngine.Start();

				base.OnStart(args);
			}
			catch(Exception e)
			{
				string strMessage = string.Format("Unexpected error.\n    {0}\n", e.Message);
				if (m_utils != null)
				{
					m_utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
				}
			}
		}

		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			try
			{
				if (m_pfwEngine != null)
				{
					m_pfwEngine.Stop(true);
				}

				base.OnStop();
			}
			catch(Exception e)
			{
				string strMessage = string.Format("Unexpected error.\n    {0}\n    {1}", e.Message, e.StackTrace);
				if (m_utils != null)
				{
					m_utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
				}
			}
		}

		protected override void OnShutdown()
		{
			OnStop();
		}
	}
}
