#ifndef __SDCARDMANIPULATOR1_H__
#define __SDCARDMANIPULATOR1_H__


#include <Arduino.h>
#include <pgmspace.h>
#include <string.h>
#include <SdFat.h>
//#include <util/crc16.h>
#include "cwdPath.h"
#include "returnValues.h"
#include "errorCodes.h"


SdFat32 sd;
File32 cwdFile;

CwdPath wp;



#define HELP_TABLE_LENGTH 7
const char h1[] PROGMEM = {"DIR (list content of the current working directory) use: dir"};
const char h2[] PROGMEM = {"CD (change or print current working directory) use: cd [<relative-path>]"};
const char h3[] PROGMEM = {"CRC (calculate crc for the given file segment) use: crc <file-name> [<start> <length>] [<crc-value>] "};
const char h4[] PROGMEM = {"PUT (append bytes in a file if exists otherwise first creates a new one) use: put <file-name> <array-of-dec-values> "};
const char h5[] PROGMEM = {"GET (read a file if exists)  use: get <file-name> [<start> <length>] [\"crc\"] "};
const char h6[] PROGMEM = {"DEL (delete a file if exists) use: del <file-name>"};
const char h7[] PROGMEM = {"MD (make a directory in current working directory) use: md <folder-name>"};
const char *const helpTable[HELP_TABLE_LENGTH] PROGMEM = {  h1,h2,h3,h4,h5,h6,h7 };

const char helpStr[] PROGMEM = {"help"};
const char dirStr[] PROGMEM = {"dir"};
const char cdStr[] PROGMEM = {"cd"};
const char putStr[] PROGMEM = {"put"};
const char getStr[] PROGMEM = {"get"};
const char getCrcStr[] PROGMEM = {"getcrc"};
const char delStr[] PROGMEM = {"del"};
const char crcStr[] PROGMEM = {"crc"};
const char mdStr[] PROGMEM = {"md"};
char strBuffer[20];
char cmdGetLineBuffer[40];

int crc16_update(uint16_t crc, char a) {
            crc ^= a;
            for (int i = 0; i < 8; ++i)
            {
                if ((crc & 1) == 1)
                    crc = (crc >> 1) ^ 0xA001;
                else
                    crc = (crc >> 1);
            }
            return crc;
}


uint16_t calcCRC(uint16_t prevCrc, char* str, int n)
{
  
    if ( n == 0 ) {
      return 0;
    }

    uint16_t crc=prevCrc; 
    int i;
    for( i = 0; i < n; ++i ) {
      crc= crc16_update (crc, str[i]); 
    }
    
    return crc;
}





int cmdDir(CmdProc* c) {
    strcpy_P(strBuffer, dirStr);

  File32 i_file;
  File32 cwdFile;
 
  if (!cwdFile.open(wp.get())) {
    return COMMAND_ERROR_SD_OPEN;
  } else {
    ReturnValues::printSuccess(wp.get(), strBuffer);
  }

  
  cwdFile.rewind();

  int cnt = 0;
  while (i_file.openNext(&cwdFile, O_RDONLY)) {

      cnt++;
      ReturnValues::printResult();
      
      ReturnValues::appendHtmlBody(i_file.fileSize());
      //i_file.printFileSize(&Serial);
      ReturnValues::appendHtmlBody(F(" "));

      char bufferFileName[20];
      i_file.getName(bufferFileName, 20);   
      
      //i_file.printName();
      ReturnValues::appendHtmlBody(bufferFileName);
      if (i_file.isDir()) {
        ReturnValues::appendHtmlBody(F("/"));
      }

      ReturnValues::appendHtmlBodyNewLine();
      i_file.close();
  }



  cwdFile.close();

  ReturnValues::printResult();
  ReturnValues::appendHtmlBody(cnt);
  ReturnValues::appendHtmlBodyNewLine();
  
  ReturnValues::printEndAndReady();
    
  return 0;
}



