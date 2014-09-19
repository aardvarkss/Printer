using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

using Print_Folder_Watcher_Common;

namespace Print_Folder_Watcher_Service
{
	/// <summary>
	/// Summary description for PFWServiceInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class PFWServiceInstaller : System.Configuration.Install.Installer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PFWServiceInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();

			ServiceProcessInstaller process = new ServiceProcessInstaller();

			process.Account = ServiceAccount.LocalSystem;

			ServiceInstaller serviceAdmin = new ServiceInstaller();

			serviceAdmin.StartType = ServiceStartMode.Automatic;
			serviceAdmin.ServiceName = Utils.SERVICE_NAME;
			serviceAdmin.ServicesDependedOn = new string[] {"Print Spooler"};

			Installers.Add( process );
			Installers.Add( serviceAdmin );
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
