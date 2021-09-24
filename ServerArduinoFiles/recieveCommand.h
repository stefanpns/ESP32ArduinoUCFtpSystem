#ifndef __RECIEVECOMMAND_H__
#define __RECIEVECOMMAND_H__

#define BUFFER_LEN_LINE 100

#include <SPI.h>
#include <WiFi.h>
#include "ByteArray.h"



class RecieveAndAnswerCommand {

    private:
      bool isPostRequest = false;
      int contentLengthInt = 0;
      bool unableToInitCommunicationWSdCard = false;
      bool unableToOpenTheRoot = false;
      WiFiServer* server = NULL;
      ByteArray currentLineBuffer;
      ByteArray currentCommandBuffer;
      ByteArray currentContentLengthBuffer;
      CmdProc* Commands = NULL;
      
    public:
      RecieveAndAnswerCommand(WiFiServer* server, CmdProc* Commands, bool unableToInitCommunicationWSdCard, bool unableToOpenTheRoot)
          : currentLineBuffer(BUFFER_LEN_LINE),
          currentCommandBuffer(BUFFER_LEN_LINE),
          currentContentLengthBuffer(BUFFER_LEN_LINE){
        this->server = server;
        this->Commands = Commands;
        this->unableToInitCommunicationWSdCard = unableToInitCommunicationWSdCard;
        this->unableToOpenTheRoot = unableToOpenTheRoot;
      }
    
    private:
    bool interpret() {
              
              Serial.print("currentCommand:");
              Serial.println(currentCommandBuffer.buffer);
              ReturnValues::printHeader200();
              ReturnValues::printContentAndConnectionType();
              ReturnValues::printHtmlHeader();

              int rez = Commands->Parse(currentCommandBuffer.buffer);
              
              if ( rez != 0 ) {
        
                int pos = getCmdName(currentCommandBuffer.buffer);
                (currentCommandBuffer.buffer)[pos] = '\0';
                
                ReturnValues::printErrorV2(currentCommandBuffer.buffer, rez);
              } 
              //currentCommandBuffer.clear();
              ReturnValues::printHtmlFooter();

              return rez == 0 ? true : false;
              
      
    }
              

    public:
    void proccess() {


      bool isPostRequest = false;
      bool contentTypeValid = false;
      bool contentLengthValid = false;
      bool payloadStarted = false;
      int contentLengthParsed = -1;
      char c;
      
      currentLineBuffer.clear();
      currentCommandBuffer.clear();
      currentContentLengthBuffer.clear();
    
      WiFiClient client = server->available();
      ReturnValues::setClient(&client);
    
      if (client) {
    
        Serial.println("new client");

    
        while (client.connected()) {

    
          if (client.available()) {
    

                
            c = client.read();
            //Serial.write((int)c);
            

            if ( contentLengthParsed > 0 )
              contentLengthParsed--;
             
            //Serial.println(); 
            //Serial.print("contentLengthParsed: ");
            //Serial.println(contentLengthParsed);

            
            if ( payloadStarted && contentLengthValid && contentLengthParsed == 0 ) {

              if (currentLineBuffer.append(c) == -1){
                    ReturnValues::printStandard404Msg();
                    break;
              }

              //Serial.println("NOW WE GONNA EVALUATE.");
              if ( currentLineBuffer.isNullTerminated() == 0 ) {
                currentLineBuffer.nullTerminate();
              }

              if (
                charArrrayStartsWith(currentLineBuffer.buffer, "commandRequest=")
                && isPostRequest
                && contentTypeValid) {
                  
                  //Serial.println("Line starts with commandRequest.");
                  
                  for ( int i = getCharArraySize("commandRequest="); i < currentLineBuffer.getLength(); i++ ) {
                    currentCommandBuffer.append(currentLineBuffer[i]);
                  }
                  currentCommandBuffer.nullTerminate();
                  
                   if ( printSdCardErrorIfExist() == false) {
                    interpret();
                  }
              
              } else {
                    ReturnValues::printStandard404Msg();
              }

             
              break;
            }

          
    
            if (c == '\n') {
    
              if ( currentLineBuffer.getLength() != 0 ) {
                  
                currentLineBuffer.nullTerminate();
               
                if (!isPostRequest 
                        && charArrrayStartsWith(currentLineBuffer.buffer, "POST")) {

                  //Serial.println("Found post line");
                  isPostRequest = true;
                  
                } else if (!contentTypeValid
                        && charArrrayEqualsTo(currentLineBuffer.buffer, "Content-Type: text/plain")) {
                          
                  //Serial.println("Found content-type line");
                  contentTypeValid = true;
                  
                }else if (!contentLengthValid 
                        && charArrrayStartsWith(currentLineBuffer.buffer, "Content-Length: ")) {

                  //Serial.println("Found content-length line");
                  for ( int i = getCharArraySize("Content-Length: "); i < currentLineBuffer.getLength(); i++ ) {
                    currentContentLengthBuffer.append(currentLineBuffer[i]);
                  }
                  currentContentLengthBuffer.nullTerminate();

                  if ( tryParseInt(currentContentLengthBuffer.buffer, contentLengthParsed)
                        && contentLengthParsed < BUFFER_LEN_LINE ) {
                          
                          contentLengthValid = true;
                          contentLengthParsed += 2;
                                  // +2 for the new line between content-length and payload and newline after payloads
                          payloadStarted = true;
                  }
                  
                  
                  
                } 
    
                
                currentLineBuffer.clear();
              }
    
            } else if (c != '\r') {
              if (currentLineBuffer.append(c) == -1){
                    ReturnValues::printStandard404Msg();
                    break;
              }
            }

            

          }
    
        }
    
        delay(1);
    
    
        client.stop();
        
    
        Serial.println("client disonnected");
      } 
      
      ReturnValues::unsetClient();
    }
   
    private:
      int getCmdName(char* buf) {
        char* input = buf;
        int i = 0;
        int len = strlen(input);
        while (i <= len && input[i] != ' ') {
            i++;
        }
  
        if ( i == len )
          return -1;
        return i;
    }

   

    private:
      bool printSdCardErrorIfExist() {
        
        if ( unableToInitCommunicationWSdCard || unableToOpenTheRoot ) {

                
                ReturnValues::printHeader200();
                ReturnValues::printContentAndConnectionType();
                ReturnValues::printHtmlHeader();
      
                
                ReturnValues::printErrorV2(F("sdCardModule"), 10); 
                
                /*
                if ( unableToInitCommunicationWSdCard ) {
                   ReturnValues::printErrorV2(F("sdCardModule"), 10);                        
                }
      
                if ( unableToOpenTheRoot ) {
                   ReturnValues::printComment(F("Error: unable to open the root"));
                }
                */
      
              ReturnValues::printHtmlFooter();
              return true;
        }
      
          return false;
      }


};

#endif
