/*
 * netinterface.c
 * Copyright (C) 2006,2007  Bill Tracey (bill@ejwt.com) (KD5TFD)
 * Copyright (C) 2010-2016 Doug Wigley (W5WC)
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2 of the License, or (at your option) any later version.
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

#include <stdio.h>
#include "network.h"
#include "obbuffs.h"

#define MDECAY 0.99f;
const int numInputBuffs = 12;

//
// StartAudio -- called when we need to start reading audio and passing it to PowerSDR via the callback.
//

PORT
int StartAudioNative(//int sample_rate, int samples_per_block,
int(__stdcall *callbackp)(void *inp, void *outp, int framcount, void *timeinfop, int flags, void *userdata)) //,int sample_bits)
{
	DWORD dwWaitResult;
	int myrc = 0;

	HaveSync = 1;

	// make sure we're not already opened
	if (IOThreadRunning) {
		return 3;
	}

	do { // once		
		if (prn == NULL)
			create_rnet();

		if (prn != NULL)
		{
			StartReadThread();

			prn->hReadThreadInitSem = CreateSemaphore(NULL, 0, 1, NULL);
			if (prn->hReadThreadInitSem == NULL) {
				printf("CreateSemaphore(ReadThreadInitSem) error: %d\n", GetLastError());
				myrc = 4;
				break;
			}
			else
				printf("CreateSemaphore(ReadThreadInitSem) is OK\n");

			prn->hReadThreadMain = CreateThread(NULL, 0, ReadThreadMain, 0, 0, NULL);
			if (prn->hReadThreadMain == NULL) {
				printf("CreateThread(ReadThreadMain) error: %d\n", GetLastError());
				myrc = 4;
				break;
			}
			else
				printf("CreateThread(ReadThreadMain) is OK\n");

			prn->hKeepAliveThread = CreateThread(NULL, 0, KeepAliveMain, 0, 0, NULL);
			if (prn->hKeepAliveThread == NULL) {
				printf("CreateThread(KeepAliveMain) error: %d\n", GetLastError());
				myrc = 4;
				break;
			}
			else
				printf("CreateThread(KeepAliveMain) is OK\n");

			dwWaitResult = WaitForSingleObject(prn->hReadThreadInitSem, INFINITE); // wait for the thread to get going
			switch (dwWaitResult)
			{
				// The semaphore object was signaled.
			case WAIT_OBJECT_0: printf("The semaphore object was signaled. Can open another window..\n");
				// OK to open another window.
				break;
				// Semaphore was nonsignaled, so a time-out occurred.
			case WAIT_TIMEOUT: printf("The semaphore object was nonsignaled, so a time-out...Can't open another window\n");
				// Cannot open another window.
				break;
			}
		}

	} while (0);

	if (myrc != 0) {  // we failed -- clean up

		StopReadThread(); /* is a no op if not running */
	}
	DotDashBits = 0;
	// printf("StartAudioNative - myrc: %d\n", myrc);
	return myrc;

}

//
// StopAudio -- undo what start audio did.  Close/Free/Destroy that which StartAudio Opened/Alloc'd/Created.
//
PORT
void StopAudio() {
	int rc;
	printf("stop audio called\n");  fflush(stdout);
	rc = IOThreadStop();
	if (rc != 0) {
		fprintf(stderr, "Warning: IOThreadStop failed with rc=%d\n", rc);
	}
	printf("iothread stopped\n");   fflush(stdout);

	StopReadThread();

	DotDashBits = 0;

	return;
}

int getDDPTTcount = 0;
int last_DDP = 0;

PORT
int nativeGetDotDashPTT() {
	return (prn->dot_in |
		prn->dash_in |
		prn->ptt_in) & 0x7;
}

PORT
int getAndResetADC_Overload() {
	int n = prn->adc[0].adc_overload | (prn->adc[1].adc_overload << 1) | (prn->adc[2].adc_overload << 2);

	return n;
}

