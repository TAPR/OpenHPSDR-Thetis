/*  cmsetup.h

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

// these parameters are used to size arrays in static structures
#define cmMAXrcvr		(16)				// maximum number of receivers
#define cmMAXxmtr		( 4)				// maximum number of transmitters
#define cmMAXSubRcvr	( 4)				// number of sub-receivers per receiver, including the base receiver
#define cmMAXspc		( 2)				// maximum number of special unit TYPES
#define cmMAXstream		(32)				// maximum number of streams


extern int rxid (int stream);

extern int txid (int stream);

extern __declspec (dllexport) int chid (int stream, int subrx);

extern int sp0id (int stream);

extern int stype (int stream);

extern __declspec (dllexport) int inid(int stype, int id);

extern int mixinid (int stream, int subrx);

extern __declspec (dllexport) int getbuffsize (int rate);