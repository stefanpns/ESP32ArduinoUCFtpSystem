using System;
using System.IO.Ports;
using System.Threading;
using System.Text;

using System.Collections.Generic;

namespace coreapp
{

    abstract class CommandObject {

        

        protected string commandName;
        protected string filePath;
        private bool complete = false;

        

        private bool valid = true;
        
        private bool waitingForChain  = false;
        public CommandObject(string commandName, string filePath) {
            this.commandName = commandName;
            this.filePath = filePath;
        }
        public abstract bool proccess(string line);

        public bool isWaitingForTheChain() {
            return waitingForChain;
        }
        
        
        public void waitForTheChain(bool wait) {
            waitingForChain = wait;
        }

        public virtual void print(){
            Console.WriteLine( commandName + "/command:");
        }
        

        public virtual void setComplete() {
            complete = true;
        }

      
        public bool isComplete() {
            return complete;
        }

       
       public bool isValid(){
           return this.valid;
       }

        public void setValid(bool valid) {
            this.valid = valid;
        }

        public string getFilePath() {
            return filePath;
        }

        public string getName(){
            return commandName;
        }

        public virtual bool isErrorCmd() {
            return false;
        }

        protected string chainCommand = null;
        public string getChainCommand() {
            return chainCommand;
        }


        protected string chainCommandName = null;
        public string getChainCommandName() {
            return chainCommandName;
        }

        public bool checkChainCommandName(string name) {
            return name.Equals(chainCommandName);
        }


        public virtual bool checkChainCmdResult(CommandObject cmdObject) {
            return true;
        }

        public virtual string getChainSuccessMessage(){
            return "";
        } 
        public virtual string getChainUnsuccessMessage(){
            return "";
        }

        protected static void WriteLineWithIndent(string text){
            WriteIndent();
            Console.WriteLine(text);
        }
        protected static void WriteWithIndent(string text){
            WriteIndent();
            Console.Write(text);
        }
        protected static void WriteWithIndent(char ch){
            WriteIndent();
            Console.Write(ch);
        }
        protected static void WriteIndent(){
            Console.Write("->");
        }
    }

   

     class CommentCommandObject : CommandObject {
        
        private List<string> lines = new List<string>();
        public CommentCommandObject(string meta):base(ArduinoCommands.COMMENT_CMD_SYMBOL, meta){
            
        }
        public override bool proccess(string line) {
            //Console.WriteLine("dodat: " + line);
            lines.Add(line);
            return true;
        }
        public override void print(){
          base.print();
          Console.WriteLine("Meta: " + filePath );
            foreach(string line in lines)
                CommandObject.WriteLineWithIndent(line);
           
        }

    }

    class DirCommandObject : CommandObject {

        private List<string> lines = new List<string>();
        
        public DirCommandObject(string filePath):base(ArduinoCommands.DIR_CMD_SYMBOL, filePath){
            
        }
        public override bool proccess(string line) {
            lines.Add(line);
            return true;
        }
        


       public override void print(){
          base.print();         

        Console.WriteLine("Count: " + lines[lines.Count-1]);

          for(int i = 0; i < lines.Count-1; ++i )   
                CommandObject.WriteLineWithIndent(lines[i]);
            
    
           

           
        }
    }

    class HelpCommandObject : CommandObject {
        
      
        private List<string> lines = new List<string>();
        
        public HelpCommandObject(string filePath):base(ArduinoCommands.HELP_CMD_SYMBOL, filePath){
            
        }
        public override bool proccess(string line) {
            lines.Add(line);
            return true;
        }
        

       public override void print(){
          base.print();
          foreach(string line in lines)
            CommandObject.WriteLineWithIndent(line);
        }
    }


     class ErrorCommandObject : CommandObject {
        
        public ErrorCommandObject(string errorCmdName, string errno):base(errorCmdName, errno){
            
        }
        public override bool proccess(string line) {
            return true;
        }
        public override void print(){
          base.print();
            string errno = filePath;
            int errno_i = int.Parse(errno);
            if ( errno_i < 0 )
                errno_i *= -1;
            ErrorsApp.ErrnoExternalCodes errno_e = (ErrorsApp.ErrnoExternalCodes) errno_i;
            Console.WriteLine("Execution error");
            CommandObject.WriteLineWithIndent("Command name: "+ commandName );
            CommandObject.WriteLineWithIndent("Errno: " + ((int) errno_e));
            CommandObject.WriteLineWithIndent("Error description: " + ErrorsApp.get(errno_e));
        }

