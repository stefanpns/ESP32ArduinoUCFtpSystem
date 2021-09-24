using System;
using System.IO.Ports;
using System.Threading;

using System.Text;

namespace coreapp
{

    
    


    // Arduino - Interpreter and writer
    class ArduinoIW {



        ResultMessageBuilder rlp = new ResultMessageBuilder();
        CommandObject previousResult = null;

        public ArduinoIW() {}


        public CommandObject interpret(TalkBuffer sb) {


            ArduinoInterpreter aci = new ArduinoInterpreter(); 
            CommandObject result = null;
            
        
            if ( false == interpretBuffer(sb) ) {
                return new NOTVALIDCommandObject("NOTVALID");
            }


            if ( rlp.isComplete() == false ) {
                return null;
            }



            result = aci.proccess(rlp);
            
            if ( result == null || result.isComplete() || (result.isValid()==false)) {
                resetResultBuilder();
            } 

            return result;
        }

        public CommandObject getPreviousCommandObject() {
            return previousResult;
        }
        public void setPreviousCommandObject(CommandObject previousResult) {
            this.previousResult = previousResult;
        }

        public void resetResultBuilder() {
            rlp = new ResultMessageBuilder();
        }

        private bool interpretBuffer(TalkBuffer sb) {
        
            while(true) {
                StringBuilder lineBuilder = new StringBuilder();
                int ch;
                while((ch = sb.read()) != -1 && ch != '\n') {
                    lineBuilder.Append((char)ch);
                }

                if ( ch == '\n')
                    lineBuilder.Append('\n');
                //Console.WriteLine("lineBuilder: " + lineBuilder.ToString());



                if ( -1 == rlp.build(lineBuilder.ToString()) )
                    return false;


                if ( rlp.isComplete() || ch == -1 )
                    return true;
            }
            


        }


    }

}
