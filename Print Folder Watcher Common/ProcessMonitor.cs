using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Print_Folder_Watcher_Common
{
	/// <summary>
	/// ProcessMonitor - Used to find errant processes left over after print jobs fail
	/// </summary>
	public class ProcessMonitor
	{
		ArrayList	processesToWatch = null;
		Hashtable	originalProcessList = new Hashtable();

		public ProcessMonitor(ArrayList toWatch)
		{
			processesToWatch = new ArrayList(toWatch);
			foreach (string processName in processesToWatch)
			{
				GetOriginalProcessList(processName);
			}
		}

		public ProcessMonitor(string processName)
		{
			GetOriginalProcessList(processName);
		}

		private void GetOriginalProcessList(string processName)
		{
			Process[] processArray = null;
			try
			{
				processArray = Process.GetProcessesByName(processName);
				foreach (Process process in processArray)
				{
					originalProcessList.Add(process.Id, process.ProcessName);
				}
			}
			finally
			{
				foreach (Process process in processArray)
				{
					process.Close();
					process.Dispose();
				}
			}
		}

		public bool IsProcessRunning(string processName)
		{
			int count = 0;
			foreach (string name in originalProcessList.Values)
			{
				if (string.Compare(processName, name, true) == 0)
				{
					count++;
					if (count == 2)
					{
						return true;
					}
				}
			}

			return false;
		}

		// Kill off any processes that have started since we called the constructor that
		// match the names of the watched processes.
		// TODO: Only kill processes owned by the same user as us.
		//
		// Throws on error situations so you'd better handle them from your calling code.
		public void KillNewWatchedProcesses()
		{
			foreach (string processName in processesToWatch)
			{
				KillNewWatchedProcesses(processName);
			}
		}

		public void KillNewWatchedProcesses(string processName)
		{
			Process[] processArray = null;
			try
			{
				processArray = Process.GetProcessesByName(processName);
				foreach (Process process in processArray)
				{
					if (!originalProcessList.ContainsKey(process.Id))
					{
						process.Kill();
					}
				}
			}
			finally
			{
				foreach (Process process in processArray)
				{
					process.Close();
					process.Dispose();
				}
			}
		}
	}
}
