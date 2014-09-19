using System.IO;
using System.Xml;
using System;

namespace Print_Folder_Watcher_Common
{
    /// <summary>
    /// Note Manifest files are left in the "Print Manifest Documents" folder until they are purged
    /// so that any retry attempt, ie moving of pdf or fdf back into the print folder, swill still 
    /// have access to it. These files are purged when the pdf/xfdf etc i purged by the user.
    /// </summary>
    public class FileManifest
    {
        private static readonly string MANIFEST_FOLDER_NAME = "Print Manifest Documents";
        private static readonly string DEFAULT_PRINTER = "DefaultPrinter";

        public string PrinterName { get; private set; }
        public short DuplexMode { get; private set; }

        public FileManifest(string pfwPrintFolder, string fileFullPath)
        {
            // Set defaults
            PrinterName = null; // => Use printer defined in PFW settings
            DuplexMode = -1;    // => Use duplex settings defined in PFW settings

            // Read the manifest if there is one.
            ReadManifest(pfwPrintFolder, fileFullPath);
        }
        private void ReadManifest(string pfwPrintFolder, string fileFullPath)
        {
            if (!string.IsNullOrEmpty(fileFullPath))
            {
                string manifestPath = GetManifestFileFullPath(pfwPrintFolder, fileFullPath);

                if (File.Exists(manifestPath))
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(manifestPath);

                    XmlNode printerNode = xmlDocument.SelectSingleNode("/file/printer");
                    if (printerNode != null)
                    {
                        ReadPrinterName(printerNode);
                        ReadDuplexMode(printerNode, fileFullPath, manifestPath);
                    }
                }
            }
        }

        private void ReadDuplexMode(XmlNode printerNode, string fileFullPath, string manifestPath)
        {
            XmlNode attrNode = printerNode.Attributes["duplexMode"];
            if (attrNode != null)
            {
                string strDuplexMode = attrNode.InnerText;
                if (strDuplexMode.Length > 0)
                {
                    try
                    {
                        DuplexMode = short.Parse(strDuplexMode);
                    }
                    catch (Exception ex)
                    {
                        string msg = string.Format("Invalid DuplexMode ({0}) reading manifest.\n  file={1}\n  manifest={2}",
                                                    strDuplexMode, fileFullPath, manifestPath);
                        msg += "\n\n" + ex.Message;
                        throw new Exception(msg); ;
                    }
                }
            }

        }

        private void ReadPrinterName(XmlNode printerNode)
        {
            XmlNode attrNode = printerNode.Attributes["name"];
            if (attrNode != null)
            {
                string printerName = attrNode.InnerText;
                PrinterName = (printerName == DEFAULT_PRINTER) ? null : printerName;
            }
        }

        private static string GetManifestFileFullPath(string pfwPrintFolder, string fileFullPath)
        {
            string manifestName = Path.GetFileNameWithoutExtension(fileFullPath) + ".xml";
            string manifestPath = pfwPrintFolder + "\\" + MANIFEST_FOLDER_NAME + "\\" + manifestName;
            return manifestPath;
        }

        public static void PurgeMatchingManifestFile(string pfwPrintFolder, string fileFullPath)
        {
            // Purge the maifest for the supplied file if it exists.
            string manifestPath = GetManifestFileFullPath(pfwPrintFolder, fileFullPath);
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }
    }
}
