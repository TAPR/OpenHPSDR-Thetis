/*  anr.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2012, 2013 Warren Pratt, NR0V

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

#include "comm.h"

ANR create_anr	(
				int run,
				int position,
				int buff_size,
				double *in_buff,
				double *out_buff,
				int dline_size,
				int n_taps,
				int delay,
				double two_mu,
				double gamma,

				double lidx,
				double lidx_min,
				double lidx_max,
				double ngamma,
				double den_mult,
				double lincr,
				double ldecr
			)
{
	ANR a = (ANR) malloc0 (sizeof(anr));
	a->run = run;
	a->position = position;
	a->buff_size = buff_size;
	a->in_buff = in_buff;
	a->out_buff = out_buff;
	a->dline_size = dline_size;
	a->mask = dline_size - 1;
	a->n_taps = n_taps;
	a->delay = delay;
	a->two_mu = two_mu;
	a->gamma = gamma;
	a->in_idx = 0;
	a->lidx = lidx;
	a->lidx_min = lidx_min;
	a->lidx_max = lidx_max;
	a->ngamma = ngamma;
	a->den_mult = den_mult;
	a->lincr = lincr;
	a->ldecr = ldecr;
	
	memset (a->d, 0, sizeof(double) * ANR_DLINE_SIZE);
	memset (a->w, 0, sizeof(double) * ANR_DLINE_SIZE);
	
	return a;
}

void destroy_anr (ANR a)
{
	_aligned_free (a);
}

void xanr (ANR a, int position)
{
    int i, j, idx;
    double c0, c1;
    double y, error, sigma, inv_sigp;
	double nel, nev;
    if (a->run && (a->position == position))
	{
		for (i = 0; i < a->buff_size; i++)
		{
			a->d[a->in_idx] = a->in_buff[2 * i + 0];

			y = 0;
			sigma = 0;

			for (j = 0; j < a->n_taps; j++)
			{
				idx = (a->in_idx + j + a->delay) & a->mask;
				y += a->w[j] * a->d[idx];
				sigma += a->d[idx] * a->d[idx];
			}
			inv_sigp = 1.0 / (sigma + 1e-10);
			error = a->d[a->in_idx] - y;

			a->out_buff[2 * i + 0] = y;
			a->out_buff[2 * i + 1] = 0.0;

			if((nel = error * (1.0 - a->two_mu * sigma * inv_sigp)) < 0.0) nel = -nel;
			if((nev = a->d[a->in_idx] - (1.0 - a->two_mu * a->ngamma) * y - a->two_mu * error * sigma * inv_sigp) < 0.0) nev = -nev;
			if (nev < nel)
			{
				if((a->lidx += a->lincr) > a->lidx_max) a->lidx = a->lidx_max;
			}
			else
			{
				if((a->lidx -= a->ldecr) < a->lidx_min) a->lidx = a->lidx_min;
			}
			a->ngamma = a->gamma * (a->lidx * a->lidx) * (a->lidx * a->lidx) * a->den_mult;

			c0 = 1.0 - a->two_mu * a->ngamma;
			c1 = a->two_mu * error * inv_sigp;

			for (j = 0; j < a->n_taps; j++)
			{
				idx = (a->in_idx + j + a->delay) & a->mask;
				a->w[j] = c0 * a->w[j] + c1 * a->d[idx];
			}
			a->in_idx = (a->in_idx + a->mask) & a->mask;
		}
	}
	else if (a->in_buff != a->out_buff)
		memcpy (a->out_buff, a->in_buff, a->buff_size * sizeof (complex));
}

void flush_anr (ANR a)
{
	memset (a->d, 0, sizeof(double) * ANR_DLINE_SIZE);
	memset (a->w, 0, sizeof(double) * ANR_DLINE_SIZE);
	a->in_idx = 0;
}

void setBuffers_anr (ANR a, double* in, double* out)
{
	a->in_buff = in;
	a->out_buff = out;
}

void setSamplerate_anr (ANR a, int rate)
{
	flush_anr(a);
}

void setSize_anr (ANR a, int size)
{
	a->buff_size = size;
	flush_anr(a);
}

/********************************************************************************************************
*																										*
*											RXA Properties												*
*																										*
********************************************************************************************************/

PORT void
SetRXAANRRun (int channel, int run)
{
	ANR a = rxa[channel].anr.p;
	if (a->run != run)
	{
		RXAbp1Check (channel, rxa[channel].amd.p->run, rxa[channel].snba.p->run, 
			rxa[channel].emnr.p->run, rxa[channel].anf.p->run, run);
		EnterCriticalSection (&ch[channel].csDSP);
		a->run = run;
		RXAbp1Set (channel);
		flush_anr (a);
		LeaveCriticalSection (&ch[channel].csDSP);
	}
}

PORT void
SetRXAANRVals (int channel, int taps, int delay, double gain, double leakage)
{
	EnterCriticalSection (&ch[channel].csDSP);
	rxa[channel].anr.p->n_taps = taps;
	rxa[channel].anr.p->delay = delay;
	rxa[channel].anr.p->two_mu = gain;
	rxa[channel].anr.p->gamma = leakage;
	flush_anr (rxa[channel].anr.p);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT void
SetRXAANRTaps (int channel, int taps)
{
	EnterCriticalSection (&ch[channel].csDSP);
	rxa[channel].anr.p->n_taps = taps;
	flush_anr (rxa[channel].anr.p);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT void
SetRXAANRDelay (int channel, int delay)
{
	EnterCriticalSection (&ch[channel].csDSP);
	rxa[channel].anr.p->delay = delay;
	flush_anr (rxa[channel].anr.p);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT void
SetRXAANRGain (int channel, double gain)
{
	EnterCriticalSection (&ch[channel].csDSP);
	rxa[channel].anr.p->two_mu = gain;
	flush_anr (rxa[channel].anr.p);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT void
SetRXAANRLeakage (int channel, double leakage)
{
	EnterCriticalSection (&ch[channel].csDSP);
	rxa[channel].anr.p->gamma = leakage;
	flush_anr (rxa[channel].anr.p);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT void
SetRXAANRPosition (int channel, int position)
{
	EnterCriticalSection (&ch[channel].csDSP);
	rxa[channel].anr.p->position = position;
	rxa[channel].bp1.p->position = position;
	flush_anr (rxa[channel].anr.p);
	LeaveCriticalSection (&ch[channel].csDSP);
}
