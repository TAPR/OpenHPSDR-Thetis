/*  pro.h

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2017 Warren Pratt, NR0V

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

warren@wpratt.com

*/

#ifndef _pro_h
#define _pro_h

#include <stdlib.h>
#include <Windows.h>

typedef struct _pro
{
	int run;
	int psize;					// packet size (bytes)
	int npacks;					// number of packets stored for re-ordering
	int lpacks;					// latency, in packets; POWER OF TWO!!!!
	unsigned char* pbuffs;		// pointer to packet memory
	unsigned char** pbuff;		// pointers to packet buffers
	int in_idx;					// index of pbuff in which to put the packet
	int out_idx;				// index of pbuff from which to take next packet
	int mask;
	int base_set;
	int in_order_count;
	unsigned int lastseqnum;
	int ooopCounter;
	unsigned int* sbuff;
	CRITICAL_SECTION cspro;
} pro, *PRO;

extern PRO create_pro (
	int run,
	int psize,
	int npacks,
	int lpacks );

extern void destroy_pro ( PRO a );

extern void xpro (PRO a, unsigned int seqnum, char* buffer);

#endif