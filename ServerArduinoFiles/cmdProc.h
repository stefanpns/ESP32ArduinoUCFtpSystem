#ifndef __CMDPROCV1_H__
#define __CMDPROCV1_H__

#include <Arduino.h>
#include <pgmspace.h>
#include <string.h>
#include <SdFat.h>

#include "errorCodes.h"

class CmdProc;

typedef int (*cmdCallback)(CmdProc*);  // tip za callback funkciju

class Cmd {
  public:
    const char* cmdName;
    cmdCallback callback;
    Cmd() { };

    void Set(const char *cmdName, cmdCallback cbk, int minTokens, int maxTokens) {
      this->cmdName = cmdName;
      callback = cbk;
      this->minTokens = minTokens;
      this->maxTokens = maxTokens;
    }

    int GetMinTokens() {
      return minTokens;
    }

    int GetMaxTokens() {
      return maxTokens;
    }

  private:
    int minTokens, maxTokens;
};


class CmdProc {
  public:
    void Init(int count) {
      this->count = count;
      commands = new Cmd[count];
      current = 0;
      
    }


    void Add(const char* command, cmdCallback cbk) {
      Add(command, cbk, 0, 0);
    }

    void Add(const char* command, cmdCallback cbk, int minTok, int maxTok) {
      if (current < count) {
        commands[current++].Set(command, cbk, minTok, maxTok);
      }
    }



    int Exec() {
      char* pch = GetNextToken();
      if (pch != nullptr) {
        for (int j = 0; j < count; j++) {
          if (strcmp_P(pch, commands[j].cmdName) == 0) {
            int rez = 0;
            int mi = commands[j].GetMinTokens();
            int ma = commands[j].GetMaxTokens();
            if ((mi > 0) && (ma > 0)) {
              rez = CheckTokenCount(mi, ma);
            }
            if (rez == 0)
              return commands[j].callback(this);
            else
              return rez;
          }
        }
      }
      return COMMAND_ERROR_UNKNOWNCOMMAND;
    }


    int Parse(char *s) {
      tokenCount = 0;
      input = s;
      int i = 0;
      len = strlen(input);
      quote = false;
      bool sep = false;   // previous char was separator (space or null)
      while (i <= len) {
        if (input[i] == '"') {
          quote = !quote;
          i++;
          continue;
        }
        if ((!quote) && ((input[i] == ' ') || (input[i] == 0))) {
          input[i++] = 0;
          tokenCount++;
          while ((i <= len) && ((input[i] == ' ') || (input[i] == 0))) {
          input[i++] = 0;
          }
          continue;
        }
        i++;
      }
      curr = nullptr;
      pos = 0;
      quote = false;
      return Exec();
    }
    char* GetNextToken() {
      if (curr != nullptr) {
        while ((pos < len) && (input[pos] != 0))
          pos++;
      }
      while ((pos < len) && (input[pos] == 0)) {
        pos++;
      }
      if (input[pos] != 0) {
        curr = &input[pos];
        return curr;
      }
      else {
        return nullptr;
      }
    }




    int GetTokenCount() {
      return tokenCount;
    }



    int CheckTokenCount(int min, int max) {
      if (tokenCount < min)
        return COMMAND_ERROR_MISSINGARGUMENT;
      if (tokenCount > max)
        return COMMAND_ERROR_EXTRAARGUMENT;
      return 0;
    }



  private:
    int count;
    int current;
    Cmd* commands;
    char* input;
    int tokenCount;
    int currentToken;
    char *curr;
    int pos;
    int len;    // length of input buffer
    bool quote; // whether we are inside a quote-delimited string
};




#endif
