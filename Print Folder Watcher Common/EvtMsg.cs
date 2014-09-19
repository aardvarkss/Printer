using System;
using System.Collections.Generic;

namespace Print_Folder_Watcher_Common
{
	/// <summary>
	/// Summary description for EvtMsg.
	/// </summary>
	public class EvtMsg
	{
		//These constants are from evtmsg.h
		public const long EVT_JOB_STATUS_PRINTED = 0x00000000L;
		public const long EVT_DPPD_WARNING = 0x800003E9L;
		public const long EVT_DPPD_CANNOT_COPY_ADOBE_PLUGIN = 0xC00003EAL;
		public const long EVT_DPPD_NOT_SUPPORTED_MEDIA = 0xC00003EBL;
		public const long EVT_DPPD_INVALID_CONFIGURATION_FILE = 0xC00003ECL;
		public const long EVT_DPPD_SERVICE_CONTROLLER_NOTFOUND = 0xC00003EDL;
		public const long EVT_DPPD_SOAP_NOT_AVAILABLE = 0xC00003EEL;
		public const long EVT_DPPD_VERSION_MISMATCH = 0x800003EFL;
		public const long EVT_DPPD_PARSER = 0xC00003F0L;
		public const long EVT_DPPD_ERROR = 0xC00003F1L;
		public const long EVT_DPPD_ACCESS_ERROR = 0xC00003F2L;
		public const long EVT_DPPD_UNKNOWN_ERROR = 0xC00003F3L;
		public const long EVT_DPPD_PLUGIN_ERROR = 0xC00003F4L;
		public const long EVT_DPPD_NOT_SUPPORTED_RESO = 0xC00003F5L;
		public const long EVT_DPPD_SERVICE_CONTROLLER_ERROR = 0xC00003F6L;
		public const long EVT_DPPD_MISSING_ADOBE_PLUGIN = 0xC00003F7L;
		public const long EVT_DPPD_INCORRECT_LICENSE_FILE = 0xC00003F8L;
		public const long EVT_JOB_STATUS_BLOCKED_DEVQ = 0xC00007D1L;
		public const long EVT_JOB_STATUS_COMPLETE = 0x400007D2L;
		public const long EVT_JOB_STATUS_DELETED = 0xC00007D3L;
		public const long EVT_JOB_STATUS_DELETING = 0x800007D4L;
		public const long EVT_JOB_STATUS_ERROR = 0xC00007D5L;
		public const long EVT_JOB_STATUS_OFFLINE = 0x800007D6L;
		public const long EVT_JOB_STATUS_PAPEROUT = 0x800007D7L;
		public const long EVT_JOB_STATUS_PAUSED = 0x800007D8L;
		public const long EVT_JOB_STATUS_PRINTING = 0x400007D9L;
		public const long EVT_JOB_STATUS_RESTART = 0x400007DAL;
		public const long EVT_JOB_STATUS_SPOOLING = 0x400007DBL;
		public const long EVT_JOB_STATUS_USER_INTERVENTION = 0xC00007DCL;
		public const long EVT_PRINTER_STATUS_BUSY = 0x800007DDL;
		public const long EVT_PRINTER_STATUS_DOOR_OPEN = 0x800007DEL;
		public const long EVT_PRINTER_STATUS_ERROR = 0xC00007DFL;
		public const long EVT_PRINTER_STATUS_INITIALIZING = 0x400007E0L;
		public const long EVT_PRINTER_STATUS_IO_ACTIVE = 0x400007E1L;
		public const long EVT_PRINTER_STATUS_MANUAL_FEED = 0x800007E2L;
		public const long EVT_PRINTER_STATUS_NO_TONER = 0xC00007E3L;
		public const long EVT_PRINTER_STATUS_NOT_AVAILABLE = 0xC00007E4L;
		public const long EVT_PRINTER_STATUS_OFFLINE = 0x800007E5L;
		public const long EVT_PRINTER_STATUS_OUT_OF_MEMORY = 0xC00007E6L;
		public const long EVT_PRINTER_STATUS_OUTPUT_BIN_FULL = 0xC00007E7L;
		public const long EVT_PRINTER_STATUS_PAGE_PUNT = 0xC00007E8L;
		public const long EVT_PRINTER_STATUS_PAPER_JAM = 0x800007E9L;
		public const long EVT_PRINTER_STATUS_PAPER_OUT = 0x800007EAL;
		public const long EVT_PRINTER_STATUS_PAPER_PROBLEM = 0x800007EBL;
		public const long EVT_PRINTER_STATUS_PAUSED = 0x800007ECL;
		public const long EVT_PRINTER_STATUS_PENDING_DELETION = 0x800007EDL;
		public const long EVT_PRINTER_STATUS_POWER_SAVE = 0x800007EEL;
		public const long EVT_PRINTER_STATUS_PRINTING = 0x400007EFL;
		public const long EVT_PRINTER_STATUS_PROCESSING = 0x400007F0L;
		public const long EVT_PRINTER_STATUS_SERVER_UNKNOWN = 0xC00007F1L;
		public const long EVT_PRINTER_STATUS_TONER_LOW = 0xC00007F2L;
		public const long EVT_PRINTER_STATUS_USER_INTERVENTION = 0xC00007F3L;
		public const long EVT_PRINTER_STATUS_WAITING = 0x400007F4L;
		public const long EVT_PRINTER_STATUS_WARMING_UP = 0x400007F5L;
		public const long EVT_USE_PRINTER_OFFLINE = 0x800007F6L;
		public const long EVT_PRINT_JOB_REQ_RECEIVED = 0x40000BB9L;
		public const long EVT_PRINT_JOB_REQ_PRINTED = 0x40000BBAL;
		public const long EVT_NO_FILE_OR_DIR = 0xC0000BBBL;
		public const long EVT_INVALID_PRINTER = 0xC0000BBCL;
		public const long EVT_NO_COMPATABLE_ACROBAT_INST = 0xC0000BBDL;
		public const long EVT_MORE_THAN_ONE_ACROBAT_RUNNING = 0x40000BBEL;
		public const long EVT_ACROBAT_OPEN_FAIL = 0xC0000BBFL;
		public const long EVT_NO_SPOOLER = 0xC0000BC0L;
		public const long EVT_NO_DUPLEX = 0xC0000BC1L;
		public const long EVT_INTERNAL_ERR = 0xC0000BC2L;
		public const long EVT_UNKNOWN_ERR = 0xC0000BC3L;
		public const long EVT_SPECIFY_CORR_VALUE_FOR_COPIES = 0xC0000BC4L;
		public const long EVT_SPECIFY_CORR_VALUE_FOR_DUPLEX = 0xC0000BC5L;
		public const long EVT_SILENT_PRN_SUPPORTS_PDF_XFDF_ONLY = 0xC0000BC6L;
		public const long EVT_NO_DFAULT_PRNTER = 0xC0000BC7L;
		public const long EVT_PRN_JOB_SEND_TO_ACROBAT = 0xC0000BC8L;
		public const long EVT_SPECIFIED_MTD_NOT_SUPPO = 0xC0000BC9L;
		public const long EVT_SILT_PRN_SUPP_PDF_XFDF_URL_ONLY = 0xC0000BCAL;
		public const long EVT_PRN_JOB_REQ_FAIL = 0xC0000BCBL;
		public const long EVT_SPECIFY_CORR_VALUE_FOR_COLLATE = 0xC0000BCCL;
		public const long EVT_FILENAME_HAVE_INVALID_CHAR = 0xC0000BCDL;
		public const long EVT_INVALID_DATA = 0xC0000BCEL;
		public const long EVT_CORRUPTED_XFDF_FILE = 0xC0000BCFL;
		public const long EVT_NO_ACROBAT_NOT_SUPPORTED = 0xC0000BD0L;
		public const long EVT_NO_ACROBAT_FOUND = 0xC0000BD1L;
		public const long EVT_INVALID_DIRECTORY = 0xC0000BD2L;
		public const long EVT_FILE_NOT_FOUND = 0xC0000BD3L;
		public const long EVT_LONG_PRINTER_NAME = 0xC0000BD4L;
		public const long EVT_PRINTER_PAPEROUT = 0x40000BD5L;
		public const long EVT_ACORBAT_READER = 0x40000FA1L;
		public const long EVT_ADOBE_ACROBAT = 0x40000FA2L;
		public const long EVT_ADOBE_REGISTRATION = 0x40000FA3L;
		public const long EVT_ADOBE_ERR = 0x40000FA4L;
		public const long EVT_ADOBE_LICENSE_AGREEMENT = 0x40000FA5L;
		public const long EVT_ADOBE_AUTO_UPDATE = 0xC0000FA6L;
		public const long EVT_ADOBE_SAVE_AS = 0xC0000FA7L;
		public const long EVT_FORM_NOT_FOUND = 0xC0000FA8L;
		public const long EVT_COULD_NOT_START_PRINT_JOB = 0xC0000FA9L;
		public const long EVT_FILE_HAS_BEEN_CORRUPTED = 0xC0000FAAL;
		public const long EVT_UNRECOGNIZED_TOKEN_FOUND = 0xC0000FABL;
		public const long EVT_ERR_OPENING_DOC = 0xC0000FACL;
		public const long EVT_ERR_IN_PROCESSING_DOC = 0xC0000FADL;
		public const long EVT_NEED_INSTALL_PRINTER = 0xC0000FAEL;
		public const long EVT_ADOBE_CRASH = 0xC0000FAFL;
		public const long EVT_SELECT_DEFAULT_PRINTER = 0xC0000FB0L;
		public const long EVT_SYNTAX_ERROR = 0xC0001389L;
		public const long EVT_MODULE_NOT_FOUND = 0xC000138AL;
		public const long EVT_FILE_NOT_SPECIFIED = 0xC000138BL;
		public const long EVT_PRINTER_NAME_NOT_SPECIFIED = 0xC000138CL;
		public const long EVT_LOGICALJOBID_NOT_SPECIFIED = 0xC000138DL;
		public const long EVT_COPIES_NOT_SPECIFIED = 0xC000138EL;
		public const long EVT_SPECIFY_INTEGER_FOR_COPIES = 0xC000138FL;
		public const long EVT_DUPLEX_NOT_SPECIFIED = 0xC0001390L;
		public const long EVT_SPECIFY_LOGICALJOBID = 0xC0001391L;
		public const long EVT_NO_PRINTJOB_STATUS = 0xC0001392L;
		public const long EVT_UNKNOWN_EROR = 0xC0001393L;
		public const long EVT_ACROBAT_NOT_SPECIFIED = 0xC0001394L;

