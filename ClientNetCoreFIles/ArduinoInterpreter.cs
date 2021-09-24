using System;
using System.IO.Ports;
using System.Threading;
using System.Text;
using System.Collections.Generic;

namespace coreapp
{


    abstract class ArduinoLineProccesor {
        public abstract int proccess(string line, ProccessorObjectResult obj);
        
    }

    

    class HeaderLineProccessor : ArduinoLineProccesor {


        public static int valid(string line, ProccessorObjectResult por) {

            bool isError = false;
            if (line.StartsWith(ArduinoInterpreter.START_SYMBOL)) {
                
                string[] msgs = line.Split(' ');

                if ( msgs.Length != 3 ){
                    Console.WriteLine("Header proccessor: Line starts with # but do not succeed with three arguments [help: documentation].");
                    ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_HEADER);
                    return -1;
                } 

                

                string status = null;
                string commandName = null;

                status = (msgs[0]).Substring(1);
                commandName = msgs[1];

                string filePath = "";
                int errnoCmd = 0;

                if(status.Equals(ArduinoInterpreter.OK_MSG)){

                    filePath = msgs[2].Trim('\n', '\r');

                }else if(status.Equals(ArduinoInterpreter.ERROR_MSG)) {
                    
                    string errnoTextNumber = msgs[2].Trim('\n', '\r');

                    if (int.TryParse(errnoTextNumber, out errnoCmd) == false ) {
                        ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_HEADER);
                        return -1;
                    } 

                    if ( ErrorsApp.errorExist(errnoCmd) == false) {
                        
                        ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_HEADER);
                        return -1;
                    }

