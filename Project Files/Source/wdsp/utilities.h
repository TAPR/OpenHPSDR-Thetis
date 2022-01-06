/*  utilities.h

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2013, 2019 Warren Pratt, NR0V

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

__declspec (dllexport) void *malloc0 (int size);

extern void print_impulse (const char* filename, int N, double* impulse, int rtype, int pr_mode);

extern __declspec (dllexport) void analyze_bandpass_filter (int N, double f_low, double f_high, double samplerate, int wintype, int rtype, double scale);

void print_peak_val(const char* filename, int N, double* buff, double thresh);

void print_peak_env (const char* filename, int N, double* buff, double thresh);

extern void print_peak_env_f2 (const char* filename, int N, float* Ibuff, float* Qbuff);

extern void print_iqc_values(const char* file, int state, double env_in, double I, double Q, double ym, double yc, double ys, double thresh);

extern void print_meter (const char* filename, double* meter, int enum_av, int enum_pk, int enum_gain);

extern void print_message (const char* filename, const char* message, int p0, int p1, int p2);

extern void print_window_gain (const char* filename, int wintype, double inv_coherent_gain, double inherent_gain);

extern void print_deviation (const char* filename, double dpmax, double rate);

extern void doCalccPrintSamples(int channel);

__declspec (dllexport) void print_buffer_parameters (const char* filename, int channel);

extern void print_anb_parms (const char* filename, ANB a);

extern void WriteAudioWDSP(double seconds, int rate, int size, double* indata, int mode, double gain);

extern void WriteScaledAudio (
	double seconds,			// number of seconds of audio to record
	int rate,				// sample rate
	int size,				// incoming buffer size
	double* indata );		// pointer to incoming data buffer
