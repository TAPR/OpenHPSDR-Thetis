/*  znobII.c

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

NOB GetRCVRNOBPointer (int stype, int id)
{
	NOB a;
	switch (stype)
	{
	case 0:
		a = pcm->rcvr[id].pnob;
		break;
	case 2:
		a = ppip->spc0[id].pnob;
		break;
	}
	return a;
}

PORT
void SetRCVRNOBRun (int stype, int id, int run)
{
	pSetRCVRNOBRun (GetRCVRNOBPointer (stype, id), run);
}

PORT
void SetRCVRNOBMode (int stype, int id, int mode)
{
	pSetRCVRNOBMode (GetRCVRNOBPointer (stype, id), mode);
}

PORT
void SetRCVRNOBBuffsize (int stype, int id, int size)
{
	pSetRCVRNOBBuffsize (GetRCVRNOBPointer (stype, id), size);
}

PORT
void SetRCVRNOBSamplerate (int stype, int id, int rate)
{
	pSetRCVRNOBSamplerate (GetRCVRNOBPointer (stype, id), rate);
}

PORT
void SetRCVRNOBTau (int stype, int id, double tau)
{
	pSetRCVRNOBTau (GetRCVRNOBPointer (stype, id), tau);
}

PORT
void SetRCVRNOBHangtime (int stype, int id, double time)
{
	pSetRCVRNOBHangtime (GetRCVRNOBPointer (stype, id), time);
}

PORT
void SetRCVRNOBAdvtime (int stype, int id, double time)
{
	pSetRCVRNOBAdvtime (GetRCVRNOBPointer (stype, id), time);
}

PORT
void SetRCVRNOBBacktau (int stype, int id, double tau)
{
	pSetRCVRNOBBacktau (GetRCVRNOBPointer (stype, id), tau);
}

PORT
void SetRCVRNOBThreshold (int stype, int id, double thresh)
{
	pSetRCVRNOBThreshold (GetRCVRNOBPointer (stype, id), thresh);
}