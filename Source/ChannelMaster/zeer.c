/*  zeer.c

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

PORT
void SetEERRun (int id, int run)
{
	EER a = pcm->xmtr[id].peer;
	ILV b = pcm->xmtr[id].pilv;
	pSetEERRun (a, run);
	pSetILVRun (b, run);
}

PORT
void SetEERAMIQ (int id, int amiq)
{
	EER a = pcm->xmtr[id].peer;
	pSetEERAMIQ (a, amiq);
}

PORT
void SetEERMgain (int id, double gain)
{
	EER a = pcm->xmtr[id].peer;
	pSetEERMgain (a, gain);
}

PORT
void SetEERPgain (int id, double gain)
{
	EER a = pcm->xmtr[id].peer;
	pSetEERPgain (a, gain);
}

PORT
void SetEERRunDelays (int id, int run)
{
	EER a = pcm->xmtr[id].peer;
	pSetEERRunDelays (a, run);
}

PORT
void SetEERMdelay (int id, double delay)
{
	EER a = pcm->xmtr[id].peer;
	pSetEERMdelay (a, delay);
}

PORT
void SetEERPdelay (int id, double delay)
{
	EER a = pcm->xmtr[id].peer;
	pSetEERPdelay (a, delay);
}

PORT
void SetEERSize (int id, int size)
{
	EER a = pcm->xmtr[id].peer;
	pSetEERSize (a, size);
}

PORT
void SetEERSamplerate (int id, int rate)
{
	EER a = pcm->xmtr[id].peer;
	pSetEERSamplerate (a, rate);
}