int cmdHelp(CmdProc* c) {
    strcpy_P(strBuffer, helpStr);

    ReturnValues::printSuccess("0", strBuffer);

  __FlashStringHelper *hello;
  for (int i = 0; i < HELP_TABLE_LENGTH; i++) {
    hello = (__FlashStringHelper*) pgm_read_word(&(helpTable[i]));
    ReturnValues::printResult();
    ReturnValues::appendHtmlBody(hello);
    ReturnValues::appendHtmlBodyNewLine();
  }
  
  ReturnValues::printEndAndReady();
  return 0;
}


int cmdPut(CmdProc* c) {
    strcpy_P(strBuffer, putStr);

    cwdFile.close();

    
    
    char* nextToken;
    nextToken = c->GetNextToken();

    
    char* filePath = nextToken;

    bool fileExists = false;
    if ( sd.exists(filePath)) {
      fileExists = true;
    } 
    if (!cwdFile.open(filePath, O_RDWR | O_CREAT | O_AT_END)) {
            return fileExists ? COMMAND_ERROR_SD_OPEN : COMMAND_ERROR_SD_OPEN_C;
    } 
        
      
    int dataLen = 0;
    char* token  = c->GetNextToken();
    int tokenDec;
  

    
    while(token != NULL) {
      if ( !tryParseDec(token, tokenDec, false) ) {  
          return COMMAND_ERROR_PARSING_ARG_1;
      }
      char ch = (char)tokenDec;
      dataLen += cwdFile.print(ch);
      
      token = c->GetNextToken();
    }
  
    
    cwdFile.close();
  
    ReturnValues::printSuccess(wp.get(), filePath, strBuffer);

    
    ReturnValues::printResult();
    ReturnValues::appendHtmlBody(dataLen);
    ReturnValues::appendHtmlBodyNewLine();
    ReturnValues::printEndAndReady();

    
    return 0;
    
}


int cmdDel(CmdProc* c) {

  strcpy_P(strBuffer, delStr);
  
  cwdFile.close();
  
  char* fileName;
  fileName = c->GetNextToken();


  if (!sd.exists(fileName)) {
          return COMMAND_ERROR_SD_FILE_EXISTS_NOT;
  }

  
  if (!cwdFile.open(fileName)){
          return COMMAND_ERROR_SD_OPEN;
  } else {
    if (cwdFile.isDir()) {
      if ( !cwdFile.rmRfStar() ) {
            return COMMAND_ERROR_SD_RM_RF_STAR;
      }
    } else {
      if ( !sd.remove(fileName) ) {
            return COMMAND_ERROR_SD_REMOVE;
      }
    }
  }
  cwdFile.close();

  ReturnValues::printSuccess(fileName, strBuffer);
  ReturnValues::printBlankResultLineEndAndReady();
  
  return 0;
}

int cmdMd(CmdProc* c) {
  strcpy_P(strBuffer, mdStr);

  cwdFile.close();
  
  char* folderName = c->GetNextToken(); 
    
  if (sd.exists(folderName)) {
          return COMMAND_ERROR_SD_FILE_EXISTS;
  }

  if (!sd.mkdir(folderName)) {
          return COMMAND_ERROR_SD_MKDIR;
  }


  ReturnValues::printSuccess(folderName, strBuffer);
  ReturnValues::printBlankResultLineEndAndReady();
  return 0;
}



