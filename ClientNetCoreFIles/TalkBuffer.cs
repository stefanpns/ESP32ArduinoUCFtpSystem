using System;
using System.IO.Ports;
using System.Threading;


namespace coreapp
{

    class TalkBuffer {



        const int maxbuf = 1000;                       
        char[] buffer = new char[maxbuf];
        bool overflow; 
        int length;                         
        int position;  



        public TalkBuffer() {} 

        private void init(){ 
            clear(); 
        } 

        public bool init(string text){ 
            clear(); 

            if ( text.Length >= maxbuf+1 )
                return false;
            else{
                for ( int i = 0 ; i < text.Length ; ++i ) {
                        buffer[this.length++] = text[i]; 
                }
                buffer[this.length] = '\0'; 
            }
            return true;
        } 


        public void clear() { 

            overflow = false; 
            length = 0; 
            position = 0; 
            buffer[0] = '\0'; // null-terminate 

        } 

          

        public bool isnext() { 

            return (position + 1) < length; 

        } 

          

        public int read() { 

            if (!isnext()) 
                return -1; 

          

            return buffer[position++]; 

        } 
        public bool isoverflow() { 

            return overflow; 

        } 


    }


}
