using System;

namespace Print_Folder_Watcher_Common
{
	/// <summary>
	/// Summary description for SilentPrintutils.
	/// </summary>
	public class Utils
	{
        public static string    MANAGER_NAME = "Meticulus Print Folder Watcher Manager";
		public static string	SERVICE_NAME = "Meticulus Print Folder Watcher Service";
		public static string	EVENT_LOG_SOURCE = "Print Folder Watcher Service";
		public static string	EVENT_LOG = "Meticulus Log";

		public static string	SERVICE_STARTED = "Service Started";
		public static string	SERVICE_STOPPED = "Service Stopped";

		public const int		MILLISECONDS_SHORT_TIME_DELAY = 500;

		public static bool ExitCodeIsSuccess(long exitCode)
		{
			return exitCode == EvtMsg.EVT_JOB_STATUS_PRINTED;
		}

		/// <summary>
		/// Fatal error codes mean the system will never be able to print ANY file e.g. config errors.
		/// </summary>
		/// <param name="exitCode"></param>
		/// <returns></returns>
		public static bool ExitCodeIsFatalError(long exitCode)
		{
			bool isFatalError = false;
			switch (exitCode)
			{
				case EvtMsg.EVT_DPPD_WARNING://0x800003E9L
				case EvtMsg.EVT_DPPD_CANNOT_COPY_ADOBE_PLUGIN://0xC00003EAL
				case EvtMsg.EVT_DPPD_NOT_SUPPORTED_MEDIA://0xC00003EBL
				case EvtMsg.EVT_DPPD_INVALID_CONFIGURATION_FILE://0xC00003ECL
				case EvtMsg.EVT_DPPD_SERVICE_CONTROLLER_NOTFOUND://0xC00003EDL
				case EvtMsg.EVT_DPPD_SOAP_NOT_AVAILABLE://0xC00003EEL
				case EvtMsg.EVT_DPPD_VERSION_MISMATCH://0x800003EFL
				case EvtMsg.EVT_DPPD_PARSER://0xC00003F0L
				case EvtMsg.EVT_DPPD_ERROR://0xC00003F1L
				case EvtMsg.EVT_DPPD_ACCESS_ERROR://0xC00003F2L
				case EvtMsg.EVT_DPPD_UNKNOWN_ERROR://0xC00003F3L
				case EvtMsg.EVT_DPPD_PLUGIN_ERROR://0xC00003F4L
				case EvtMsg.EVT_DPPD_NOT_SUPPORTED_RESO://0xC00003F5L
				case EvtMsg.EVT_DPPD_SERVICE_CONTROLLER_ERROR://0xC00003F6L
				case EvtMsg.EVT_DPPD_MISSING_ADOBE_PLUGIN://0xC00003F7L
				case EvtMsg.EVT_DPPD_INCORRECT_LICENSE_FILE://0xC00003F8L
				case EvtMsg.EVT_JOB_STATUS_BLOCKED_DEVQ://0xC00007D1L
				case EvtMsg.EVT_JOB_STATUS_ERROR://0xC00007D5L
				case EvtMsg.EVT_INVALID_PRINTER://0xC0000BBCL
				case EvtMsg.EVT_NO_COMPATABLE_ACROBAT_INST://0xC0000BBDL
				case EvtMsg.EVT_NO_SPOOLER://0xC0000BC0L
				case EvtMsg.EVT_INTERNAL_ERR://0xC0000BC2L
				case EvtMsg.EVT_UNKNOWN_ERR://0xC0000BC3L
				case EvtMsg.EVT_NO_DFAULT_PRNTER://0xC0000BC7L
				case EvtMsg.EVT_SPECIFIED_MTD_NOT_SUPPO://0xC0000BC9L
				case EvtMsg.EVT_NO_ACROBAT_NOT_SUPPORTED://0xC0000BD0L
				case EvtMsg.EVT_NO_ACROBAT_FOUND://0xC0000BD1L
				case EvtMsg.EVT_LONG_PRINTER_NAME://0xC0000BD4L
				case EvtMsg.EVT_ACORBAT_READER://0x40000FA1L
				case EvtMsg.EVT_ADOBE_ACROBAT://0x40000FA2L
				case EvtMsg.EVT_ADOBE_REGISTRATION://0x40000FA3L
				case EvtMsg.EVT_ADOBE_ERR://0x40000FA4L
				case EvtMsg.EVT_ADOBE_LICENSE_AGREEMENT://0x40000FA5L
				case EvtMsg.EVT_ADOBE_AUTO_UPDATE://0xC0000FA6L
				case EvtMsg.EVT_ADOBE_SAVE_AS://0xC0000FA7L
				case EvtMsg.EVT_COULD_NOT_START_PRINT_JOB://0xC0000FA9L
				case EvtMsg.EVT_UNRECOGNIZED_TOKEN_FOUND://0xC0000FABL
				case EvtMsg.EVT_NEED_INSTALL_PRINTER://0xC0000FAEL
				case EvtMsg.EVT_SELECT_DEFAULT_PRINTER://0xC0000FB0L
				case EvtMsg.EVT_SYNTAX_ERROR://0xC0001389L
				case EvtMsg.EVT_MODULE_NOT_FOUND://0xC000138AL
				case EvtMsg.EVT_NO_PRINTJOB_STATUS://0xC0001392L
					isFatalError = true;
					break;

				default:
					isFatalError = false;
					break;
			}
			return isFatalError;
		}