// ok
int cmdCrc(CmdProc* c) {

  strcpy_P(strBuffer, crcStr);
           
  char* fileName;
  fileName = c->GetNextToken();

  
  char* thirdArg; // should be start or crc-value
  thirdArg = c->GetNextToken();
  char* fourthArg; // should be start
  fourthArg = c->GetNextToken();
  char* fifthArg; // should be length
  fifthArg = c->GetNextToken();

  int crcToChk = -1;
  int start = 0;
  int lenToRead = -1;
  
  // case: crc ww1 32
  // case: crc ww1 0 10
  if ( thirdArg != NULL ) {
     if ( !tryParseDec(thirdArg, fourthArg == NULL ? crcToChk : start , false ) ) {
            return COMMAND_ERROR_PARSING_ARG_1;
    }
  }

  if ( fourthArg != NULL ) {
     if ( !tryParseDec(fourthArg, lenToRead, false) ) {
            return COMMAND_ERROR_PARSING_ARG_1;
    }
  }
  if ( fifthArg != NULL ) {
     if ( !tryParseDec(fifthArg, crcToChk, false) ) {
            return COMMAND_ERROR_PARSING_ARG_1;
    }
  }

  
    cwdFile.close();
 
    if (sd.exists(fileName)) {
     
      if (!cwdFile.open(fileName, FILE_READ)) {
            return COMMAND_ERROR_SD_OPEN;
      } else {
        
            if ( lenToRead == -1 ) 
              lenToRead = cwdFile.fileSize();
              
            if ( start + lenToRead  > cwdFile.fileSize()) {
                return COMMAND_ERROR_INVALIDVALUE;
            } 
      }
      
      cwdFile.rewind();
      cwdFile.seekCur(start);
      

      uint16_t sum_from_to = lenToRead;
      uint16_t sum_n = 0;
      uint16_t crcRes = 0;
      bool readingIsOver = false;
      
      while (cwdFile.available()) {
         int n = cwdFile.fgets(cmdGetLineBuffer, 
          sum_n + sizeof(cmdGetLineBuffer) >= sum_from_to ? sum_from_to - sum_n + 1 : sizeof(cmdGetLineBuffer)  );
          
          
        if (n <= 0) {
                return COMMAND_ERROR_C_FGETS;
        }
       
        if ( sum_n + n >= sum_from_to ) {
          readingIsOver = true;
        } 

        sum_n += n;
   
        crcRes = calcCRC(crcRes, cmdGetLineBuffer, n);
        if ( readingIsOver ) 
          break;
      }
     cwdFile.close();

     
      ReturnValues::printSuccess(fileName, strBuffer);
      
      ReturnValues::printResult();
      ReturnValues::appendHtmlBody(start);
      ReturnValues::appendHtmlBody(F(" "));
      ReturnValues::appendHtmlBody(lenToRead);  
      ReturnValues::appendHtmlBodyNewLine(); 

      
      ReturnValues::printResult();
      ReturnValues::appendHtmlBody(sum_n);
      ReturnValues::appendHtmlBody(F(" "));
      ReturnValues::appendHtmlBody((crcToChk == -1) ? crcRes : (crcRes == crcToChk));   
      ReturnValues::appendHtmlBodyNewLine(); 
    } else {
        return COMMAND_ERROR_SD_FILE_EXISTS;
    }
  
  ReturnValues::printEndAndReady();
  return 0;
}


