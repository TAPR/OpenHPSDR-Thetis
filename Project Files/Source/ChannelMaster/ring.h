/*  ring.h

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2015-2016 Warren Pratt, NR0V
Copyright (C) 2015-2016 Doug Wigley, W5WC

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

#ifndef _ringbuffer_h
#define _ringbuffer_h

typedef struct _ringbuffer {
    double	*buf;
    volatile int write_ptr;
    volatile int read_ptr;
	volatile int	write_flag; //W4WMT added flag to distinguish between full/empty when pointers are equal
    int	 size;
    int  size_mask;
}
ringbuffer_t ;

extern ringbuffer_t *ringbuffer_create(int sz);
extern int ringbuffer_read_space(const ringbuffer_t *rb);
extern int ringbuffer_write_space(const ringbuffer_t *rb);
extern void ringbuffer_free(ringbuffer_t *rb);
extern int ringbuffer_read(ringbuffer_t *rb, double *dest, int cnt);
extern int ringbuffer_write(ringbuffer_t *rb, const double *src, int cnt);
extern void ringbuffer_reset_size (ringbuffer_t * rb, int sz);
extern void ringbuffer_restart (ringbuffer_t * rb, int sz);
extern void ringbuffer_free(ringbuffer_t *rb);
extern void ringbuffer_reset(ringbuffer_t *rb);


#endif
