using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http;



namespace coreapp
{

    
    


    class Program
    {


        static async Task Main(string[] args)
        {  

            await ThreadProcReadWrite();
           // Thread t_read = new Thread(new ThreadStart(ThreadProcReadWrite));
           // t_read.Start();
        }


         public static async Task  ThreadProcReadWrite() {

            
             HttpRequestRW _postRequestTalk = new PostHttpRequestRW();
            _postRequestTalk.setup("http://10.42.0.216/");

            TalkBuffer _talkBuffer = new TalkBuffer();

            ArduinoIW arduinoIW = new ArduinoIW();


            bool waitingForTheChain = false;
            string chainCommand = null;


            while (true)
            {
                _talkBuffer.clear();

                try
                {

                    string commandToSend = null;
                    if ( waitingForTheChain ) {
                        commandToSend = chainCommand;
                    } else {
                        Console.WriteLine("Command to send:");
                        commandToSend = Console.ReadLine();
                    }
                    
                    string response = await _postRequestTalk.SendCommandAndRecieveResult(commandToSend);
                    //Console.WriteLine("response: \n" + response);
                    
                    if ( response == null ) {
                        Console.WriteLine("404: General error");
                        break;
                    }

                    if ( _talkBuffer.init(response) == false ) {
                        Console.WriteLine("Buffer overflow - Unhandled error 3.");
                        break;
                    }

                    CommandObject res = null;
                    res = arduinoIW.interpret(_talkBuffer);

                    if ( res == null ) {
                        ErrorsApp.print();
                        break;
                    }
                    

                            
                    if ( res.isValid() ) {

                        if ( res.isComplete()) {

                            if ( waitingForTheChain ) {

                                
                                if ( arduinoIW.getPreviousCommandObject() != null ) {

                                    CommandObject previousCmdObject = arduinoIW.getPreviousCommandObject();

                                    
                                        if ( previousCmdObject.checkChainCommandName(res.getName()) ) {

                                        if ( previousCmdObject.checkChainCmdResult(res) == true) {
                                            previousCmdObject.print();
                                            Console.WriteLine(previousCmdObject.getChainSuccessMessage());
                                        } else {
                                            Console.WriteLine(previousCmdObject.getChainUnsuccessMessage());
                                        }
                                            Console.WriteLine();


                                    }else {
                                        Console.WriteLine("The new command result is not compatible with the previous command result.");
                                        Console.WriteLine("The chain cannot be continued.");
                                        Console.WriteLine("The new command result is:");
                                        res.print();
                                        Console.WriteLine();
                                    }
                                }else {
                                    Console.WriteLine("Program.cs: Unhandled case 1");
                                }

                                waitingForTheChain = false;

                            } else if ( res.isWaitingForTheChain() ) {

                                waitingForTheChain = true;
                                chainCommand = res.getChainCommand();

                            } else {
                                res.print();
                                Console.WriteLine();
                            }



                        } else {
                            Console.WriteLine("Program.cs: We have recieved not completed command result.");
                        }

                    } else  {

                        Console.WriteLine("Program.cs: We have recieved a command result that is not valid.");
                        Console.WriteLine("The command was ["+res.getName()+"].");
                        
                    }
                        
                    arduinoIW.setPreviousCommandObject(res);


                    

                } finally {
                }

               
            }
        }



    }
}
