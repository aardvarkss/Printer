using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace Print_Folder_Watcher_Common
{
    public class TempFilesManager
    {
        private const string PURGE_TMP_FILES_REG_KEY = "Software\\Meticulus\\Remote Printing\\PurgeTmpFiles";

        private clsUtils m_Utils;

        private List<string> Folders { get; set; }
        private List<string> FileTypes { get; set; }

        private int DeleteFilesOlderThanMinutes { get; set; }
        public int FrequencyMinutes { get; private set; }
        private bool RecurseSubFolders { get; set; }
        private bool LogAllEvents { get; set; }

        public TempFilesManager()
        {
            this.m_Utils = new clsUtils();
            SetDefaults();
            ReadRegistry();
        }
        private void SetDefaults()
        {
            // Setup the defaults
            DeleteFilesOlderThanMinutes = 10;
            FrequencyMinutes = 10;
            RecurseSubFolders = false;
            LogAllEvents = false;

            FileTypes = new List<string>();
            FileTypes.Add("Z@*");
            FileTypes.Add("acr*");
            FileTypes.Add("*.pdf");
            FileTypes.Add("*.xfdf");

            Folders = new List<string>();
            Folders.Add(Environment.GetEnvironmentVariable("SystemRoot") + "\\Temp");
        }
        public void PurgeAll()
        {
            int numberDeleted = 0;
            int numberFound = 0;

            DateTime startedAt = DateTime.Now;
            foreach (string folder in Folders)
            {
                DeleteFilesInFolder(folder, ref numberDeleted, ref numberFound);
            }

            if (LogAllEvents)
            {
                string msg = string.Format("{0}\nDeleted ({1}) of ({2}) files.\nTime taken ({3}).",
                                                            this.GetType(),
                                                            numberDeleted,
                                                            numberFound,
                                                            DateTime.Now.Subtract(startedAt));

                msg += "\n\nFileTypes:\n";
                foreach (string item in FileTypes)
                {
                    msg += "  " + item + "\n";
                }

                msg += "\nFolders:\n";
                foreach (string item in Folders)
                {
                    msg += "  " + item + "\n";
                }

                m_Utils.WriteEventLogEntry(msg, EventLogEntryType.Information, Utils.EVENT_LOG_SOURCE);
            }
        }
        private void DeleteFilesInFolder(string path, ref int numberDeleted, ref int numberFound)
        {
            List<string> matchingFiles = GetFiles(path);
            if (matchingFiles != null)
            {
                foreach (string fileToDelete in matchingFiles)
                {
                    //Delete the file if it is old enough
                    if (File.GetCreationTime(fileToDelete).AddMinutes(DeleteFilesOlderThanMinutes) < DateTime.Now)
                    {
                        try
                        {
                            numberFound++;
                            File.Delete(fileToDelete);
                            numberDeleted++;
                        }
                        catch (Exception ex)
                        {
                            //Ignore any failures and dont log because it can create a lot of events.
                            if (LogAllEvents)
                            {
                                string msg = this.GetType() + " failed deleting (" + fileToDelete + ")\n\n" + ex.Message;
                                m_Utils.WriteEventLogEntry(msg, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
                            }
                        }
                    }
                }                
            }

        }
        private List<string> GetFiles(string path)
        {
            List<string> matchingFiles = null;
            try
            {
                matchingFiles = new List<string>();
                foreach (string pattern in FileTypes)
                {
                    SearchOption searchOption = RecurseSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    matchingFiles.AddRange(Directory.GetFiles(path, pattern, searchOption));
                }
            }
            catch (Exception ex)
            {
                //Ignore any failures and dont log because it can create a lot of events.
                if (LogAllEvents)
                {
                    string msg = this.GetType() + " failed building file list in (" + path + ")\n\n" + ex.Message;
                    m_Utils.WriteEventLogEntry(msg, EventLogEntryType.Error, Utils.EVENT_LOG_SOURCE);
                }
            }

            return matchingFiles;
        }
        private void ReadRegistry()
        {
            LogAllEvents = m_Utils.ReadLogAllEvents();

            RecurseSubFolders = m_Utils.ReadRegistryValue(PURGE_TMP_FILES_REG_KEY, "RecurseSubFolders", RecurseSubFolders);
            FrequencyMinutes = m_Utils.ReadRegistryValue(PURGE_TMP_FILES_REG_KEY, "FrequencyMinutes", FrequencyMinutes);
            DeleteFilesOlderThanMinutes = m_Utils.ReadRegistryValue(PURGE_TMP_FILES_REG_KEY, "DeleteFilesOlderThanMinutes", DeleteFilesOlderThanMinutes);
            FileTypes = m_Utils.ReadRegistryValue(PURGE_TMP_FILES_REG_KEY, "FileTypes", FileTypes, ';');

            //NOTE: Never allow users to override the default hard coded value because 
            //      it is too dangerous to allow users to possibly delete the entire c dirve.
            //Folders = m_Utils.ReadRegistryValue(PURGE_TMP_FILES_REG_KEY, "Folders", Folders, ';');
        }
    }
}
