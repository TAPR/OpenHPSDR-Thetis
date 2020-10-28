/*  pro.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2017 Warren Pratt, NR0V

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

#include "pro.h"

PRO create_pro (
	int run,
	int psize,
	int npacks,
	int lpacks )
{
	PRO a = (PRO) malloc (sizeof (pro));
	int i;
	a->run = run;
	a->psize = psize;
	a->npacks = npacks;
	a->lpacks = lpacks;
	a->pbuffs = (unsigned char *) calloc (a->npacks, a->psize * sizeof (unsigned char));
	a->pbuff = (unsigned char **) calloc (a->npacks, sizeof (unsigned char *));
	for (i = 0; i < a->npacks; i++)
		a->pbuff[i] = a->pbuffs + i * a->psize;
	a->sbuff = (unsigned int *)calloc(a->npacks, sizeof(unsigned int));
	a->mask = a->npacks - 1;
	a->base_set = 0;
	a->in_order_count = 0;
	a->lastseqnum = 0;
	a->ooopCounter = 0;
	InitializeCriticalSectionAndSpinCount (&a->cspro, 2500);
	return a;
}

void destroy_pro ( PRO a )
{
	if (a != NULL)
	{
		DeleteCriticalSection(&a->cspro);
		free(a->pbuff);
		free(a->pbuffs);
		free(a->sbuff);
		free(a);
	}
}

void xpro (PRO a, unsigned int seqnum, char* buffer)
{
	if (a->run)
	{
		EnterCriticalSection (&a->cspro);
		if (!a->base_set)
		{
			if (seqnum == a->lastseqnum + 1)
			{
				a->in_order_count++;
				if (a->in_order_count == a->npacks)
				{
					a->out_idx = (seqnum - a->lpacks) & a->mask;
					a->base_set = 1;
				}
			}
			else
			{
				a->in_order_count = 0;
			}
			a->lastseqnum = seqnum;
		}
		else
		{
			a->in_idx = seqnum & a->mask;
			memcpy (a->pbuff[a->in_idx], buffer, a->psize * sizeof (unsigned char));
			a->sbuff[a->in_idx] = seqnum;
			a->out_idx = (a->out_idx + 1) & a->mask;
			memcpy (buffer, a->pbuff[a->out_idx], a->psize * sizeof (unsigned char));
			if (a->sbuff[a->out_idx] != a->lastseqnum + 1) a->ooopCounter++;
			a->lastseqnum = a->sbuff[a->out_idx];
		}
		LeaveCriticalSection (&a->cspro);
	}
}
