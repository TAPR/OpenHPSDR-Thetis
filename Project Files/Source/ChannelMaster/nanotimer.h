/*
 * JanusAudio.dll - Support library for HPSDR.org's Janus/Ozy Audio card
 * Copyright (C) 2006,2007  Bill Tracey (bill@ejwt.com) (KD5TFD)
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 *
 */


// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the NANOTIMER_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// NANOTIMER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifndef wjtNANOTIMER_INCLUDED
#define wjtNANOTIMER_INCLUDED 1 
#ifdef NANOTIMER_AS_DLL 
#ifdef NANOTIMER_EXPORTS
#ifndef LINUX 
#define NANOTIMER_API __declspec(dllexport)
#else 
#define NANOTIMER_API 
#endif 
#else
#ifndef LINUX 
#define NANOTIMER_API __declspec(dllimport)
#else 
#define NANOTIMER_API 
#endif 
#endif
#else 
#ifndef NANOTIMER_EXPORTS 
#define NANOTIMER_API extern 
#else 
#define NANOTIMER_API
#endif 
#endif 


#ifdef LINUX 
#define __int64 long long 
#endif
#include <stdio.h> 
#if 0 
// This class is exported from the nanotimer.dll
class NANOTIMER_API Cnanotimer {
public:
	Cnanotimer(void);
	// TODO: add your methods here.
};

extern NANOTIMER_API int nnanotimer;

NANOTIMER_API int fnnanotimer(void);
#endif 

NANOTIMER_API __int64 getPerfTicks(void); 
NANOTIMER_API __int64 getPerfFreq(void); 
NANOTIMER_API __int64 perfTicksToNanos(__int64 ticks);


/* hi low average routines */ 
typedef struct HLA_count { 
	__int64 sum; 
	__int64 count; 
	__int64 hi; 
	__int64 lo; 
}  HLA_COUNTER; 

NANOTIMER_API void updateHLA(HLA_COUNTER *p, __int64 v); 
NANOTIMER_API void initHLA(HLA_COUNTER *p); 
NANOTIMER_API void printHLA(HLA_COUNTER *p, /* FILE *f, */ unsigned char *prefix); 
NANOTIMER_API void printHLANano(HLA_COUNTER *p, unsigned char *prefix); 

#endif
