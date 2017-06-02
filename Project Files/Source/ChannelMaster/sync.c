/*  sync.c

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

#include "cmcomm.h"

sync syn  = {0};
SYNC psyn = &syn;

void create_sync()
{
	psyn->divbuff = (double *) malloc0 (pcm->cmMAXInbound[inid (0, 0)] * sizeof (complex));
	create_divEXT(0, 0, 2, 1024);
}

void destroy_sync()
{
	destroy_divEXT (0);
	_aligned_free (psyn->divbuff);
}

PORT
void InboundBlock (int id, int nsamples, double** data)
{
	switch (id)
	{
	case 0: // diversity receivers
		xdivEXT (0, nsamples, data, psyn->divbuff);
		Inbound (0, nsamples, psyn->divbuff);
		break;
	case 1: // puresignal receivers
		pscc (chid (inid (1, 0), 0),
			nsamples,
			data[_InterlockedAnd (&psyn->xmtr[0].ps_tx_idx, 0xffffffff)], 
			data[_InterlockedAnd (&psyn->xmtr[0].ps_rx_idx, 0xffffffff)]);
		break;
	case 2: // synchronous receivers
		Inbound(0, nsamples, data[0]);
		Inbound(1, nsamples, data[1]);
		break;
	case 3: // send synchronous only to first software receiver
		Inbound(0, nsamples, data[0]);
		break;
	}
}

PORT
void SetPSTxIdx (int id, int idx)
{
	_InterlockedExchange (&psyn->xmtr[id].ps_tx_idx, idx);
}

PORT
void SetPSRxIdx (int id, int idx)
{
	_InterlockedExchange (&psyn->xmtr[id].ps_rx_idx, idx);
}