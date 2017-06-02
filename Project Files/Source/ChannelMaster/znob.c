/*  znob.c

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

ANB GetRCVRANBPointer (int stype, int id)
{
	ANB a;
	switch (stype)
	{
	case 0:
		a = pcm->rcvr[id].panb;
		break;
	case 2:
		a = ppip->spc0[id].panb;
		break;
	}
	return a;
}

PORT
void SetRCVRANBRun (int stype, int id, int run)
{
	pSetRCVRANBRun (GetRCVRANBPointer (stype, id), run);
}

PORT
void SetRCVRANBBuffsize (int stype, int id, int size)
{
	pSetRCVRANBBuffsize (GetRCVRANBPointer (stype, id), size);
}

PORT
void SetRCVRANBSamplerate (int stype, int id, int rate)
{
	pSetRCVRANBSamplerate (GetRCVRANBPointer (stype, id), rate);
}

PORT
void SetRCVRANBTau (int stype, int id, double tau)
{
	pSetRCVRANBTau (GetRCVRANBPointer (stype, id), tau);
}

PORT
void SetRCVRANBHangtime (int stype, int id, double time)
{
	pSetRCVRANBHangtime (GetRCVRANBPointer (stype, id), time);
}

PORT
void SetRCVRANBAdvtime (int stype, int id, double time)
{
	pSetRCVRANBAdvtime (GetRCVRANBPointer (stype, id), time);
}

PORT
void SetRCVRANBBacktau (int stype, int id, double tau)
{
	pSetRCVRANBBacktau (GetRCVRANBPointer (stype, id), tau);
}

PORT
void SetRCVRANBThreshold (int stype, int id, double thresh)
{
	pSetRCVRANBThreshold (GetRCVRANBPointer (stype, id), thresh);
}