void cmdGetHelperPrintBufferInDec(char* buf, int n);
// ok
int cmdGet(CmdProc* c) {

  strcpy_P(strBuffer, getStr);
  char* fileName;
  fileName = c->GetNextToken();

  
  char* startToken;
  startToken = c->GetNextToken();

  char* lengthToken;
  lengthToken = c->GetNextToken();

  
  char* crcToken;
  crcToken = c->GetNextToken();

  int from = 0; 
  int len_from = 0; // to


   if ( startToken != NULL ) {
      if (lengthToken != NULL) {

            if ( !tryParseDec(startToken, from, false) ) {
                return COMMAND_ERROR_PARSING_ARG_6;
            }
            if ( !tryParseDec(lengthToken, len_from, false) ) {
                return COMMAND_ERROR_PARSING_ARG_7;
            }

            
      } else if (strcmp(startToken, "crc") != 0) {
                return COMMAND_ERROR_INVALIDVALUE;
      } else {
          crcToken = startToken;
      }
     
  } 

  
  cwdFile.close();
  if (sd.exists(fileName)) {
   
    if (!cwdFile.open(fileName, FILE_READ)) {
                return COMMAND_ERROR_SD_OPEN;
    } else {
            if ( len_from == 0 ) 
              len_from = cwdFile.fileSize();
              
            if ( from + len_from  > cwdFile.fileSize()) {
                return COMMAND_ERROR_INVALIDVALUE;
            } 
    }

    int sum_from_to = len_from;
     
    
    if ( crcToken != NULL ){
      strcpy_P(strBuffer, getCrcStr);
    }
    ReturnValues::printSuccess(fileName, strBuffer);
    ReturnValues::printResult();
    ReturnValues::appendHtmlBody(from);
    ReturnValues::appendHtmlBody(F(" "));
    ReturnValues::appendHtmlBody(len_from);
    ReturnValues::appendHtmlBodyNewLine();
    
    cwdFile.rewind();
    cwdFile.seekCur(from);
      
    int sum_n = 0;

    bool readingIsOver = false;
    
    uint16_t crcRes = 0;
    while (cwdFile.available()) {
      int n = cwdFile.fgets(cmdGetLineBuffer, 
          sum_n + sizeof(cmdGetLineBuffer) >= sum_from_to ? sum_from_to - sum_n + 1 : sizeof(cmdGetLineBuffer)  );
      
      if (n <= 0) {
                return COMMAND_ERROR_C_FGETS;
      }
     
      ReturnValues::printResult();
      
      if ( sum_n + n >= sum_from_to ) {
          readingIsOver = true;
      } 

          sum_n += n;
     
      
      cmdGetHelperPrintBufferInDec(cmdGetLineBuffer, n);
       ReturnValues::appendHtmlBodyNewLine();
      crcRes = calcCRC(crcRes, cmdGetLineBuffer, n);
      if ( readingIsOver ) 
        break;
        
    }

    
    
      if ( crcToken != NULL ){
        ReturnValues::printResult();
        ReturnValues::appendHtmlBody(crcRes);
        ReturnValues::appendHtmlBodyNewLine();
      }
      
      
      cwdFile.close();
   
  } else {
        return COMMAND_ERROR_SD_FILE_EXISTS_NOT;
  }

  ReturnValues::printEndAndReady();
  return 0;
}

void cmdGetHelperPrintBufferInDec(char* buf, int n) {

    if ( n == 0 ) {
      ReturnValues::appendHtmlBody(int(buf[0]));
      return;
    }

    int i;
    for( i = 0; i < n-1; ++i ) {
        ReturnValues::appendHtmlBody(int(buf[i]));
        ReturnValues::appendHtmlBody(F(" "));
    }
    
    ReturnValues::appendHtmlBody(int(buf[n-1]));
 
}


int cmdCd(CmdProc* c) {


      strcpy_P(strBuffer, cdStr);
  
  char* t;
  t = c->GetNextToken();


  bool succ = true;
  
  if (t == NULL || t == "") {
    
    ReturnValues::printSuccess(wp.get(),strBuffer);
    ReturnValues::printResult();
    ReturnValues::appendHtmlBody(wp.get());
    ReturnValues::appendHtmlBodyNewLine();
    ReturnValues::printEndAndReady();
    return 0;
    
  } else if (strcmp(t, "/") == 0) {
    if(sd.chdir())
      wp.reset();
    else
      succ = false;
  } else if (strcmp(t, "..") == 0){
    wp.pop();
    if (!sd.chdir(wp.get())){
      succ = false;
    }
  } else{
    if (sd.chdir(t))
      wp.add(t);
    else {
      succ = false;
    }
  }

  if (succ == false)
      return COMMAND_ERROR_SD_CHDIR;

  
  ReturnValues::printSuccess(wp.get(),strBuffer);
  ReturnValues::printBlankResultLineEndAndReady();
  return 0;
}




#endif