PORT
int getOOO() { // OOO == Out Of Order packet
	int result = 0;
	if (prn->cc_seq_err > 0) // High Priority Command & Control
		result = 1;
	if (prn->rx[0].rx_in_seq_err > 0) // DDC0 I/Q data
		result += 2;
	if (prn->rx[1].rx_in_seq_err > 0) // DDC1 I/Q data
		result += 4;
	if (prn->rx[2].rx_in_seq_err > 0) // DDC2 I/Q data
		result += 8;
	if (prn->rx[3].rx_in_seq_err > 0) // DDC3 I/Q data
		result += 16;
	if (prn->rx[4].rx_in_seq_err > 0) // DDC4 I/Q data
		result += 32;
	if (prn->rx[5].rx_in_seq_err > 0) // DDC5 I/Q data
		result += 64;
	if (prn->rx[6].rx_in_seq_err > 0) // DDC6 I/Q data
		result += 128;
	if (prn->tx[0].mic_in_seq_err > 0) // mic_in data
		result += 256;


	if (result > 0)
	{
		//PrintTimeHack();
		//printf("- seq_error %d\n", result);

		prn->cc_seq_err = 0;
		prn->rx[0].rx_in_seq_err = 0;
		prn->rx[1].rx_in_seq_err = 0;
		prn->rx[2].rx_in_seq_err = 0;
		prn->rx[3].rx_in_seq_err = 0;
		prn->rx[4].rx_in_seq_err = 0;
		prn->rx[5].rx_in_seq_err = 0;
		prn->rx[6].rx_in_seq_err = 0;
		prn->tx[0].mic_in_seq_err = 0;
	}

	return result;
}

PORT
int getSeqInDeltaStart(int rx) {
	prn->rx[rx].snapshot = prn->rx[rx].snapshots_head;
	return prn->rx[rx].snapshot != NULL;
}

PORT
int getSeqInDelta(int rx, int deltas[]) {
	if (prn->rx[rx].snapshot != NULL)
	{
		memcpy(deltas, prn->rx[rx].snapshot->rx_in_seq_snapshot, sizeof(int) * MAX_IN_SEQ_LOG);

		prn->rx[rx].snapshot = prn->rx[rx].snapshot->next;

		return 1;
	}
	return 0;
}

PORT
int getUserI01() {
	return (prn->user_io & 0x1) != 0;

}

PORT
int getUserI02() {

	return (prn->user_io & 0x2) != 0;

}

PORT
int getUserI03() {

	return (prn->user_io & 0x4) != 0;

}

PORT
int getUserI04() {

	return (prn->user_io & 0x8) != 0;

}

PORT
int getExciterPower() {

	return prn->tx[0].exciter_power;

}

PORT
float getFwdPower() {

	// PeakFwdPower((float)prn->tx[0].fwd_power);
	return FwdPower;
}

PORT
float getRevPower() {

	// PeakRevPower((float)prn->tx[0].rev_power);
	return RevPower;
}

PORT
int getUserADC0() {


	return prn->user_adc0;

}

PORT
int getUserADC1() {


	return prn->user_adc1;

}

PORT
int getUserADC2() {


	return prn->user_adc2;

}

PORT
int getUserADC3() {


	return prn->user_adc3;

}

PORT
int getHermesDCVoltage() {

	return prn->supply_volts;
}

PORT
void SetPttOut(int xmit) {

	if (prn->tx[0].ptt_out != xmit)
	{
		prn->tx[0].ptt_out = xmit & 0x1;
		if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
			CmdHighPriority();
	}
}

PORT
void SetTRXrelay(int bit) {

	if (prbpfilter->_TR_Relay != bit)
	{
		if (!prn->tx[0].pa) // disable PA
		prbpfilter->_TR_Relay = bit & 0x1;
		prbpfilter->_trx_status = prbpfilter->_TR_Relay; // TXRX_STATUS
		if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
			CmdHighPriority();
	}
}

PORT
void SetC1Bits(int bits) { // for HPSDR
	C1Mask = bits;
	return;
}

PORT
void EnableEClassModulation(int bit) {
	if (prn->cw.eer != bit) {
		prn->cw.eer = bit;
		if (listenSock != INVALID_SOCKET)
			CmdRx();
	}
}

PORT
void SetOCBits(int b) {
	if (prn->oc_output != b)
	{
		prn->oc_output = b;
		if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
			CmdHighPriority();
	}
}

PORT
void SetAlexAtten(int bits) {

	if (mkiibpf) return;

	if ((prbpfilter->_20_dB_Atten | prbpfilter->_10_dB_Atten) != bits)
	{
		prbpfilter->_20_dB_Atten = (bits & 0x2) == 0x2;
		prbpfilter->_10_dB_Atten = bits & 0x1;
		if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
			CmdHighPriority();
	}
	return;
}

PORT
void SetADCDither(int bits) {

	if (prn->adc[0].dither != bits) {
		prn->adc[0].dither = prn->adc[1].dither = prn->adc[2].dither = bits;
		if (listenSock != INVALID_SOCKET)
			CmdRx();
	}
	return;
}

