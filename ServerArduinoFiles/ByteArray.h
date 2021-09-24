/*
    ByteArray V2.0.2
    Djordje Herceg, 18.6.2021.

    GitHub: https://github.com/djherceg/????

    A fixed-size byte-array
*/

#ifndef __BYTEARRAY_
#define __BYTEARRAY_

#include <stdint.h>

class ByteArray
{
private:
    int maxlen;
    int pos;
    int len;

public:
    char *buffer;       

    ByteArray(int size);
    ~ByteArray();

    /** Vraća bajt iz bafera na poziciji index. Ako je index izvan opsega, vraća 0. */
    char operator[](int index);

    /** Postavlja tekuću poziciju. Vraća postavljenu poziciju, koja može biti različita od zadate ako je van granica. */
    //int setPos(int p);

    /** Vraća tekuću poziciju. */
    //int getPos();

    /** Vraća trenutnu dužinu bafera */
    int getLength();

    /**
     * @brief Sets the length of the buffer contents. Length must be less than maxlen. Resets current position to 0.
     * @return int New length or -1 if the specified length was too large.
     */
    //int setLen(int newlen);

    /**
     * @brief Gets the maximum buffer length.
     * 
     * @return int 
     */
    int getSize();

    /**
     * @brief Resets the position to 0.
     */
    //void reset();

    /**
     * @brief Resets the array to zero length, and writes 0x0 to the first byte of the buffer.
     * 
     */
    void clear();

    /**
     * @brief  Writes a byte at the current buffer position. Returns the next position or -1 if the buffer is full.
     * 
     * @param b Byte to write
     * @return int The next position or -1 if failed.
     */
    int append(char b);

    /**
     * @brief Writes 0x0 after the last byte in the buffer. 
     * 
     * @return int Returs the position of the 0x0 char, or -1 if the buffer is full.
     */
    int nullTerminate();

    int isNullTerminated();

};

#endif
