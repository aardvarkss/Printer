using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Print_Folder_Watcher_Engine
{
    class CmdLine
    {
        private string path;
        private string name;
        private string arguments;
        private bool successExitCodeSpecified;
        private int successExitCode;
        
        private bool runAsFireAndForget;

        public CmdLine(string path, string name, string arguments, bool successExitCodeSpecified, int successExitCode)
        {
            this.path = path;
            this.name = name;
            this.arguments = arguments;

            this.successExitCodeSpecified = successExitCodeSpecified;
            this.successExitCode = successExitCode;

            //TODO: Look for dpp_print.exe in cmd if/when we support Xml controlling printer selection for CPAT etc.
            runAsFireAndForget = true;
        }

        public bool RunAsFireAndForget
        {
            get
            {
                return runAsFireAndForget;
            }
        }

        public string ExecutableFullName
        {
            get
            {
                return path + Path.DirectorySeparatorChar + name;
            }
        }

        public string Arguments
        {
            get
            {
                return arguments;
            }
        }

        public bool SuccessExitCodeSpecified
        {
            get
            {
                return successExitCodeSpecified;
            }
        }
        public int SuccessExitCode
        {
            get { return successExitCode; }
        }
    }
}
