/*  ivac.c

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

#include "cmcomm.h"

__declspec (align (16))			IVAC pvac[MAX_EXT_VACS];

void create_resamps(IVAC a)
{
	// data FROM VAC TO TX MIC INPUT
	if (a->vac_rate != a->mic_rate && !a->iq_type)
	{
		a->in_resamp = 1;
		a->resampPtrIn = create_resampleV(a->vac_rate, a->mic_rate);
	}
	else
		a->in_resamp = 0;
	// data FROM RADIO TO VAC
	if (a->vac_rate != a->audio_rate && !a->iq_type)
	{
		a->out_resamp = 1;
		a->resampPtrOut = create_resampleV(a->audio_rate, a->vac_rate);
	}
	else
		a->out_resamp = 0;
}

PORT void create_ivac(
	int id,
	int run,
	int iq_type,				// 1 if using raw IQ samples, 0 for audio
	int stereo,					// 1 for stereo, 0 otherwise
	int mic_rate,				// sample rate of data from VAC to TX MIC input
	int audio_rate,				// sample rate of data from RCVR Audio data to VAC
	int txmon_rate,				// sample rate of data from TX Monitor to VAC
	int vac_rate,				// VAC sample rate
	int mic_size,				// buffer size for data from VAC to TX MIC input
	int iq_size,				// buffer size for RCVR IQ data to VAC
	int audio_size,				// buffer size for RCVR Audio data to VAC
	int txmon_size,				// buffer size for TX Monitor data to VAC
	int vac_size				// VAC buffer size
	)
{
	IVAC a = (IVAC)calloc(1, sizeof(ivac));
	if (!a) 
	{ 
		printf("mem failure in ivac \n");
		exit(EXIT_FAILURE);
	}
	a->run = run;
	a->iq_type = iq_type;
	a->stereo = stereo;
	a->mic_rate = mic_rate;
	a->audio_rate = audio_rate;
	a->txmon_rate = txmon_rate;
	a->vac_rate = vac_rate;
	a->mic_size = mic_size;
	a->iq_size = iq_size;
	a->audio_size = audio_size;
	a->txmon_size = txmon_size;
	a->vac_size = vac_size;

	a->rb_vacIN = ringbuffer_create(16384);
	ringbuffer_restart(a->rb_vacIN, 2 * a->vac_size);
	a->rb_vacOUT = ringbuffer_create(16384);
	ringbuffer_restart(a->rb_vacOUT, a->iq_type ? 2 * a->iq_size : 2 * a->vac_size);

	// create fixed buffers
	a->res_in = (double *)calloc(4, 65536);
	a->res_out = (double *)calloc(4, 65536);

	// create resamplers as needed
	create_resamps(a);
	// create audio mixer
	{
		int inrate[2] = { a->audio_rate, a->txmon_rate };
		a->mixer = create_aamix(-1, id, a->audio_size, a->audio_size, 2, 3, 3, 1.0, 4096, inrate, a->audio_rate, xvac_out, 0.0, 0.0, 0.0, 0.0);
	}

	// initialize other items
	InitializeCriticalSectionAndSpinCount(&a->cs_vac, 2500);
	InitializeCriticalSectionAndSpinCount(&a->cs_vacw, 2500);
	a->reset = 0;
	pvac[id] = a;
}

void destroy_resamps(IVAC a)
{
	if (a->resampPtrOut != 0)
	{
		destroy_resampleV(a->resampPtrOut);
		a->resampPtrOut = 0;
	}
	if (a->resampPtrIn != 0)
	{
		destroy_resampleV(a->resampPtrIn);
		a->resampPtrIn = 0;
	}
}

PORT void destroy_ivac(int id)
{
	IVAC a = pvac[id];
	DeleteCriticalSection(&a->cs_vacw);
	DeleteCriticalSection(&a->cs_vac);

	// destroy resamplers
	destroy_resamps(a);

	// destroy fixed buffers
	if (a->res_out != 0)
	{
		free(a->res_out);
		a->res_out = 0;
	}
	if (a->res_in != 0)
	{
		free(a->res_in);
		a->res_in = 0;
	}

	// destroy ring buffers
	if (a->rb_vacOUT != 0)
	{
		ringbuffer_free(a->rb_vacOUT);
		a->rb_vacOUT = 0;
	}
	if (a->rb_vacIN != 0)
	{
		ringbuffer_free(a->rb_vacIN);
		a->rb_vacIN = 0;
	}

	free(a);
}

PORT void xvacIN(int id, double* in_tx)
{
	IVAC a = pvac[id];
	if (a->run && !a->vac_bypass)
	{
		if ((int)ringbuffer_read_space(a->rb_vacIN) >= 2 * a->mic_size)
		{		// copy data from rings into buffers
			EnterCriticalSection(&a->cs_vacw);
			ringbuffer_read(a->rb_vacIN, in_tx, 2 * a->mic_size);
			LeaveCriticalSection(&a->cs_vacw);
		}
		else	// zero output if not enough samples
		{
			memset(in_tx, 0, a->mic_size * sizeof(complex));
		}

		if (a->vac_combine_input)
		{
			combinebuff(a->mic_size, in_tx, in_tx);
		}

		if (a->vox || a->mox)
		{
			scalebuff(a->mic_size, in_tx, a->vac_preamp, in_tx);
		}
		else
		{
			memset(in_tx, 0, a->mic_size * sizeof(complex));
		}
	}
}

PORT void xvacOUT(int id, int stream, double* data)
{
	IVAC a = pvac[id];
	// receiver input data (iq_type) -> stream = 0
	// receiver output data (audio)  -> stream = 1
	// transmitter output data (mon) -> stream = 2
	if (a->run)
	{
		if (a->iq_type && stream == 0)				// iq data, never resampled
		{
			if ((int)ringbuffer_write_space(a->rb_vacOUT) >= 2 * a->iq_size)
			{	// copy IQ samples from buffer to ring
				EnterCriticalSection(&a->cs_vac);
				ringbuffer_write(a->rb_vacOUT, data, 2 * a->iq_size);
				LeaveCriticalSection(&a->cs_vac);
			}
			else// not enough write_space
				a->reset = 1;
		}
		if (!a->iq_type)
		{	// call mixer to synchronize the two streams
			if (stream == 1)
				xMixAudio(a->mixer, -1, 0, data);
			else if (stream == 2)
				xMixAudio(a->mixer, -1, 1, data);
		}
	}
}

void xvac_out(int id, int nsamples, double* buff)
{	// called by the mixer with a buffer of output data
	IVAC a = pvac[id];
	if (!a->out_resamp)
	{	// no need to resample
		if ((int)ringbuffer_write_space(a->rb_vacOUT) >= 2 * a->audio_size)
		{	// copy IQ samples from buffer to ring
			EnterCriticalSection(&a->cs_vac);
			ringbuffer_write(a->rb_vacOUT, buff, 2 * a->audio_size);
			LeaveCriticalSection(&a->cs_vac);
		}
		else// not enough write_space
			a->reset = 1;
	}
	else
	{	// must resample to the VAC sampling rate
		int outsamps = 0;
		xresampleV(buff, a->res_out, a->audio_size, &outsamps, a->resampPtrOut);
		if (ringbuffer_write_space(a->rb_vacOUT) >= (unsigned int)outsamps * 2)
		{
			EnterCriticalSection(&a->cs_vac);
			ringbuffer_write(a->rb_vacOUT, a->res_out, (unsigned int)outsamps * 2);
			LeaveCriticalSection(&a->cs_vac);
		}
		else
			a->reset = 1;
	}
}

int CallbackIVAC(const void *input,
	void *output,
	unsigned long frameCount,
	const PaStreamCallbackTimeInfo* timeInfo,
	PaStreamCallbackFlags statusFlags,
	void *userData)
{
	int id = (int)userData;		// use 'userData' to pass in the id of this VAC
	IVAC a = pvac[id];
	double* out_ptr = (double*)output;
	double* in_ptr = (double*)input;
	(void)timeInfo;
	(void)statusFlags;
	//(void) userData;

	if (!a->run) return 0;

	if (a->reset)
	{
		a->reset = 0;
		memset(out_ptr, 0, frameCount * sizeof(complex));

		EnterCriticalSection(&a->cs_vacw);
		ringbuffer_reset(a->rb_vacIN);
		LeaveCriticalSection(&a->cs_vacw);

		EnterCriticalSection(&a->cs_vac);
		ringbuffer_reset(a->rb_vacOUT);
		LeaveCriticalSection(&a->cs_vac);

		return 0;
	}

	if (a->in_resamp)
	{
		int outsamps = 0;
		xresampleV(in_ptr, a->res_in, frameCount, &outsamps, a->resampPtrIn);

		if (ringbuffer_write_space(a->rb_vacIN) >= (unsigned int)outsamps * 2)
		{
			EnterCriticalSection(&a->cs_vacw);
			ringbuffer_write(a->rb_vacIN, a->res_in, (unsigned int)outsamps * 2);
			LeaveCriticalSection(&a->cs_vacw);
		}
		else
		{
			a->reset = 1;
		}
	}
	else
	{
		if (ringbuffer_write_space(a->rb_vacIN) >= 2 * frameCount)
		{
			EnterCriticalSection(&a->cs_vacw);
			ringbuffer_write(a->rb_vacIN, in_ptr, 2 * frameCount);
			LeaveCriticalSection(&a->cs_vacw);
		}
	}

	if (ringbuffer_read_space(a->rb_vacOUT) >= 2 * frameCount)
	{
		EnterCriticalSection(&a->cs_vac);
		ringbuffer_read(a->rb_vacOUT, out_ptr, 2 * frameCount);
		LeaveCriticalSection(&a->cs_vac);
	}
	else
	{
		memset(out_ptr, 0, frameCount * sizeof(complex));
	}

	return 0;
}

PORT int StartAudioIVAC(int id)
{
	IVAC a = pvac[id];
	int error = 0;
	int in_dev = Pa_HostApiDeviceIndexToDeviceIndex(a->host_api_index, a->input_dev_index);
	int out_dev = Pa_HostApiDeviceIndexToDeviceIndex(a->host_api_index, a->output_dev_index);

	a->inParam.device = in_dev;
	a->inParam.channelCount = 2;
	a->inParam.suggestedLatency = a->pa_in_latency;
	a->inParam.sampleFormat = paFloat64;

	a->outParam.device = out_dev;
	a->outParam.channelCount = 2;
	a->outParam.suggestedLatency = a->pa_out_latency;
	a->outParam.sampleFormat = paFloat64;

	error = Pa_OpenStream(&a->Stream,
		&a->inParam,
		&a->outParam,
		a->vac_rate,
		a->vac_size, // paFramesPerBufferUnspecified,
		0,
		CallbackIVAC,
		(void *)id);	// pass 'id' as userData

	if (error != 0) return -1;

	error = Pa_StartStream(a->Stream);

	if (error != 0) return -1;

	return 1;
}

PORT void SetIVACRBReset(int id, int reset)
{
	IVAC a = pvac[id];
	a->reset = reset;
}

PORT void StopAudioIVAC(int id)
{
	IVAC a = pvac[id];
	Pa_CloseStream(a->Stream);
}

PORT void SetIVACrun(int id, int run)
{
	IVAC a = pvac[id];
	a->run = run;
}

PORT void SetIVACiqType(int id, int type)
{
	IVAC a = pvac[id];
	if (type != a->iq_type)
	{
		a->iq_type = type;
		//ringbuffer_restart(a->rb_vacOUT, a->iq_type ? 2 * a->iq_size : 2 * a->vac_size); 
		SetRingBufferSize(id);
	}
}

PORT void SetIVACstereo(int id, int stereo)
{
	IVAC a = pvac[id];
	a->stereo = stereo;
}

PORT void SetIVACvacRate(int id, int rate)
{
	IVAC a = pvac[id];
	if (rate != a->vac_rate)
	{
		a->vac_rate = rate;
		SetRingBufferSize(id);
		destroy_resamps(a);
		create_resamps(a);
	}
}

PORT void SetIVACmicRate(int id, int rate)
{
	IVAC a = pvac[id];
	if (rate != a->mic_rate)
	{
		a->mic_rate = rate;
		destroy_resamps(a);
		create_resamps(a);
	}
}

PORT void SetIVACaudioRate(int id, int rate)
{
	IVAC a = pvac[id];
	if (rate != a->audio_rate)
	{
		a->audio_rate = rate;
		destroy_aamix(a->mixer, 0);
		{
			int inrate[2] = { a->audio_rate, a->txmon_rate };
			a->mixer = create_aamix(-1, id, a->audio_size, a->audio_size, 2, 3, 3, 1.0, 4096, inrate, a->audio_rate, xvac_out, 0.0, 0.0, 0.0, 0.0);
		}
		destroy_resamps(a);
		create_resamps(a);
	}
}

void SetIVACtxmonRate(int id, int rate)
{
	IVAC a = pvac[id];
	if (rate != a->txmon_rate)
	{
		a->txmon_rate = rate;
		destroy_aamix(a->mixer, 0);
		{
			int inrate[2] = { a->audio_rate, a->txmon_rate };
			a->mixer = create_aamix(-1, id, a->audio_size, a->audio_size, 2, 3, 3, 1.0, 4096, inrate, a->audio_rate, xvac_out, 0.0, 0.0, 0.0, 0.0);
		}
	}
}

PORT void SetIVACvacSize(int id, int size)
{
	IVAC a = pvac[id];
	if (size != a->vac_size)
	{
		a->vac_size = (unsigned int)size;
		//ringbuffer_restart(a->rb_vacIN, 2 * a->vac_size);

		//if (!a->iq_type)
		//{
		//	ringbuffer_restart(a->rb_vacOUT, 2 * a->vac_size);
		//}

		SetRingBufferSize(id);
	}
}

PORT void SetIVACmicSize(int id, int size)
{
	IVAC a = pvac[id];
	if (size != a->mic_size)
	{
		a->mic_size = (unsigned int)size;
	}
}

PORT void SetIVACiqSize(int id, int size)
{
	IVAC a = pvac[id];
	if (size != a->iq_size)
	{
		a->iq_size = (unsigned int)size;
		if (a->iq_type)
		{
			ringbuffer_restart(a->rb_vacOUT, 2 * a->iq_size);
		}
	}
}

PORT void SetIVACaudioSize(int id, int size)
{
	IVAC a = pvac[id];
	a->audio_size = (unsigned int)size;
	destroy_aamix(a->mixer, 0);
	{
		int inrate[2] = { a->audio_rate, a->txmon_rate };
		a->mixer = create_aamix(-1, id, a->audio_size, a->audio_size, 2, 3, 3, 1.0, 4096, inrate, a->audio_rate, xvac_out, 0.0, 0.0, 0.0, 0.0);
	}
}

void SetIVACtxmonSize(int id, int size)
{
	IVAC a = pvac[id];
	a->txmon_size = (unsigned int)size;
}

PORT void SetIVAChostAPIindex(int id, int index)
{
	IVAC a = pvac[id];
	a->host_api_index = index;
}

PORT void SetIVACinputDEVindex(int id, int index)
{
	IVAC a = pvac[id];
	a->input_dev_index = index;
}

PORT void SetIVACoutputDEVindex(int id, int index)
{
	IVAC a = pvac[id];
	a->output_dev_index = index;
}

PORT void SetIVACnumChannels(int id, int n)
{
	IVAC a = pvac[id];
	a->num_channels = n;
}

PORT void SetIVACInLatency(int id, double lat, int reset)
{
	IVAC a = pvac[id];

	if (a->in_latency != lat)
	{
		a->in_latency = lat;
	}
		if (reset)
		SetRingBufferSize(id);
}

PORT void SetIVACOutLatency(int id, double lat, int reset)
{
	IVAC a = pvac[id];

	if (a->out_latency != lat)
	{
		a->out_latency = lat;
	}
		if (reset)
			SetRingBufferSize(id);
}

PORT void SetIVACPAInLatency(int id, double lat, int reset)
{
	IVAC a = pvac[id];

	if (a->pa_in_latency != lat)
	{
		a->pa_in_latency = lat;
	}
		if (reset)
			SetRingBufferSize(id);
}

PORT void SetIVACPAOutLatency(int id, double lat, int reset)
{
	IVAC a = pvac[id];

	if (a->pa_out_latency != lat)
	{
		a->pa_out_latency = lat;
	}
		if (reset)
			SetRingBufferSize(id);
}

PORT void SetIVACvox(int id, int vox)
{
	IVAC a = pvac[id];
	a->vox = vox;
}

PORT void SetIVACmox(int id, int mox)
{
	IVAC a = pvac[id];
	a->mox = mox;
	if (!a->mox)
	{
		SetAAudioMixWhat(a->mixer, 0, 1, 0);
		SetAAudioMixWhat(a->mixer, 0, 0, 1);
	}
	else if (a->mon)
	{
		SetAAudioMixWhat(a->mixer, 0, 0, 0);
		SetAAudioMixWhat(a->mixer, 0, 1, 1);
	}
	else
	{
		SetAAudioMixWhat(a->mixer, 0, 0, 0);
		SetAAudioMixWhat(a->mixer, 0, 1, 0);
	}
}

PORT void SetIVACmon(int id, int mon)
{
	IVAC a = pvac[id];
	a->mon = mon;
	if (!a->mox)
	{
		SetAAudioMixWhat(a->mixer, 0, 1, 0);
		SetAAudioMixWhat(a->mixer, 0, 0, 1);
	}
	else if (a->mon)
	{
		SetAAudioMixWhat(a->mixer, 0, 0, 0);
		SetAAudioMixWhat(a->mixer, 0, 1, 1);
	}
	else
	{
		SetAAudioMixWhat(a->mixer, 0, 0, 0);
		SetAAudioMixWhat(a->mixer, 0, 1, 0);
	}
}

PORT void SetIVACpreamp(int id, double preamp)
{
	IVAC a = pvac[id];
	a->vac_preamp = preamp;
}

PORT void SetIVACrxscale(int id, double scale)
{
	IVAC a = pvac[id];
	a->vac_rx_scale = scale;
	SetAAudioMixVolume(a->mixer, 0, a->vac_rx_scale);
}

PORT void SetIVACbypass(int id, int bypass)
{
	IVAC a = pvac[id];
	a->vac_bypass = bypass;
}

PORT void SetIVACcombine(int id, int combine)
{
	IVAC a = pvac[id];
	a->vac_combine_input = combine;
}

void combinebuff(int n, double* a, double* combined)
{
	int i;
	for (i = 0; i < 2 * n; i += 2)
		combined[i] = combined[i + 1] = a[i] + a[i + 1];
}

void scalebuff(int size, double* in, double scale, double* out)
{
	int i;
	for (i = 0; i < 2 * size; i++)
		out[i] = scale * in[i];
}

void SetRingBufferSize(int id)
{
	IVAC a = pvac[id];

	int rb_vacIN_size = (int)(2 * a->vac_rate * a->in_latency);
	if (a->vac_size * 2 > rb_vacIN_size)
		rb_vacIN_size = 2 * a->vac_size;
	else if (rb_vacIN_size % a->vac_size > 0)
	{
		rb_vacIN_size = rb_vacIN_size + a->vac_size -
			(rb_vacIN_size % a->vac_size);
	}

	int rb_vacOUT_size = (int)(2 * a->vac_rate * a->out_latency);
	if (a->vac_size * 2 > rb_vacOUT_size)
		rb_vacOUT_size = 2 * a->vac_size;
	else if (rb_vacOUT_size % a->vac_size > 0)
	{
		rb_vacOUT_size = rb_vacOUT_size + a->vac_size -
			(rb_vacOUT_size % a->vac_size);
	}

	if (a->rb_vacIN != 0)
	{
		ringbuffer_free(a->rb_vacIN);
		a->rb_vacIN = 0;
		a->rb_vacIN = ringbuffer_create(2 * rb_vacIN_size); //W4WMT here we account for two samples per frame in Thetis
		ringbuffer_restart(a->rb_vacIN, 2 * a->vac_size);
	}
	if (a->rb_vacOUT != 0)
	{
		ringbuffer_free(a->rb_vacOUT);
		a->rb_vacOUT = 0;
		a->rb_vacOUT = ringbuffer_create(2 * rb_vacOUT_size); //W4WMT here we account for two samples per frame in Thetis
		ringbuffer_restart(a->rb_vacOUT, a->iq_type ? 2 * a->iq_size : 2 * a->vac_size);
	}
}

