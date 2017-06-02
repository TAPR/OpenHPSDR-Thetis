/*  txgain.h

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

#ifndef _txgain_h
#define _txgain_h

typedef struct _txgain
{
	volatile long run_fixed;
	volatile long run_amp_protect;
	int size;
	double* in;
	double* out;
	double Igain;
	double Qgain;
	int adc_value;
	int adc_supply;
	volatile long amp_protect_warning;
	CRITICAL_SECTION cs_update0, cs_update1;
} txgain, *TXGAIN;

TXGAIN create_txgain(
	int run_fixed,
	int run_amp_protect,
	int size,
	double* in,
	double* out,
	double Igain,
	double Qgain,
	int adc_value,
	int adc_supply
	);

void destroy_txgain(TXGAIN a);

void xtxgain(TXGAIN a);

void SetTXGainSize(TXGAIN p, int size);

void SetAmpProtectADCValue (int txid, int value);

#endif