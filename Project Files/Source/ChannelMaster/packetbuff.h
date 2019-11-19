/*  packetbuff.h

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2019 Warren Pratt, NR0V

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

The author can be reached by email at  

warren@pratt.one

*/

#include <Windows.h>
#include <process.h>
#include <intrin.h>
#include <math.h>

#ifndef _pipe_h
#define _pipe_h

#define NUMDDC		16						// maximum number of DDCs per ADC
#define NUMADC		 4						// maximum number of ADCs

typedef struct _packetbuff
{
	int max_packets;						// maximum number of packets to be simultaneously buffered
	int packet_size;						// number of bytes in each packet, 1444 per protocol
	int in_index;							// byte index to next input location
	int out_index;							// byte index to next output location
	int port_in_index;						// index to next input port number location
	int port_out_index;						// index to next output port number location
	int buff_size;							// total size of the packet buffer in bytes
	byte *packet_buffer;					// packet buffer storage
	int  *port_buffer;						// port_number buffer storage
	HANDLE sem_num_packets;					// count of packets currently stored and ready to output
	CRITICAL_SECTION cs_input_lock;			// lock so input comes from one thread at a time
	// for DDCs
	unsigned char** ddcpacket;				// storage arrays in which to form ddc packets
	unsigned int* ddcseq;					// sequence numbers for DDC packets 
} packetbuff, *PACKETBUFF;

PACKETBUFF pbuff;

#endif