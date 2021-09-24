#ifndef __RETURN_VALUES_H__
#define __RETURN_VALUES_H__



#include <WiFi.h>
#include <Arduino.h>
#include <pgmspace.h>


class ReturnValues {

  public:
    static WiFiClient* client;

  public:
     static void setClient(WiFiClient* client) {
        ReturnValues::client = client;
     }
     static void unsetClient(){
        ReturnValues::client = NULL;
     }


  public:
    static void printStandard404Msg() {
      
          ReturnValues::printHeader404();
          ReturnValues::printContentAndConnectionType();
          ReturnValues::client->println("Not valid header");
      
    }
     
  public:
    static void printHeader200() {

            ReturnValues::client->println(F("HTTP/1.1 200 OK"));
            
    }
    static void printHeader404() {

            ReturnValues::client->println(F("HTTP/1.1 404 Not Found"));
            
    }

    static void printContentAndConnectionType() {
            ReturnValues::client->println("Content-Type: text/plain");
            ReturnValues::client->println("Connection: close");
            ReturnValues::client->println("Refresh: 5");  
            ReturnValues::client->println();
    }
    
    static void printHtmlHeader() {

            ReturnValues::client->println("<!DOCTYPE HTML>");
            ReturnValues::client->println("<html>");
            ReturnValues::client->println("<body>");
            
    }

    static void appendHtmlBodyNewLine() {
            ReturnValues::client->print('\n');        
    }
    
    static void appendHtmlBody(const __FlashStringHelper* text) {
            ReturnValues::client->print(text);            
    }
    
    static void appendHtmlBody(const int number) {
            ReturnValues::client->print(number);            
    }
    
    static void appendHtmlBody(const char* text) {
              ReturnValues::client->print(text);            
    }
    
    
    static void printHtmlFooter() {
            ReturnValues::client->println("</body>");
            ReturnValues::client->println("</html>"); 
    }

    /*
    static void send(const char* body) {
            ReturnValues::printHeader();
            ReturnValues::printHtmlHeader();
            ReturnValues::appendHtmlBody(client, body);
            ReturnValues::printHtmlFooter(client);
    }
    */
    
  public:
     static void printOK() {ReturnValues::appendHtmlBody(F("#OK"));}
     static void printNOTOK() {ReturnValues::appendHtmlBody(F("#ERROR"));}
     static void printBlankResultLineEndAndReady() {
      ReturnValues::printResult();
      ReturnValues::appendHtmlBodyNewLine();
      ReturnValues::printEndAndReady();
    }
     static void printEndAndReady() {
      ReturnValues::appendHtmlBody(F("#END"));
      ReturnValues::appendHtmlBodyNewLine();
      ReturnValues::appendHtmlBody(F("$"));
      ReturnValues::appendHtmlBodyNewLine();
      
    }
     static void printResult() {ReturnValues::appendHtmlBody(F(">"));}


  public:
    static void printComment(const __FlashStringHelper* comment) {
      
        ReturnValues::printSuccess(F("1"), F("comment"));
        ReturnValues::printResult();
        ReturnValues::appendHtmlBody(comment);
        ReturnValues::appendHtmlBodyNewLine();
        ReturnValues::printEndAndReady();
        
    }
    
    static void printErrorV2(const char* cmdName, int err) {

        ReturnValues::printNOTOK();
        ReturnValues::appendHtmlBody(F(" "));
        ReturnValues::appendHtmlBody(cmdName);
        ReturnValues::appendHtmlBody(F(" "));
        ReturnValues::appendHtmlBody(err);
        ReturnValues::appendHtmlBodyNewLine();
        ReturnValues::printBlankResultLineEndAndReady();
    }

    static void printErrorV2(const __FlashStringHelper* arg, int err) {

        ReturnValues::printNOTOK();
        ReturnValues::appendHtmlBody(F(" "));
        ReturnValues::appendHtmlBody(arg);
        ReturnValues::appendHtmlBody(F(" "));
        ReturnValues::appendHtmlBody(err);
        ReturnValues::appendHtmlBodyNewLine();
        ReturnValues::printBlankResultLineEndAndReady();
    }


    
  static void printSuccess(const __FlashStringHelper* arg, const __FlashStringHelper* command) {

        ReturnValues::printOK();
        ReturnValues::appendHtmlBody(F(" "));
        ReturnValues::appendHtmlBody(command);
        ReturnValues::appendHtmlBody(F(" "));
        ReturnValues::appendHtmlBody(arg);
        ReturnValues::appendHtmlBodyNewLine();
    }

    static void printSuccess(const char* arg, const char* command) {

        ReturnValues::printOK();
        ReturnValues::appendHtmlBody(F(" "));
        ReturnValues::appendHtmlBody(command);
        ReturnValues::appendHtmlBody(F(" "));
        ReturnValues::appendHtmlBody(arg);
        ReturnValues::appendHtmlBodyNewLine();
    }

     static void printSuccess(const char* parent, const char* child, const char* command) {

        ReturnValues::printOK();
        ReturnValues::appendHtmlBody(F(" "));
        ReturnValues::appendHtmlBody(command);
        ReturnValues::appendHtmlBody(F(" "));
        ReturnValues::appendHtmlBody(parent);
        ReturnValues::appendHtmlBody(child);
        ReturnValues::appendHtmlBodyNewLine();
    }

    static void printHelloMessage(){
       ReturnValues::printComment(F("@sd card demo projekat"));
    }

    static void send(char* text) {
        client->println(text);
    }
    
};


#endif
