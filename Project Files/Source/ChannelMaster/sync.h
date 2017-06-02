/*  sync.h

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2014 Warren Pratt, NR0V

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

#ifndef _sync_h
#define _sync_h
#include "comm.h"

typedef struct _sync
{
	double* divbuff;
	struct _sxmtr
	{
		volatile long ps_tx_idx;							// stream to use for PureSignal TX samples
		volatile long ps_rx_idx;							// stream to use for PureSignal RX samples
	} xmtr[cmMAXxmtr];
} sync, *SYNC;

extern SYNC psync;

extern void create_sync();

extern void destroy_sync();

extern __declspec (dllexport) void InboundBlock (int id, int nsamples, double** data);

#endif