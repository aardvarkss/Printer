using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Print_Folder_Watcher_Engine
{
    class CmdLineParser
    {
        private const string FILENAME = "_FILENAME_";
        private const string PRINTER = "_PRINTER_";
        private const string DUPLEX_MODE = "_DUPLEX_MODE_";

        private XmlDocument cmdLineAsXml;
        private XmlNode cmdLineNode;
        public CmdLineParser(string cmdLineString)
        {
            cmdLineAsXml = new XmlDocument();
            using (StringReader stringReader = new StringReader(cmdLineString))
            {
                cmdLineAsXml.Load(stringReader);
            }

            cmdLineNode = cmdLineAsXml.SelectSingleNode("/cmdLine");
            if (cmdLineNode == null)
            {
                throw new ApplicationException("cmdLine node not found");
            }
        }

        public CmdLine Parse(string fullPath, string printerName, short duplexMode)
        {
            int successExitCode;
            bool successExitCodeSpecified = SuccessExitCode(out successExitCode);
            string arguments = Arguments(fullPath, printerName, duplexMode);
            CmdLine cmdLine = new CmdLine(Path, Name, arguments, successExitCodeSpecified, successExitCode);
            return cmdLine;
        }

        private string Path
        {
            get
            {
                return cmdLineNode.Attributes["path"].Value;
            }
        }

        private string Name
        {
            get
            {
            return cmdLineNode.Attributes["name"].Value;
            }
        }

        /// <summary>
        /// Parse Xml command definition which will be in a format such as this:
        /// <cmdLine path='c:\Program Files\Meticulus\Print Folder Watcher\' name='dpp_print.exe'>
        /// <argList>
        /// <arg name='/f' value='_FILENAME_' />
        /// <arg name='/p' value='_PRINTER_' />
        /// <arg name='/du' value='_DUPLEX_MODE_' />
        /// </argList>
        /// <successExitCode value='0' />
        /// </cmdLine>
        /// 
        /// If no command line switch is required can specify 
        /// <arg name='' value='_FILENAME_' /> 
        /// or conversely
        /// <arg name='/x' value='' /> 
        /// 
        /// _FILENAME_, _PRINTER_, and _DUPLEX_MODE_ are variables which are substituted at runtime for the current values.
        /// 
        /// Optionally can specify the success exit code for the process, if known. This must be an integer value.
        /// If specified the PFW manager will check the exitcode and treat the job as succeeded or failed accordingly.
        /// If no successExitCode is specified then all jobs are regarded as succeeding.
        /// </summary>
        private string Arguments(string fullPath, string printerName, short duplexMode)
        {
            StringBuilder arguments = new StringBuilder();
            XmlNodeList argList = cmdLineAsXml.SelectNodes("/cmdLine/argList/arg");
            foreach (XmlNode argNode in argList)
            {
                arguments.Append(argNode.Attributes["name"].Value);
                arguments.Append(" ");

                string value = argNode.Attributes["value"].Value;
                switch (value)
                {
                    case FILENAME:
                        value = '"' + fullPath + '"';
                        break;
                    case PRINTER:
                        value = '"' + printerName + '"';
                        break;
                    case DUPLEX_MODE:
                        value = duplexMode.ToString();
                        break;
                    default:
                        //used value from Xml
                        break;
                }
                arguments.Append(value);
                arguments.Append(" ");
            }
            return arguments.ToString();
        }

        private bool SuccessExitCode(out int successExitCode)
        {
            bool successExitCodeSpecified = false;
            successExitCode = 0;
            XmlNode node = cmdLineAsXml.SelectSingleNode("/cmdLine/successExitCode");
            if (node != null)
            {
                string value = node.Attributes["value"].Value;
                successExitCode = int.Parse(value);
                successExitCodeSpecified = true;
            }
            return successExitCodeSpecified;
        }

    }
}