                    filePath = "";
                    isError = true;
                    
                     
                } else {
                    ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_HEADER);
                    return -1;
                }

                CommandObject cmdObj = null;

                if ( isError ) {

                    cmdObj = new ErrorCommandObject(commandName, errnoCmd.ToString());

                } else {
                    if ( commandName.Equals(ArduinoCommands.COMMENT_CMD_SYMBOL)){
                        cmdObj = new CommentCommandObject(filePath);
                    }else if ( commandName.Equals(ArduinoCommands.DIR_CMD_SYMBOL)){
                        cmdObj = new DirCommandObject(filePath);
                    } else if ( commandName.Equals(ArduinoCommands.HELP_CMD_SYMBOL)){
                        cmdObj = new HelpCommandObject(filePath);
                    } else if ( commandName.Equals(ArduinoCommands.CD_CMD_SYMBOL)){
                        cmdObj = new CdCommandObject(filePath);
                    } else if ( commandName.Equals(ArduinoCommands.PUT_CMD_SYMBOL)){
                        cmdObj = new PutCommandObject(filePath);

                    } else if ( commandName.Equals(ArduinoCommands.GET_CMD_SYMBOL)){
                        cmdObj = new GetCommandObject(filePath, false, ArduinoCommands.GET_CMD_SYMBOL);

                    } else if ( commandName.Equals(ArduinoCommands.GET_CRC_CMD_SYMBOL)){
                        cmdObj = new GetCommandObject(filePath, true, ArduinoCommands.GET_CRC_CMD_SYMBOL);

                    } else if ( commandName.Equals(ArduinoCommands.DEL_CMD_SYMBOL)){
                        cmdObj = new DelCommandObject(filePath);

                    } else if ( commandName.Equals(ArduinoCommands.CRC_CMD_SYMBOL)){
                        cmdObj = new CrcCommandObject(filePath);

                    } else if ( commandName.Equals(ArduinoCommands.MD_CMD_SYMBOL)){
                        cmdObj = new MdCommandObject(filePath);

                    } else {
                        ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_UNKNOWN_CMD);
                        return -1;
                    }

                }

                por.set(cmdObj);
            
                return (int)ArduinoInterpreter.modes.PAYLOAD_PROCCESSOR;

            } else {
                    return ArduinoInterpreter.NOT_A_START_LINE;
            }

            
        } 
        public override int proccess(string line, ProccessorObjectResult obj) {
            return HeaderLineProccessor.valid(line, obj);
        }
    }

    class FooterLineProccessor : ArduinoLineProccesor {



        public static int valid(string line, ProccessorObjectResult por) {
            
              if (line.Equals(ArduinoInterpreter.END_SYMBOL)) {                    
                    
                    CommandObject cObjResult1 = por.get();

                    if (cObjResult1 != null ){
                        cObjResult1.setComplete();
                        return (int)ArduinoInterpreter.modes.MSG_COMPLETE;
                    } else {
                        Console.WriteLine("ArduinoInterpreter: Unhandled case 1.");
                        return -1;
                    }

              } else {
                  return -1;
              }

        }
        public override int proccess(string line, ProccessorObjectResult obj) {
            return FooterLineProccessor.valid(line, obj);
        }
        

    }


    class ResultLineProccessor : ArduinoLineProccesor {



        public static int valid(string line, ProccessorObjectResult por) {

            if ( line.StartsWith(ArduinoInterpreter.RESULT_LINE_SYMBOL) ){
                
                line = line.Substring(1);
                if ( por.get().proccess(line) ){
                    return (int)ArduinoInterpreter.modes.PAYLOAD_PROCCESSOR;
                } else {
                    por.get().setValid(false);
                    ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.RESULT_PROCCESSOR_ERROR_COMMAND_PARSE);
                    return -1;
                }

            }   else {
                    ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.RESULT_PROCCESSOR_ERROR_RESULT_LINE_MISSING);
                    return -1;
            }
        }
        public override int proccess(string line, ProccessorObjectResult obj) {
            return ResultLineProccessor.valid(line, obj);
        }
        

    }


    
    class ProccessorObjectResult {

        private CommandObject cmdObj = null;

        public ProccessorObjectResult() {
        }

        public void set(CommandObject cmdObj){
            this.cmdObj = cmdObj;
        }
        public CommandObject get(){
            return cmdObj;
        }

    }

    
    class ArduinoInterpreter {

        
        public static readonly string OK_SYMBOL = "#OK";
        public static readonly string ERR_SYMBOL = "#ERROR";
        public static readonly char START_SYMBOL = '#';
        public static readonly char COMMENT_SYMBOL = '@';
        public static readonly char RESULT_LINE_SYMBOL = '>';
        public static readonly string END_SYMBOL = "#END";
        public static readonly string READY_SYMBOL = "$";
        public static readonly string CRC_SYMBOL = "^";
        public static readonly string MSG_COMPLETE_SYMBOL = "%";
        



        public static readonly string OK_MSG = "OK";
        public static readonly string ERROR_MSG = "ERROR";


        public const int NOT_A_START_LINE = 1000;


        public enum modes {

            HEADER_PROCCESSOR = 0,
            PAYLOAD_PROCCESSOR,
            FOOTER_PROCCESSOR,
            MSG_COMPLETE
        }
    




        private static ArduinoLineProccesor[] lineProccesors = {
            new HeaderLineProccessor(), 
            new ResultLineProccessor(),
            new FooterLineProccessor()
        };
        
        public static ArduinoLineProccesor GetLineProccesor(int i) {
            return lineProccesors[i];
        }
        public CommandObject proccess(ResultMessageBuilder sb) {
            


            ProccessorObjectResult por = new ProccessorObjectResult();
            ArduinoLineProccesor lineProccesor = null;

            

            int res;
            string line;


            lineProccesor = lineProccesors[(int)modes.HEADER_PROCCESSOR];
            HeaderPartitionBuilder hBuilder = sb.getHeaderBuilder();
            line = hBuilder.get();
            if ( (res = lineProccesor.proccess(line, por)) != (int)modes.PAYLOAD_PROCCESSOR ) {

                ErrorsApp.print();
                return null;

            } 



            lineProccesor = lineProccesors[(int)modes.PAYLOAD_PROCCESSOR];

            PayloadPartitionBuilder pBuilder = sb.getPayloadBuilder();
            pBuilder.resetLinePos();
            while ( (line = pBuilder.getNextLine()) != null) {
                
                if ( por.get() == null ){
                    Console.WriteLine("ArduinoInterpreter: Unhandled case 2.");
                    break;
                }
                 
                if ( por.get().isErrorCmd() ) {
                    break;
                } 


                bool isGetCmd = por.get().getName() == ArduinoCommands.GET_CMD_SYMBOL;

                if ( !isGetCmd ) 
                    line = line.Trim();                

                if ( String.IsNullOrEmpty(line) ){
                    continue;
                }
                    

                if ( ( lineProccesor.proccess(line, por) ) != (int)modes.PAYLOAD_PROCCESSOR ) {
                    ErrorsApp.print();
                    return null;
                }   

                
               

            }



            lineProccesor = lineProccesors[(int)modes.FOOTER_PROCCESSOR];
            FooterPartitionBuilder fBuilder = sb.getFooterBuilder();
            line = fBuilder.get();
            if ( (res = lineProccesor.proccess(line, por)) != (int)modes.MSG_COMPLETE ) {
                ErrorsApp.print();
                return null;
            }


            return por.get();
        }

        
        
    }



}
