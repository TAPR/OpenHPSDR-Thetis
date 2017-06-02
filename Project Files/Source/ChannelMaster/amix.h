/*  amix.h

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

#ifndef _amix_h
#define _amix_h

typedef struct _amix
{
	int id;
	int run;
	int size;
	double** in0;
	double** in1;
	double* out;
	unsigned int what0;
	unsigned int what1;
	double vol0[8 * sizeof (int)];
	double vol1[8 * sizeof (int)];
	double tvol0[8 * sizeof (int)];
	double tvol1[8 * sizeof (int)];
	double volume;
	CRITICAL_SECTION cs_update;
} amix, *AMIX;

extern void create_amixEXT (int id, int run, int size, unsigned int what0, unsigned int what1, double volume);

extern void destroy_amixEXT (int id);

extern void xamixEXT (int id, double** in0, double** in1, double* out);

#endif