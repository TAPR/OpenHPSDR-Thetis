#define _CRT_SECURE_NO_WARNINGS
#include "cmcomm.h"

PORT
void print_cmbuff_parameters(const char* filename, int id)
{
	CMB a = pcm->pcbuff[id];
	FILE* file = fopen (filename, "a");
	fprintf (file, "id                 = %d\n", id);
	fprintf (file, "accept             = %d\n", a->accept);
	fprintf (file, "max_insize         = %d\n", a->max_in_size);
	fprintf (file, "outsize            = %d\n", a->r1_outsize);
	fprintf (file, "\n");
	fflush (file);
	fclose (file);
}

// Audacity:  Import Raw Data, Signed 32-bit PCM, Little-endian, Mono/Stereo per mode selection, 48K rate

int audiocount = 0;
int* data;
int ready = 0;
int done = 0;

void WriteAudioFile (void* arg)
{
	byte* dat = (byte *) arg;
	FILE* file = fopen ("AudioFile", "wb");

	// reverse bits of each byte (possibly needed on some platforms)
	// int i;
	// byte b;
	// for (i = 0; i < 4 * audiocount; i++)
	// {
	//	b = dat[i];
	// 	b = ((b >> 1) & 0x55) | ((b << 1) & 0xaa);
	// 	b = ((b >> 2) & 0x33) | ((b << 2) & 0xcc);
	// 	b = ((b >> 4) & 0x0f) | ((b << 4) & 0xf0);
	// 	dat[i] = b;
	// }
	fwrite ((int *)dat, sizeof (int), audiocount, file);
	fflush (file);
	fclose (file);
	free (data);
	_endthread();
}

PORT
void WriteAudio(double seconds, int rate, int size, double* indata, int mode)
{
	// seconds - number of seconds of audio to record
	// rate - sample rate
	// size - number of complex samples
	// indata - pointer to data
	static int n; 
	int i;
	const double conv = 2147483647.0;
	if (!ready)
	{
		if (mode < 3)
			n = (int)(seconds * rate);
		else
			n = 2 * (int)(seconds * rate);
		data = (int *) calloc (n, sizeof (int));
		ready = 1;
	}
	for (i = 0; i < size; i++)
	{
		if (audiocount < n)
		{
			switch (mode)
			{
				case 0:	// I only (mono)
					data[audiocount++] = (int)(conv * indata[2 * i + 0]);
					break;
				case 1: // Q only (mono)
					data[audiocount++] = (int)(conv * indata[2 * i + 1]);
					break;
				case 2: // envelope (mono)
					data[audiocount++] = (int)(conv * sqrt (indata[2 * i + 0] * indata[2 * i + 0] + indata[2 * i + 1] * indata[2 * i + 1]));
					break;
				case 3:	// complex samples (stereo)
					data[audiocount++] = (int)(conv * indata[2 * i + 0]);
					data[audiocount++] = (int)(conv * indata[2 * i + 1]);
					break;
			}
		}
	}
	if ((audiocount == n) && !done)
	{
		done = 1;
		_beginthread (WriteAudioFile, 0, (void *)data);
	}
}