        public override bool isErrorCmd() {
            return true;
        }
    }

     class MdCommandObject : CommandObject {
        
        public MdCommandObject(string filePath):base(ArduinoCommands.MD_CMD_SYMBOL, filePath){
            
        }
        public override bool proccess(string line) {
            return true;
        }
        public override void print(){
          base.print();
           Console.WriteLine("Folder \\" + filePath + " has been made");
        }
    }

    class CdCommandObject : CommandObject {
        
        public CdCommandObject(string filePath):base(ArduinoCommands.CD_CMD_SYMBOL, filePath){
            
        }
        public override bool proccess(string line) {
            return true;
        }
        public override void print(){
          base.print();
          Console.WriteLine("Current working directory now is: " + base.filePath);
        }
    }

  
    class PutCommandObject : CommandObject {
        
        long writtenBytes = 0;
        public PutCommandObject(string filePath):base(ArduinoCommands.PUT_CMD_SYMBOL, filePath){
            
        }
        public override bool proccess(string line) {
            if ( !long.TryParse(line, out writtenBytes) ) {
                setValid(false);
                return false;
            }
            return true;
        }
        public override void print(){
          base.print();
          Console.Write("In the file " + filePath + " have been written " + writtenBytes + " bytes.");
         }
    }

    class GetCommandObject : CommandObject {
        
        private List<int> data = new List<int>();
        private bool isWithCrc = false;
        private int from,len;
        public GetCommandObject(string filePath, bool withCrc, string name):base(name, filePath){
            this.isWithCrc = withCrc;
            this.chainCommandName = ArduinoCommands.CRC_CMD_SYMBOL;
            //Console.WriteLine("filePath: " + filePath);
            this.chainCommand = this.chainCommandName + " " + filePath;
            from = len = -1;
        }
        public override bool proccess(string line) {

            string[] line_split = line.Split(" ");

            
            if ( from == -1 ){
                
                string fromStr = line_split[0].Trim('\n', '\r');
                string lenStr = line_split[1].Trim('\n', '\r');

                int from, len;
                
                if (int.TryParse(fromStr, out from) == false ) {
                    setValid(false);
                    ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.RESULT_PROCCESSOR_ERROR_COMMAND_PARSE);
                    return false;
                } 

                
                if (int.TryParse(lenStr, out len) == false ) {
                    setValid(false);
                    ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.RESULT_PROCCESSOR_ERROR_COMMAND_PARSE);
                    return false;
                } 

                this.from = from;
                this.len = len;

                this.chainCommand += " " + from + " " + len;

            } else {

                for(int i = 0; i < line_split.Length; i++) {
                    int res;

                    if ( int.TryParse(line_split[i], out res) == false ) {
                        setValid(false);
                        return false;
                    } 
                        data.Add(res);
                }

                /*
                if ( line.Length > 3 && (10 == (int)line[line.Length-3]) ) {

                    data.Add(13);
                    data.Add(10);
                }  
                */
            }
            
