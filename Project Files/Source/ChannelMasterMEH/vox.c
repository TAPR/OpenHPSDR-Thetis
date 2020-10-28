/*  vox.c

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

VOX create_vox (int id, int run, int size, double* in, int mode, double thresh)
{
	VOX a = (VOX) malloc0 (sizeof (vox));
	a->id = id;
	a->run = run;
	a->size = size;
	a->in = in;
	a->mode = mode;
	a->thresh = thresh;
	a->active = 0;
	a->oldactive = 0;
	a->peak = 0.0;
	InitializeCriticalSectionAndSpinCount(&a->cs_update, 2500);
	return a;
}

void destroy_vox (VOX a)
{
	DeleteCriticalSection (&a->cs_update);
	_aligned_free (a);
}

void flush_vox (VOX a)
{
	a->active = 0;
	a->oldactive = 0;
	a->peak = 0.0;
}

void xvox (VOX a)
{
	int i;
	double absval;
	double max = 0.0;
	EnterCriticalSection (&a->cs_update);
	a->oldactive = a->active;
	a->active = 0;
	if (a->run)
	{
		switch (a->mode)
		{
		case 0:
			for (i = 0; i < a->size; i++)
			{
				if ((absval = a->in[2 * i + 0]) < 0.0) absval = - absval;
				if (absval > max) max = absval;
			}
			break;
		case 1:
			for (i = 0; i < a->size; i++)
			{
				if ((absval = a->in[2 * i + 1]) < 0.0) absval = - absval;
				if (absval > max) max = absval;
			}
			break;
		case 2:
			for (i = 0; i < a->size; i++)
			{
				if ((absval = a->in[2 * i + 0]) < 0.0) absval = - absval;
				if (absval > max) max = absval;
				if ((absval = a->in[2 * i + 1]) < 0.0) absval = - absval;
				if (absval > max) max = absval;
			}
			break;
		}
		a->peak = max;
		if (max > a->thresh)
			a->active = 1;
	}
	if (a->active != a->oldactive)
		(*pcm->xmtr[a->id].pushvox)(a->id, a->active);
	LeaveCriticalSection (&a->cs_update);
}

/********************************************************************************************************
*																										*
*									    CALLS FOR EXTERNAL USE											*
*																										*
********************************************************************************************************/

#define MAX_EXT_VOX	(32)							// maximum number of VOXs called from outside wdsp
__declspec (align (16)) VOX pvox[MAX_EXT_VOX];		// array of pointers for VOXs used EXTERNAL to wdsp

void create_voxEXT (int channel, int run, int size, double* in, int mode, double thresh)
{
	pvox[channel] = create_vox (channel, run, size, in, mode, thresh);
}

void destroy_voxEXT (int channel)
{
	destroy_vox (pvox[channel]);
}

void flush_voxEXT (int channel)
{
	flush_vox (pvox[channel]);
}

void xvoxEXT (int channel, double* in)
{
	VOX a = pvox[channel];
	a->in = in;
	xvox (a);
}

/********************************************************************************************************
*																										*
*									    ChannelMaster PROPERTIES										*
*																										*
********************************************************************************************************/

PORT
void SendCBPushVox (int id, void (__stdcall *pushvox)(int id, int active))
{
	pcm->xmtr[id].pushvox = pushvox;
}

PORT
void SetTXAVoxRun (int id, int run)
{
	VOX a = pcm->xmtr[id].pvox;
	EnterCriticalSection (&a->cs_update);
	a->run = run;
	LeaveCriticalSection (&a->cs_update);
}

PORT 
void SetTXAVoxSize (int id, int size)
{
	VOX a = pcm->xmtr[id].pvox;
	EnterCriticalSection (&a->cs_update);
	a->size = size;
	LeaveCriticalSection (&a->cs_update);
}

PORT
void SetTXAVoxThresh (int id, double thresh)
{
	VOX a = pcm->xmtr[id].pvox;
	EnterCriticalSection (&a->cs_update);
	a->thresh = thresh;
	LeaveCriticalSection (&a->cs_update);
}

PORT
void GetTXAVoxPeak (int id, double* peak)
{
	VOX a = pcm->xmtr[id].pvox;
	EnterCriticalSection (&a->cs_update);
	*peak = a->peak;
	LeaveCriticalSection (&a->cs_update);
}