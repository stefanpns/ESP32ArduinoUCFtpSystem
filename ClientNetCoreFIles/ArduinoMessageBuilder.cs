using System;
using System.IO.Ports;
using System.Threading;
using System.Text;

using System.Collections.Generic;

namespace coreapp
{



    abstract class MessagePartitionBuilder {


        protected int mode = 0;
        protected bool completed = false;

        protected int linePos = 0;

        public MessagePartitionBuilder() {
        }
        public abstract int build(string line);
        public bool isComplete() {
            return completed;
        }
        public virtual void setComplete() {
            completed =true;
        }

        public int getMode() {
            return this.mode;
        }

        public void setMode(int n){
            this.mode = n;
        }

        public abstract string get();

        public abstract int numModes();



        public abstract string getNextLine();
        public void resetLinePos() {
            linePos = 0;
        }
        public int getLinePos() {
            return linePos;
        }
        public void incLinePos() {
            linePos++;
        }

    }
    
    class HeaderPartitionBuilder : MessagePartitionBuilder {

        

        private StringBuilder statusPart = new StringBuilder();
        private StringBuilder cmdPart = new StringBuilder();
        private StringBuilder filePart = new StringBuilder();


        enum modes {

            STATUS_MODE = 0,
            CMD_NAME_MODE,
            FILE_PATH_MODE,

        }

        public override int numModes(){
            return Enum.GetNames(typeof(modes)).Length;
        }

        

        public void buildStatus(char ch) {
            statusPart.Append(ch);
        }
        public string getStatus() {
            return statusPart.ToString();
        }
        
        public void buildCmdName(char ch) {
            cmdPart.Append(ch);
        }
        public string getCmdName() {
            return cmdPart.ToString();
        }
        public void buildCmdFilePath(char ch) {
            filePart.Append(ch);
        }
        public string getCmdFilePath() {
            return filePart.ToString();
        }



        public override int build(string line) {

                if ( line.StartsWith(ArduinoInterpreter.RESULT_LINE_SYMBOL) ) {
                        this.setComplete();
                        return 0;
                }
            
                if ( getMode() > numModes() ) {
                    ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_HEADER);
                    return -1;
                }
            

                int i = 0;
                for ( ; i < line.Length;++i ) {


                    if ( line[i] == ' ' ) {
                        mode++;
                    } 


                    if ( mode == (int)modes.STATUS_MODE ) {
                        buildStatus(line[i]);
                    }
                    else if ( mode == (int)modes.CMD_NAME_MODE ){
                        buildCmdName(line[i]);
                    }
                    else if ( mode == (int)modes.FILE_PATH_MODE) {
                        buildCmdFilePath(line[i]);
                    } else {
                        ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_HEADER);
                        return -1;
                    }

                }