            return true;
        }

        public override void setComplete() {
            base.setComplete();

            if ( isWithCrc == true ) {

                waitForTheChain(false);
                if ( calculateCrc() != data[data.Count-1] )
                    setValid(false);
            } else {

                
                waitForTheChain(true);

            }

        }

        public int getCrc() {
            if ( isWithCrc == false )
                return calculateCrc();
            return data[data.Count-1];
        }

        public override void print(){
          base.print();

            Console.WriteLine("File: " + this.filePath + "");
            Console.WriteLine("Segment: [" + this.from + ", " + (this.from + this.len-1) + "]");
            if ( isWithCrc ) {
                Console.WriteLine("CRC16 of the segment: " + data[data.Count-1]);
            }
            
            int i = 0;
            
            CommandObject.WriteIndent();
            int rB = isWithCrc ? data.Count - 1 : data.Count;
            for ( ; i < rB; i++ ) {

                Console.Write((char)data[i]);
                if ( ( (char)data[i]) == '\n'  ) {
                    CommandObject.WriteIndent();
                }
            }
            
            
            Console.WriteLine();
           
        }


        private int calculateCrc() {
            
            int crc = 0;

            for ( int i = 0; i < data.Count-1; i++ ) {
                crc = CrcMath.crc_int(crc, data[i]);
            }

            if ( !isWithCrc ) {
                crc = CrcMath.crc_int(crc, data[data.Count-1]);
            }

            return crc;
        }

        public override bool checkChainCmdResult(CommandObject cmdObject) {

            CrcCommandObject crcCmdObject = null;
            try {
                crcCmdObject = (CrcCommandObject) cmdObject;
                //crcCmdObject.print();

                if ( crcCmdObject.getCrc() ==  getCrc() ) {
                    return true;
                } else {
                    return false;
                }
            } catch(InvalidCastException e) {
                Console.WriteLine(e);
                return false;
            }

        }

        public override string getChainSuccessMessage(){
            return "Crc is valid for this message.";
        }
        public override string getChainUnsuccessMessage(){
            return "Crc is not valid for this message.";
        }

    }

    class DelCommandObject : CommandObject {
        
        public DelCommandObject(string filePath):base(ArduinoCommands.DEL_CMD_SYMBOL, filePath){
            
        }
        public override bool proccess(string line) {
            return true;
        }
        public override void print(){
          base.print();
           Console.WriteLine("File: " + filePath + " has been erased.");
           
        }
    }

    class CrcCommandObject : CommandObject {
        

        private int fileSize, crcArg;

        private int from, len;
        public CrcCommandObject(string filePath):base(ArduinoCommands.CRC_CMD_SYMBOL, filePath){
            from = -1;
            len = -1;
        }
        public override bool proccess(string line) {

                string[] msgs = line.Split(' ');

                if ( from == -1 ){
                    
                    string fromStr = msgs[0].Trim('\n', '\r');
                    string lenStr = msgs[1].Trim('\n', '\r');

                    int from, len;
                    
                    if (int.TryParse(fromStr, out from) == false ) {
                    setValid(false);
                        ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.RESULT_PROCCESSOR_ERROR_COMMAND_PARSE);
                        return false;
                    } 

                    
                    if (int.TryParse(lenStr, out len) == false ) {
                    setValid(false);
                        ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.RESULT_PROCCESSOR_ERROR_COMMAND_PARSE);
                        return false;
                    } 

                    this.from = from;
                    this.len = len;

                } else {

                    string fileSizeStr = msgs[0].Trim('\n', '\r');
                    string crcArgStr = msgs[1].Trim('\n', '\r');

                    int fileSize, crcArg;

                    if (int.TryParse(fileSizeStr, out fileSize) == false ) {
                    setValid(false);
                        ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.RESULT_PROCCESSOR_ERROR_COMMAND_PARSE);
                        return false;
                    } 

                    
                    if (int.TryParse(crcArgStr, out crcArg) == false ) {
                    setValid(false);
                        ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.RESULT_PROCCESSOR_ERROR_COMMAND_PARSE);
                        return false;
                    } 

                    this.fileSize = fileSize;
                    this.crcArg = crcArg;
                }


            return true;
        }
        public override void print(){
          base.print();
            
            Console.WriteLine("File: " + this.filePath + "");
            Console.WriteLine("Segment: [" + this.from + ", " + (this.from + this.len-1) + "]");
            Console.Write("Crc: ");
           if ( this.crcArg == 0 || this.crcArg == 1 ) {
                Console.WriteLine((this.crcArg == 1 ? "" : "not ")  +"valid");
            } else 
                Console.WriteLine(this.crcArg);
           
        }

        public int getCrc() {
            return this.crcArg;
        }


       
    }

    class NOTVALIDCommandObject : CommandObject {

        
        public NOTVALIDCommandObject(string filePath):base(ArduinoCommands.NOT_VALID_CMD_SYMBOL, filePath){
            setValid(false);
        }
        public override bool proccess(string line) {
            return true;
        }
        


       public override void print(){
        }
    }



    class ArduinoCommands {

        public static readonly string DIR_CMD_SYMBOL = "dir";
        public static readonly string HELP_CMD_SYMBOL = "help";
        public static readonly string CD_CMD_SYMBOL = "cd";
        public static readonly string PUT_CMD_SYMBOL = "put";
        public static readonly string GET_CMD_SYMBOL = "get";
        public static readonly string GET_CRC_CMD_SYMBOL = "getcrc";
        
        public static readonly string DEL_CMD_SYMBOL = "del";
        public static readonly string CRC_CMD_SYMBOL = "crc";
        public static readonly string MD_CMD_SYMBOL = "md";
        public static readonly string ERROR_CMD_SYMBOL = "er";
        public static readonly string COMMENT_CMD_SYMBOL = "comment";
        public static readonly string NOT_VALID_CMD_SYMBOL = "NOTVALID";

        



    }



}