PORT
void SetADCRandom(int bits) {

	if (prn->adc[0].random != bits) {
		prn->adc[0].random = prn->adc[1].random = prn->adc[2].random = bits;
		if (listenSock != INVALID_SOCKET)
			CmdRx();
	}
	return;
}

PORT
void SetAntBits(int rx_only_ant, int trx_ant, int rx_out, char tx) {

	if (mkiibpf)
	{
		if (rx_only_ant == 1 || tx) // set rx bypass only if Ext2 enabled
		{
			prbpfilter->_Rx_1_Out = (rx_out & 0x01) != 0; // RX BYPASS OUT RL17
			prbpfilter->_10_dB_Atten = 0; // RX MASTER IN SEL RL22
		}
		else
		{
			prbpfilter->_Rx_1_Out = 0; // RX BYPASS OUT RL17
			prbpfilter->_10_dB_Atten = rx_out & 0x1; // RX MASTER IN SEL RL22
		}
	}
	else
	{
		prbpfilter->_Rx_1_Out = (rx_out & 0x01) != 0; // RX BYPASS OUT RL17
	}

	prbpfilter->_Rx_1_In = (rx_only_ant & (0x01 | 0x02)) == 0x01; // Ext2
	prbpfilter->_Rx_2_In = (rx_only_ant & (0x01 | 0x02)) == 0x02; // Ext1
	prbpfilter->_XVTR_Rx_In = (rx_only_ant & (0x01 | 0x02)) == (0x01 | 0x02); // XVTR

	prbpfilter->_ANT_1 = (trx_ant & (0x01 | 0x02)) == 0x01;
	prbpfilter->_ANT_2 = (trx_ant & (0x01 | 0x02)) == 0x02;
	prbpfilter->_ANT_3 = (trx_ant & (0x01 | 0x02)) == (0x01 | 0x02);
	if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
		CmdHighPriority();
	return;
}

PORT
void SetVFOfreq(int id, int freq, int tx) {
	if (tx)
	{
		if (prn != NULL && prn->tx[id].frequency != freq)
		{
			prn->tx[id].frequency = freq;
			if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
				CmdHighPriority();
		}
	}
	else
	{
		if (prn != NULL && prn->rx[id].frequency != freq)
		{
			prn->rx[id].frequency = freq;
			if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
				CmdHighPriority();
		}
	}
	return;
}

PORT
void SetOutputPowerFactor(int u) {

	
	if (prn->tx[0].drive_level != u)
	{
		prn->tx[0].drive_level = u;
		if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
			CmdHighPriority();
	}
}

PORT
void SetMicBoost(int bits) {

	if (prn->mic.mic_boost != bits) {
		prn->mic.mic_boost = bits;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}
	return;
}

