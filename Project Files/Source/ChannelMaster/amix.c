/*  amix.c

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

AMIX create_amix (int id, int run, int size, double** in0, double** in1, double* out, unsigned int what0, unsigned int what1, double volume)
{
	int i;
	AMIX a = (AMIX) malloc0 (sizeof (amix));
	a->id = id;
	a->run = run;
	a->size = size;
	a->in0 = in0;
	a->in1 = in1;
	a->out = out;
	a->what0 = what0;
	a->what1 = what1;
	a->volume = volume;
	for (i = 0; i < 8 * sizeof (int); i++)
	{
		a->vol0[i] = 1.0;
		a->vol1[i] = 1.0;
	}
	InitializeCriticalSectionAndSpinCount(&a->cs_update, 2500);
	return a;
}

void destroy_amix (AMIX a)
{
	DeleteCriticalSection (&a->cs_update);
	_aligned_free (a);
}

void xamix (AMIX a)
{
	int i, j;
	unsigned int what, mask;
	EnterCriticalSection(&a->cs_update);
	memset (a->out, 0, a->size * sizeof (complex));
	what = a->what0;
	i = 0;
	while (what != 0)
	{
		mask = 1 << i;
		if ((mask & what) != 0)
		{
			for (j = 0; j < 2 * a->size; j++)
				a->out[j] += a->tvol0[i] * a->in0[i][j];
			what &= ~mask;
		}
		i++;
	}
	what = a->what1;
	i = 0;
	while (what != 0)
	{
		mask = 1 << i;
		if ((mask & what) != 0)
		{
			for (j = 0; j < 2 * a->size; j++)
				a->out[j] += a->tvol1[i] * a->in1[i][j];
			what &= ~mask;
		}
		i++;
	}
	LeaveCriticalSection(&a->cs_update);
}

/********************************************************************************************************
*																										*
*									    CALLS FOR EXTERNAL USE											*
*																										*
********************************************************************************************************/

#define MAX_EXT_AMIX	(2)								// maximum number of AMIXs called from outside wdsp
__declspec (align (16)) AMIX pamix[MAX_EXT_AMIX];		// array of pointers for AMIXs used EXTERNAL to wdsp

void create_amixEXT (int id, int run, int size, unsigned int what0, unsigned int what1, double volume)
{
	pamix[id] = create_amix (id, run, size, 0, 0, 0, what0, what1, volume);
}

void destroy_amixEXT (int id)
{
	destroy_amix (pamix[id]);
}

void xamixEXT (int id, double** in0, double** in1, double* out)
{
	AMIX a = pamix[id];
	a->id = id;
	a->in0 = in0;
	a->in1 = in1;
	a->out = out;
	xamix (a);
}

PORT
void SetAudioMixWhat (int id, int bank, unsigned int stream, int state)
{
	AMIX a = pamix[id];
	EnterCriticalSection(&a->cs_update);
	switch (bank)
	{
	case 0:
		if (state)
			a->what0 |=  (1 << stream);
		else
			a->what0 &= ~(1 << stream);
		break;
	case 1:
		if (state)
			a->what1 |=  (1 << stream);
		else
			a->what1 &= ~(1 << stream);
		break;
	}
	LeaveCriticalSection(&a->cs_update);
}

PORT
void SetAudioMixSize (int id, int size)
{
	AMIX a = pamix[id];
	EnterCriticalSection(&a->cs_update);
	a->size = size;
	LeaveCriticalSection(&a->cs_update);
}

PORT
void SetAudioMixVolume (int id, double volume)
{
	int i;
	AMIX a = pamix[id];
	EnterCriticalSection(&a->cs_update);
	a->volume = volume;
	for (i = 0; i < 8 * sizeof (int); i++)
	{
		a->tvol0[i] = a->volume * a->vol0[i];
		a->tvol1[i] = a->volume * a->vol1[i];
	}
	LeaveCriticalSection(&a->cs_update);
}

PORT
void SetAudioMixVol (int id, int bank, unsigned int stream, double vol)
{
	AMIX a = pamix[id];
	EnterCriticalSection(&a->cs_update);
	switch (bank)
	{
	case 0:
		a->vol0[stream] = vol;
		a->tvol0[stream] = a->vol0[stream] * a->volume;
		break;
	case 1:
		a->vol1[stream] = vol;
		a->tvol1[stream] = a->vol1[stream] * a->volume;
		break;
	}
	LeaveCriticalSection(&a->cs_update);
}