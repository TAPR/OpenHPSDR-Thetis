/*  ilv.h

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

#ifndef _ilv_h
#define _ilv_h

typedef struct _ilv
{
	volatile long run;
	int obid;
	int insize;
	int nin;
	volatile long what;
	double* outbuff;
	void(*Outbound)(int id, int nsamples, double* buff);
} ilv, *ILV;

ILV create_ilv (
	int run,
	int outbound_id,			// id to use in the outbound call
	int insize,					// number of complex samples in EACH INPUT BUFFER
	int ninputs,				// maximum number of inputs
	long what,					// bits specify which inputs are to be interleaved, one bit per input
	void(*Outbound) (int id, int nsamples, double* buff)
	);

void destroy_ilv (ILV a);

void xilv (ILV a, double** data);

void SetILVOutputPointer(int xmtr_id, void(*Outbound)(int id, int nsamples, double* buff));

void pSetILVRun(ILV a, int run);

void pSetILVInsize(ILV a, int size);

#endif