PORT
void SetLineIn(int bits) {
	if (prn->mic.line_in != bits) {
		prn->mic.line_in = bits;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

	return;
}

PORT
void EnableApolloFilter(int bits) {
	if (bits != 0)
		ApolloFilt = 0x4;
	else
		ApolloFilt = 0;
	return;
}

PORT
void EnableApolloTuner(int bits) {
	if (bits != 0)
		ApolloTuner = 0x8;
	else
		ApolloTuner = 0;
	return;
}

PORT
void EnableApolloAutoTune(int bits) {
	if (bits != 0)
		ApolloATU = 0x10;
	else
		ApolloATU = 0;
	return;
}

PORT
void SetAlexHPFBits(int bits) {
	if (AlexHPFMask != bits) {
		prbpfilter->_13MHz_HPF = (bits & 0x01) != 0;
		prbpfilter->_20MHz_HPF = (bits & 0x02) != 0;
		prbpfilter->_9_5MHz_HPF = (bits & 0x04) != 0;
		prbpfilter->_6_5MHz_HPF = (bits & 0x08) != 0;
		prbpfilter->_1_5MHz_HPF = (bits & 0x10) != 0;
		prbpfilter->_Bypass = (bits & 0x20) != 0;
		prbpfilter->_6M_preamp = (bits & 0x40) != 0;
		if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
			CmdHighPriority();
	}

	AlexHPFMask = bits;
	return;
}

PORT
void DisablePA(int bit) {
	if (prn->tx[0].pa != bit) {
		prn->tx[0].pa = bit;
	}
	return;
}

PORT
void SetAlex2HPFBits(int bits) {
	if (Alex2HPFMask != bits) {
		prbpfilter2->_13MHz_HPF = (bits & 0x01) != 0;
		prbpfilter2->_20MHz_HPF = (bits & 0x02) != 0;
		prbpfilter2->_9_5MHz_HPF = (bits & 0x04) != 0;
		prbpfilter2->_6_5MHz_HPF = (bits & 0x08) != 0;
		prbpfilter2->_1_5MHz_HPF = (bits & 0x10) != 0;
		prbpfilter2->_Bypass = (bits & 0x20) != 0;
		prbpfilter2->_6M_preamp = (bits & 0x40) != 0;
		if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
			CmdHighPriority();
	}

	Alex2HPFMask = bits;
	return;
}

PORT
void SetBPF2Gnd(int bits) {

	if ((prbpfilter2->_rx2_gnd) != bits)
	{
		prbpfilter2->_rx2_gnd = (bits & 0x1) != 0;
		if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
			CmdHighPriority();
	}
	return;
}

PORT
void SetAlex3HPFBits(int bits) {
	Alex3HPFMask = bits;
	return;
}

PORT
void SetAlex4HPFBits(int bits) {
	Alex4HPFMask = bits;
	return;
}

PORT
void SetAlexLPFBits(int bits) {
	if (AlexLPFMask != bits) {
		prbpfilter->_30_20_LPF = (bits & 0x01) != 0;
		prbpfilter->_60_40_LPF = (bits & 0x02) != 0;
		prbpfilter->_80_LPF = (bits & 0x04) != 0;
		prbpfilter->_160_LPF = (bits & 0x08) != 0;
		prbpfilter->_6_LPF = (bits & 0x10) != 0;
		prbpfilter->_12_10_LPF = (bits & 0x20) != 0;
		prbpfilter->_17_15_LPF = (bits & 0x40) != 0;
		if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
			CmdHighPriority();
	}

	AlexLPFMask = bits;
	return;
}

PORT
void SetAlex2LPFBits(int bits) {
	Alex2LPFMask = bits;
	return;
}

PORT
void SetAlex3LPFBits(int bits) {
	Alex3LPFMask = bits;
	return;
}

PORT
void SetAlex4LPFBits(int bits) {
	Alex4LPFMask = bits;
	return;
}

PORT
void SetRX1Preamp(int bits) {

	if (prn->rx[0].preamp != bits)
	{
		prn->rx[0].preamp = bits;
		if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
			CmdHighPriority();
	}
	return;
}

PORT
void SetRX2Preamp(int bits) {

	if (prn->rx[1].preamp != bits)
	{
		prn->rx[1].preamp = bits;
		if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
			CmdHighPriority();
	}

	return;
}

PORT
void SetMicTipRing(int bits) {

	if (prn->mic.mic_trs != bits) {
		prn->mic.mic_trs = bits;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

	return;
}

PORT
void SetMicBias(int bits) {
	if (prn->mic.mic_bias != bits) {
		prn->mic.mic_bias = bits;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

	return;
}

PORT
void SetMicPTT(int bits) {

	if (prn->mic.mic_ptt != bits) {
		prn->mic.mic_ptt = bits;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

	return;
}

PORT
void SetLineBoost(int bits) {

	if (prn->mic.line_in_gain != bits) {
		prn->mic.line_in_gain = bits;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

	return;
}

PORT
void SetMercTxAtten(int bit) {
	if (bit != 0) {
		MercTxAtten = 0x20;
	}
	else {
		MercTxAtten = 0;
	}

	return;
}

PORT
void SetPureSignal(int bit) {
	if (prn->puresignal_run != bit)
	{
		prn->puresignal_run = bit & 0x1;
		if (listenSock != INVALID_SOCKET)
			// CmdRx();
			CmdHighPriority();
	}

	return;
}

PORT
void SetUserOut0(int out) {
	if (out == 0) {
		UserOut0 = 0;
	}
	else {
		UserOut0 = 1;
	}

	return;
}

PORT
void SetUserOut1(int out) {
	if (out == 0) {
		UserOut1 = 0;
	}
	else {
		UserOut1 = 0x2;
	}
	return;
}

PORT
void SetUserOut2(int out) {
	if (out == 0) {
		UserOut2 = 0;
	}
	else {
		UserOut2 = 0x4;
	}

	return;
}

PORT
void SetUserOut3(int out) {
	if (out == 0) {
		UserOut3 = 0;
	}
	else {
		UserOut3 = 0x8;
	}

	return;
}

PORT
void SetADC1StepAttenData(int data) {

	if (prn->adc[0].rx_step_attn != data)
	{
		prn->adc[0].rx_step_attn = data;
		if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
			CmdHighPriority();
	}
	return;
}

PORT
void SetADC2StepAttenData(int data) {

	if (prn->adc[1].rx_step_attn != data)
	{
		prn->adc[1].rx_step_attn = data;
		if (listenSock != INVALID_SOCKET && prn->sendHighPriority != 0)
			CmdHighPriority();
	}

	return;
}

PORT
void SetADC3StepAttenData(int data) {

	if (prn->adc[2].rx_step_attn != data)
	{
		prn->adc[2].rx_step_attn = data;
		if (listenSock != INVALID_SOCKET)
			CmdHighPriority();
	}

	return;
}

PORT
void ReversePaddles(int bits) {

	if (prn->cw.rev_paddle != bits) {
		prn->cw.rev_paddle = bits;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

	return;
}

PORT
void SetCWKeyerSpeed(int speed) {

	if (prn->cw.keyer_speed != speed) {
		prn->cw.keyer_speed = speed;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

}

PORT
void SetCWKeyerMode(int mode) {
	if (prn->cw.mode_b != mode) {
		prn->cw.mode_b = mode;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

}

PORT
void SetCWKeyerWeight(int weight) {
	if (prn->cw.keyer_weight != weight) {
		prn->cw.keyer_weight = weight;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}
}

PORT
void EnableCWKeyerSpacing(int bits) {

	if (prn->cw.strict_spacing != bits) {
		prn->cw.strict_spacing = bits;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

	return;
}

PORT
void SetADC_cntrl1(int bits) {

	if (ADC_cntrl1 != bits) {
		prn->rx[0].rx_adc = bits & 0x3;
		prn->rx[1].rx_adc = (bits >> 2) & 0x3;
		prn->rx[2].rx_adc = (bits >> 4) & 0x3;
		prn->rx[3].rx_adc = (bits >> 6) & 0x3;
		if (listenSock != INVALID_SOCKET)
			CmdRx();
	}

	ADC_cntrl1 = bits;
	return;
}

PORT
int GetADC_cntrl1() {
	return ADC_cntrl1;
}

PORT
void SetADC_cntrl2(int bits) {
	if (ADC_cntrl2 != bits) {
		prn->rx[4].rx_adc = bits & 0x3;
		prn->rx[5].rx_adc = (bits >> 2) & 0x3;
		prn->rx[6].rx_adc = (bits >> 4) & 0x3;
		if (listenSock != INVALID_SOCKET)
			CmdRx();
	}

	ADC_cntrl2 = bits;
	return;
}

PORT
int GetADC_cntrl2() {
	return ADC_cntrl2;
}

PORT
void SetTxAttenData(int bits) {
	int i;

	if (prn->adc[0].tx_step_attn != bits) {
		for (i = 0; i < MAX_ADC; i++) {
			prn->adc[i].tx_step_attn = bits;
		}
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

	return;
}

PORT
void EnableCWKeyer(int enable) {

	if (prn->cw.cw_enable != enable) {
		prn->cw.cw_enable = enable;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

	return;
}

PORT
void SetCWSidetoneVolume(int vol) {

	if (prn->cw.sidetone_level != vol) {
		prn->cw.sidetone_level = vol;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

}

PORT
void SetCWPTTDelay(int delay) {

	if (prn->cw.rf_delay != delay) {
		prn->cw.rf_delay = delay;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

}

PORT
void SetCWHangTime(int time) {

	if (prn->cw.hang_delay != time) {
		prn->cw.hang_delay = time;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

}

PORT
void SetCWSidetoneFreq(int freq) {
	if (prn->cw.sidetone_freq != freq) {
		prn->cw.sidetone_freq = freq;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}

}

PORT
void SetEERPWMmin(int min) {
	if (prn->tx[0].epwm_min != min)
	{
		prn->tx[0].epwm_min = min;
		if (listenSock != INVALID_SOCKET)
			CmdGeneral();
	}
}

PORT
void SetEERPWMmax(int max) {
	if (prn->tx[0].epwm_max != max)
	{
		prn->tx[0].epwm_max = max;
		if (listenSock != INVALID_SOCKET)
			CmdGeneral();
	}
}

// *************************************************
// misc functions
// *************************************************
PORT
void SetCWSidetone(int enable) {

	if (prn->cw.sidetone != enable) {
		prn->cw.sidetone = enable;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}
}

PORT
void SetCWIambic(int enable) {

	if (prn->cw.iambic != enable) {
		prn->cw.iambic = enable;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}
}

PORT
void SetCWBreakIn(int enable) {

	if (prn->cw.break_in != enable) {
		prn->cw.break_in = enable;
		if (listenSock != INVALID_SOCKET)
			CmdTx();
	}
}

PORT
void SetCWDash(int bit) {

	if (prn->tx[0].dash != bit) {
		prn->tx[0].dash = bit;
		if (listenSock != INVALID_SOCKET)
			CmdHighPriority();
	}
}

PORT
void SetCWDot(int bit) {
	if (prn->tx[0].dot != bit) {
		prn->tx[0].dot = bit;
		if (listenSock != INVALID_SOCKET)
			CmdHighPriority();
	}
}

PORT
void SetCWX(int bit) {
	if (prn->tx[0].cwx != bit) {
		prn->tx[0].cwx = bit;
		if (listenSock != INVALID_SOCKET)
			CmdHighPriority();
	}
}

PORT
int GetC1Bits(void) {
	return C1Mask;
}

PORT
int getHaveSync() {
	return HaveSync;
}

PORT
int getControlByteIn(int n) {
	if (n < 0 || n > 4) {
		return -1;
	}
	return ControlBytesIn[n];
}

PORT
void SetLegacyDotDashPTT(int bit) {
	if (bit) {
		CandCAddrMask = 0xfc;
		CandCFwdPowerBit = 0x4;
		DotDashMask = 0x3;
	}
	else {
		CandCAddrMask = 0xf8;
		CandCFwdPowerBit = 0x8;
		DotDashMask = 0x7;
	}
}

PORT
void EnableRx(int id, int enable) {
	if (prn->rx[id].enable != enable)
	{
		prn->rx[id].enable = enable & 0x1;
		if (listenSock != INVALID_SOCKET)
			CmdRx();
	}
}

PORT
void EnableRxs(int rxs) {
	int i, sum = 0;

	for (i = 0; i < 4; i++)
	{
		sum += (prn->rx[i].enable << i);
	}

	if (sum != rxs)
	{
		for (i = 0; i < 4; i++)
		{
			prn->rx[i].enable = (rxs >> i) & 0x1;
		}
		/*if (listenSock != INVALID_SOCKET)
			CmdRx();*/
	}

}

PORT
void EnableRxSync(int id, int sync)
{
	if (prn->rx[id].sync != sync)
	{
		prn->rx[id].sync = sync & 0xff;
		/*if (listenSock != INVALID_SOCKET)
			CmdRx();*/
	}
}

PORT
void SetSampleRate(int id, int rate)
{
	if (prn->rx[id].sampling_rate != rate)
	{
		prn->rx[id].sampling_rate = rate;
		switch (rate)
		{
		case 48000:
			prn->rx[id].sampling_rate = 48;
			break;
		case 96000:
			prn->rx[id].sampling_rate = 96;
			break;
		case 192000:
			prn->rx[id].sampling_rate = 192;
			break;
		case 384000:
			prn->rx[id].sampling_rate = 384;
			break;
		case 768000:
			prn->rx[id].sampling_rate = 768;
			break;
		case 1536000:
			prn->rx[id].sampling_rate = 1536;
			break;
		}

		if (listenSock != INVALID_SOCKET)
			CmdRx();
	}

}

PORT
void SetDDCRate(int id, int rate)
{
	if (prn->rx[id].sampling_rate != rate)
	{
		prn->rx[id].sampling_rate = rate;
		switch (rate)
		{
		case 48000:
			prn->rx[id].sampling_rate = 48;
			break;
		case 96000:
			prn->rx[id].sampling_rate = 96;
			break;
		case 192000:
			prn->rx[id].sampling_rate = 192;
			break;
		case 384000:
			prn->rx[id].sampling_rate = 384;
			break;
		case 768000:
			prn->rx[id].sampling_rate = 768;
			break;
		case 1536000:
			prn->rx[id].sampling_rate = 1536;
			break;
		}
	}
}

PORT
void SetRxADC(int n) {

	if (prn->num_adc != n)
	{
		prn->num_adc = n;
		if (listenSock != INVALID_SOCKET)
			CmdRx();
	}
}

// wideband data display
PORT
void SetWBPacketsPerFrame(int ppf)
{
	prn->wb_packets_per_frame = ppf;
	if (listenSock != INVALID_SOCKET)
		CmdGeneral();
}

PORT
void SetWBUpdateRate(int ur)
{
	prn->wb_update_rate = ur;
	if (listenSock != INVALID_SOCKET)
		CmdGeneral();
}

PORT
void SetWBEnable(int adc, int enable)
{
	if (enable) InterlockedBitTestAndSet(&prn->wb_enable, adc);
	else        InterlockedBitTestAndReset(&prn->wb_enable, adc);
	if (listenSock != INVALID_SOCKET)
		CmdGeneral();
}

PORT
void SendHighPriority(int send) {
	if (send)
	{
		prn->sendHighPriority = 1;
		if (listenSock != INVALID_SOCKET)
			CmdHighPriority();
	}
	else prn->sendHighPriority = 0;
}

PORT
void SetWatchdogTimer(int enable)
{
	if (prn->wdt != enable)
	{
		prn->wdt = enable;
		if (listenSock != INVALID_SOCKET)
			CmdGeneral();
	}
}

PORT
void SetMKIIBPF(int bpf)
{
	mkiibpf = bpf;
}

PORT
void SetXVTREnable(int enable)
{
	if (xvtr_enable != enable)
	{
		xvtr_enable = enable;
		if (listenSock != INVALID_SOCKET)
			CmdHighPriority();
	}
}

PORT
int getLEDs()
{
	return prn->hardware_LEDs;
}

PORT
void create_rnet() {
	int i;

	prn = (RADIONET)malloc(sizeof(radionet));
	if (prn) {
		prn->RxReadBufp = (double*)calloc(1, 2 * sizeof(double) * 240);
		prn->TxReadBufp = (double*)calloc(1, 2 * sizeof(double) * 720);
		prn->ReadBufp = (unsigned char*)calloc(1, sizeof(unsigned char) * 1444);
		//prn->ReadBufp = (char *)calloc(1, sizeof(char) * 1444);
		prn->OutBufp = (char*)calloc(1, sizeof(char) * 1440);
		//prn->readbuf = prn->ReadBufp;
		prn->rx_base_port = 1035;
		prn->run = 0;
		prn->wdt = 0;
		prn->sendHighPriority = 1;
		prn->num_adc = 1;
		prn->num_dac = 1;
		prn->ptt_in = 0;
		prn->dot_in = 0;
		prn->dash_in = 0;
		prn->cc_seq_no = 0;
		prn->cc_seq_err = 0;

		prn->cw.mode_control = 0;
		prn->cw.sidetone_level = 0;
		prn->cw.sidetone_freq = 0;
		prn->cw.keyer_speed = 0;
		prn->cw.keyer_weight = 0;
		prn->cw.hang_delay = 0;
		prn->cw.rf_delay = 0;

		prn->mic.mic_control = 0;
		prn->mic.line_in_gain = 0;
		prn->mic.spp = 64; // 64;	720;					// I-samples per packet

		for (i = 0; i < 2; i++)
			prn->syncrxbuff[i] = (double*)calloc(1, 2 * sizeof(double) * 120);

		prn->wb_base_port = 1027;
		prn->wb_base_dispid = 32;
		prn->wb_enable = 0;
		prn->wb_samples_per_packet = 512;
		prn->wb_sample_size = 16;
		prn->wb_update_rate = 70;
		prn->wb_packets_per_frame = 32;
		for (i = 0; i < MAX_ADC; i++) {
			prn->adc[i].id = i;
			prn->adc[i].rx_step_attn = 0;
			prn->adc[i].tx_step_attn = 31;
			prn->adc[i].adc_overload = 0;
			prn->adc[i].dither = 0;
			prn->adc[i].random = 0;
			prn->adc[i].wb_seqnum = 0;
			prn->adc[i].wb_state = 0;
			prn->adc[i].wb_buff = (double*)malloc0(1024 * sizeof(double));
		}

		for (i = 0; i < MAX_RX_STREAMS; i++) {
			prn->rx[i].id = i;
			prn->rx[i].rx_adc = 0;
			prn->rx[i].frequency = 0;
			prn->rx[i].enable = 0;
			prn->rx[i].sync = 0;
			prn->rx[i].sampling_rate = 48;
			prn->rx[i].bit_depth = 24;
			prn->rx[i].preamp = 0;
			prn->rx[i].rx_in_seq_no = 0;
			prn->rx[i].rx_out_seq_no = 0;
			prn->rx[i].rx_in_seq_err = 0;
			prn->rx[i].time_stamp = 0;
			prn->rx[i].bits_per_sample = 0;
			prn->rx[i].spp = 238;				// IQ-samples per packet

			//MW0LGE
			prn->rx[i].rx_in_seq_delta_index = 0;
			for (int ii = 0; ii < MAX_IN_SEQ_LOG; ii++) {
				prn->rx[i].rx_in_seq_delta[ii] = 0;
			}
			prn->rx[i].snapshots_head = NULL;
			prn->rx[i].snapshots_tail = NULL;
			prn->rx[i].snapshot_length = 0;
		}

		for (i = 0; i < MAX_TX_STREAMS; i++) {
			prn->tx[i].id = i;
			prn->tx[i].frequency = 0;
			prn->tx[i].sampling_rate = 192;
			prn->tx[i].cwx = 0;
			prn->tx[i].dash = 0;
			prn->tx[i].dot = 0;
			prn->tx[i].ptt_out = 0;
			prn->tx[i].drive_level = 0;
			prn->tx[i].phase_shift = 0;
			prn->tx[i].epwm_max = 0;
			prn->tx[i].epwm_min = 0;
			prn->tx[i].pa = 0;
			prn->tx[i].mic_in_seq_no = 0;
			prn->tx[i].mic_in_seq_err = 0;
			prn->tx[i].mic_out_seq_no = 0;
			prn->tx[i].spp = 240;				// IQ-samples per packet
		}

		for (i = 0; i < MAX_AUDIO_STREAMS; i++)
		{
			prn->audio[i].spp = 64; // 64;	360;			// LR-samples per packet
		}

		prn->puresignal_run = 0;

		for (i = 0; i < 6; i++)
			prn->discovery.MACAddr[i] = 0;
		prn->discovery.BoardType = 0;
		prn->discovery.protocolVersion = 0;
		prn->discovery.fwCodeVersion = 0;
		prn->discovery.MercuryVersion_0 = 0;
		prn->discovery.MercuryVersion_1 = 0;
		prn->discovery.MercuryVersion_2 = 0;
		prn->discovery.MercuryVersion_3 = 0;
		prn->discovery.PennyVersion = 0;
		prn->discovery.MetisVersion = 0;
		prn->discovery.numRxs = 0;

		prbpfilter = (RBPFILTER)malloc0(sizeof(rbpfilter));
		prbpfilter->bpfilter = 0;
		prbpfilter->enable = 1;

		prbpfilter2 = (RBPFILTER2)malloc0(sizeof(rbpfilter2));
		prbpfilter2->bpfilter = 0;
		prbpfilter2->enable = 2;

		prn->hReadThreadMain = NULL;
		prn->hReadThreadInitSem = NULL;
		prn->hKeepAliveThread = NULL;
		prn->hTimer = NULL;

		create_obbuffs(0, 1, 2048, prn->audio[0].spp);	// last parameter is number of samples per packet
		create_obbuffs(1, 1, 2048, prn->tx[0].spp);
		(void)InitializeCriticalSectionAndSpinCount(&prn->udpOUT, 2500);
		(void)InitializeCriticalSectionAndSpinCount(&prn->rcvpkt, 2500);
		(void)InitializeCriticalSectionAndSpinCount(&prn->sndpkt, 2500);
		SendpOutbound(OutBound);
	}
}

PORT
void clearSnapshots()
{
	int i;
	for (i = 0; i < MAX_RX_STREAMS; i++) {
		while (prn->rx[i].snapshots_head != NULL)
		{
			_seqLogSnapshot_t* tmp = prn->rx[i].snapshots_head;
			prn->rx[i].snapshots_head = tmp->next;
			free(tmp);
		}
		prn->rx[i].snapshot_length = 0;
		prn->rx[i].snapshots_tail = NULL;
	}
}

PORT
void destroy_rnet() {
	int i;
	for (i = 0; i < MAX_ADC; i++)
	{
		_aligned_free(prn->adc[i].wb_buff);
	}
	DeleteCriticalSection(&prn->udpOUT);
	DeleteCriticalSection(&prn->rcvpkt);
	DeleteCriticalSection(&prn->sndpkt);
	free(prn->OutBufp);
	for (i = 0; i < 2; i++)
		free(prn->syncrxbuff[i]);
	free(prn->TxReadBufp);
	free(prn->RxReadBufp);
	free(prn->ReadBufp);
	//MW0LGE
	clearSnapshots();
	//
	free(prn);
	_aligned_free(prbpfilter);
	_aligned_free(prbpfilter2);
	destroy_obbuffs(0);
	destroy_obbuffs(1);
}

void
PrintTimeHack() {
	GetLocalTime(&lt);
	printf("(%02d/%02d %02d:%02d:%02d:%03d) ", lt.wMonth, lt.wDay, lt.wHour, lt.wMinute, lt.wSecond, lt.wMilliseconds);
}

void PeakFwdPower(float fwd)
{
	if (fwd > FwdPower)
		FwdPower = fwd;
	else
		FwdPower *= MDECAY;
}

void PeakRevPower(float rev)
{
	if (rev > RevPower)
		RevPower = rev;
	else
		RevPower *= MDECAY;
}
