#ifndef __UTILIS_H__
#define __UTILIS_H__

bool tryParseDec(char *s, int &outResult, bool allowMinus);
bool tryParseInt(char *s, int &outResult);

bool tryParseInt(char *s, int &outResult) {
  if (!tryParseDec(s, outResult, true))
    return false;
  else
    return true;
}

bool tryParseDec(char *s, int &outResult, bool allowMinus) {
  outResult = 0;
  int n = strlen(s);
  int sign = 1;
  int a = 0;
  if ((allowMinus) && (s[0] == '-')) {
    sign = -1;
    a = 1;
  }
  for (int i = a; i < n; i++) {
    if (isDigit(s[i])) {
      outResult = 10 * outResult + sign * (s[i] - '0');
    }
    else
      return false;
  }
  return true;
}

bool charArrrayEqualsTo(const char* arr1, const char* arr2){

   while(*arr1 != '\0' && *arr2 != '\0') {
      if ( *(arr1++) != *(arr2++) ){
        return false;
      }
   }

   if ( *arr1 != '\0' || *arr2 != '\0' )
    return false;
    
   return true;
  
}

int getCharArraySize(const char* charArray){
  int res = 0;
  while( *(charArray++) != '\0' )
    res++;

  return res;
}

bool charArrrayStartsWith(const char* charArray, const char* prefix){

   while(*charArray != '\0' && *prefix != '\0') {
      if ( *(charArray++) != *(prefix++) )
       return false;
   }
   return true;
  
}
#endif
