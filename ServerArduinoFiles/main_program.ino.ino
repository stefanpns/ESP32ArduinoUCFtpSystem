#include <SPI.h>
#include <WiFi.h>
#include <SdFat.h>


#include "utilis.h"
#include "cmdProc.h"
#include "sdCardManipulator.h"
#include "returnValues.h"
#include "ByteArray.h"
#include "recieveCommand.h"

//#define SECRET_SSID "HUAWEI-B311-535B"
//#define SECRET_PASS "4H5D514AQQQ"
#define SECRET_SSID "desibre"
#define SECRET_PASS "desibre1"
#define BUFFER_LEN 10000


void printWifiStatus();

CmdProc Commands;
WiFiServer server(80);
ByteArray sbuf(BUFFER_LEN);
const int chipSelect = 5;


RecieveAndAnswerCommand* recieveAndAnswerCommand = NULL;

void setup() {
  char ssid[] = SECRET_SSID;
  char pass[] = SECRET_PASS;
  bool unableToInitCommunicationWSdCard = false;
  bool unableToOpenTheRoot = false;


  Serial.begin(9600);
  while (!Serial);
  delay(400);
  
  int status = WL_IDLE_STATUS;
  while (status != WL_CONNECTED) {
    Serial.print("Connecting to ");
    Serial.println(SECRET_SSID);
    status = WiFi.begin(ssid, pass);
    delay(5000);
  }

  
  server.begin();

  printWifiStatus();


  if (!sd.begin(chipSelect, SPI_HALF_SPEED)) {
      unableToInitCommunicationWSdCard = true;
      Serial.println("unable to init communication with SdCard");
  }

  
  if (!cwdFile.open("/")) {
    unableToOpenTheRoot = true;
    Serial.println("Error: unable to open the root");
  } else {
    cwdFile.close();
  }
  

  
  Commands.Init(8);   
  Commands.Add(helpStr, cmdHelp, 1, 1);
  Commands.Add(mdStr, cmdMd, 2, 2); 
  Commands.Add(dirStr, cmdDir, 1, 2); 
  Commands.Add(cdStr, cmdCd, 1, 2); 
  Commands.Add(getStr, cmdGet, 2, 5);
  Commands.Add(delStr, cmdDel, 2, 2); 
  Commands.Add(crcStr, cmdCrc, 2, 5); 
  Commands.Add(putStr, cmdPut, 3, 104); 
  
  recieveAndAnswerCommand = new RecieveAndAnswerCommand(&server, &Commands, unableToInitCommunicationWSdCard, unableToOpenTheRoot);

}


void loop() {
  recieveAndAnswerCommand->proccess();
}


void printWifiStatus() {

  Serial.print("SSID: ");
  Serial.println(WiFi.SSID());


  IPAddress ip = WiFi.localIP();
  Serial.print("IP Address: ");
  Serial.println(ip);

  long rssi = WiFi.RSSI();

  Serial.print("signal strength (RSSI):");
  Serial.print(rssi);
  Serial.println(" dBm");
}