		/// <summary>
		/// Error code implies problem with printer.
		/// </summary>
		/// <param name="exitCode"></param>
		/// <returns></returns>
		public static bool ExitCodeIsPrinterError(long exitCode)
		{
			bool isPrinterError = false;
			switch (exitCode)
			{
				case EvtMsg.EVT_JOB_STATUS_OFFLINE://0x800007D6L
				case EvtMsg.EVT_JOB_STATUS_PAPEROUT://0x800007D7L
				case EvtMsg.EVT_JOB_STATUS_PAUSED://0x800007D8L
				case EvtMsg.EVT_JOB_STATUS_USER_INTERVENTION://0xC00007DCL
				case EvtMsg.EVT_PRINTER_STATUS_BUSY://0x800007DDL
				case EvtMsg.EVT_PRINTER_STATUS_DOOR_OPEN://0x800007DEL
				case EvtMsg.EVT_PRINTER_STATUS_ERROR://0xC00007DFL
				case EvtMsg.EVT_PRINTER_STATUS_MANUAL_FEED://0x800007E2L
				case EvtMsg.EVT_PRINTER_STATUS_NO_TONER://0xC00007E3L
				case EvtMsg.EVT_PRINTER_STATUS_NOT_AVAILABLE://0xC00007E4L
				case EvtMsg.EVT_PRINTER_STATUS_OFFLINE://0x800007E5L
				case EvtMsg.EVT_PRINTER_STATUS_OUT_OF_MEMORY://0xC00007E6L
				case EvtMsg.EVT_PRINTER_STATUS_OUTPUT_BIN_FULL://0xC00007E7L
				case EvtMsg.EVT_PRINTER_STATUS_PAGE_PUNT://0xC00007E8L
				case EvtMsg.EVT_PRINTER_STATUS_PAPER_JAM://0x800007E9L
				case EvtMsg.EVT_PRINTER_STATUS_PAPER_OUT://0x800007EAL
				case EvtMsg.EVT_PRINTER_STATUS_PAPER_PROBLEM://0x800007EBL
				case EvtMsg.EVT_PRINTER_STATUS_PAUSED://0x800007ECL
				case EvtMsg.EVT_PRINTER_STATUS_PENDING_DELETION://0x800007EDL
				case EvtMsg.EVT_PRINTER_STATUS_POWER_SAVE://0x800007EEL
				case EvtMsg.EVT_PRINTER_STATUS_SERVER_UNKNOWN://0xC00007F1L
				case EvtMsg.EVT_PRINTER_STATUS_TONER_LOW://0xC00007F2L
				case EvtMsg.EVT_PRINTER_STATUS_USER_INTERVENTION://0xC00007F3L
				case EvtMsg.EVT_PRINTER_STATUS_WAITING://0x400007F4L
				case EvtMsg.EVT_PRINTER_STATUS_WARMING_UP://0x400007F5L
				case EvtMsg.EVT_USE_PRINTER_OFFLINE://0x800007F6L
				case EvtMsg.EVT_PRINTER_PAPEROUT://0x40000BD5L
					isPrinterError = true;
					break;

				default:
					isPrinterError = false;
					break;
			}
			return isPrinterError;
		}

