#ifndef __ERRORCODES_H__
#define __ERRORCODES_H__

enum errorCodes{
            COMMAND_ERROR_GENERALERROR = 10,
            COMMAND_ERROR_MISSINGARGUMENT = 11, 
            COMMAND_ERROR_EXTRAARGUMENT = 12,
            COMMAND_ERROR_INVALIDVALUE = 13,
            COMMAND_ERROR_VALUEOUTOFRANGE = 14,
            COMMAND_ERROR_UNKNOWNCOMMAND = 15,
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
};

#endif
