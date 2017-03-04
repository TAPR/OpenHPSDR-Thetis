/*  ilv.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2015 Warren Pratt, NR0V

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

// ilv is a simple SYNCHRONOUS interleaver:  it assumes all inputs are available when it is called

ILV create_ilv (
	int run,
	int outbound_id,			// id to use in the outbound call
	int insize,					// number of complex samples in EACH INPUT BUFFER
	int ninputs,				// maximum number of inputs
	long what,					// bits specify which inputs are to be interleaved, one bit per input
	void (*Outbound) (int id, int nsamples, double* buff)
	)
{
	ILV a = (ILV) malloc0 (sizeof (ilv));
	a->run = run;
	a->obid = outbound_id;
	a->insize = insize;
	a->nin = ninputs;
	a->what = what;
	a->Outbound = Outbound;
	a->outbuff = (double *) malloc0 (a->nin * a->insize * sizeof(complex));
	return a;
}

void destroy_ilv (ILV a)
{
	_aligned_free (a->outbuff);
	_aligned_free (a);
}

void xilv (ILV a, double** data)
{
	int i, j, k;
	int what, mask;
	if (_InterlockedAnd(&a->run, 1))
	{
		k = 0;
		for (j = 0; j < a->insize; j++)
		{
			what = _InterlockedAnd(&a->what, 0xffffffff);
			i = 0;
			while (what != 0)
			{
				mask = 1 << i;
				if ((mask & what) != 0)
				{
					a->outbuff[2 * k + 0] = data[i][2 * j + 0];
					a->outbuff[2 * k + 1] = data[i][2 * j + 1];
					what &= ~mask;
					k++;
				}
				i++;
			}
		}
	}
	else
	{
		k = a->insize;
		if (a->outbuff != data[0])
			memcpy(a->outbuff, data[0], a->insize * sizeof(complex));
	}
	(*a->Outbound)(a->obid, k, a->outbuff);
}

/********************************************************************************************************
*																										*
*									         INTERLEAVER PROPERTIES										*
*																										*
********************************************************************************************************/

void SetILVOutputPointer (int xmtr_id, void(*Outbound)(int id, int nsamples, double* buff))
{
	ILV a = pcm->xmtr[xmtr_id].pilv;
	a->Outbound = Outbound;
}

PORT
void SetILVRun (int xmtr_id, int run)
{
	ILV a = pcm->xmtr[xmtr_id].pilv;
	if (run)
		InterlockedBitTestAndSet(&a->run, 0);
	else
		InterlockedBitTestAndReset(&a->run, 0);
}

PORT
void SetILVWhat(int xmtr_id, int stream, int state)
{
	ILV a = pcm->xmtr[xmtr_id].pilv;
	if (state)
		InterlockedBitTestAndSet(&a->what, stream);		// put stream in output
	else
		InterlockedBitTestAndReset(&a->what, stream);	// remove stream from output
}

PORT
void SetILVInsize(int xmtr_id, int size)
{
	ILV a = pcm->xmtr[xmtr_id].pilv;
	a->insize = size;
}

PORT
void SetILVOutboundId(int xmtr_id, int obid)
{
	ILV a = pcm->xmtr[xmtr_id].pilv;
	a->obid = obid;
}

void pSetILVRun(ILV a, int run)
{
	if (run)
		InterlockedBitTestAndSet(&a->run, 0);
	else
		InterlockedBitTestAndReset(&a->run, 0);
}

void pSetILVInsize(ILV a, int size)
{
	a->insize = size;
}