		/// <summary>
		/// Print failed because of invalid parameter specifications.
		/// </summary>
		/// <param name="exitCode"></param>
		/// <returns></returns>
		public static bool ExitCodeIsParameterError(long exitCode)
		{
			bool isParameterError = false;
			switch (exitCode)
			{
				case EvtMsg.EVT_NO_DUPLEX://0xC0000BC1L
				case EvtMsg.EVT_SPECIFY_CORR_VALUE_FOR_COPIES://0xC0000BC4L
				case EvtMsg.EVT_SPECIFY_CORR_VALUE_FOR_DUPLEX://0xC0000BC5L
				case EvtMsg.EVT_SPECIFY_CORR_VALUE_FOR_COLLATE://0xC0000BCCL
				case EvtMsg.EVT_FILE_NOT_SPECIFIED://0xC000138BL
				case EvtMsg.EVT_PRINTER_NAME_NOT_SPECIFIED://0xC000138CL
				case EvtMsg.EVT_LOGICALJOBID_NOT_SPECIFIED://0xC000138DL
				case EvtMsg.EVT_COPIES_NOT_SPECIFIED://0xC000138EL
				case EvtMsg.EVT_SPECIFY_INTEGER_FOR_COPIES://0xC000138FL
				case EvtMsg.EVT_DUPLEX_NOT_SPECIFIED://0xC0001390L
				case EvtMsg.EVT_SPECIFY_LOGICALJOBID://0xC0001391L
				case EvtMsg.EVT_ACROBAT_NOT_SPECIFIED://0xC0001394L
					isParameterError = true;
					break;

				default:
					isParameterError = false;
					break;
			}
			return isParameterError;
		}

		/// <summary>
		/// Error code implies problem with this particular Xfdf / Pdf data.
		/// </summary>
		/// <param name="exitCode"></param>
		/// <returns></returns>
		public static bool ExitCodeIsDataError(long exitCode)
		{
			bool isDataError = false;
			switch (exitCode)
			{
				case EvtMsg.EVT_NO_FILE_OR_DIR://0xC0000BBBL
				case EvtMsg.EVT_SILT_PRN_SUPP_PDF_XFDF_URL_ONLY://0xC0000BCAL
				case EvtMsg.EVT_FILENAME_HAVE_INVALID_CHAR://0xC0000BCDL
				case EvtMsg.EVT_INVALID_DATA://0xC0000BCEL
				case EvtMsg.EVT_CORRUPTED_XFDF_FILE://0xC0000BCFL
				case EvtMsg.EVT_INVALID_DIRECTORY://0xC0000BD2L
				case EvtMsg.EVT_FILE_NOT_FOUND://0xC0000BD3L
				case EvtMsg.EVT_FORM_NOT_FOUND://0xC0000FA8L
				case EvtMsg.EVT_FILE_HAS_BEEN_CORRUPTED://0xC0000FAAL
				case EvtMsg.EVT_UNRECOGNIZED_TOKEN_FOUND://0xC0000FABL
				case EvtMsg.EVT_ERR_OPENING_DOC://0xC0000FACL
				case EvtMsg.EVT_ERR_IN_PROCESSING_DOC://0xC0000FADL
				case EvtMsg.EVT_SYNTAX_ERROR://0xC0001389L
					isDataError = true;
					break;

				default:
					isDataError = false;
					break;
			}
			return isDataError;
		}

		/// <summary>
		/// Unknown errors.
		/// </summary>
		public static bool ExitCodeIsUnknownError(long exitCode)
		{
			bool isUnknownError = false;
			switch (exitCode)
			{
				case EvtMsg.EVT_PRN_JOB_REQ_FAIL://0xC0000BCBL
					isUnknownError = true;
					break;

				default:
					isUnknownError = false;
					break;
			}
			return isUnknownError;
		}

		/// <summary>
		/// Codes not actually returned by CPAT!
		/// </summary>
		/// <param name="exitCode"></param>
		/// <returns></returns>
		public static bool ExitCodeIsNotUsed(long exitCode)
		{
			switch (exitCode)
			{
				case EvtMsg.EVT_MORE_THAN_ONE_ACROBAT_RUNNING://0xC0000BCBL
				case EvtMsg.EVT_ACROBAT_OPEN_FAIL://0xC0001393L
				case EvtMsg.EVT_UNKNOWN_EROR://0xC0001393L
					return true;
				default:
					return false;
			}
		}

		private Utils()
		{
		}
	}
}
