using System;
using System.IO.Ports;
using System.Threading;
using System.Text;

using System.Collections.Generic;

namespace coreapp
{

   class ErrorsApp {

        public static int errno;


        public enum ErrnoInternalCodes {
            
            STATUS_PROCCESSOR_ERROR_NOT_VALID_HEADER = 0,
            STATUS_PROCCESSOR_ERROR_NOT_VALID_PAYLOAD,
            STATUS_PROCCESSOR_ERROR_NOT_VALID_FOOTER,
            STATUS_PROCCESSOR_ERROR_UNKNOWN_CMD,
            RESULT_PROCCESSOR_ERROR_COMMAND_PARSE,
            RESULT_PROCCESSOR_ERROR_RESULT_LINE_MISSING
        }
        public enum ErrnoExternalCodes {
            
            CMD_ERR_GENERALERROR = 10,
            CMD_ERR_MISSINGARGUMENT, 
            CMD_ERR_EXTRAARGUMENT,
            CMD_ERR_INVALIDVALUE,
            CMD_ERR_VALUEOUTOFRANGE,
            CMD_ERR_UNKNOWNCOMMAND,
            COMMAND_ERROR_SD_OPEN = 50,
            COMMAND_ERROR_SD_OPEN_C, 
            COMMAND_ERROR_SD_CHDIR,
            COMMAND_ERROR_SD_FILE_EXISTS, 
            COMMAND_ERROR_SD_FILE_EXISTS_NOT,
            COMMAND_ERROR_SD_MKDIR, 
            COMMAND_ERROR_SD_RM_RF_STAR,
            COMMAND_ERROR_SD_REMOVE,
            COMMAND_ERROR_PARSING_ARG,
            COMMAND_ERROR_C_FGETS,      
            COMMAND_ERROR_C_FGETS_BUFFER,
                  
            COMMAND_ERROR_PARSING_ARG_1, 
            COMMAND_ERROR_PARSING_ARG_2, 
            COMMAND_ERROR_PARSING_ARG_3, 
            COMMAND_ERROR_PARSING_ARG_4, 
            COMMAND_ERROR_PARSING_ARG_5, 
            COMMAND_ERROR_PARSING_ARG_6, 
            COMMAND_ERROR_PARSING_ARG_7
            
           
        }

        

        private static string[] errors = new string[100];

        static ErrorsApp() {

            for ( int i = 0; i < errors.Length; ++ i ) {
                errors[i] = null;
            }
            setTextForErrorId(ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_HEADER, "NOT VALID HEADER");
            setTextForErrorId(ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_PAYLOAD, "NOT VALID PAYLOAD");
            setTextForErrorId(ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_FOOTER, "NOT VALID FOOTER");
            setTextForErrorId(ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_UNKNOWN_CMD, "COMMAND UNKNOWN");
            setTextForErrorId(ErrnoInternalCodes.RESULT_PROCCESSOR_ERROR_COMMAND_PARSE, "COMMAND OBJECT CAN'T PARSE THE LINE");
            setTextForErrorId(ErrnoInternalCodes.RESULT_PROCCESSOR_ERROR_RESULT_LINE_MISSING, "RESULT LINE MISSING");
            

            setTextForErrorCmdCodeId(ErrnoExternalCodes.CMD_ERR_GENERALERROR, "General error");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.CMD_ERR_MISSINGARGUMENT, "Missing argument");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.CMD_ERR_EXTRAARGUMENT, "Extra argument");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.CMD_ERR_INVALIDVALUE, "Invalid value placed as argument");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.CMD_ERR_VALUEOUTOFRANGE, "Value out of range");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.CMD_ERR_UNKNOWNCOMMAND, "Unknown command");
           
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_SD_OPEN, "SdFat open: Unable to access to the file");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_SD_OPEN_C, "SdFat open: Unable to create file with the open command");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_SD_CHDIR, "SdFat chdir: Unable to change to the file");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_SD_FILE_EXISTS, "SdFat exists: File with the same name already exists");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_SD_FILE_EXISTS_NOT, "SdFat exists: File doesn't exists ");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_SD_MKDIR, "SdFat mkdir: Unable to make a folder");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_SD_RM_RF_STAR, "SdFat rmRfStar: Unable to delete the folder ");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_SD_REMOVE, "SdFat remove: Unable to delete the folder ");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_PARSING_ARG, "Parsing error: Not valid argument.");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_C_FGETS, "Fgets: Unable to read from a file  ");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_C_FGETS_BUFFER, "Fgets: Unable to read from a file buffer overflow ");
        
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_PARSING_ARG_1, "Parsing error: Not valid argument. (Unsuccessful decimal value parsing)");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_PARSING_ARG_2, "Parsing error: Not valid argument. (Unsuccessful parsing for the path option.) ");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_PARSING_ARG_3, "Parsing error: Not valid argument. (Unsuccessful parsing for the data option.) ");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_PARSING_ARG_4, "Parsing error: Not valid argument. (Unsuccessful decimal value parsing for the <start> argument)");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_PARSING_ARG_5, "Parsing error: Not valid argument. (Unsuccessful decimal value parsing for the <length> argument)");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_PARSING_ARG_6, "Parsing error: Not valid arguments. (<start> + <length> is greater then the size of the file)");
            setTextForErrorCmdCodeId(ErrnoExternalCodes.COMMAND_ERROR_PARSING_ARG_7, "Parsing error: Not valid arguments. (The <start> argument is not null but the <length> argument is null)");
        

            

        }


        public static bool errorExist(int errno) {

            if (errno < 0)
                errno *= -1;
            if ( errno < errors.Length && errors[errno] != null) 
                return true;
            else
                return false;
        }
        private static void setTextForErrorId(ErrnoInternalCodes errorId, string text) {
                errors[(int)errorId] = text;
        }
        private static void setTextForErrorCmdCodeId(ErrnoExternalCodes errorId, string text) {
                errors[(int)errorId] = text;
        }
        public static string get() {
            return errors[errno];
        }

        public static string get(ErrorsApp.ErrnoExternalCodes code) {
            return "External error/" + errors[(int)code];
        }
        public static void set(ErrorsApp.ErrnoInternalCodes code) {
            ErrorsApp.errno = (int) code;
        }   
        public static void set(ErrorsApp.ErrnoExternalCodes code) {
            ErrorsApp.errno = (int) code;
        }   
        public static void print() {
            Console.WriteLine(ErrorsApp.get());
        }


    }



}