        private static Dictionary<long, string> shortDescrtiptions = null;

		private EvtMsg(){}

        static public Dictionary<long, string> GetShortDescrtiptionsList()
        {
            if (shortDescrtiptions == null)
            {
                shortDescrtiptions = new Dictionary<long, string>();

                shortDescrtiptions.Add(EVT_JOB_STATUS_PRINTED, "EVT_JOB_STATUS_PRINTED");
                shortDescrtiptions.Add(EVT_DPPD_WARNING, "EVT_DPPD_WARNING");
                shortDescrtiptions.Add(EVT_DPPD_CANNOT_COPY_ADOBE_PLUGIN, "EVT_DPPD_CANNOT_COPY_ADOBE_PLUGIN");
                shortDescrtiptions.Add(EVT_DPPD_NOT_SUPPORTED_MEDIA, "EVT_DPPD_NOT_SUPPORTED_MEDIA");
                shortDescrtiptions.Add(EVT_DPPD_INVALID_CONFIGURATION_FILE, "EVT_DPPD_INVALID_CONFIGURATION_FILE");
                shortDescrtiptions.Add(EVT_DPPD_SERVICE_CONTROLLER_NOTFOUND, "EVT_DPPD_SERVICE_CONTROLLER_NOTFOUND");
                shortDescrtiptions.Add(EVT_DPPD_SOAP_NOT_AVAILABLE, "EVT_DPPD_SOAP_NOT_AVAILABLE");
                shortDescrtiptions.Add(EVT_DPPD_VERSION_MISMATCH, "EVT_DPPD_VERSION_MISMATCH");
                shortDescrtiptions.Add(EVT_DPPD_PARSER, "EVT_DPPD_PARSER");
                shortDescrtiptions.Add(EVT_DPPD_ERROR, "EVT_DPPD_ERROR");
                shortDescrtiptions.Add(EVT_DPPD_ACCESS_ERROR, "EVT_DPPD_ACCESS_ERROR");
                shortDescrtiptions.Add(EVT_DPPD_UNKNOWN_ERROR, "EVT_DPPD_UNKNOWN_ERROR");
                shortDescrtiptions.Add(EVT_DPPD_PLUGIN_ERROR, "EVT_DPPD_PLUGIN_ERROR");
                shortDescrtiptions.Add(EVT_DPPD_NOT_SUPPORTED_RESO, "EVT_DPPD_NOT_SUPPORTED_RESO");
                shortDescrtiptions.Add(EVT_DPPD_SERVICE_CONTROLLER_ERROR, "EVT_DPPD_SERVICE_CONTROLLER_ERROR");
                shortDescrtiptions.Add(EVT_DPPD_MISSING_ADOBE_PLUGIN, "EVT_DPPD_MISSING_ADOBE_PLUGIN");
                shortDescrtiptions.Add(EVT_DPPD_INCORRECT_LICENSE_FILE, "EVT_DPPD_INCORRECT_LICENSE_FILE");
                shortDescrtiptions.Add(EVT_JOB_STATUS_BLOCKED_DEVQ, "EVT_JOB_STATUS_BLOCKED_DEVQ");
                shortDescrtiptions.Add(EVT_JOB_STATUS_COMPLETE, "EVT_JOB_STATUS_COMPLETE");
                shortDescrtiptions.Add(EVT_JOB_STATUS_DELETED, "EVT_JOB_STATUS_DELETED");
                shortDescrtiptions.Add(EVT_JOB_STATUS_DELETING, "EVT_JOB_STATUS_DELETING");
                shortDescrtiptions.Add(EVT_JOB_STATUS_ERROR, "EVT_JOB_STATUS_ERROR");
                shortDescrtiptions.Add(EVT_JOB_STATUS_OFFLINE, "EVT_JOB_STATUS_OFFLINE");
                shortDescrtiptions.Add(EVT_JOB_STATUS_PAPEROUT, "EVT_JOB_STATUS_PAPEROUT");
                shortDescrtiptions.Add(EVT_JOB_STATUS_PAUSED, "EVT_JOB_STATUS_PAUSED");
                shortDescrtiptions.Add(EVT_JOB_STATUS_PRINTING, "EVT_JOB_STATUS_PRINTING");
                shortDescrtiptions.Add(EVT_JOB_STATUS_RESTART, "EVT_JOB_STATUS_RESTART");
                shortDescrtiptions.Add(EVT_JOB_STATUS_SPOOLING, "EVT_JOB_STATUS_SPOOLING");
                shortDescrtiptions.Add(EVT_JOB_STATUS_USER_INTERVENTION, "EVT_JOB_STATUS_USER_INTERVENTION");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_BUSY, "EVT_PRINTER_STATUS_BUSY");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_DOOR_OPEN, "EVT_PRINTER_STATUS_DOOR_OPEN");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_ERROR, "EVT_PRINTER_STATUS_ERROR");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_INITIALIZING, "EVT_PRINTER_STATUS_INITIALIZING");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_IO_ACTIVE, "EVT_PRINTER_STATUS_IO_ACTIVE");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_MANUAL_FEED, "EVT_PRINTER_STATUS_MANUAL_FEED");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_NO_TONER, "EVT_PRINTER_STATUS_NO_TONER");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_NOT_AVAILABLE, "EVT_PRINTER_STATUS_NOT_AVAILABLE");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_OFFLINE, "EVT_PRINTER_STATUS_OFFLINE");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_OUT_OF_MEMORY, "EVT_PRINTER_STATUS_OUT_OF_MEMORY");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_OUTPUT_BIN_FULL, "EVT_PRINTER_STATUS_OUTPUT_BIN_FULL");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_PAGE_PUNT, "EVT_PRINTER_STATUS_PAGE_PUNT");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_PAPER_JAM, "EVT_PRINTER_STATUS_PAPER_JAM");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_PAPER_OUT, "EVT_PRINTER_STATUS_PAPER_OUT");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_PAPER_PROBLEM, "EVT_PRINTER_STATUS_PAPER_PROBLEM");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_PAUSED, "EVT_PRINTER_STATUS_PAUSED");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_PENDING_DELETION, "EVT_PRINTER_STATUS_PENDING_DELETION");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_POWER_SAVE, "EVT_PRINTER_STATUS_POWER_SAVE");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_PRINTING, "EVT_PRINTER_STATUS_PRINTING");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_PROCESSING, "EVT_PRINTER_STATUS_PROCESSING");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_SERVER_UNKNOWN, "EVT_PRINTER_STATUS_SERVER_UNKNOWN");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_TONER_LOW, "EVT_PRINTER_STATUS_TONER_LOW");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_USER_INTERVENTION, "EVT_PRINTER_STATUS_USER_INTERVENTION");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_WAITING, "EVT_PRINTER_STATUS_WAITING");
                shortDescrtiptions.Add(EVT_PRINTER_STATUS_WARMING_UP, "EVT_PRINTER_STATUS_WARMING_UP");
                shortDescrtiptions.Add(EVT_USE_PRINTER_OFFLINE, "EVT_USE_PRINTER_OFFLINE");
                shortDescrtiptions.Add(EVT_PRINT_JOB_REQ_RECEIVED, "EVT_PRINT_JOB_REQ_RECEIVED");
                shortDescrtiptions.Add(EVT_PRINT_JOB_REQ_PRINTED, "EVT_PRINT_JOB_REQ_PRINTED");
                shortDescrtiptions.Add(EVT_NO_FILE_OR_DIR, "EVT_NO_FILE_OR_DIR");
                shortDescrtiptions.Add(EVT_INVALID_PRINTER, "EVT_INVALID_PRINTER");
                shortDescrtiptions.Add(EVT_NO_COMPATABLE_ACROBAT_INST, "EVT_NO_COMPATABLE_ACROBAT_INST");
                shortDescrtiptions.Add(EVT_MORE_THAN_ONE_ACROBAT_RUNNING, "EVT_MORE_THAN_ONE_ACROBAT_RUNNING");
                shortDescrtiptions.Add(EVT_ACROBAT_OPEN_FAIL, "EVT_ACROBAT_OPEN_FAIL");
                shortDescrtiptions.Add(EVT_NO_SPOOLER, "EVT_NO_SPOOLER");
                shortDescrtiptions.Add(EVT_NO_DUPLEX, "EVT_NO_DUPLEX");
                shortDescrtiptions.Add(EVT_INTERNAL_ERR, "EVT_INTERNAL_ERR");
                shortDescrtiptions.Add(EVT_UNKNOWN_ERR, "EVT_UNKNOWN_ERR");
                shortDescrtiptions.Add(EVT_SPECIFY_CORR_VALUE_FOR_COPIES, "EVT_SPECIFY_CORR_VALUE_FOR_COPIES");
                shortDescrtiptions.Add(EVT_SPECIFY_CORR_VALUE_FOR_DUPLEX, "EVT_SPECIFY_CORR_VALUE_FOR_DUPLEX");
                shortDescrtiptions.Add(EVT_SILENT_PRN_SUPPORTS_PDF_XFDF_ONLY, "EVT_SILENT_PRN_SUPPORTS_PDF_XFDF_ONLY");
                shortDescrtiptions.Add(EVT_NO_DFAULT_PRNTER, "EVT_NO_DFAULT_PRNTER");
                shortDescrtiptions.Add(EVT_PRN_JOB_SEND_TO_ACROBAT, "EVT_PRN_JOB_SEND_TO_ACROBAT");
                shortDescrtiptions.Add(EVT_SPECIFIED_MTD_NOT_SUPPO, "EVT_SPECIFIED_MTD_NOT_SUPPO");
                shortDescrtiptions.Add(EVT_SILT_PRN_SUPP_PDF_XFDF_URL_ONLY, "EVT_SILT_PRN_SUPP_PDF_XFDF_URL_ONLY");
                shortDescrtiptions.Add(EVT_PRN_JOB_REQ_FAIL, "EVT_PRN_JOB_REQ_FAIL");
                shortDescrtiptions.Add(EVT_SPECIFY_CORR_VALUE_FOR_COLLATE, "EVT_SPECIFY_CORR_VALUE_FOR_COLLATE");
                shortDescrtiptions.Add(EVT_FILENAME_HAVE_INVALID_CHAR, "EVT_FILENAME_HAVE_INVALID_CHAR");
                shortDescrtiptions.Add(EVT_INVALID_DATA, "EVT_INVALID_DATA");
                shortDescrtiptions.Add(EVT_CORRUPTED_XFDF_FILE, "EVT_CORRUPTED_XFDF_FILE");
                shortDescrtiptions.Add(EVT_NO_ACROBAT_NOT_SUPPORTED, "EVT_NO_ACROBAT_NOT_SUPPORTED");
                shortDescrtiptions.Add(EVT_NO_ACROBAT_FOUND, "EVT_NO_ACROBAT_FOUND");
                shortDescrtiptions.Add(EVT_INVALID_DIRECTORY, "EVT_INVALID_DIRECTORY");
                shortDescrtiptions.Add(EVT_FILE_NOT_FOUND, "EVT_FILE_NOT_FOUND");
                shortDescrtiptions.Add(EVT_LONG_PRINTER_NAME, "EVT_LONG_PRINTER_NAME");
                shortDescrtiptions.Add(EVT_PRINTER_PAPEROUT, "EVT_PRINTER_PAPEROUT");
                shortDescrtiptions.Add(EVT_ACORBAT_READER, "EVT_ACORBAT_READER");
                shortDescrtiptions.Add(EVT_ADOBE_ACROBAT, "EVT_ADOBE_ACROBAT");
                shortDescrtiptions.Add(EVT_ADOBE_REGISTRATION, "EVT_ADOBE_REGISTRATION");
                shortDescrtiptions.Add(EVT_ADOBE_ERR, "EVT_ADOBE_ERR");
                shortDescrtiptions.Add(EVT_ADOBE_LICENSE_AGREEMENT, "EVT_ADOBE_LICENSE_AGREEMENT");
                shortDescrtiptions.Add(EVT_ADOBE_AUTO_UPDATE, "EVT_ADOBE_AUTO_UPDATE");
                shortDescrtiptions.Add(EVT_ADOBE_SAVE_AS, "EVT_ADOBE_SAVE_AS");
                shortDescrtiptions.Add(EVT_FORM_NOT_FOUND, "EVT_FORM_NOT_FOUND");
                shortDescrtiptions.Add(EVT_COULD_NOT_START_PRINT_JOB, "EVT_COULD_NOT_START_PRINT_JOB");
                shortDescrtiptions.Add(EVT_FILE_HAS_BEEN_CORRUPTED, "EVT_FILE_HAS_BEEN_CORRUPTED");
                shortDescrtiptions.Add(EVT_UNRECOGNIZED_TOKEN_FOUND, "EVT_UNRECOGNIZED_TOKEN_FOUND");
                shortDescrtiptions.Add(EVT_ERR_OPENING_DOC, "EVT_ERR_OPENING_DOC");
                shortDescrtiptions.Add(EVT_ERR_IN_PROCESSING_DOC, "EVT_ERR_IN_PROCESSING_DOC");
                shortDescrtiptions.Add(EVT_NEED_INSTALL_PRINTER, "EVT_NEED_INSTALL_PRINTER");
                shortDescrtiptions.Add(EVT_ADOBE_CRASH, "EVT_ADOBE_CRASH");
                shortDescrtiptions.Add(EVT_SELECT_DEFAULT_PRINTER, "EVT_SELECT_DEFAULT_PRINTER");
                shortDescrtiptions.Add(EVT_SYNTAX_ERROR, "EVT_SYNTAX_ERROR");
                shortDescrtiptions.Add(EVT_MODULE_NOT_FOUND, "EVT_MODULE_NOT_FOUND");
                shortDescrtiptions.Add(EVT_FILE_NOT_SPECIFIED, "EVT_FILE_NOT_SPECIFIED");
                shortDescrtiptions.Add(EVT_PRINTER_NAME_NOT_SPECIFIED, "EVT_PRINTER_NAME_NOT_SPECIFIED");
                shortDescrtiptions.Add(EVT_LOGICALJOBID_NOT_SPECIFIED, "EVT_LOGICALJOBID_NOT_SPECIFIED");
                shortDescrtiptions.Add(EVT_COPIES_NOT_SPECIFIED, "EVT_COPIES_NOT_SPECIFIED");
                shortDescrtiptions.Add(EVT_SPECIFY_INTEGER_FOR_COPIES, "EVT_SPECIFY_INTEGER_FOR_COPIES");
                shortDescrtiptions.Add(EVT_DUPLEX_NOT_SPECIFIED, "EVT_DUPLEX_NOT_SPECIFIED");
                shortDescrtiptions.Add(EVT_SPECIFY_LOGICALJOBID, "EVT_SPECIFY_LOGICALJOBID");
                shortDescrtiptions.Add(EVT_NO_PRINTJOB_STATUS, "EVT_NO_PRINTJOB_STATUS");
                shortDescrtiptions.Add(EVT_UNKNOWN_EROR, "EVT_UNKNOWN_EROR");
                shortDescrtiptions.Add(EVT_ACROBAT_NOT_SPECIFIED, "EVT_ACROBAT_NOT_SPECIFIED");
            }
            return shortDescrtiptions;
        }
        static public string GetShortDescriptionFromCode(long code)
        {
            Dictionary<long, string> shortDescrtiptions = GetShortDescrtiptionsList();
            if (shortDescrtiptions.ContainsKey(code))
            {
                return shortDescrtiptions[code];
            }
            else
            {
                return "UNKNOWN_ERROR_CODE";
            }
        }
	}
}
