using System;
using System.IO.Ports;
using System.Threading;

using System.Text;

namespace coreapp
{

    class CrcMath {

            
        private static int crc16_update(int crc, int a)
        {
            crc ^= a;
            for (int i = 0; i < 8; ++i)
            {
                if ((crc & 1) == 1)
                    crc = (crc >> 1) ^ 0xA001;
                else
                    crc = (crc >> 1);
            }
            return crc;
        }

        public static int crc_string(int crc, string s)
        {
            for ( int i = 0; i < s.Length; ++i ) 
                crc = crc16_update(crc, s[i]);
            return crc;
        } 

        public static int crc_int(int crc, int s)
        {
            return crc16_update(crc, s);
            
        } 

    }
   
}
