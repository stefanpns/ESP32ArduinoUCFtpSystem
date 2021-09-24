#include "ByteArray.h"
#include <string.h>
#include <stdlib.h>

ByteArray::ByteArray(int size)
{
    buffer = (char *)malloc(size);
    len = 0;
    maxlen = size;
}

ByteArray::~ByteArray()
{
    free(buffer);
}

int ByteArray::append(char b)
{
    if (len < maxlen)
    {
        buffer[len++] = b;
        return len; // return the next position in the buffer
    }
    else
    {
        return -1; // failed
    }
}

int ByteArray::nullTerminate()
{
    if (len < maxlen)
    {
        buffer[len] = 0;
        return len;
    }
    else
    {
        return -1;  // failed
    }
}


int ByteArray::isNullTerminated() {
    return buffer[len] == 0;
}



void ByteArray::clear()
{
    len = 0;
    nullTerminate();
}



int ByteArray::getLength()
{
    return len;
}

int ByteArray::getSize()
{
    return maxlen;
}

char ByteArray::operator[](int index)
{
    if (index >= len)
    {
        return (char)0;
    }
    else
    {
        return buffer[index];
    }
}
