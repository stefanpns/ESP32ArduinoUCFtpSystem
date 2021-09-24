#ifndef __CWDPATH_H__
#define __CWDPATH_H__
#define CURRENT_WORKING_NAME_SECTOR_SUM_LENGTH 100

class CwdPath {

  public:
    CwdPath(){
       reset();
    }

    int add(char path[]) {
      int len = strlen(path);
      
      if(strlen(result) + len + 1> CURRENT_WORKING_NAME_SECTOR_SUM_LENGTH)
        return -1;
      else {
        
        strcat(result, path);
        
        if (path[len-1] != '/'){
          strcat(result,"/");
        }
        
      }
    }

    void reset() {
       result[0] = '/';
       result[1] = '\0';
    }
    char* get() {
      return result;
    }

    void pop() {

      int len = strlen(result);
      int rLength = len -1;
      while(result[rLength] != '/') {
        rLength--;
      }

      if (rLength !=0 ) {
          rLength-=1;
          while(result[rLength] != '/') {
            rLength--;
          }
      }
      
      rLength++;
      result[rLength] = '\0';

    }

  private:
    char result[CURRENT_WORKING_NAME_SECTOR_SUM_LENGTH]; 
  
};
#endif