            return i;
        }

       
        public override string get(){
            return getStatus() + getCmdName() + getCmdFilePath();
        }

        
        public override string getNextLine(){
            return get();
        }

    }   


    class PayloadPartitionBuilder : MessagePartitionBuilder {


        enum modes {

            LINE_MODE = 0

        }
        private List<StringBuilder> payloadPart = new List<StringBuilder>();



        private int payloadPartIdx = 0;
        public void buildPayload(char ch) {
            if ( payloadPart.Count <= payloadPartIdx )
                payloadPart.Add(new StringBuilder());
            payloadPart[payloadPartIdx].Append(ch);
        }

        

        public override string get(){
            StringBuilder res = new StringBuilder();
            for (int i = 0; i <payloadPart.Count; ++ i) {
                res.Append(payloadPart[i]);
            }

            return res.ToString();
        }

        public override int build(string line) {

                if ( line.StartsWith(ArduinoInterpreter.START_SYMBOL) ) {
                        this.setComplete();
                        return 0;
                }

                if ( getMode() > numModes() ) {
                    ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_PAYLOAD);
                    return -1;
                }
            

                int i = 0;
                for ( ; i < line.Length;++i ) {

                    buildPayload(line[i]);
                    
                    if ( line[i] == '\n' ){
                        payloadPartIdx++;
                    }
                }


            return i;
        }

         public override int numModes(){
            return Enum.GetNames(typeof(modes)).Length;
        }


        public override string getNextLine(){
            if ( this.getLinePos() >= payloadPart.Count ){
                return null;
            }
            string res = payloadPart[this.getLinePos()].ToString();
            this.incLinePos();
            return res;
        }

    } 
    class FooterPartitionBuilder : MessagePartitionBuilder {


        enum modes {

            STATUS_MODE = 0

        }
        private StringBuilder statusPart = new StringBuilder();
        public void buildStatus(char ch) {
            statusPart.Append(ch);
        }

        public string getStatus(){
            return statusPart.ToString();
        }
        

        public override string get(){
            return getStatus().Trim();
        }

        public override int build(string line) {


                if ( line.StartsWith(ArduinoInterpreter.READY_SYMBOL) ) {
                        this.setComplete();
                        return 0;
                }

                if ( getMode() > numModes() ) {
                    ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_FOOTER);
                    return -1;
                }
            

                int i = 0;
                for ( ; i < line.Length;++i ) {

                    
                    if ( line[i] == ' ' ) {
                        mode++;
                    } 


                    if ( mode == (int)modes.STATUS_MODE ) {
                        buildStatus(line[i]);
                    } else {
                        ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_FOOTER);
                        return -1;
                    }

                }


            return i;
        }

         public override int numModes(){
            return Enum.GetNames(typeof(modes)).Length;
        }

        public override string getNextLine(){
            return get();
        }
    } 

    


    class ResultMessageBuilder : MessagePartitionBuilder {

        HeaderPartitionBuilder headerBuilder = new HeaderPartitionBuilder();
        FooterPartitionBuilder footerBuilder = new FooterPartitionBuilder();
        PayloadPartitionBuilder payloadBuilder = new PayloadPartitionBuilder();

        

        public HeaderPartitionBuilder getHeaderBuilder() {
            return headerBuilder;
        }
        public PayloadPartitionBuilder getPayloadBuilder() {
            return payloadBuilder;
        }
        public FooterPartitionBuilder getFooterBuilder() {
            return footerBuilder;
        }
        enum modes {

            HEADER_MODE = 0,
            PAYLOAD_MODE,
            FOOTER_MODE,
            MSG_COMPLETE_MODE

        }

        public override int numModes(){
            return Enum.GetNames(typeof(modes)).Length;
        }


       

        public override int build(string line) {

            
                if ( getMode() > numModes() ) {
                    ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_HEADER);
                    return -1;
                }

                ErrorsApp.errno = 0;
                int i = 0;
                if ( getMode() == (int)modes.HEADER_MODE ) {

                    i = headerBuilder.build(line);
                    if ( i < 0 ) {
                        ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_HEADER);
                        return -1;
                    }
                    if(headerBuilder.isComplete() ) {

                        if(i == 0) {
                            setMode(getMode()+1);
                        } else {
                            Console.WriteLine("ArduinoMessageBuilder.cs: Unhandled case 1");
                        } 
                    }

                } 
                
                if ( getMode() == (int)modes.PAYLOAD_MODE ) {
                    
                    i = payloadBuilder.build(line);
                    if ( i < 0 ) {
                        ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_PAYLOAD);
                        return -1;
                    }
                    if(payloadBuilder.isComplete() ) {
                        if(i == 0) {
                            setMode(getMode()+1);
                        } else {
                            Console.WriteLine("ArduinoMessageBuilder.cs: Unhandled case 2");
                        } 
                    }
                } 
                
                if ( getMode() == (int)modes.FOOTER_MODE ) {
                    
                    i = footerBuilder.build(line);
                    if ( i < 0 ) {
                        ErrorsApp.set(ErrorsApp.ErrnoInternalCodes.STATUS_PROCCESSOR_ERROR_NOT_VALID_FOOTER);
                        return -1;
                    }
                    if(footerBuilder.isComplete() ) {
                        if(i == 0) {
                            setComplete();
                            setMode(getMode()+1);
                        } else {
                            Console.WriteLine("ArduinoMessageBuilder.cs: Unhandled case 3");
                        } 
                    }
                } 

            
            return i;
        }

        public int build(TalkBuffer sb) {


            StringBuilder line = new StringBuilder();
            int ch;
            

            ErrorsApp.errno = 0;

            while(sb.isnext() && (this.isComplete() == false) ) {



                while((ch = sb.read()) != -1 && ch != '\n') {
                    line.Append((char)ch);
                }

                string lineToString = line.ToString();
                    
                if ( build(lineToString) < 0 ) {
                    ErrorsApp.print();
                    return -1;
                }

                  
            }
            return 1;
        }


        public override void setComplete(){
            base.setComplete();
        }

        public override string get() {
            return headerBuilder.get() + "\n" + payloadBuilder.get() + "\n" + footerBuilder.get();
        }


        public override string getNextLine(){
                return get();
        }


        
    }


    


}
