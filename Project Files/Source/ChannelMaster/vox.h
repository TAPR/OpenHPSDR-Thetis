/*  vox.h

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

#ifndef _vox_h
#define _vox_h

typedef struct _vox
{
	int id;
	int run;
	int size;
	double* in;
	int mode;
	double thresh;
	int active;
	int oldactive;
	double peak;
	CRITICAL_SECTION cs_update;
} vox, *VOX;

extern void create_voxEXT (int id, int run, int size, double* in, int mode, double thresh);

extern void destroy_voxEXT (int id);

extern void xvoxEXT (int id, double* in);

extern VOX create_vox (int id, int run, int size, double* in, int mode, double thresh);

extern void destroy_vox (VOX a);

extern void xvox (VOX a);

extern __declspec (dllexport) void SetTXAVoxSize (int id, int size);

#endif