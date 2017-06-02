//=================================================================
// release_notes.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2009  FlexRadio Systems
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// You may contact us via email at: sales@flex-radio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    8900 Marybank Dr.
//    Austin, TX 78750
//    USA
//=================================================================


//	Bug Fixes:
//
//		Issue:			No IQ correction on CW Xmit 
//		Fix:			Add call to correctIQ in CW Xmit path, fix CWTone generator to 
//						set 'have' field of buffer so correctIQ works 
//		Fixed By:		KD5TFD 
//		SVN:			740


#region Beta v1.6.3 Released 10/06/06 SVN Rev 695
// Beta v1.6.3 Released 10/06/06 SVN Rev 695
//
//	Bug Fixes:
//
//		Issue:			TUN in AM/SAM/FMN is 11kHz off frequency.
//		Fix:			This issue was caused by the 11kHz IF when in these modes.
//						We have adjusted the tone to adjust for this IF.
//		Fixed By:		KE5DTO
//		SVN:			642
//
//		Issue:			Squelch transitions sound rough.
//		Fix:			There was a bug in the transition routine that was causing
//						the ramp to do strange things.  This has been addressed.
//		Fixed By:		KE5DTO, N4HY
//		SVN:			633
//
//		Issue:			Panadapter transmit display is affected by the preamp
//						setting.
//		Fix:			The preamp offset was being used by the display routine
//						regardless of MOX mode.  This has been addressed and
//						the transmit data line should no longer be affected by
//						preamp setting.
//		Reported by:	W4TME
//		Fixed By:		KE5DTO
//		SVN:			574
//
//		Issue:			Hover tuning is sometimes off on VFO B.
//		Fix:			A typo error was causing this problem.  The hover
//						tuning for VFO B now works correctly like VFO A.
//		Reported by:	K6JCA
//		Fixed By:		K6JCA
//		SVN:			573
//
//		Issue:			Spur reduction is always off after MOX.
//		Fix:			In the refactoring of the MOX handler, some code was
//						left out to put the spur reduction in the right state.
//						This has been addressed.
//		Reported by:	NO2T, K6JCA
//		Fixed By:		KE5DTO
//		SVN:			571
//
//		Issue:			Click tuning a signal in DIGL mode always causes the
//						signal to be centered in the filter passband regardless
//						of the DIGL click tuning offset.
//		Fix:			There was an error that caused the click tuning
//						algorithm to think that the offset was always outside
//						of the selected filter.  This has been fixed.
//		Reported by:	N2UO
//		Fixed By:		K6JCA
//		SVN:			559
//
//		Issue:			Keyspan USB Serial Adapter's can't be selected in com port selections.										
//		Fix:			The Keyspan adapter returns FILE_TYPE_UNKNOWN on a GetFileType call so 
//                      we now allow that filetype in the serial port opening code (SerialPort)
//		Reported by:	N4DWK (5/29/2006 on Flex-Radio list) 
//		Fixed By:		KD5TFD
//		SVN:			690
//
//		Issue:			ZZSF (set filter) CAT command does not work in versions greater than 1.6.1. 									
//		Fix:			Update CAT code to use new filter setting logic added after 1.6.1
//		Reported by:	KD5TFD
//		Fixed By:		KD5TFD
//		SVN:			690
//
//
//  Modifications:
//
//		Feature:		Iambic Mode A.
//		Change:			Users now have the option to choose Iambic Mode A in the Keyer form.
//		Coded By:		KE5DTO
//		SVN Rev:		650
//
//		Feature:		Numerous new appearance controls.
//		Change:			In order to allow complete customization of the console, we have added
//						many more color controls to the appearance menu in the Setup Form.
//		Coded By:		KE5DTO
//		SVN Rev:		650
//
//		Feature:		SWR Protection revised.
//		Change:			The SWR protection now has a fold-back type of setup that will allow
//						recovery without having to return to receive mode.
//		Coded By:		KE5DTO
//		SVN Rev:		649
//
//		Feature:		Small last 3 digits on VFO.
//		Change:			We have added an option to allow the last 3 digits of each VFO to be
//						in a smaller font for easier readability.
//		Coded By:		K6JCA, KE5DTO
//		SVN Rev:		647
//
//		Feature:		PA Calibration routine now uses nominal values.
//		Change:			The cal routine uses 100W on all bands except 10 and 12m where it uses
//						75W.
//		Coded By:		KE5DTO
//		SVN Rev:		641
//
//		Feature:		Display code cleanup.
//		Change:			We have done a major league cleanup of the display code and associated
//						routines.  This has resulted in a significant performance increase even
//						with the larger display.
//		Coded By:		KE5DTO
//		SVN Rev:		595
//
//		Feature:		SWR selection now possible anytime.
//		Change:			SWR mode is now selectable at any time.  It will display "in TUN only"
//						when it is selected in TX mode outside of TUN.  This is another mod
//						thanks to K6JCA.
//		Coded By:		K6JCA
//		SVN Rev:		587
//
//		Feature:		Improved transition handling.
//		Change:			We changed some mechanics of how we transition from RX to TX and back
//						in order to prevent popping and hiss.  The result is a much cleaner
//						transition.
//		Coded By:		KE5DTO, N4HY
//		SVN Rev:		581
//
//		Feature:		New Tuning Step Rates.
//		Change:			This is the first of many interface features we intend
//						to improve thanks to K6JCA and his modifications.
//		Coded By:		K6JCA
//		SVN Rev:		569
//
//		Feature:		Console/Display Redesigned.
//		Change:			The console has been made larger.  It was designed to
//						fit within a 1024x768 or higher resolution.  The
//						display has been extended to match the new layout. We have also
//						moved to more sliders vs. the up/down controls.  Other changes
//						include graphical controls for setting Squelch (SQL), the noise
//						gate, and VOX.
//		Coded By:		K6JCA, KE5DTO
//		SVN Rev:		572
//
//		Feature:		TX Meter SWR option always available.
//		Change:			The SWR option can now be selected outside of TUN mode.
//						While transmitting outside of TUN mode, it will display
//						"in TUN only".
//		Coded By:		K6JCA
//		SVN Rev:		587
//
//	New Features:
//
//		Feature:		Sub Receiver.
//		Description:	We have talked about it for a long time and have finally implemented
//						a second receiver within the passband.  Use VFO to control the
//						frequency.  For now the mode and filter are shared (locked) between
//						the two VFOs.  Credit goes to Frank AB2KT and Bob N4HY for having this
//						in the DSP long before we got it into the interface.
//		Coded By:		KE5DTO
//		SVN Rev:		683
//
//		Feature:		Edge style multimeter.
//		Description:	Again, big thanks to Jeff, K6JCA, the user can now select between
//						the original or Edge style multimeter displays.
//		Coded By:		K6JCA, KE5DTO
//		SVN Rev:		669
//
//		Feature:		Panadapter Zoom and Pan controls.
//		Description:	Thanks to Jeff, K6JCA, the user can now adjust the bandwidth
//						of the display.  When zoomed in tight, you can pan to see the
//						entire spectrum.  We all owe Jeff a huge thanks for implementing
//						this in his console (and sharing his source with us).
//		Coded By:		K6JCA, KE5DTO
//		SVN Rev:		665
//
//		Feature:		Mode specific control area.
//		Description:	To ease operation, we have a new area on our console that is
//						dedicated to the specific mode currently in use.  This should
//						keep the user from having to reference controls on the Setup
//						Form too often.
//		Coded By:		KE5DTO
//		SVN Rev:		625
//
//		Feature:		Edirol FA-66 Support.
//		Description:	The new 192kHz FA-66 supported soundcard made by Edirol is
//						now supported.
//		Coded By:		KE5DTO
//		SVN Rev:		621
//
//		Feature:		Wave File Gain.
//		Description:	In an effort to make wave files easier to play over the air,
//						we have added a gain control that is used whenever the user
//						transmits while playback is active.  This helps keep the
//						microphone gain in a reasonable range while allowing the wave
//						file to play at an adaquate power level.
//		Coded By:		KE5DTO
//		SVN Rev:		659
//
//		Feature:		Custom Filter Setup.
//		Description:	We have added a complete interface that allows the user to
//						setup all of the receiver filters for each mode.  Right click
//						on the filter area and select "Configure..." to see this new
//						feature.
//		Coded By:		KE5DTO
//		SVN Rev:		601
//
//		Feature:		Whole filter drag'n'drop.
//		Description:	To complement the ability to drag the filter edges, you can
//						now grab the whole filter and slide it around.  The cursor
//						will change to indicate which drag mode it is in.
//		Coded By:		KE5DTO
//		SVN Rev:		596
//
//		Feature:		Filter Width Modes.
//		Description:	We have added Filter Width Slider Modes to allow Linear
//						as well as Log mappings for the width slider.  These
//						new options are available on the Setup Form->General
//						Tab->Options SubTab.
//		Coded By:		KE5DTO
//		SVN Rev:		528
//
//		Feature:		FireBox Mixer Bug Workaround.
//		Description:	With the FireBox beta firmware, there is a bug that
//						causes the input to bleed into the output until the
//						FireBox mixer is launched.  To combat this issue, when
//						the FireBox is selected (and physically detected), the
//						software will briefly launch the FireBox mixer and then
//						close it.
//		Coded By:		KE5DTO
//		SVN Rev:		586
//
//		Feature:		Enable Keyboard Shortcuts control added.
//		Description:	Due to popular demand, we have added a checkbox that
//						will enable/disable all keyboard shortcuts.  This can
//						be unchecked to prevent keystrokes intended for a
//						logger or other application from causing unintended
//						results.
//		Coded By:		KE5DTO
//		SVN Rev:		588
#endregion

#region v1.6.2 Released 06/23/06 SVN Rev 555
//v1.6.2 Released 06/23/06 SVN Rev 555
//
//	Bug Fixes:
//
//		Issue:			Zero beating an AM signal with an asymmetrical filter
//						results in the carrier being at the wrong place.
//		Fix:			We have adjusted the zero beat function to be mode
//						specific.  SSB and CW use the CW pitch while the DIGx
//						modes use their click tune offsets as defined in the
//						Setup Form.  All of these modes will zero beat to the 
//						middle of the filter if the pitch is not within the
//						passband of the filter.  For AM/SAM/FMN modes, the
//						carrier is always zero beated to the carrier regardless
//						of the filter selection.
//		Reported by:	KC2LFI, BugID# 360
//		Fixed By:		KE5DTO
//		SVN:			555
//
//		Issue:			Display is not updated when changing sample rates in
//						SPEC DSP mode.
//		Fix:			The display is now forced to update when changing the
//						sample rate in this mode.
//		Reported by:	VK6APH, BugID# 359
//		Fixed By:		KE5DTO
//		SVN:			554
//
//		Issue:			Display during CW Transmit is unpredictable.
//		Fix:			In CW, the display should now do the following:  In 
//						Panadapter mode, it will have the usual display less
//						the filter.  In all other modes, it will display as in
//						receive mode without a dataline.
//		Reported by:	VE6IV, BugID# 349
//		Fixed By:		KE5DTO
//		SVN:			543
//
//		Issue:			160 Hz transmit notch.
//		Fix:			Bandstop filters added for 96000 and 192000
//						samples per second.
//		Reported by:	W5GI and others
//		Fixed By:		N4HY
//		SVN:			540
//
//		Issue:			VAC Stereo enable with Power On causes crash.
//		Fix:			The test for the power on condition was faulty in
//						deciding what do to when the VAC stereo checkbox
//						state was changed.
//		Reported by:	N6SF,  BugID 345
//		Fixed By:		N4HY
//		SVN:			534
//
//		Issue:			6m output low if PA is present.
//		Fix:			The PA TR relay was being put in the wrong state for
//						this mode and was preventing RF from passing through
//						the PA.  This has been revolved.
//		Reported By:	KL7JDR, K9FOH
//		Coded By:		KE5DTO
//		SVN:			525
//
//		Issue:			VAC Latency.
//		Fix:			The VAC ring buffers had the same mechanisms in them for
//						adding zero padding at the beginning of the ring buffers
//						rather than allowing the minimum amount required by the
//						callbacks.
//		Reported By:	N4HY, W4TME
//		Coded By:		N4HY
//		SVN:			521
//
//		Issue:			CW Latency.
//		Fix:			Not since we first built the keyer had we done anything
//						other than optimize the audio class and cw class structures
//						for handling callbacks and ring buffer manipulations. We
//						had not looked at the console and its MOX/PTT/etc. functions.
//						The same poorly designed MOX code has been holding up the
//						switching since the first day.  We have reduced the switch
//						time significantly and deleted ALL delay in the CW ring
//						buffer filling code. All the needed delay is in the IF/Audio
//						ring buffers already.
//		Reported By:	N4HY, VE6IV, KE5DTO
//		Fixed By:		KE5DTO, N4HY
//		SVN Rev:		520
//						
//		Issue:			TUN is reversed.
//		Fix:			When we fixed the "TUN is always USB" issue, we made a
//						mistake that caused TUN to always be on the wrong
//						sideband.  This has been corrected.
//		Reported By:	K6JCA
//		Fixed By:		KE5DTO
//		SVN Rev:		512
//
//		Issue:			CW Ring Buffer protection.
//		Fix:			When we moved the CW Ring Buffer resets to the callback
//						we mistakenly assumed that all instances of overwrites
//						were eliminated.  The critical sections were removed.
//						It is POSSIBLE this was mistaken.  They have been
//						reintroduced to see if the offset l/r buffers are fixed
//						as a result.  CW Ring reset size modified to be a bit longer
//						than the audio buffer.  PollPTT period decreased to help
//						shorten the time between key closure and RF generation.
//						In addition,  critical sections have been replaced with
//						critical sections and spin locks.  We have also added
//						critical section protection to the ring buffers used in
//						exchange of samples for VAC.  Clean Up of the VAC
//						initialization also done.
//		Reported By:	K6JCA, VE6IV, W4TME
//		Fixed By:		N4HY
//		SVN Rev:		516
//
//		Issue:			Squelch enable interferes with RX EQ.
//	    Fix:			The graphic equalizer was called only if squelch
//						was disabled.
//		Reported By:	W3DUQ
//		Fixed By:		N4HY
//		SVN Rev:		482
//
//		Issue:			Frequency Calibration changes the filter.
//		Fix:			The frequency calibration now saves the filter before
//						changing it and restores it once the calibration has
//						been completed.
//		Reported By:	W5SXD
//		Fixed By:		KE5DTO
//		SVN Rev:		470
//
//		Issue:			Data is sent to the LPT port on initialization even if
//						configured for the USB Adapter.
//		Fix:			We have rearranged the initialization so that no
//						commands are sent until the configuration is read and
//						has taken affect.
//		Reported By:	K5KDN
//		Fixed By:		KE5DTO
//		SVN Rev:		472
//
//		Issue:			The default soundcard mixer's volume gets set to
//						maximum when starting the software for the first time.
//		Fix:			We have rearranged the initialization so that the
//						windows mixer is not touched until the user picks their
//						soundcard (and then only if necessary).
//		Reported By:	Many
//		Fixed By:		KE5DTO
//		SVN Rev:		473
//
//		Issue:			The TX Meter reads 3.0dB lower than the analog meter.
//		Fix:			We had a built in compensation for a 3.0dB gain factor 
//						in the DSP in the meter code.  We eliminated the gain
//						factor when cleaning up the DSP, but failed to catch
//						our compensation.  This is now working as intended.
//		Reported By:	HB9AJP
//		Fixed By:		KE5DTO
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=312&it=B
//		SVN Rev:		477
//
//		Issue:			The TX Meter reads 3.0dB lower than the analog meter.
//		Fix:			We had a built in compensation for a 3.0dB gain factor 
//						in the DSP in the meter code.  We eliminated the gain
//						factor when cleaning up the DSP, but failed to catch
//						our compensation.  This is now working as intended.
//		Reported By:	VK6APH, N6SF
//		Fixed By:		KE5DTO
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=311&it=B
//		SVN Rev:		478
//
//  Modifications:
//
//		Feature:		Filter Shift limits removed.
//		Change:			The filter shift control has been changed to be able to
//						cover the whole +/- 10kHz passband.
//		Coded By:		KE5DTO
//		SVN Rev:		495
//
//		Feature:		RIT/XIT Min/Max changed.
//		Change:			As requested, we have modified the the RIT and XIT
//						controls to have more range.  The limit is now
//						+/-100kHz vs the older +/-10kHz.
//		Requested By:	AI6A
//		Coded By:		KE5DTO
//		SVN Rev:		495
//
//		Feature:		Varible Offset Click Tuning for DIGL/DIGU.
//		Change:			By popular demand, we have changed the RTTY and SSTV
//						click tuning controls to be more generically aimed at
//						the DIGL and DIGU modes.  You can now set the offset in
//						Hz on the Setup Form.
//		Requested By:	ES7AAZ
//		Coded By:		KE5DTO
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=242&it=B
//		SVN Rev:		476
//
//	New Features:
//
//		Feature:		Filter Width Modes.
//		Description:	We have added Filter Width Slider Modes to allow Linear
//						as well as Log mappings for the width slider.  These
//						new options are available on the Setup Form->General
//						Tab->Options SubTab.
//		Coded By:		KE5DTO
//		SVN Rev:		528
//
//		Feature:		Display Click 'n' Drag Features.
//		Description:	Moving the mouse near the edge of the filter in the
//						Panadapter display mode turns the cursor into a hand.
//						Click and drag to move that edge of the filter.
//						Similarly, click and drag any part of the spectrum to
//						move the signals (i.e. retune the VFO).
//		Coded By:		KE5DTO
//		SVN Rev:		531
#endregion

#region v1.6.1 Released 05/04/06 SVN Rev 469
//v1.6.1 Released 05/04/06 SVN Rev 469
//
//	Bug Fixes:
//
//		Issue:			Sometimes Display Level Calibration is off following
//						the cal routine.
//		Fix:			The display routine used the old calibration value when
//						calculating the new value.  This caused the display to
//						be off.  This has been addressed in the calibration
//						routine.
//		Reported By:	N4HY
//		Fixed By:		KE5DTO, N4HY
//		SVN Rev:		466
//
//		Issue:			Changed voltage does not take effect.
//		Fix:			When changing the voltage either manually or by
//						selecting another supported soundcard, the voltage was
//						not being used until the PWR control was changed.  This
//						has been addressed and the voltage change now takes
//						affect immediately when the control changes.
//		Reported By:	W5GI
//		Fixed By:		N4HY
//		SVN Rev:		460
//
//		Issue:			TUN is always USB.
//		Fix:			The TUN function has been modified to output a tone in
//						the lower sideband if in LSB, CWL, or DIGL.
//		Reported By:	K1RQG
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=292&it=B
//		Fixed By:		N4HY, KE5DTO
//		SVN Rev:		465
//		
//		Issue:			Tortoise SVN Settings for Putty Problems.
//		Fix:			Tortoise SVN may, or may not successfully
//						communicate when using Putty.exe.  Use Putty.exe
//						as normal to set up your sessions.  BUT then
//						in the explorer (not Internet Explorer) right click
//						on the folder of interest,  click on TortoiseSVN,
//						Settings, Network and REMOVE the entire contents
//						of the SSH client box.  Leave it blank.  This will
//						cause the internal SSH mechanism to run
//						(TortoisePlink).  I have been unable to find a fix
//						until the link below.
//		References:		http://www.svnforum.org/2017/viewtopic.php?p=3619#3619
//		Reported By:	N4HY
//
//		Issue:			Ring Buffer settings.
//		Fix:			The default ring buffer settings in the dsp, keyer
//						were set incorrectly.  This led to occasional
//						burps which were most evident if using VAC on
//						a mode giving a display.  More work is likely
//						needed to understand all cases.
//		Reported By:	W4TME, N4HY
//		Fixed By:		N4HY
//		SVN Rev:		428
//
//		Issue:			Level calibration is broken on 3-board stacks.
//		Fix:			The rewritten level calibration routine had a problem
//						with the 3-board stack algorithm.  It had both a sign
//						error (+ instead of -) and a comparison flaw in the
//						algorithm.  These have both been fixed and the
//						calibration should now be accurate for all
//						configurations of the radio.
//		Reported by:	VK6APH
//		Fixed By:		KE5DTO
//		SVN Rev:		434
//
//		Issue:			VAC reset, and PTT handling faulty.
//		Fix:			Just as with the regular IF path ring buffers, one must
//						be careful about attempting asynch operations on the
//						ring buffers associated	with Virtual Audio Cables
//						operation.  In addition, no attempt at all was made to
//						keep the depth/latency constant from one push to talk
//						to the next.  So, this is handled by the rb resets and
//						they are implemented in the	fashion developed for DttSP
//						(done only in the callback).
//		Reported By:	IK3VIG, W4TME
//		Fixed By:		N4HY
//
//		Issue:			Audio start request wrong in audio.cs.
//		Fix:			Irrespective of the buffer size setting on the setup
//						form, the actual value submitted to portaudio was being
//						set to 2048.  This has been in all versions since the
//						big audio cleanup.
//		Reported by:	KD5TFD
//		Fixed By:		N4HY
//
//		Issue:			CW breaks in even if SemiBreakin is disabled.
//		Fix:			The PollPTT	routine did not handle the SemiBreakin flag
//						correctly.  This has been fixed.
//		Reported By:	VE6IB
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=280&it=B
//		Fixed By:		N4HY
//		SVN Rev:		449
//
//		Issue:			The Tooltip on Audio Sample Rate on setup form wrong.
//		Fix:			The Tooltip still read "locked to 48000 samples per
//						second" even though this rate may now be changed.
//		Reported By:	NJ1H
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=251&it=B
//		Fixed By:		N4HY
//		SVN Rev:		450
//
//		Issue:			Phase2 causes segment violations.
//		Fix:			The Phase2 display process was not being initialized
//						with large enough buffers to handle all cases.  This
//						has been adressed to prevent the memory errors.
//		Reported By:	WA6AHL
//		Fixed By:		N4HY
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=283&it=B
//		SVN Rev:		451
//
//  Modifications:
//
//		Feature:		Offset for I,Q provided for in DSP.
//		Change:			Some A/D's have a integer sample offset and provision has
//						been made in DttSP for this offset.
//		Requested By:	N4HY, KD5TFD, VK6APH, WB9YIG
//		Coded By:		N4HY, KD5TFD
//		SVN Rev:		456
//
//		Feature:		Click Tuning Expanded to include VFO B.
//		Change:			The Click Tuning feature will now cycle through three
//						states when Split is turned on: Off, VFOA, and VFOB.
//						The crosshairs will be red when click tuning is in the
//						VFO B state.  Left clicking while in this mode will
//						tune VFO B.  Click tuning when Split is off is
//						unchanged.
//		Requested By:	WO0Z, AB7R, WA6AHL
//		Coded By:		KE5DTO
//		SVN Rev:		469
//
//		Feature:		XIT, RIT, and SPLT can now work concurrently.
//		Change:			We have changed the way XIT, RIT and SPLT work so that
//						all three of these can be used with or without the
//						others.
//		Requested By:	N2UO
//		Coded By:		KE5DTO
//		SVN Rev:		468
#endregion

#region v1.6.0 Released 03/28/06 SVN Rev 383
//v1.6.0 Released 03/28/06 SVN Rev 383
//
// Bug Fixes 
//
//		Issue:			Confusing TX Profile results in lost settings.
//		Fix:			The TX Profile controls have been modified so that when
//						you are prompted to save your previous changed settings
//						(on an attempt to change profiles), you are correctly
//						prompted with the old profile name rather than the new
//						one.
//      Reported by:	W4TME
//		Fixed by:		KE5DTO
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=117&it=B
//		SVN Rev:		382 
//
//		Issue:			Variable filters sometimes forget the last setting.
//		Fix:			The variable filters are designed to remember their
//						settings independantly by mode for both filters (Var1 &
//						Var2).  The Filter Shift slider was affecting this
//						behavior when it was offset from 0 (center).  This was
//						due to a decision made to store the original variable
//						filters rather than the shifted values when using the
//						shift control.  From now on, adjusting the variable
//						filter Low or High cut controls manually will result in
//						the Shift slider being automatically centered.
//      Reported by:	W9OY, K5SDR
//		Fixed by:		KE5DTO
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=229&it=B
//		SVN Rev:		381 
//
//		Issue:			Level calibration is not accurate.
//		Fix:			We rewrote the level calibration routine and it now
//						uses 50 samples to average the meter and preamp offsets
//						and 10 samples to average the display correction.  This
//						results in a much more consistant and accurate
//						calibration across all preamp modes for both the
//						display and the RX Meter.  Because of the extra
//						samples, the routine is slightly slower.  For this
//						reason we have added a progress meter to the level
//						calibration routine.
//      Reported by:	K5KDN, N4HY
//		Fixed by:		KE5DTO, N4HY
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=232&it=B
//		SVN Rev:		376 
//
//		Issue:			The Old Keyer buffers are lost if the New Keyer is
//						enabled.
//		Fix:			This was a problem with how we handled turning off the
//						old keyer.  This has been addressed and the buffers no
//						longer revert to the default values when turning the
//						new keyer on and off.
//      Reported by:	K0PFX
//		Fixed by:		KE5DTO
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=230&it=B
//		SVN Rev:		377 
#endregion

#region v1.4.5 Beta Preview 19 Released 03/24/06 SVN Rev 369
//v1.4.5 Beta Preview 19 Released 03/24/06 SVN Rev 369
//
// Bug Fixes 
//
//		Issue:			When AVG is on, the RX<->TX transitions are slower.
//		Fix:			This problem was caused by processing the silenced
//						buffers that go through the DSP while in the transition
//						state.  These zero filled buffers caused the average to
//						get pulled very low and the recovery time constant was
//						too long.  To fix this, we have added a delayed average
//						reset for one buffer length for the transition periods.
//      Reported by:	W9DR, W5GI
//		Fixed by:		KE5DTO 
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=214&it=B
//		SVN Rev:		368 
//
//		Issue:			With VAC Auto Enable on, switching from DIGU to CWU
//						causes the CW tone to break up.
//		Fix:			This issue was due to multiple calls to StartKeyer
//						without matching calls to StopKeyer.  The reason this
//						happens in this particular situation is that the
//						Standby handler fires because of the transition to non
//						VAC mode and this starts up a keyer as well as the mode
//						switch.  Matching calls have been added and this is no
//						longer an issue.
//      Reported by:	W0VB
//		Fixed by:		KE5DTO 
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=192&it=B
//		SVN Rev:		367 
//
//		Issue:			Both Rev Pow and Ref Pow show up in TX Meter.
//		Fix:			This was an oversight and is now fixed. 
//      Reported by:	NJ1H
//		Fixed by:		KE5DTO 
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=206&it=B
//		SVN Rev:		354 
#endregion

#region v1.4.5 Beta Preview 18 Released 03/17/06 SVN Rev 340
//v1.4.5 Beta Preview 18 Released 03/17/06 SVN Rev 340
//
// Bug Fixes 
//
//		Issue:			SoftRock cannot do 96kHz.
//		Fix:			This is due to the tuning limits for SoftRock Tuning
//						not being adjusted when the sampling rate is changed. 
//      Reported by:	Hmmm.. I forgot -- someone told me about it! 
//		Fixed by:		KD5TFD 
//		SVN Rev:		328 
//
//		Issue:			DRM mode broken with SoftRock.
//		Fix:			Tuning is not correct for DRM mode with SoftRock.
//						This was fixed to set the software osccillator up 12kHz
//						from  where we typically would when in DRM Mode.
//      Reported by:	DJ9CS (Bodo) 
//		Fixed By:		KD5TFD 
//		SVN Rev:		328 
//
//		Issue:			PTT does not work on CW, non-iambic with  SDR/CAT
//						selected for primary/secondary connections.
//		Fix:			This turned out to be an initialization ordering bug.
//						the siolisten variable has been moved forward in the
//						init order to prevent this problem.
//		Reported By:	N1SG
//		Fixed By:		KE5DTO
//		SVN Rev:		322 
//
//		Issue:			TUN backcolor does not change when ATU Bypass mode is
//						manually selected.
//		Fix:			With the changes to the ATU related controls, we failed
//						to catch this in Preview 16/17.  This now works as it
//						should.
//		Reported By:	N1SG
//		Fixed By:		KE5DTO
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=143&it=B
//		SVN Rev:		325 
//
//		Issue:			DRM panadapter display is incorrect in new sample rates.
//		Fix:			In many places in the display code there were hard coded
//						numbers that needed to be made independent of the sampling
//						rate. This was one such issue.
//		Reported By:	NJ1H
//		Fixed By:		N4HY
//		References:		http://support.flex-radio.com/AdminEditBug.aspx?id=137&it=B
//
//		Issue:			Gain mangement issues in the transmitter.
//		Fix:			The DSP core has been returned to operating on signals in the
//						power range 0 to 1 and producing signals intended for the
//						power range (0,1).  There are small excursions, only a few
//						percent over this in the DSP.  This allows complete gain setting
//						to be done on the radio side and not in the dsp core.  This
//						made a noticeable difference in the performance of the
//						compressor and compandor.
//		Fixed By:		N4HY
//		SVN:			320, 329, 332
//
//  Modifications:
//
//		Feature:		Two new TX meters added.
//		Change:			In the continuing tradition of allowing monitoring of every
//						signal in the transmitter, we have enabled two new meters.
//						These allow inspection of the Leveler's applied gain (not
//						just the power coming from the leveler) as well as the ALC
//						gain (not just the power following the ALC).
//
//		Feature:		DRM Filter bandwidth changed.
//		Change:			The DRM receive filter was about 1 kHz (!) too wide. The DRM
//						and DREAM measured SNR both went up after this modification.
//		Coded By:		N4HY
//
//		Feature:		AGC Slope Setting.
//		Change:			The AGC Slope was in steps of centiBels and was confusing.
//						We really only need dB steps and the control has been modified
//						to change the gain slope from 0 dB to 10 dB.
//		Suggested By:	VK6APH
//		Coded By:		N4HY
//		Reference:		http://support.flex-radio.com/AdminEditBug.aspx?id=118&it=B
//		SVN:			319
#endregion

#region v1.4.5 Beta Preview 17 Released 03/10/06 SVN Rev 313
//v1.4.5 Beta Preview 17 Released 03/10/06 SVN Rev 313
//
//	Bug Fixes:
//
//		Issue:			"Binaural / DSB appears" Part 2
//		Fix:			In the DSP code, if the ring buffer loading and
//						unloading subroutine (gethold inside winmain.c)
//						detects a pathology, it will issue a ring buffer
//						reset command.  It will no longer do the rb reset
//						in this subroutine and for the same reasons.
//						Yet more pops, glitches and burps are now gone.
//						The keyer was also subject to this problem. That
//						is repaired as well.
//		Fixed By:		N4HY
//		SVN Rev:		312
//
//		Issue:			BandText is missing data for 17, 12 and 10m.
//		Fix:			This issue stems from problems with importing
//						conflicting databases from early in this preview
//						series.  The short term fix is just to simply not
//						import the BandText table from previous versions.
//						Those of you using a version that has been modified
//						clearly have the tools (MS Access) to import the
//						database manually.  We understand that this is an
//						inconvenience to those that have modified the database,
//						but we cannot continue to cause problems for the
//						majority of users that do not edit the BandText table
//						in order to satisfy the minority.  We will plan to take
//						another look at this in the new architecture as the 
//						database design (likely XML) should be much more
//						forgiving.
//		Reported By:	W0IVJ
//		Fixed By:		KE5DTO
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=97&it=B
//		SVN Rev:		311
//
//		Issue:			ATU + Bypass does not work correctly.
//		Fix:			This state was not being handled properly in the event
//						handler for the TUN button.  This has been fixed.
//		Fixed By:		KE5DTO
//		SVN Rev:		310		
//
//		Issue:			Preview 16 crashes on startup.
//		Fix:			This was a problem with initialization after the
//						console was closed with a Var filter selected.  This
//						has been fixed.
//		Reported By:	ZL1WN, RW3PS
//		Fixed By:		N4HY, KE5DTO
//		References:		http://support.flex-radio.com/ReviewBug.aspx?id=91&it=B
//		SVN Rev:		306
//
//		Issue:			FireBox with 1W can overdrive the transmitter.
//		Fix:			We have added a hard clipper at 1.5V RMS to protect the
//						hardware (TRX IC2).  This means that with some setups,
//						if you severely overdrive the input, you may get
//						distorted output (splatter).  This is necessary at this
//						point to protect the hardware.
//		Fixed By:		KE5DTO
//		SVN Rev:		309
#endregion

#region v1.4.5 Beta Preview 16 Released 03/08/06 SVN Rev 301
//v1.4.5 Beta Preview 16 Released 03/08/06 SVN Rev 301
//
//	Bug Fixes:
//
//		Issue:			"Binaural / DSB appears"
//		Fix:			Upon MOX, PTT, key press and/or release and at random
//						times, the console would go into what appears at
//						first glance to be binaural in SSB/RX and DSB on
//						TX.  A poorly understood condition happens when we
//						reset the ring buffers in the DSP in the MOX handler.
//						This reset was supposedly protected by the update
//						semaphore. Its job is to prevent changes of state
//						while the DSP is operating on a buffer of samples.
//						It should only allow changes to occur when the DSP
//						is idle.  Some condition causes this process to
//						hang on for too long.  We have implemented the
//						obvious fix which is to do the ring buffer
//						manipulations at this level only in the callback
//						and is implemented by a flag.  Since the developers
//						could not get the PTT to make this condition,  we
//						leave it to the users to tell us if it is fixed.					
//		Reported By:	KE5DTO, DK7XL, others
//		Possible Fix By:N4HY, KE5DTO
//
//		Issue:			LSB filter memory is forgotten on startup.
//		Fix:			This is another case of the initialization monster.
//						Things are now done in the correct order and the LSB
//						filter is preserved.
//		Reported By:	W3DUQ
//		Fixed By:		KE5DTO
//
//		Issue:			The RX/TX Meter sometimes flickers.
//		Fix:			There was a race condition in the UpdateMultimeter
//						function that was causing this issue.  This has been
//						fixed and the meter should no longer flicker.
//		Reported By:	K1OC
//		Fixed By:		N4HY
//
//		Issue:			Using quick QSY while the VFO is locked messes up the
//						VFO frequency.
//		Fix:			This was a visual bug only (frequency stayed locked).
//						The VFO no longer changes when it is locked.
//		Reported By:	W4TME
//		Fixed By:		KE5DTO
//
//		Issue:			BIN does not work in AM/SAM/FM modes.
//		Fix:			This is not really a bug, but more of a consequence
//						that the demodulated audio is already a real audio
//						signal as opposed to a complex I/Q signal.  Therefore,
//						the BIN function serves no purpose in these modes.  The
//						BIN button is now disabled in these modes.
//		Reported By:	W3DUQ
//		Fixed By:		KE5DTO
//
//		Issue:			ATU controls are enabled in 2m band.
//		Fix:			This was a bug and it has been fixed.  Also, the TUN
//						button should no longer "lock" the console and it will
//						turn yellow when it has been tuned.  It will turn gray
//						when it is bypassed to be able to more easily tell its
//						current state.  SWR will now show up when using TUN
//						with the ATU in Bypass mode.  ATU Mode is now
//						remembered from the previous session.
//		Reported By:	K5BOT, K0PFX, others
//		Fixed By:		KE5DTO
//
//		Issue:			PTT sometimes does not work.
//		Fix:			There was a bug in the PollPTT() function that would
//						cause the PTT thread to exit if certain conditions 
//						were true (CW paddles hit while semi-break in was
//						turned off with the new keyer).  This has been fixed
//						and this case is now handled appropriately.
//		Reported By:	KM0T
//		Fixed By:		KE5DTO
//
//		Issue:			CWX will not change speed.
//		Fix:			The default Up/Down control behaviour was causing this
//						problem.  We needed to handle the LostFocus event for
//						the up down	controls in CWX so that Tab would terminate
//						an entry properly.
//		Reported By:	AA8K and others
//		Fixed By:		W5SXD
//
//		Issue:			Major source of instability repaired.
//		Fix:			In the audio callback processing class there were
//						several variables accessed from the console class.  In
//						many cases, these console members were written.  Since
//						each variable uses .NET memory management, access to
//						these variables within the callback proved to be
//						problematic.  We have replaced all such references with
//						local copies of the data to prevent this issue.  We are
//						looking feedback on the results in terms of stability.
//		Reported By:	KE5DTO, DK7XL, VK6APH
//		Fixed By:		KE5DTO, N4HY
//
//  Modifications:
//
//		Feature:		Receive AGC slope implemented.
//		Change:			The slope calculation has been included in the agc code.
//						If a signal computes that it needs 50 dB of gain, the
//						actual gain will be (50dB + slope).  This is limited by
//						the maximum agc gain setting. This is set on the setup,
//						dsp, agc/alc panel.
//		Coded By:		N4HY
//
//		Feature:		Receive AGC hang threshold implemented.
//		Change:			The receive agc now has hang threshold implemented. If the
//						received signal is below the hang threshold,  hang will not
//						occur.  AT A LATER TIME, we fill determine how to display
//						this since the calculation of hang threshold depends on
//						maximum agc gain and some thought must be given to the
//						display.
//		Coded By:		N4HY
//
//		Feature:		Noise Blanker improved.
//		Change:			The duration of real world pulses was not taken into account
//						in the previous version of the regular noise blanker. We
//						now assume that it is  seven samples long and is asymmetric
//						(stretched) by the analog equipment in front of the A/D.
//		Coded By:		N4HY
//
//		Feature:		Higher sample rate support.
//		Change:			Audio rates of 96000 samples per second and 192000 are
//						added as selectable rates.  The rates are restricted
//						to allowable rates on supported cards.  This will allow
//						wider panadapter displays.
//		Coded By:		N4HY, KE5DTO
//
//		Feature:		BLMS default values changed.
//		Change:			The forgetting factor on both BLMS-NR and BLMS-ANF were
//						causing forgetting to occur too quickly.  The result is
//						more stable performance of the filters on both ANF and NR.
//		Coded By:		N4HY
//
//	New Features:
//
//		Feature:		Quick QSY Enabled option added to Setup Form.
//		Description:	The Quick QSY feature allows the user to directly enter
//						a frequency in MHz and hit enter while the main console
//						has the focus to jump to that frequency.  While this
//						feature is enabled by default, we had a	request to
//						allow this feature to be disabled.
//		Suggested By:	WO0Z
//		Coded By:		KE5DTO
#endregion

#region v1.4.5 Beta Preview 15 Released 02/20/06 SVN Rev 165
//v1.4.5 Beta Preview 15 Released 02/20/06 SVN Rev 165
//
//	Bug Fixes:
//
//		Issue:			TX Equalizer enabled during DIGU and DIGL.
//		Fix:			Ooops.  The TX equalizer call was not wrapped
//						in the test to see if we were in DIGital
//						modes
//		Reported by:	IK3VIG
//		Fixed By:		N4HY
//
//		Issue:			AGC overshoot present in Custom settings.
//		Fix:			The preset code had all been brought into compliance
//						with the new computation so that the anticipation
//						(overshoot prevention) could work properly.  However,
//						changed computations were not done for the custome
//						controls.
//		Reported By:	DK7XL
//		Fixed By:		N4HY
//
//		Issue:			Memories are not available after an import until the
//						software is restarted.
//		Description:	The memory form is now closed and recreated whenever an
//						import is performed.
//		Fixed By:		KE5DTO
//
//		Issue:			Repeatedly pressing Apply or Apply + OK causes crashes.
//		Fix:			This was a threading issue.  Added protection prevents
//						functions from being re-entered that could cause
//						problems.
//		Fixed By:		KE5DTO
//
//		Issue:			Mono/Stereo selection on VAC was broken.
//		Fix:			Mono/Stereo selection was being attempted irrespective
//						of the Standby/On state of the console.  Now if the
//						radio is running, it is put into standby, the
//						changeover is made and then the processing is
//						restarted. Also, the handler was given a confusing
//						name.
//		Reported By:	AB1DO, W4TME
//		Fixed By:		N4HY
//
//		Issue:			Higher CPU load found in Preview 14.
//		Fix:			This ended up being an issue related to the new CWX
//						form.  The CWX threads and the timer CWX uses were all
//						running at startup in Preview 14 even if the form was
//						not open.  We have since made sure that these threads
//						only run when the form is open.
//		Reported By:	Many
//		Fixed By:		W5SXD, KE5DTO
//
//		Issue:			Memory keyer word space was wrong.
//		Fix:			Word space was 5 instead of the standard 7 elements.
//		Reported By:	N6VS
//		Fixed By:		W5SXD
//
//  Modifications:
//
//		Feature:		TX Meter changes
//		Change:			Removed Peak Power meter and replaced it with TX ALC gain
//		Coded By:		N4HY
//
//		Feature:		Leveler default is now on.
//		Change:			Now that we have the ALC working properly, many have
//						noticed a drop in average power when compared with
//						Preview 12 and before.  To help bring the average power
//						back up, enable the Leveler on the Setup Form -> DSP ->
//						ALC/AGC Tab at the default 10dB setting.  You might
//						also consider using the Compander (CPDR) at a level of
//						3.  This should give decent average power without
//						distorting the signal.
//		Suggested By:	W5GI, K5SDR
//		Coded By:		KE5DTO
//
//		Feature:		Block LMS gain control activated.
//		Change:			The Block LMS ANF and NR are still under development
//						but we need some feedback on the performance to
//						understand what	issues there are with the
//						implementation.  Now when you modify the LMS ANF/NR
//						gains,  you also change the gain for Block LMS
//						algorithms.  It is the only independent parameter at
//						this time.
//		Coded By:		N4HY
//
//		Feature:		VAC Controls Enabled.
//		Change:			The VAC controls are now enabled by default so you can
//						set them without having to turn it on.
//		Suggested By:	AB1DO
//		Coded By:		KE5DTO
//
//		Feature:		TX EQ Preamp re-enabled.
//		Change:			This control was disabled when we realized the ALC was
//						not functioning.  Now that the ALC is operating
//						properly, we are restoring this feature.
//		Suggested By:	K2WS
//		Coded By:		KE5DTO
//
//	New Features:
//
//		Feature:		AM Carrier Level control.
//		Description:	After many requests, we have added the carrier level
//						adjustment to the Transmit Tab on the Setup Form.  This
//						allows the operator to drop the carrier level from a
//						full 25W (assuming 100W PA) down to no carrier with
//						1000 steps in between.
//		Suggested By:	W3DUQ, W9AD
//		Coded By:		N4HY, KE5DTO
#endregion

#region v1.4.5 Beta Preview 14 Released 02/09/06
//v1.4.5 Beta Preview 14 Released 02/09/06
//
//	Bug Fixes:
//
//		Issue:			TX EQ is broken.
//		Fix:			While removing the TX EQ Preamp (duplicate
//						functionality with the front panel MIC control), we
//						broke the other	sliders.  This has been fixed.
//		Reported By:	AA5XE
//		Fixed By:		KE5DTO
//
//		Issue:			Double clicking on the Tuning Step Box hides it.
//		Fix:			This gotcha was added to allow the VHF people to view
//						frequencies up to 99GHz.  We have added a checkbox on
//						the XVTR Form to allow this box to be hidden.
//		Reported By:	WA8BXN
//		Fixed By:		KE5DTO
//
//		Issue:			Saving a new Transmit Profile causes crashing on exit.
//		Fix:			Changes to the TXEQ caused this issue back in Preview
//						12.  This has been fixed.
//		Reported By:	LA9EX
//		Fixed By:		KE5DTO
//
//		Issue:			The filter is not restored after the Level calibration.
//		Fix:			This was a result of restoring the filter followed by
//						restoring the mode (which changed the filter yet
//						again).  This has been fixed by restoring the mode
//						first and then restoring the filter.
//		Reported By:	VE3BGE
//		Fixed By:		KE5DTO
//
//		Issue:			Zero Beat is enabled by default (AVG is off).
//		Fix:			Zero Beat is now disabled initially until the AVG is
//						turned on.  This keeps the state consistent.
//		Reported By:	KC9FOL
//		Fixed By:		KE5DTO
//
//		Issue:			ALC computed in the wrong place on AM and FM.
//		Fix:			The ALC was computed on FM output (where it is not
//						needed because of constant envelope) and on the AM
//						output rather than on the audio input to the AM
//						modulator.  This levels the settings for AM with those
//						for SSB	and prevents severe overmodulation from
//						regularly occurring on AM.
//		Reported By:	WA6AHL
//		Fixed By:		N4HY
//
//	New Features:
//
//		Feature:		New CWX Form brings more CW options.
//		Description:	W5SXD, author of the original VB CW keyer, brings us
//						his latest and greatest.  His new CW form includes
//						options to send predefined buffers of text as well as
//						sending code directly from the keyboard with semi-
//						break in.  To open the form, click the CWX Menu option
//						on the front console.  Click the Notes button to see
//						the author's comments and instructions.
//		Coded By:		W5SXD
#endregion

#region v1.4.5 Beta Preview 13 Released 02/08/06
//v1.4.5 Beta Preview 13 Released 02/08/06
//
//	Bug Fixes:
//
//		Issue:			Sample processing thread duplication
//		Fix:			When we changed to the use of pthreads, we
//						created two threads that called the dsp functions.
//						The samples were processed by the winner in the
//						race to acquire the buffer semaphore.  Since only
//						one of the threads could acquire the semaphore and
//						since all processing after that was correct, there were
//						probably no consequences outside our faces turning red.
//		Reported by:	Bob Cowdery
//		Fixed by:		N4HY
//
//		Issue:			ALC is not functional.
//		Fix:			The ALC function was being calculated and then
//						overwritten, causing it to have no effect on the
//						transmit DSP chain.  This has been addressed and the
//						ALC is now performed in the right place.  This should
//						prevent FireBox owners from damaging IC2 on the TRX
//						board.  Note that because the ALC was non-functional
//						in the last series of releases (since 1.4.5 started),
//						the output power for inputs which are being overdriven
//						will be attenuated.  As a reminder, on SSB, you will
//						not get 100W on a bird wattmeter.  Something closer to
//						30 or 40W is a more reasonable expectation for average
//						power for most voices/microphones.
//		Reported By:	Many
//		Fixed By:		KE5DTO, N4HY
//
//		Issue:			Squelch is defaulted on (even when not shown enabled).
//		Fix:			There was an inconsistency between the DSP and the GUI
//						with respect to the initial state of the Squelch
//						control.  This has been addressed and it is now turned
//						off initially.
//		Reported By:	W5GI
//		Fixed By:		KE5DTO
//
//		Issue:			Peak does not reset when changing frequency.
//		Fix:			This was a design oversight and has been addressed.
//		Reported By:	K2OX
//		Fixed By:		KE5DTO
#endregion

#region v1.4.5 Beta Preview 12 Released 01/20/06
//v1.4.5 Beta Preview 12 Released 01/20/06
//
//	Bug Fixes:
//
//		Issue:			Display Average does not reset on MOX.
//		Fix:			This has been fixed.
//		Reported By:	AB7R
//		Fixed By:		KE5DTO
//
//		Issue:			Display sometimes freezes after calibrations, etc.
//		Fix:			There was a bug that caused the display thread to not
//						be restarted after some operations.  This has been
//						addressed and the display should now properly recover
//						during such operations.
//		Fixed By:		KE5DTO
//
//		Issue:			Display Average does not reset when turned off.
//		Fix:			Similar to the peak bug in a previous version, this bug
//						related to our refactoring of the display code has been
//						fixed.
//		Reported By:	K2OX
//		Fixed By:		KE5DTO
//
//		Issue:			Title bar says Preview 10.
//		Fix:			The title was labeled incorrectly.  This has been fixed
//						and now reads the appropriate preview number.
//		Reported By:	W4RAK, N4HY, F5JD, K1RQG
//		Fixed By:		KE5DTO
//
//		Issue:			VAC Ringbuffers glitch with larger buffer sizes.
//		Description:	Ring Buffer requests from console were too small to
//						handle large buffers that come from upsampling.  Tests
//						with JVComm32 show no more "line glitches" in the WEFAX
//						display	with 2048 Audio buffers and 2048 VAC buffers
//						selected at 11025 samples per second selected for VAC.
//		Reported by:	W0VB, N4HY
//		Fixed By:		N4HY
//
//		Issue:			TX MIC meter 0dB point too high.
//		Fix:			It was a misnomer to call the "MIC setting" meter ALC.
//						This meter computes the average power of the incoming
//						microphone signal.  It is not computed in the ALC code
//						but at the opposite end of the TX processing!  Also,
//						an inadvertent deletion of the number 10 from log10
//						replace this with log and the meter was computing on
//						the wrong number base (base e).  This has been fixed.
//		Reported By:	W5GI, KE5DTO, several others.
//		Fixed By:		N4HY
//
//  Modifications:
//
//		Feature:		Split mode auto disable.
//		Change:			As a protective measure, when VFO A changes bands, if
//						Split mode is active, it will be disabled in order to
//						prevent accidentally transmitting on the VFO B
//						frequency.
//		Suggested By:	W8ER
//		Coded By:		KE5DTO
//
//		Feature:		Delta 44 Auto Initialization.
//		Change:			The Delta 44 Input and Output level settings (-10dBv,
//						Consumer, or +4dBu) are now set automatically when the
//						Delta 44 is selected from the Supported Sound Card
//						list.  Note that this only applies to inputs 1 and 2 as
//						inputs 3 and 4 may be adjusted to match the microphone/
//						audio equipment used to drive the soundcard.
//		Coded By:		KE5DTO
//
//		Feature:		DRM now works with VAC Auto Enable.
//		Change:			DRM now works in addition to DIGL and DIGU for the Auto
//						Enable to work.  When Auto Enable is on and the DRM
//						mode is chosen, the VAC will be enabled.
//		Suggested By:	N4HY, others.
//		Coded By:		KE5DTO
//
//		Feature:		Equalizer is back and better than ever.
//		Change:			The equalizer has been replaced with a simpler design
//						including 3 band sliders (Low, Mid, High).  This new
//						design uses a high performance frequency domain filter
//						with minimal latency (~5ms).  Low covers 0-400Hz, Med
//						is 400-1500Hz, and high is 1500-6000Hz.  In addition, a
//						160 Hz notch filter (IIR) is enabled for TX audio only.
//						It applies more than 15 dB of suppression with 3 dB
//						points at 150 Hz and 170 Hz.
//		Suggested By:	W5GI, K5SDR
//		Coded By:		N4HY
//
//		Feature:		Fast channel AGC operation/settings optimized
//		Change:			The fast channel of the agc, intended to prevent
//						clipping was not being applied at the optimal point.
//						Furthermore, its operation was greatly improved by
//						using a few ms hang.  The sharp peaks on transmit and
//						the overshoots on received CW and high dynamic range
//						SSB are greatly improved if not	absent altogether.
//						This code will continue to be improved based on
//						constructive testing and feedback.
//		Suggested by:	DK7XL, VK6APH, WA8SRA, WA6AHL
//		Coded By:		N4HY, WA6AHL
//
//	New Features:
//
//		Feature:		Fast Block LMS
//		Description:	The Complex LMS routines for automatic noise reduction
//						and	automatic notch filtering are implemented.  The
//						routines do all computations in the frequency domain
//						over a block of data.  This helps in several ways.  It
//						is more stable.  A 256 tap filter is faster than the
//						MINIMUM size NR/ANF	filter before this.  The update is
//						done using a block of data for the filter update has
//						greater noise immunity.  And, BIN (binaural mode) will
//						work with these filters.  The ADJUSTMENTS ARE NOT
//						IMPLEMENTED.  Some of them make	no sense (such as
//						filter length).  You select the block versions by going
//						to Setup->DSP->Options and clicking the check boxes to
//						select between the old and the new block LMS routines.
//		Coded By:		N4HY

#endregion

#region v1.4.5 Beta Preview 11 Released 01/06/06
//v1.4.5 Beta Preview 11 Released 01/06/06
//
//	Bug Fixes:
//
//		Issue:			TX frequency is 11kHz low.
//		Fix:			An ordering problem with the MOX handler was introduced
//						while fixing the X2 problem in Preview 10.  This has
//						been fixed.
//		Reported By:	KM0T, K2OX
//		Fixed By:		KE5DTO
#endregion

#region v1.4.5 Beta Preview 10 Released 01/03/06
//v1.4.5 Beta Preview 10 Released 01/03/06
//
//	Bug Fixes:
//
//		Issue:			Preview 9 still requires latest DirectX.
//		Fix:			There was one leftover that was overlooked in the
//						source that was causing this dependancy.  This code has
//						been removed and the latest DirectX should no longer be
//						required for Thetis.
//		Reported By:	ZL1WN, K7RSB
//		Fixed By:		N6VS, KE5DTO
//
//		Issue:			Background does not update when in Standby mode.
//		Fix:			This problem was related to the changes made to include
//						DirectX support.  This has been fixed.
//		Fixed By:		KE5DTO
//
//		Issue:			Peak Hold feature does not reset when turned off.
//		Fix:			This problem was related to the changes made to include
//						DirectX support.  This has been fixed.
//		Reported By:	WA8SRA
//		Fixed By:		KE5DTO
//
//		Issue:			ColorButtons sometimes do not redraw correctly.
//		Fix:			There was a bug in the original source where it was
//						assumed that the ClipRectangle (area needing to be
//						redrawn) was always the whole control.  When this was
//						not the case, the control would be drawn incorrectly.
//						This has been fixed by forcing the ClipRectangle to
//						always be the whole control.
//		Fixed By:		KE5DTO
//
//  Modifications:
//
//		Feature:		EQ being rewritten.
//		Change:			While the EQ is being rewritten, this feature has been
//						disabled in the console.  Look for the EQ to return in
//						future versions.
//		Coded By:		N4HY
#endregion

#region v1.4.5 Beta Preview 9 Released 12/22/05
//v1.4.5 Beta Preview 9 Released 12/22/05
//
//	Bug Fixes:
//
//		Issue:			Filter Width slider behaves strangely in DIGx modes.
//		Fix:			Because this feature was written before the DIGx modes
//						were implemented, the behavior was undefined for these
//						modes.  These modes have now been accounted for and
//						operate much more predictably.
//		Reported By:	Many
//		Fixed By:		KD5TFD
//
//		Issue:			DirectX implementation causes exceptions/high CPU.
//		Fix:			We (FlexRadio) have made a decision to hold off on
//						DirectX development for the time being.  This is in the
//						interest of stabilizing the software with the goal of
//						getting to an official release.  We were clearly not
//						ready for the troublesome launch of this complex new
//						feature and will be removing this option until after
//						the official release.  Subsequent Beta releases will
//						likely reinstate the DirectX development. 
//		Reported By:	Many
//		Fixed By:		KE5DTO
//
//  Modifications:
//
//		Feature:		Use of FFTW 3 modified and precision changed in DSP.
//		Change:			The DSP code has been modified to largely use floats
//						instead of double precision numbers.  Where doubles
//						are still needed, they are used. This includes the
//						oscillators for phase and frequency.  As we make this
//						change we have cut the memory bandwidth requirement
//						in half for reading the buffers in and out of memory.
//						In addition, this means that those processors with
//						small cache will take many fewer cache hits.  The FFTW3
//						library uses SSE and 3DNOW where appropriate.  Please
//						run fftw_wisdom ONE MORE TIME.  You will see that it
//						plans the use of fftwf codelets rather than fftw,
//						reflecting the change from double to float.
//		Coded By:		N4HY
//
//		Feature:		Custom AGC settings enabled.
//		Change:			We have had custom settings for receive AGC on the
//						Setup Form.  We have not enabled their use clearly
//						until now.  That is now done.  On the front	panel, you
//						will notice a new AGC setting: Custom.  If you select
//						custom,  the DSP Tab AGC settings are enabled and they
//						will be applied to the RX.
//		Coded By:		KE5DTO and N4HY
#endregion

#region v1.4.5 Beta Preview 8 Released 12/16/05
//v1.4.5 Beta Preview 8 Released 12/16/05
//
//	Bug Fixes:
//
//		Issue:			AGC Attack set incorrectly.
//		Fix:			Attack setting for the slow channel on the agc was
//						being overwritten by the fast setting.  This has been
//						addressed and now it is set properly.
//		Reported By:	WA6AHF
//		Fixed By:		N4HY
//
//		Issue:			AGC gain is limited to 70dB.
//		Fix:			The unimplemented fast attack AGC channel was
//						artificially limiting the system AGC gain to 3000.
//						This limitation has been taken out and the maximum 
//						gain is now correct for values above 70dB.
//		Reported By:	MANY
//		Fixed By:		N4HY, KE5DTO
//				
//		Issue:			DIGU transmits on the lower sideband.
//		Fix:			This was an oversight and has been fixed.
//		Reported By:	KC9FOL
//		Coded By:		KE5DTO
//
//  Modifications:
//
//		Feature:		AGC fast channel.
//		Change:			The dual channel agc system is active.  We may not have
//						all of settings exactly right but the sound of this new
//						feature as well the bug fixes appear to have a given us
//						a new sound
//		Coded By:		N4HY
//
//	New Features:
//
//		Feature:		DirectX Display modes.
//		Description:	The long awaited DirectX method of driving the display
//						has been implemented.  This should result in lower CPU
//						loads due to the display.  This will be a key going
//						forward as offering larger displays in GDI+ would be
//						prohibitive due to the CPU processing required.  Note
//						that those without the necessary hardware to run
//						DirectX in hardware will use software processing.
//						Whether the DirectX mode will improve/degrade
//						performance in these cases will depend on the driver
//						and the hardware.  Until testing is complete, GDI+ will
//						continue to be the default engine.  Use the new control
//						on the Display Tab of the Setup Form to try out the new
//						DirectX mode.
//		Coded By:		KE5DTO
#endregion

#region v1.4.5 Beta Preview 7 Released 11/18/05
//v1.4.5 Beta Preview 7 Released 11/18/05
//
//	Bug Fixes:
//
//		Issue:			Importing database gives error about duplicate entries.
//		Fix:			This problem was due to the abstraction layer for
//						databases in the .NET framework called adapters.
//						Apparently using the Update function is not enough to
//						guarantee that the database has been updated.  It is
//						necessary to call the Close function on the database
//						connection to ensure that the DataSet changes are
//						accurately reflected in the database.
//		Reported By:	AB1DO
//		Coded By:		KE5DTO
//						
//	New Features:
//
//		Feature:		VAC Auto-Enable for Digital Modes.
//		Description:	When the new VAC Auto Enable option is checked, the VAC
//						will be enabled automatically in DIGL and DIGU and will
//						be disables automatically in all other modes.
//		Suggested By:	W0VB
//		Coded By:		KE5DTO
#endregion

#region v1.4.5 Beta Preview 6 Released 11/15/05
//v1.4.5 Beta Preview 6 Released 11/15/05
//
//	Bug Fixes:
//
//		Issue:			DC Block does not work.
//		Fix:			A programming error caused the DC removal in SSB
//						transmit and Digital mode transmit to be nonfunctional.
//						This has been fixed and DC removal has been confirmed.
//		Coded By:		N4HY
//
//		Issue:			Compander/Compressor seem to need lower input for good
//						quality output than plain SSB.
//		Fix:			The compressor and compander have been approximately
//						power normalized.  The quality of the normalization
//						will depend heavily on the users individual voice but
//						this is a large step in the right direction.
//		Reported By:	W5GI
//		Coded By:		N4HY
//
//		Issue:			Random relays seem to switch during operation.
//		Fix:			A race condition was found in the hardware code when
//						enabling the 2:4 decoder on the RFE board.  A bit more
//						care has been taken to make sure that things happen in
//						right order to prevent this issue.
//		Reported By:	WA6AHL
//		Coded By:		WA6AHL
//
//		Issue:			Database error on 17m.
//		Fix:			Changes to the default database caused this issue when
//						importing an older database.  The mix of old and new
//						records in the BandText table did not work well
//						together.  We have added a filter that screens the
//						BandText table on startup and when importing to make
//						sure that the table is not left in a state that could
//						cause problems.
//		Reported By:	W9DR, DL5YEJ, W4TME, W5SXD, AA5XE, AB1DO, WA2N
//		Coded By:		KE5DTO
//
//		Issue:			Softrock40 endpoints show strange signals.
//		Fix:			The alias free end points were being calculated using
//						the wrong variable.  This has been corrected.
//		Reported By:	K9JRI
//		Coded By:		KD5TFD
//
//		Issue:			RX audio output is low (since around Beta 1.3.11).
//		Fix:			The AGC attempts to achieve power 1.0 in the complex
//						signal as it passes its way through the chain.  For all
//						of the outputs except BIN, we then throw away the
//						imaginary part of the signal, and half the power as a
//						result.  This has been remedied by a single scaling by
//						sqrt(2) in the scaling done at the end of the receive
//						chain.
//		Reported By:	N9DG, KE5DTO
//		Coded By:		N4HY
//
//	New Features:
//
//		Feature:		Digital Modes (DIGU and DIGL) Added.
//		Description:	DigU and DigL modes have replaced the non-functional
//						RTTY and PSK mode buttons.  They have been added to
//						better optimize the signal processing path for digital
//						modes (non voice).  Both modes remove all processing
//						(Leveling, ALC, etc) in the transmitter.  It is then
//						completely linear.  For DigL, the Var1 filter defaults
//						to 2.6kHz LSB and Var2 filter defaults to a filter
//						optimized for RTTY.  A tuning aid is still needed to
//						speed RTTY tuning and this is in the to-do list.  DigU,
//						intended primarily for SSTV users, defaults Var1 and
//						Var2 to the 2.6kHz USB filter.
//		Suggested By:	AB7R
//		Coded By:		N4HY
//
//		Feature:		RTTY and SSTV click tuning aids.
//		Description:	To go along with the new digital modes above, we have
//						added two options to allow for easier tuning of 2 of
//						the more popular digital modes.  If you enable RTTY
//						Click Tuning, when you click tune on a signal in the
//						display, it will show up offset to the left by 2040Hz.
//						Similarly, if you enable SSTV Click Tuning, signals
//						will be offset 1200Hz to the right when clicked.  
//		Suggested By:	AB7R
//		Coded By:		KE5DTO
#endregion

#region v1.4.5 Beta Preview 5 Released 11/11/05
//v1.4.5 Beta Preview 5 Released 11/11/05
//
//	Bug Fixes:
//
//		Issue:			Error in band edge display for Softrock40.
//		Fix:			The variable center frequency for the Softrock40 was
//						not being used in a subroutine.  This has been fixed
//						and the display now works as intended.
//		Reported By:	KB9YIG
//		Coded By:		KD5TFD
//
//		Issue:			RX Audio is muted after transmitting.
//		Fix:			The AF changed flag was being used incorrectly and was
//						not firing the AF_ValueChanged function as a
//						consequence.  This has been fixed.
//		Reported By:	F5JD, W8IKN, WK0J, DL5YEJ
//		Coded By:		KE5DTO
//
//		Issue:			Spectrum display shows post-filtered data.
//		Fix:			When fixing several issues related to changing display
//						modes, the Spectrum display mode was set to display
//						pre-filtered data.  This problem caused the automatic
//						receive image reject routine to fail.  This has been
//						corrected and the spectrum once again shows the
//						post-filtered data as it has in previous versions.
//		Reported By:	AI4IN, AA8K, WK0J, AB1DO
//		Coded By:		N4HY
//
//	Modifications:
//
//		Feature:		VAC RX and TX Gain controls.
//		Change:			The VAC is no longer affected by the AF and MIC
//						controls as it was in the past.  Now the gain is set
//						using the controls on the SetupForm->Audio->VAC Tab.
//						Use the RX gain to control the audio level going to
//						the VAC for third party application use.  Use the TX
//						gain to set	the input level of the audio coming FROM
//						the VAC for transmit.  Typically, you would do this
//						while in transmit mode while watching the ALC meter
//						and aim for 0dB on peaks.  This should allow you to
//						continue to use the MIC control for your microphone,
//						the AF control for audio volume (without affecting
//						the gain going to the VAC) and most importantly, the
//						PWR control to accurately set the output power even
//						when using the VAC.
//		Suggested By:	W0VB
//		Coded By:		KE5DTO
//
//	New Feature:
//
//		Feature:		WDM-KS Audio support.
//		Description:	With this release, we now have native support for WDM
//						audio drivers through Portaudio.  WDM-KS will show up
//						as an option below ASIO in the driver selection on the
//						Audio Tab.  We are interested in feedback on how this
//						works as the support in Portaudio for WDM-KS is
//						considered Beta.  In order to select the WDM-KS drivers
//						it will be necessary to select Unsupported Card.  If
//						you are using a FireBox or Delta 44, make sure that you
//						set the Channels control to 4.  We expect this to
//						eventually eliminate the need for ASIO4ALL for a much
//						lower latency solution for those still having to use
//						it.  The Delta 44 and FireBox will not likely see
//						tangible improvements in latency when using WDM-KS as
//						the ASIO latency is	already very low.
//		Coded By:		KE5DTO
#endregion

#region v1.4.5 Beta Preview 4 Released 11/04/05
//v1.4.5 Beta Preview 4 Released 11/04/05
//
//	Bug Fixes:
//
//		Issue:			DRM is broken.
//		Fix:			In the rewrite of the DDSFreq and hardware code, the
//						special case of DRM (and anticipated high speed digital
//						modes) was not handled correctly in DDSFreq.  The
//						DSP oscillator was not told where to tune. So there
//						was no superhet action occuring as needed.
//		Reported By:	N4HY
//		Coded By:		N4HY
//
//		Issue:			Switching from SPEC Mode or changing display mode.
//						caused display failure
//		Fix:			If you went into SPEC mode or if you did "Phase" display
//						and then switched either of them, the display was
//						computed incorrectly.  This was caused by leaving one
//						line out of the setup function in the dsp.
//		Reported By:	KE5DTO, K0PFX
//		Coded By:		N4HY
//
//		Issue:			Monitor is always on for 4-port cards (D44, FireBox).
//		Fix:			In fixing the AF issues in the previous preview, we
//						broke the monitor function for these cards.  This has
//						been fixed.
//		Reported By:	W4DWG, W5GI, WA8SRA, AH6JR, KM0T
//		Coded By:		KE5DTO
//
//		Issue:			Leveler controls are disabled.
//		Fix:			This was an oversight and has been fixed.
//		Reported By:	W5GI
//		Coded By:		KE5DTO
//
//		Issue:			Icon names read preview 2.
//		Fix:			This was an oversight and has been fixed.
//		Reported By:	G0ORX
//		Coded By:		KE5DTO
//
//		Issue:			Semi break-in control does not function properly.
//		Fix:			When you do not have semi breakin selected,  and you
//						are using an external keyer or program that	asserts
//						PTT, you do not want to have a semi breakin timer
//						keeping the MOX asserted after the keyer/program has
//						released.  The hardware limitations prevent this from
//						being less than about 40ms.  So that is the minimum
//						setting for the turn around.  Note that if you are
//						using SDR for the primary and None on the secondary,
//						the semi break-in works as it has in the past (if it is
//						unchecked, it does not key at all).  This is a 
//						temporary fix and this will likely change for the
//						better in the near future.
//		Reported By:	WA8SRA
//		Coded By:		N4HY
//
//		Issue:			First XVTR PWR field defaults to 50 on startup.
//		Fix:			This was an initialization issue that has been fixed.
//		Reported By:	K3IB
//		Coded By:		KE5DTO
#endregion

#region v1.4.5 Beta Preview 3 Released 11/01/05
//v1.4.5 Beta Preview 3 Released 11/01/05
//
//	Bug Fixes:
//
//		Issue:			Buzzsaw sound happens when changing controls.
//		Fix:			A problem was found within the callback function that
//						was causing this issue.  This has been fixed.
//		Reported By:	N4HY, K5KDN, AH6JR
//		Coded By:		KE5DTO
//
//		Issue:			Misc glitches and audio artifacts.
//		Fix:			The way that we were doing VOX within the callback was
//						causing problems in some circumstances.  We have moved
//						the VOX guts out of the callback and into the PollPTT
//						function and testing has shown much smoother results.
//		Reported By:	N4HY, AH6JR
//		Coded By:		KE5DTO
//
//		Issue:			Database error on import database with message about
//						duplicate entries.
//		Fix:			This turned out to be a case of poor naming.  In the
//						database adapter, the names "ESSB" and "ESSB+" resolve
//						to the same string.  Since this field (Name) is the
//						primary key for this particular table, this caused the
//						duplicate key error.  We have renamed the "ESSB+"
//						profile to "ESSB Plus" to correct this issue.
//						Importing older databases with the "ESSB+" name is
//						now taken care of as well.
//		Reported By:	N4HY
//		Coded By:		KE5DTO
//
//		Issue:			MIC control does not function when transmitting with
//						the new VAC feature enabled.
//		Fix:			The MIC scaling was being done to the incoming samples
//						(from the microphone) before being replaced by the
//						input from the VAC.  This has been addressed and now
//						works as intended.
//		Reported By:	W0VB
//		Coded By:		N4HY
//
//		Issue:			Third party CW keying will not work if semi-break in is
//						not enabled.
//		Fix:			With the new options for keying the radio via third
//						party options, there was an accidental clause that
//						semi-break in had to be enabled for it to work.  This
//						stipulation was unintended and has been removed.
//		Reported By:	KD5TFD
//		Coded By:		N4HY
//
//		Issue:			EQ does not get restored correctly on startup.
//		Fix:			While the sliders were being set, the values were not
//						being passed to the DSP.  This has been resolved.
//		Reported By:	W9DR
//		Coded By:		KE5DTO
//		References:		Email 10/31/05 Subject: [Flexradio] E/Q Bug?
//
//		Issue:			Error when importing database about selectedindex.
//		Fix:			Users with 3 board stacks (no RFE) noticed that we had
//						some code that was incompatible with that
//						configuration.  This caused this error and could have
//						caused various other problems with the Preamp control.
//						This has been addressed and all configurations should
//						now be able to import and use the preamp as intended.
//		Reported By:	DK7XL, K3IB
//		Coded By:		KE5DTO
//
//		Issue:			Recording wave file gives a fatal exception.
//		Fix:			Careless use of Replace(":", " ") caused an issue with
//						using the full file name.  This logic has been
//						corrected and recording now works again.
//		Reported By:	K2OX
//		Coded By:		K2OX
//		References:		Email 11/01/05 Subject: [Flexradio] preview 2 & wave tx
//
//		Issue:			Cards other than Delta 44/FireBox only transmit one
//						time then will not transmit anymore.
//		Fix:			Due to some rearranging associated with the new FireBox
//						card support, we introduced a bug in the monitor volume
//						control code.  This has been addressed and transmit
//						should work each time now.
//		Reported By:	DK7XL, W6THW, AB1DO, K7MDL
//		Coded By:		KE5DTO
//
//		Issue:			Having CAT and CAT PTT enabled caused an error message
//						saying the two could not be set to the same port.
//		Fix:			Some initialization code associated with the new
//						comboboxes for the CAT and CAT PTT ports caused this
//						issue.  The inialization code has been fixed and the
//						error message no longer appears on startup.
//		Reported By:	AB7R
//		Coded By:		KE5DTO
//
//		Issue:			Switching to FM while in a configured XVTR band causes
//						the PWR to drop more than it should.
//		Fix:			There was a bug in the XVTR code that was causing the
//						PWR to be cut by 4 twice instead of just once (as it
//						should).  This has been corrected.
//		Reported By:	KM0T
//		Coded By:		KE5DTO
//		References:		Email 10/30/05 Subject: [Flexradio] Preview 2
//							Observations
#endregion

#region v1.4.5 Beta Preview 2 Released 10/28/05
//v1.4.5 Beta Preview 2 Released 10/28/05
//
//	Bug Fixes:
//
//		Issue:			Leveler does not work as intended.
//		Fix:			The Leveler is a piece of code intended to level out
//						audio presented from the microphone to the sound system
//						and the DSP on transmit.  The Leveler had two major
//						problems.  The first was ALC settings accidentally
//						being used in the leveler.  The second was due to
//						a unique condition in the leveler.  It has minimum
//						gain 1.0 or 0 dB.  It is intended to raise the audio
//						level in the case the distance or angle from your
//						mouth to the microphone changes.  Its default setting
//						is 10 dB max gain and should be increased only with
//						care as it will increase the background noise.
//						HINT: Use the noise gate.
//		Coded By:		N4HY
//
//		Issue:			Compander has distortion and can act as a limiter.
//		Fix:			Compander was accidentally initialized as an expander,
//						not a compressor.  The default setting is now correct
//						and the default performance is much better
//		Reported By:	N4HY
//		Coded By:		N4HY
//
//		Issue:			Transmit ALC setting wrong.
//		Fix:			The Transmitter ALC had its minimum gain, which set the
//						negative gain it would apply, being set by the Leveler
//						setting.  The ALC minimum gain setting had no effect.
//		Reported By:	Many
//		Coded By:		N4HY
//
//		Issue:			Integrated ATU performance is spotty at best.
//		Fix:			There was a problem in the hardware code that was
//						causing the ATU to get bypassed immediately following
//						a good tune sequence.  This has been corrected and the
//						ATU is now working properly for the first time since
//						we began installing them.  This hardware change may
//						also fix several other issues.  We are looking into
//						this and will confirm in future versions once testing
//						has been completed.
//		Reported By:	K5SDR
//		Coded By:		KE5DTO
//
//		Issue:			Filter disappears on panadapter when using very narrow
//						filters.
//		Fix:			The panadapter graphics now limit the filter width to a
//						minimum of one pixel.
//		Reported By:	W5SXD
//		Coded By:		KE5DTO
//		References:		Email 10/20/05: Subject: [Flexradio] Some suggestions
//						for cosmetic improvements
//
//		Issue:			Saving a TX Profile with the same name does not work.
//		Fix:			The save function has been changed to properly handle
//						when the new TX Profile name matches one that is
//						already in the list.  It now prompts to make sure that
//						you want to overwrite it.
//		Reported By:	AB1DO
//		Coded By:		KE5DTO
//		References:		Email 10/20/05: Subject: [Flexradio] Some suggestions
//						for cosmetic improvements
//
//		Issue:			Audio voltage cal does not work with Delta 44.
//		Fix:			The function for calculating the max VRMS for the
//						soundcard has been modified to work with the Delta 44.
//		Reported By:	K2OX
//		Coded By:		KE5DTO
//		References:		Email 10/11/05: Subject: [Flexradio] TX Voltage Cal and
//						Image Null
//
//		Issue:			Non Delta 44 card does not recover after MOX.
//		Fix:			A problem with the new TX RF setting was causing this
//						issue and it is now fixed.
//		Reported By:	K5BOT
//		Coded By:		KE5DTO
//
//		Issue:			VOX and footswitch do not work well together.
//		Fix:			If using VOX with a footswitch (or any other automatic
//						transmit mode), the console would switch rapidly from
//						receive to transmit and back.  We have corrected this
//						and now VOX cooperates with the other PTT modes
//						properly.
//		Reported By:	AH6JR
//		Coded By:		KE5DTO
//
//		Issue:			Enabling CAT on a non-existant COM port causes the
//						console to crash.
//		Fix:			This was due to poor exception handling.  The driver
//						was throwing the correct exception, but the level above
//						was not handling things correctly.  A fix is in place
//						to keep the console from crashing in this situation.
//		Coded By:		KE5DTO
//
//		Issue:			Image Reject Algorithm sometimes fails.
//		Fix:			There were cases where the current filter setting was
//						too narrow to catch the image signal when running the
//						Image Reject Calibration.  The filter is now set to
//						6kHz automatically during the calibration and then
//						returned to the previous setting when finished.
//		Coded By:		KE5DTO
//
//		Issue:			Importing Database causes band errors on 80 and 160m.
//		Fix:			Due to the way the import algorithm tries to
//						incorporate any user changes to the BandText table,
//						some default entries from previous versions were
//						conflicting with the newer entries.  This has been
//						addressed and importing should no longer be a problem.
//		Reported by:	F5JD, KC9FOL, DK7XL, IK3VIG
//		Coded By:		KE5DTO
//
//		Issue:			Display Peak will not work if AVG is on.
//		Fix:			Rather than making these options mutually exclusive
//						using if/else, both of these options can now be enabled
//						concurrently.  The AVG will take place first and then
//						the peak will be taken.
//		Reported by:	VK6APH
//		Coded By:		KE5DTO
//
//		Issue:			XVTR LO Error cannot be negative.
//		Fix:			The error value needs to be +/-.  This was an oversight
//						that has now been fixed.
//		Reported by:	VK6APH
//		Coded By:		KE5DTO
//
//		Issue:			XVTR PWR does not update when changing PWR on front
//						panel.
//		Fix:			This was an oversight that has now been addressed.
//		Reported by:	K3IB
//		Coded By:		KE5DTO
//
//	Modifications:
//
//		Feature:		Background threads for database functions.
//		Change:			We now create and use low priority threads when doing
//						database operations like clicking the Apply button on
//						the Setup Form.  This prevents things like popping in
//						the audio and "hanging" in the GUI.
//		Suggested By:	N4HY
//		Coded By:		KE5DTO
//
//		Feature:		Panadapter is restored after SPEC mode.
//		Change:			If you are using Panadapter mode when switching to SPEC
//						DSP mode, you are automatically switched to Spectrum
//						display mode.  The console now remembers if you were in
//						Panadapter when you switched into SPEC mode and will
//						restore the Panadapter as long as the Display Mode is
//						not touched while in the SPEC mode.
//		Suggested By:	AB1DO
//		Coded By:		KE5DTO
//		References:		Email 10/20/05 Subject: [Flexradio] Some suggestions
//						for cosmetic improvements
//
//		Feature:		Clock Display / Updates.
//		Change:			The clock is now updated 10 times/sec in order to
//						better align the clock with the system time.  We have
//						also set the display so that you can set it to Local,
//						UTC, or Off by clicking on the date or time textboxes.
//		Coded By:		W5SXD, KE5DTO
//
//	New Features:
//
//		Feature:		1/3 and 2/3 Octave Graphic Equalizer.
//		Description:	A new facility is enabled to do a graphic (slide bar)
//						equalizer on both transmit and receive.  Both are ISO
//						standard filters.  They are NOT constant or perfect Q
//						filters as in RANE equalizers.  As such,  we have
//						limited the range of control to the range that makes
//						technical sense.  If you lower the gain applied to a
//						filter below -12dB, the notch gets WORSE.  This is caused
//						by it being in a system and the system Q getting worse
//						at that frequency.  As we implement dynamic design and
//						more efficient code, these ranges could change.
//		Coded By:		N4HY, KE5DTO
//
//      Feature:		CW Keying by external programs/keyers.
//		Description:	With this release we have added support for external
//						programs' keyers to key the radio.  On the DSP/Keyer
//						tab on the Setup Form, you will find some rearrangement
//						of the control groups.  The Primary control is expected
//						to be used to plug in a set of paddles or a hardware
//						keyer.  Secondary is to be used for all external keying
//						sources such as a third party program.  You may select
//						PTT and CW signals as indicated	on the form.  If you
//						have CAT enabled, then programs such as MixW can key
//						the DTR/RTS lines on the COM port the CAT interface is
//						currently using.  This is very nice as it allows MixW
//						to read the frequency and mode as well as key the radio
//						in CW modes through the same serial port.  This shows
//						up as CAT in the secondary keying port selection.  Note
//						that using the primary connection will override any
//						keying being performed by the seondary connection.
//		Coded By:		N4HY, KE5DTO
//
//		Feature:		Virtual Audio Card Support.
//		Description:	With this release comes the much anticipated virtual
//						audio card (VAC) support.  With this added function, we
//						have eliminated the existing Card 2 controls and
//						functionality.  This enables third party software to
//						easily integrate with our software without having to
//						use a second sound card.  Since there is no	longer the
//						extra D/A and A/D connections going between the
//						applications, the signal to noise radio should be
//						improved.  For more details about where to buy the VAC
//						and how to set it up, please see the reference link
//						below.  Note that this has only been tested on the
//						Delta 44.  The functionality has been enabled for other
//						cards, but has not been tested adaquately.  Use with
//						any card other than the Delta 44 is at your own risk.
//		Coded By:		N4HY, KE5DTO
//		References:		http://www.flex-radio.com/download_files/PowerSDR/Docs/PowerSDR_VAC.pdf
//
//		Feature:		Integrated support for Soft Rock 40.
//		Description:	The Soft Rock 40 is now supported in the main console.
//						In order to make room for the new controls, the General
//						Tab was split into 3 sub tabs.  Select the model radio
//						you are using (SDR-1000, Soft Rock 40, or Demo) on the
//						Hardware Config Sub Tab.  Set the center frequency of
//						the Soft Rock after selecting it as the model.
//						Special thanks goes to Bill KD5TFD and Tony KB9YIG for
//						their work with the Soft Rock 40.
//		Coded By:		KD5TFD, adapted by KE5DTO
//
//		Feature:		Wave Options added to select pre/post processing.
//		Description:	The Wave form now has a menu option to allow
//						configuration of whether to record the pre or post
//						processed signal on receive and transmit independantly.
//						This will allow a user to record someone they are
//						listening to and play it back for them over the air
//						much more easily than was previously possible.
//		Coded By:		KE5DTO
#endregion

#region v1.4.5 Beta Preview 1 Released 09/21/05
//v1.4.5 Beta Preview 1 Released 09/21/05
//
//	Bug Fixes:
//
//		Issue:			Inconsistant hardware state after power off/on.
//		Fix:			A flaw in the design of the new hardware code was
//						causing some of the hardware not to be initialized
//						properly when powering on.  This oversight has been
//						addressed and now each and every hardware register
//						is updated when doing a power off/on sequence.
//		Reported by:	K5SDR, N5UJ
//		Coded By:		KE5DTO
//
//		Issue:			FM RX Frequency Deviation.
//		Fix:			In the recent QST review, it was pointed out that our
//						IMD	on FM receive was not up to par.  In checking out
//						the	code, we found that the frequency deviation was set
//						to 10kHz instead of a more typical 5kHz.  This has been
//						addressed.
//		Reported by:	KC1SX
//		Coded By:		N4HY
//
//		Issue:			Holding the up arrow on the TX Filter High cut control
//						can cause an exception.
//		Fix:			This problems arises from calling a MessageBox up from
//						within a control handler when it is rapidly being
//						changed.  We fixed this by launching the MessageBox in
//						it's own thread in order not to interact with the
//						controls natural behavior.  Note that this has been 
//						fixed in several other places where similar issues
//						might have occurred.
//		Reported by:	W5GI
//		Coded By:		KE5DTO
//
//		Issue:			Mouse wheel tuning interacts with Filter Width slider.
//		Fix:			This problem can be attributed to default mouse wheel
//						event handlers built into the .NET framework for some
//						controls.  We have run into the same problem with
//						UpDown controls in the past and are now using the same
//						workaround to prevent this issue (remove focus after
//						the value is changed).
//		Reported by:	SM6OMH
//		Coded By:		KE5DTO
//
//		Issue:			X2-6 checkboxes do not work.
//		Fix:			The new checkboxes in 1.4.4 were missing event
//						handlers.  This prevent the last checkbox from taking
//						effect unless another box on the same line changed
//						state afterwards.  This has been fixed.
//		Reported by:	IK3VIG
//		Coded By:		KE5DTO
//
//		Issue:			Importing database adds bandstack registers.
//		Fix:			A logical error was in the database code that caused
//						the import function to add new rows rather than
//						overwrite the existing rows / registers.
//		Reported by:	N1RL
//		Coded By:		KE5DTO
//		
//	Modifications:
//
//		Feature:		AGC modified
//		Change:			The first baby steps towards implementing the AGC
//						recommendations from the Harman AGC paper are taken.
//						This process is about 1/4 complete.  This is beta code
//						and  should be viewed as such.  Attack is the attack
//						time constant.  It controls how quickly the system
//						responds to a large signal.  Decay is the decay time
//						constant.  It controls how quickly the system responds
//						to the need for increased gain.  Hang is the time the
//						system will hang at "low gain" before decaying (as in
//						ALC voltage decays) to a higher gain.  All  other 
//						controls are unimplemented.  The Leveler has a bug that
//						is not understood and as such is disabled. For now, the
//						custom controls on the DSP form for RX AGC are
//						problematic.  Until we find a mechanism for including
//						custom settings easily on the front panel AGC, please
//						use the presets. 
//		Coded By:		N4HY
//
//		Feature:		TX DSP Chain modified.
//		Change:			In order to simplify the transmit chain, the two AGC
//						components (leveler and limiter) have been modified.
//						The leveler will now only increase the gain while the
//						limiter will only decrease the gain (only if necessary
//						in both cases).
//		Coded By:		N4HY
//
//		Feature:		XIT/RIT Increment value.
//		Change:			When the bandwidth of the filter is 250Hz or larger,
//						the increment value is set to 10Hz from 50Hz.  This 
//						means that you hit the up/down arrow, it will
//						add/subtract 10 to the current value.  The previous 
//						value of 50Hz was too coarse for fine tuning on SSB.
//
//		Feature:		Escape from VFO uses last frequency.
//		Change:			While entering a new frequency, if you hit the Escape
//						button, the last frequency used will be put in the
//						VFO.  This is a good way to recover if you (or your
//						favorite pet) accidentally hits a key that changes the
//						VFO.
//		Coded By:		KE5DTO
//
//		Feature:		Serial Keying of the CW enhanced.
//		Change:			Several new controls are now added to enhance the 
//						keying control for CW.  You may now select independent
//						keying sources for the paddle and "other sources".  The
//						other sources CANNOT use LPT keying.  That is reserved
//						for the paddle.  The other sources are restricted to
//						the Com ports only.  This is intended to allow external
//						program keying.  It is absolutely restricted "straight
//						keying" by a keyer or a program.  RTS or DTR may be
//						selected for CW keying and RTS or DTR may be selected
//						for PTT.  Some keyers and programs will PTT before they
//						send the first note.  For example, in MixW, in
//						configuration, TRCVR PTT/CAT, select details by the
//						port and configure the com port number, and make PTT
//						and CW Key selections.  Match this in the PowerSDR
//						setup panel for the new keyer.
//		Coded By:		N4HY
//
//		Feature:		PA Calibration timing changed.
//		Change:			The timing for the PA Calibration has been changed as
//						some Audigy 2 ZS users were having trouble calibrating
//						with the faster timing.  Now the timing is determined 
//						by the sound card selection.  The Audigy 2 ZS uses 3s
//						on and 4s off.  The Delta 44 uses 2s on and 2s off.
//						All others use 2.5s on and 2.5s off.
//		Coded By:		KE5DTO
//
//		Feature:		Improved USB Adapter error handling.
//		Change:			We have modified the PowerSDR software to take
//						advantage of the error handling present in the USB
//						adapter driver.  This should prevent the driver from
//						getting "stuck", requiring a system reboot or the
//						adapter to be disconnected in rare circumstances.
//		Coded By:		KE5DTO
//
//		Feature:		XVTR TR Mode now defaults to Positive.
//		Change:			The TR mode on XVTRs going forward are being wired for
//						Postive TR polarity.  This is now the default when
//						installing without importing a database.
//		Coded By:		KE5DTO
//
//		Freature:		BandText Table Updated.
//		Change:			The Band Text has been updated to include the higher
//						bands through 47GHz.
//		Coded By:		KE5DTO
//		References:		http://www.arrl.org/FandES/field/regulations/bandplan.html
//
//	New Features:
//
//		Feature:		TX Profiles.
//		Description:	Controls were added to enable quick switching of all of
//						the TX options including TX low/high filter cuts,
//						compression on/off and level, compander on/off and
//						level, TXEQ on/off and settings, and the front panel
//						MIC	setting.
//		Suggested By:	W5GI
//		Coded By:		KE5DTO
//
//		Feature:		VOX Operation (with caveat).
//		Description:	We have implemented a simple VOX system that allows you
//						to set a threshold above which the PowerSDR software 
//						will go into TX mode for the hang time specified.  Note
//						that this will only work on the Delta 44.  
//						***CAVEAT*** Due to current hardware limitations with
//						the TR turnaround time, the first syllable will nearly
//						always be clipped by this VOX operation.
//		Coded By:		KE5DTO
//
//		Feature:		Display Peak Hold.
//		Description:	Since the PowerSDR software can be used as a high 
//						resolution spectrum analyzer, we have added the ability
//						to hold the peak signal.  This allows the radio to be
//						used to analyze a swept tone (handy for evaluating
//						linearity).
//		Suggested By:	W5GI
//		Coded By:		KE5DTO
//
//		Feature:		TX Monitor Volume (TX AF).
//		Description:	When using the Delta 44, the AF control adjusts both
//						the RX audio volume and the TX monitor volume.  While
//						this is a nice feature, many times a single setting
//						does not match the RX and TX volume very well.  This
//						new control (on the Transmit Tab of the Setup Form)
//						will set the AF to the specified value in the
//						transition to TX.  Also, any changes to the AF control
//						while in TX will be reflected in this new TX AF
//						control.  This should especially help on CW modes where
//						the created tones are full scale on the TX monitor.
//		Coded By:		KE5DTO
//
//		Feature:		New ISO standard EQ for RX and TX.
//		Description:	The new Equalizer can be accessed using the menu option
//						on the front panel (next to Wave).  There are options 
//						to use a 15 or 31 band ISO standard EQ to modify the RX
//						and TX audio independantly with +/- 30dB of gain.
//		Coded By:		N4HY (implementation), KE5DTO (interface)
//
//		Feature:		UCB / XVTR Support.
//		Description:	We have added complete support for the UCB (Universal
//						Controller Board) as well as a configuration screen for
//						setting up transverters with or without the UCB board.
//						This includes custom band stack registers for each
//						XVTR and direct frequency readout up to 99.9GHz.
#endregion

#region v1.4.4 Released 08/01/05
//v1.4.4 Released 08/01/05
//
//	Bug Fixes:
//
//		Issue:			X2 TR Sequencing broken.
//		Fix:			The sequencing was broken with the release of the new
//						hardware code back in Beta v1.3.3.  The X2 pin 7 was
//						not switching before the delay.  This has been
//						addressed and is now working as intended.
//		Reported by:	W0VB
//		Coded By:		KE5DTO
//
//		Issue:			Thread stall prevents Standby from completing.
//		Fix:			A technique used to insure that all threads that
//						request data from the dsp are finished before the
//						audio shuts down contained a fault.  This prevented
//						calibration from working on some computers and caused
//						others to hang.
//		Reported by:	K5SDR
//		Coded By:		KE5DTO
//		
//	Modifications:
//
//		Feature:		Ext Ctrl X2 pin 6.
//		Change:			By popular demand, the controls for the Ext Ctrl Tab
//						have been extended to include X2-6.
//		Requested By:	IK3VIG
//		Coded By:		KE5DTO
//
//		Feature:		ANF enabled in AM/SAM.
//		Change:			ANF now works in SAM and AM.  This helps remove those
//						nearby carriers which can be very irritating.
//		Requested By:	N4HY
//		Coded By:		N4HY
//
//		Feature:		Serial/Com Port keying
//		Change:			On Setup/DSP/Keyer/Options panel,  you may now select
//						other ports for driving the new keyer.  This will work
//						for both a hardware COM port or using one of the N8VB
//						virtual com ports and driving it with an external
//						program.  When using an external program such as MixW,
//						you want to set the CW keying to be DTR keying and RTS
//						always off.  If using a hardware COM port, use the
//						following table to connect paddles:
//						
//							Dot:	Pin 6 (DSR)
//							Dash:	Pin 8 (CTS)
//							Common: Pin 4 (DTR)
//
//		Requested by:	Many				
//		Coded By:		N8VB, N4HY
#endregion

#region v1.4.3 Released 07/19/05
//v1.4.3 Released 07/19/05
//
//	Bug Fixes:
//
//		Issue:			No audio sometimes on startup.
//		Fix:			There was a problem with the new buffer filling
//						mechanism in 1.4.2 that caused the audio in some
//						settings to be persistently reset and filled with
//						silence.  This has been addressed and startup in all
//						situations should work as intended.						
//		Reported by:	W8IKN
//		Fix Coded By:	N4HY
//
//		Issue:			PTT does not work when in DSB.
//						This was an oversight.  DSB is now listed as one of the
//						modes that uses the microphone PTT signal.
//		Reported by:	DK7XL
//		Fix Coded By:	KE5DTO
//		References:		Email 07/18/05 Subject: [Flexradio] 1.4.1 remarks
#endregion

#region v1.4.2 Released 07/18/05
//v1.4.2 Released 07/18/05
//
//	Bug Fixes:
//
//		Issue:			Transmitter splatters when overdriven, etc.
//						The transmit audio DSP chain needed a thorough grooming
//						and this is exactly what it got this weekend.  We found
//						that we were not prescaling SSB signals correctly 
//						during the last	release.  There are now two gain
//						control mechanisms in the transmit chain.  The gain
//						control at the beginning of the transmit audio chain is
//						a leveler and overdrive prevention.  It can apply as
//						much as 10dB of gain or impose -20 dB of gain 
//						(compression) if you are overdriving.  This signal is
//						now sent to	the ALC meter which replaces the Mic Meter
//						option.  At the end of the processing chain, is the
//						overdrive prevention ALC.  This applies no positive
//						gain but will impose negative gain if you go over 110%
//						of the selected power level.  It does this with a
//						linear scaling and will not cause splatter as the
//						clipper did.  The normalized TX_EQ turned out to be a
//						mistake and it is gone.  If you apply 20dB of gain
//						across the board, the ALC will roll you back.  The
//						TX_EQ rewrite WILL wait until the new audio tap
//						facility is present.  We apologize for making you go
//						through all of your settings again, but	this was
//						really needed.
//		Reported By:	W5GI, K2WS, VK6APH, K5SDR, W9AD, KE5DTO, N4HY
//		Coded By:		N4HY,KE5DTO
//
//		Issue:			Buffer reset sizes wrong.
//						The ring buffer reset mechanism and the ring buffer
//						code has stabilized much in the last release, but it
//						was realized that we were not resetting the ring
//						buffers correctly.  The wrong number of 0 samples were
//						being pre-loaded when reset occurred.  This is now
//						handled by the audio system buffer size changes
//						informing us what the resize should be.
//		Reported By:	K5SDR
//		Coded By:		N4HY
//
//	Modifications:
//
//		Feature:		TX Mic setting gone, TX ALC back.
//						The TX Meter Mic setting did not work well in practice
//						for setting the Mic gain.  An ALC tap, from the correct
//						plance in the audio chain is now provided and this is
//						read by the TX Meter when in the ALC setting.  This is
//						independent of the fixed control limiter at	the end of
//						the dsp chain (described above) which is strictly to
//						prevent overdrive.
//		Coded By:		N4HY, KE5DTO
#endregion		

#region v1.4.1 Released 07/15/05
//v1.4.1 Released 07/15/05
//
//	Bug Fixes:
//
//		Issue:			Input to TX Filters not scaled properly.
//		Fix:			In sideband modes, we take the microphone, a real input
//						signal, and we filter off either the positive 
//						frequencies to make LSB or filter the negative
//						frequencies to make USB.  This discards half of	the
//						input power.  So we have been overdriving the
//						compressor,	compander, and the limiter before to get
//						full power out of the amplifier.  This has been 
//						corrected by simply doubling the SSB output power from
//						the DSP.  For AM, FM and all double sided modulations
//						the scaling was	correct.
//		Reported By:	KE5DTO
//		Fix Coded By:	N4HY
//
//		Issue:			The monitor gets left on sometimes in CW.
//		Fix:			The monitor comes on automatically in CW modes unless
//						the Disable Monitor function is turned on.  The monitor
//						is supposed to get turned off if it gets automatically
//						turned on.  This was not happening in some modes.  This
//						has been fixed.
//		Reported By:	W0VB
//		Fix Coded By:	KE5DTO
//		References:		AIM conversation 07/14/05
//
//		Issue:			Resizing the DSP while in MOX locks up the console.
//		Fix:			There was an ordering problem with the new CW keyer
//						that was causing a deadlock.  This has been fixed by
//						placing the events in the right order.
//		Coded By:		N4HY
//
//		Issue:			Switching to MOX causes exception for some.
//		Fix:			A value of "0.0 %" was being set in the multimeter.  
//						This was causing issues with localizations where the
//						decimal delimiter is a ','.  This has been fixed.
//		Reported By:	F5JD
//		Fix Coded By:	KE5DTO
//		References:		Email 07/12/05 Subject: Re: 1.4.0
//
//		Issue:			X2 TR Sequencing cannot be turned off once turned on.
//		Fix:			The property was being set to true whether the checkbox
//						was checked or not.  This has been fixed.
//		Reported By:	KC9FOL  
//		Fix Coded By:	KE5DTO
//		References:		Email 07/08/05 Subject: [Flexradio] 1.3.14 X2 Delay
//
//		Issue:			Display glitches when using MOX.
//		Fix:			The DrawBackground routine was writing the background
//						image to the Image property of the display control.  
//						This poor use of the display control combined with
//						times where we did not draw anything in the paint
//						handler caused the glitching effect.  This would also
//						show up as a different background in CW mode.
//		Reported By:	DK7XL  
//		Fix Coded By:	KE5DTO
//
//		Issue:			Semi Breakin control not working.
//		Fix:			The semi breakin control on the setup panel	was
//						not setting the correct variable nad was therefore
//						useless.  This has been fixed and the control now works
//						as intended.
//		Reported By:	K6GGO
//		Fix Coded By:	N4HY
//
//		Issue:			CW Keyer can sometimes produce whitenoise.
//		Fix:			When we go to MOX and reset the ring buffers for the
//						keyer, this must be protected from interference by
//						semaphores as all other	commands must be.  This ring
//						buffers had been left unprotected.
//		Reported By:	K5SDR, KE5DTO, N4HY
//		Fix Coded By:	N4HY
//
//		Issue:			CW Keyer can sometimes produce whitenoise and other
//						various audio noises.
//		Fix:			When we go to MOX and reset the ring buffers for the
//						keyer, this must be protected from interference by
//						semaphores as all other	commands must be.  This ring
//						buffer had been left unprotected.  This revealed
//						several major threading and data buffering issues.
//						Portaudio guarantees that it will always deliver
//						floats and a fixed number of them.  The ring buffer
//						code which has all the machinery to handle other
//						more variable cases, was not optimized for our use.
//						This was easily remedied by opting for "float buffer"
//						only ring buffers.  This eliminates the possibility
//						for the generation of white noise events during times
//						of recovery.  It is more efficient since all pointers
//						are float pointers.  The CW Ring buffers get reset on
//						MOX release when going back to receive.  There were two
//						non-threadsafe functions called to operate on entities
//						shared by three different threads.  With probability
//						close to one, we saw blow ups and thread contention for
//						these resources.  All threads are blocked from this
//						buffer now when it is reset.  See ringb.[ch] for major
//						changes	to add float ring buffers and keyd.c for
//						changes to the keyer.
//		Reported By:	K5SDR, KE5DTO, N4HY
//		Fix Coded By:	N4HY
//
//	Modifications:
//
//		Feature:		Keyer Clock resolution setting.
//		Change:			A keyer clock resolution check box has been added
//						to Setup->DSP->Keyer->Options panel. Use the
//						setting that produces the most stable dit stream
//						for your computer.
//		Suggested By:	N4HY
//		Coded By:		N4HY
//
//		Feature:		Multimeter and Display data retrieval method.
//		Change:			Previous to this release, the multimeter and display
//						threads would call blocking routines on the DSP to
//						retrieve data.  Under heavy loads, this could cause the
//						audio to drop out while waiting for the console
//						to draw the new data.  A new algorithm has been 
//						implemented which decoupes the retrieval of the data 
//						and the subsequent painting routines that display the
//						new data.
//		Suggested By:	N4HY
//		Coded By:		KE5DTO
//
//		Feature:		SWR Protection.
//		Change:			The algorithm that detects SWR has been modified to be
//						less strict when using TUN.  This will allow the user
//						to more easily tune the radio through heavy loads for 
//						up to 15 seconds.
//		Suggested By:	W5GI, N4HY
//		Coded By:		KE5DTO
//
//		Feature:		ATU Timing.
//		Change:			The ATU timing has been adjusted so that the ATU has
//						more time to finish tuning before the software gives
//						up.  Also, the power output when tuning on 20m has been
//						slightly boosted.
//		Suggested By:	K5SDR
//		Coded By:		KE5DTO
#endregion

#region 1.4.0 Released 07/08/05
//1.4.0 Released 07/08/05
//
//	Bug Fixes:
//
//		Issue:			Keyer fails when Iambic is unchecked.
//		Fix:			On those computers where the high performance counter
//						has the "ACPI 1.0 value" for QueryPerformanceFrequency
//						(roughly 3.5MHz), the CW interval timing request was 
//						too small. This	caused an errant value to be passed to
//						the keyer.  The result was that the element would drop
//						out.  The temporary fix is to lower the resolution of
//						the	keyer interval for all machines while we work out a
//						strategy for enabling those people with higher end
//						machines to get perfect CW to very high speeds.  
//						Another	problem was having the dot and dash memory 
//						enabled with midelement state corrections from the
//						iambic still enabled.  These are now turned on and off
//						with the changing keyer mode.  You can run the test on
//						your system using the link in the reference below.
//		Reported By:	KE5DTO
//		Fix Coded By:	N4HY
//		References:		http://lightconsulting.com/~thalakan/perf.exe
//
//	Modifications:
//
//		Feature:		Zero Beat in CW modes.
//		Change:			Zero Beat now tunes the peak signal to the CW offset if
//						the radio is in CW mode and CW Offset is within the 
//						current	passband.  Otherwise, it tunes to the center of
//						the filter passband just like SSB modes.
//		Reported By:	WA8SRA
//		Coded By:		KD5TFD
//
//		Feature:		TX IQ correction enabled.
//		Change:			On the Image Reject Tab in the Transmit rejection box,
//						an enable calibration tone checkbox has been added.
//						When this is checked and the radio is put in MOX in a 
//						voice mode (not CW), a tone will be present.  
//						WARNING: The power of this tone will be set by the
//						power control. Do not run this at full power for long.
//						You can then calibrate the opposite sideband image.  On
//						the Delta 44, I found this to be impossible without a
//						spectrum analyzer because the image	is down low enough
//						that it is inaudible in my other receiver with my 1
//						watt radio.  This should be more easily done with a
//						second receiver with higher power.  This addresses the
//						issue that the TUN tone does not go through the DSP.
//		Reported By:	DK7XL
//		Coded By:		N4HY
//
//		Feature:		Reverse Paddle Enabled
//		Change:			On the Setup->DSP->Keyer Tab, a new control has been
//						added enabling reverse paddles for you southpaws.
//		Reported By:	VK6APH
//		Coded By:		N4HY
//
//	New Features:
//
//		Feature:		Mic TX Meter mode.
//		Description:	In order to calibrate the microphone without the use of
//						the ALC meter (which was removed in this version), we 
//						have added a Mic meter which shows modulation from 0 to
//						120%.  In order to calibrate the MIC setting, you want
//						peak values to hit 100% with rare peaks hitting 120%.
//						Note that when you hit 120%, you will distort due to
//						clipper that replaced the ALC.
//		Reported By:	N4HY, VK6APH
//		Coded By:		KE5DTO

#endregion

#region Beta 1.3.14 Released 07/05/05
//Beta 1.3.14 Released 07/05/05
//
//	Bug Fixes:
//
//		Issue:			TX IMD Test does not work with Delta 44.
//		Fix:			The output on this test was being scaled twice so the
//						resulting output was much lower than intended.
//		Reported By:	K5SDR
//		Fix Coded By:	KE5DTO
//
//		Issue:			Opening and closing a serial port causes problems.
//		Fix:			After a port had been opened and closed, the port would
//						no longer be allowed to open until the PowerSDR
//						application was restarted.  This has been corrected by
//						handling the existing events appropriately before
//						freeing the handle for the port.
//		Reported By:	N9VV
//		Fix Coded By:	N8VB
//
//		Issue:			MOX not activating MON in external PTT.
//		Fix:			When the PollPTT function was rewritten, some of the
//						logic needed to be improved.  Now when keying the radio
//						via the X2 or CAT connections, it will check to see if
//						radio is in a CW mode and if the monitor should be
//						turned on, just like the CW PTT.  The microphone PTT
//						will continue to work as it has.
//		Reported By:	KM0T
//		Fix Coded By:	KE5DTO
//
//		Issue:			Switching between CWU/CWL loses last VFO digit.
//		Fix:			The algorithm for switching dsp modes was using 16-bit
//						floats to calculate the frequencies.  This was causing
//						a rounding error where the last digit would be lost.  
//						To fix this, the offset calculations are now done using
//						32-bit doubles.
//		Fix Coded By:	KE5DTO
//
//		Issue:			Starting console in CWU/CWL causes broken keyer.
//		Fix:			There were some holes in our initialization logic for
//						the	new keyer.  These holes have been filled and the no
//						longer has to be changed before getting good audio
//						output with the new keyer.
//		Reported By:	W0VB
//		Fix Coded By:	KE5DTO
//
//		Issue:			CWU transmits CW pitch * 2 too low.
//		Fix:			The tones produced by the CW keyer were not being 
//						switched appropriately for CWU.  This has been fixed
//						and the output is at the right frequency now.
//		Reported By:	W0VB, VE3MM
//		Fix Coded By:	KE5DTO
//
//		Issue:			TX EQ impacts transmitter power / gain.
//		Fix:			The Transmit equalizer could raise and lower the system
//						gain on TX by as much as 15 dB.  The TX EQ filter has
//						now been normalized.
//		Reported By:	VK6APH
//		Fix Coded By:	N4HY
//
//		Issue:			CWL and New Keyer broken.
//		Fix:			When CWL was selected with the new keyer, broken audio
//						was emitted.  This was caused by actually starting
//						two timers and both were calling the keyer. One
//						is destroyed when you change modes and a new one is
//						started only if CW mode is entered. The ChangeMode
//						handler was repaired.
//		Reported By:	W0VB, N9VV, WA8SRA
//		Fix Coded By:	N4HY
//						
//	Modifications:
//
//		Feature:		DSP Group reorganized.
//		Change:			The DSP group on the front panel has been reorganized
//						for more logical layout.
//		Coded By:		KE5DTO
//
//		Feature:		ALC Multimeter Option Removed.
//		Change:			Given the changes in Beta 1.3.13, the ALC meter no
//						has meaning.  It has been removed.  Use the Peak Power
//						setting to calibrate the front panel MIC control.
//		Suggested By:	N9VV, W0VB
//		Coded By:		KE5DTO
//
//	General Release Notes:
//
//		During the past week we began two major transmit development tracks.
//		One was on the transmitter code for voice which resulted in the 1.3.13
//		release.  The other development path was on making the new keyer more
//		reliable.  We inadvertantly left this keyer work in an unfinished
//		state.  While working on problems that seemed to be caused by parallel
//		port contention, we	learned that there is essentially no parallel port
//		contention because of the way the OS mirrors the port values.  This is
//		one of the MAJOR reasons PortTalk is necessary to access these 
//		protected ports.  We had unnecessarily restricted the number of times
//		per second we are reading the port whilst looking for various types of
//		PTT signals.  Writing to these ports is expensive.  Reading from them
//		is expensive _only_ when there is a change of state and is extremely
//		fast otherwise.  Using this reasoning, we have raised the polling rate
//		to 1000 times per second without measureable impact on the CPU meter or
//		amplifier power monitoring on a 2GHz Celeron in the lab.  The remaining
//		issues to be dealt with are plugging an observable glitch in the keyer
//		logic.  There is a bug in the dot/dash memory handling under certain
//		conditions.  The first dit problem was introduced in the first keyer
//		incarnation under this OS by making an error in the	ring buffer sizing.
//		This caused a ringbuffer reset to be generated often when the key was
//		first depressed for	semi-breakin.  This seems to be fixed.
#endregion

#region Beta 1.3.13 Released 07/01/05
//Beta 1.3.13 Released 07/01/05
//
//	Bug Fixes:
//
//		Issue:			Transmit ALC distorting output.
//		Fix:			The transmit limiter was an attempt to guarantee that
//						we had linear output and did not overdrive the
//						amplifier or the QRP version.  After careful testing,
//						it became clear that it was the primary	cause of all
//						transmit distortion in this software.  It caused the
//						Transmit Equalizer, Compressor, Compandor, etc. to
//						sound distorted.  The only thing that continued to 
//						allow the transmitter to be useful was the outstanding
//						transmit filter.  We have replaced it with the simplest
//						possible function -- a clipper.  If you overdrive, you
//						are clipped.  You are given 20% overhead.  You set the
//						voice operation by using the Peak Power METER.  The ALC
//						is no longer functional as it DOES NOT EXIST.  Using
//						the Peak Power Meter,  set the Mike gain until you are
//						around 1.0 (100W w/PA) and rarely, if ever, hit	above
//						that.	 If you hit 1.2 (120 w/PA all the time, you are
//						being clipped.  You will not overdrive the amp 
//						excessively and you will not splatter because of the 
//						terrific transmit filters but expect to be highly
//						distorted.
//		Fixed By:		N4HY
//
//		Issue:			100W Peak Power reading wrong on analog meter.
//		Fix:			The peak power digital meter was not matching the bar
//						graph style meter below it.  This was a text parsing
//						error and has been resolved.  The two numbers now
//						match.
//		Fixed By:		KE5DTO
//
//		Issue:			First dit truncation.
//		Fix:			The new keyer can have a small variable onset of
//						about 40-50 ms.  We have added a small zero insert
//						when the keyer puts the radio into switch MOX mode.
//						Please comment on the change in first dit truncation
//						as it would be optimal to minimize this delay.
//		Reported By:	N4HY, WA8SRA
//		Fixed By:		N4HY
//		References:		Private email from Dale, WA8SRA to N4HY
//
//		Issue:			TX_EQ was destroying transmitter filter response.
//		Fix:			New TX_EQ was installed with an attempt to smooth
//						the transition bands between filter segments.  The
//						attempt to do the smoothing was faulty and actually
//						caused reduction in the inband power and a spurious
//						response over a large range.  
//		Reported By:	K2WS, N4HY
//		Fixed By:		N4HY
//		References:		Email 06/29/05 Subject: [Flexradio] 1.3.9 plus, private
//							email from Alan, K2WS to N4HY
//
//		Issue:			BandStack registers not saving filters properly.
//		Fix:			Some references to non-thread safe controls were
//						this problem.  This is now fixed.
//		Reported By:	N4HY
//		Fixed By:		KE5DTO
//
//		Issue:			AGC Max Gain setting caps at 23dB (too quiet).
//		Fix:			While updating the DSP calls to the new named pipes
//						structure, a calculation error was introduced that was
//						shorting the max gain by about 60dB.  This has been
//						fixed.  Please be careful about importing databases
//						from Beta 1.3.12 with elevated Max Gain settings.
//		Reported By:	N9DG, K5SDR
//		Fixed By:		N4HY
//		References:		Email 06/26/05 Subject: [Flexradio] Seriously reduced
//							RX audio ouput level in versionsafter 1.3.8
//
//		Issue:			The dialog when opening multiple PowerSDR windows
//						is confusing.
//		Fix:			The dialog has been changed to ask if you would like to
//						continue opening the current PowerSDR instance even 
//						though another one is already open.  This should be
//						much more clear and will keep us from having to close
//						other processes forcibly.
//		Reported By:	F5JD, W0VB, AB1DO
//		Fixed By:		KE5DTO
//		References:		Email 06/26/05 Subject: RE: [Flexradio] Trying to open
//							two instances of PowerSDR - My fault
//
//		Issue:			Display Average getting reset too often.
//		Fix:			The average data was being reset when either of the
//						high or low filter controls were changed.  This has
//						been fixed and the display is now steady while tuning
//						with AVG on.
//		Reported By:	KD5TFD
//		Fixed By:		KD5TFD
//		References:		Email 06/25/05 Subject: Re: Smooth scrolling average 
//							display- fix included 
//
//		Issue:			TXEQ causes distortion on RX.
//		Fix:			A typo was causing the TXEQ window to be applied to the
//						RX filter.  Note that this was also causing problems in
//						RX when just changing the TX filter limits.  This has
//						been fixed and is now applied to the TX filter
//						correctly.
//		Reported By:	M0ZAZ
//		Fixed By:		N4HY
//		References:		Email 06/26/05 Subject: Re: [Flexradio] TX and RX
//							Filter problems, response
//
//		Issue:			Display Limits not checked properly.
//		Fix:			Rather than simply checking to make sure the min was
//						less than the max (and max was more than min), we now
//						check to make sure that the max-min is at least two
//						steps apart in order for the drawing routines to 
//						operate correctly.
//		Reported By:	N9VV
//		Fixed By:		KE5DTO
//		References:		Email 06/25/05 Subject: unhandled exception
//
//	Modifications:
//
//		Feature:		Software Update Notification.
//		Change:			The update pop-up will now let you know not only that a
//						new version has been released, but will also let you
//						specifically what bugs have been fixed, which features
//						have been modified, and what new features have been
//						added.
//		Coded By:		KE5DTO
//
//		Feature:		Semi Break In Delay.
//		Change:			Maximum raised to 5000ms from 1000ms.
//		Requested By:	N9VV, other CW ops.
//		Coded By:		KE5DTO
//
//		Feature:		Old Keyer vs New Keyer.
//		Change:			In order to more clearly indicate which keyer is
//						active (old or new), when the new keyer is active, the
//						old keyer form will be closed (if it was open) and the
//						menu to open it will be disabled.  Turning the new
//						keyer off on the DSP Tab of the Setup Form will restore
//						the old keyer functionality.
//		Requested By:	N4HY
//		Coded By:		KE5DTO
//
//		Feature:		Front panel MIC control.
//		Change:			When using the Delta 44, in previous versions the front
//						panel MIC control was not functional.  For this reason
//						we replaced the original MIC control with one that now
//						functions as the Mic Preamp (previously on Transmit Tab
//						of the Setup Form).  This new control maps the values
//						0-100 to the range 0-70dB of software gain.  If using a
//						different sound card, the older MIC control can be 
//						found on the Audio Tab which has been split into Card 1
//						and Card 2 tabs (see Mic In Gain).
//		Requested By:	K2WS
//		Coded By:		KE5DTO
//
//		Feature:		PA Gain Calibration.
//		Change:			The PA Gain Cal has been further adjusted from 1.3.10.
//						It is now faster than before (4 second cycles instead
//						of 7) and begins at 51dB instead of 54 (which was too
//						high).
//		Coded By:		KE5DTO
//
//		Feature:		All audio scaling.
//		Change:			The audio scaling now all takes place in the C#
//						callbacks rather than in the DSP itself.  Please note
//						any changed levels on RX or TX on the reflector so we
//						can debug any related issues as quickly as possible.
//		Coded By:		N4HY, KE5DTO
//
//	New Features:
//
//		Feature:		Display Label Alignment Options.
//		Description:	This new control allows the user to align the signal
//						level labels to the Left, Center, Right, or Off.  The
//						Auto option enables the alignment to adjust depending
//						on the selected filter/display mode (as it was in the
//						past).
//		Suggested By:	K5SDR, W5ZL, N4HY
//		Coded By:		KE5DTO
//
//		Feature:		Zero Beat Function.
//		Description:	Pushing the 0 Beat button tunes the VFO to the highest
//						peak found in the display.  This is useful for CW and
//						eventually we hope to make this useful for SSB.  Note
//						that this feature is only enabled when display 
//						averaging (AVG) is active on an appropriate display
//						mode.
//		Coded By:		KD5TFD
//
//		Feature:		IF to VFO Function.
//		Description:	Pushing the IF->V button takes the current filter width
//						and shift and retunes the VFO (and filters) so that the
//						signal is tuned without use of the filter shift.  For
//						CW, it puts the center at the CW pitch (selectable on
//						Setup Form on the DSP Tab).  For SSB, it puts the
//						lower edge of the current passband at 100Hz.  For AM,
//						it takes the center of the current filter and tunes the
//						VFO to that point.
//		Coded By:		KD5TFD
#endregion

#region Beta 1.3.12 Released 06/24/05
//Beta 1.3.12 Released 06/24/05
//
//	Bug Fixes:
//
//		Issue:			PTT incorrectly sensed for those with USB parallel port
//						adapter.
//		Fix:			When rewriting the PTT routine, the x2_ptt signal was
//						not inverted as it should have been.  Once this was 
//						fixed, the ptt signal is once again correctly sensed.
//		Reported By:	K5KDN
//		Fixed By:		N4HY
//
//		Issue:			Switching Audio buffers on the fly causes crashing.
//		Fix:			While fixing a problem with the process not letting 
//						some of the threads die when closing, we added a call
//						to Application.DoEvents() that was causing this error.
//						We now only make this call if the console is being 
//						closed.
//		Reported By:	KE5DTO, N9VV, many others
//		Fix Coded By:	KE5DTO
#endregion

#region Beta 1.3.11 Released 06/24/05
//Beta 1.3.11 Released 06/24/05
//
//	Bug Fixes:
//
//		Issue:			Footswitch not working with PTT.
//		Fix:			The PTT thread function was in dire need of a complete
//						rewrite even before we introduced the new keyer.  This
//						problem was the last straw.  The new function is much
//						cleaner and asserts that unless you click the button,
//						whatever turned MOX on must turn it off.
//		Reported By:	KM0T, K3IB, W0VB
//		Fix Coded By:	KE5DTO, N4HY
//		References:		Email 06/23/05 Subject: Re: [Flexradio] 1.3.10 EXT PTT INPUT?
//
//		Issue:			Swap Between New and Old Keyer Broken.
//		Fix:			When swapping to the old keyer, the new keyers timer
//						was still running and interfering with the parallel
//						port operation. When swapping from the old keyer to the
//						new	keyer, the keyer timer was not started.  Setup.cs
//						handler for new keyer check box now	closes the CW form
//						when enabling new keyer and	starts keyer timer.  It
//						stops the keyer timer if the old keyer is selected but
//						does not open the form.
//		Reported by:	WO0Z
//		Fixed By:		N4HY
//
//		Issue:			New Keyer has timing issues when running with the
//						amplifier.
//		Fix:			The demand on the parallel port is becoming large.  In 
//						this case,  we were oversampling the parallel port
//						considerably for the new keyer.  It was sampling 500
//						times a second.  This is not needed and has been 
//						reduced to 100 times a second.
//		Reported By:	WA8SRA
//		Fixed By:		N4HY
//
//		Issue:			Harmonic Distortion in New compressor.
//		Fix:			The new compandor code had a bug in the constructor.  A
//						wrong variable was used to initialize the compression
//						table and made the table too small for the PowerSDR
//						console and as a result had increased harmonic
//						distortion. This has been fixed and now the compander
//						table is very large compared to first instantiation.
//		Reported By:	AB2KT
//		Fixed By:		AB2KT
#endregion

#region Beta 1.3.10 Released 06/23/05
//Beta 1.3.10 Released 06/23/05
//
//	Bug Fixes:
//
//		Issue:			Importing a database on the startup wizard causes
//						CAT errors.
//		Fix:			The CAT code XML file was being looked for in the
//						wrong directory if a file selection form had been
//						opened.  This was addressed by adding a reference
//						to the Application.StartupPath before the XML
//						filename.
//		Reported By:	N9VV
//		Fix Coded By:	KE5DTO
//		References:		Email 06/21/05 Subject: hummm... install problem
//
//		Issue:			Swapping VFO with certain frequencies and AVG on causes
//						crashing.
//		Fix:			The smoothed tuning method had 2 checks that needed to
//						be >= instead of >.  This has been fixed.
//		Reported By:	
//		Fix Coded By:	KD5TFD
//		References:		Email 06/21/05 Subject: RE: Averaging and smooth motion
//
//		Issue:			Setting PWR to 0 while TUN is active causes an error.
//		Fix:			The Tune Power control on the Setup Form had a minimum
//						of 1.  This has been changed to 0 to correct this
//						issue.
//		Reported By:	K5SDR
//		Fix Coded By:	KE5DTO
//
//		Issue:			Filter selection on SPEC and DRM are wrong.
//		Fix:			The algorithms that handled the special cases for these
//						modes had not been modified to use the new ThreadSafe
//						controls.  This has been corrected.
//		Reported By:	DL2JA, OH2BFO
//		Fix Coded By:	KE5DTO
//		References:		Email 06/21/05 Subject: [Flexradio] 1.3.9 SPEC, DRM
//							modes still not working correctly
//
//		Issue:			Freq is 11kHz off in SPEC mode with Spur Reduction off.
//		Fix:			The ChangeMode function was setting the TX oscillator
//						when it should have been setting the RX oscillator. 
//						Once this was fixed, everything checked out here.
//		Reported By:	OH2BFO
//		Fix Coded By:	KE5DTO
//		References:		Email 06/21/05 Subject: [Flexradio] 1.3.9 SPEC, DRM
//							modes still not working correctly
//
//	Modifications:
//
//		Feature:		PA Gain Calibration.
//		Description:	This cal routine has been changed to be more 
//						intelligent when attempting to find a starting point
//						on each band.  It will now try 54, 48 and finally 45dB
//						before giving up.  This should allow some people who 
//						were having trouble running the cal on certain bands
//						to be able to run it.
//		Suggested By:	K5SDR
//		Fix Coded By:	KE5DTO
#endregion

#region Beta 1.3.9 Released 06/21/05
//Beta 1.3.9 Released 06/21/05
//
//	Bug Fixes:
//
//		Issue:			MOX button enabled when in Standby Mode.
//		Fix:			This slipped in when the receive-only checkbox was
//						implemented.  This has been fixed.
//		Fix Coded By:	N4HY
//
//		Issue:			MouseWheel gets stuck on Filter Width slider.
//		Fix:			This is a problem with the .NET framework in that the
//						TrackBar (slider) control itself has a default 
//						mousewheel handler that is called even if you define
//						a custom event.  The best we can do is to remove
//						focus from the slider after the first tick of the
//						scroll to prevent further events from triggering the
//						default handler.  Note that this affects NumericUpDown
//						and ComboBox controls as well.
//		Reported By:	AB1DO
//		Fix Coded By:	KE5DTO
//		References:		Email 06/14/05 Subject: [Flexradio] v1.3.8 Mouse knob
//							tuning not active when filter widthslider is used
//
//		Issue:			Alt + Arrow Key shortcuts were broken in v1.3.8.
//		Fix:			The addition of the new shortcuts caused problems with
//						some existing setups using the arrow keys.  This has
//						been fixed.
//		Reported By:	WA8SRA
//		Fix Coded By:	KE5DTO
//
//	New Features:
//
//		Feature:		Ext Ctrl Band Control screen.
//		Description:	In advance of supporting the full UCB (Universal 
//						Controller Board) that Terry W0VB and Tony KB9YIG have
//						toiled over, I have added support for switching	the
//						first 5 pins of the X2 connector.  See the new Ext Ctrl
//						Tab on the setup form.  A check indicates logic high
//						for that pin.
//		Suggested By:	W0VB, KB9YIG
//		Coded By:		KE5DTO
//
//	Modifications:
//
//		Feature:		Updated CAT Parsing Code.
//		Description:	Bob Tracy has released Beta 2 of the CAT parsing code.
//						See his website below for more info.
//		Coded By:		K5KDN
//		References:		http://www.btracy.com
//
//		1.3.9 contains the first implementation of resizing.
//		We have placed limitations on it to get you to concentrate
//		on helping us do debugging.   We need to further decouple
//		the audio system from the dsp side so we can have completely
//		independently selectable dsp and audio buffer sizes.  There
//		are still some constraints in this code.
//
//		Soon you will be able to set 16384 size audio buffers, and 256
//		size dsp buffers or 256 size audio buffers and 4096 size dsp
//		buffers, etc. The other thing is we have not sufficiently
//		decoupled the fft size from the audio and dsp sizes so that will
//		have to be done.  
//
//		If the gotcha's with this dsp release are not too great, the next
//		thing will be the keyer.  Otherwise, we concentrate on bug fixes.
//
//		A neat thing to do is to put on a narrow DSP filter on CW (say
//		100 Hz) and to decrease the dsp size (don't forget to lower the
//		audio size FIRST). You will see the dsp filter in the SPECTRUM
//		display mode widen as a result of it getting shorter.
#endregion

#region Beta 1.3.8 Released 06/13/05
//Beta 1.3.8 Released 06/13/05
//
//	Bug Fixes:
//
//		Issue:			The VFO frequency check uses the wrong value if Split
//						mode is active.
//		Fix:			This issue has been fixed as well as a few more checks
//						for when XIT is active. 
//		Reported By:	K2WS
//		Fix Coded By:	KE5DTO
//		References:		Email 06/08/05 Subject: Re: [Flexradio] Version 1.3.7
//							released
//
//		Issue:			CW Form crashing the console.
//		Fix:			Some test code for the new keyer was inadvertantly left
//						in the source and was causing this problem.  This has
//						been handled appropriately in a conditional 
//						pre-compiler directive.
//		Reported By:	WA8SRA
//		Fix Coded By:	N4HY
//		References:		Email 06/08/05 Subject: Re: [Flexradio] Version 1.3.7
//							released
//
//		Issue:			Wave file playback tuning jumps when on loop/restart.
//		Fix:			In correlation with the new feature below, this issue
//						has been addressed.  Now your tuned signal should
//						stay in the filter passband when restarting file
//						playback (manually, or when Loop is enabled).
//		Reported By:	W0VB
//		Fix Coded By:	KE5DTO
//		References:		AIM Conversations
//
//		Issue:			VFO Lock back color not changing.
//		Fix:			This was a simple oversight and has been corrected.
//		Reported By:	KD5TFD, N4HY
//		Fix Coded By:	KE5DTO
//		References:		Email 06/07/05 Subject: Re: [Flexradio] PowerSDR Beta
//							v1.3.6 is released 
//
//		Issue:			Speech processor (COMP), CW keyer code, meters crashing.
//		Fix:			Reset on TX<->RX issues addressed, other bugs in speechprocessor
//						addressed.  Pops on TX<->RX remedied.
//		Reported By:	AB1DO, KE5DTO, N4HY, DK7XL, KM0T
//		Fix Coded By:	N4HY
//
//		Issue:			Power meter not reading correctly for 1W setting.
//		Fix:			The new DSP was not setting this value correctly and
//						it was not being scaled by the Power setting.
//		Reported by:	KM0T, N4HY
//		Fix coded by:	N4HY
//
//	Modifications:
//
//		Feature:		Recording TX Wave Files.
//		Description:	The wave file recorder has been modified to record the
//						the correct audio when in TX mode.  This means taking
//						the filtered I/Q output of the DSP as opposed to the
//						untouched I/Q input of the radio (when in RX).
//		Suggested By:	W5GI, K5KDN
//		Fix Coded By:	KE5DTO
//		References:		Email 06/07/05 Subject: RE: Delta 44 hookup
//
//		Feature:		Shuttle Pro v2 Support.
//		Description:	In an effort to more completely support the Shuttle Pro
//						USB controller, we have added a number of additional
//						keyboard shortcuts.  Please see the spreadsheet in the
//						references for more info on the key mappings.
//		Suggested By:	W5ZL, KD5TFD
//		Fix Coded By:	KE5DTO
//		References:		Email 06/09/05 Subject: RE: ShuttlePro Pref file
//						http://www.flex-radio.com/key-map.xls
//
//	New Features:
//
//		Feature:		Filter Width Slider.
//		Description:	Sliding this control allows the user to adjust the
//						width of the filter on a logarithmic scale.  When
//						used, it automatically selects the first variable
//						filter (if one is not already selected).
//		Suggested By:	KD5TFD, K2WS
//		Coded By:		KD5TFD
//		References:		http://www.flex-radio.com/forum/viewtopic.php?t=1661
//
//		Feature:		DC Blocking Filter.
//		Description:	A DC blocking filter has been enabled in the DSP
//						now in the console.  In Setup Transmit tab, in the
//						TX equalizer group box,  you will find a check box
//						to enable/disable this filter.  This preceeds the
//						transmit equalizer and all other processing. It is
//						a quick attempt to help those whose sound system is
//						producing junk below 100Hz that is capturing the
//						compressor and the limiter.
//		Suggested By:	VK6APH
//		Coded By:		N4HY
#endregion

#region Beta 1.3.7 Released 06/08/05
//Beta 1.3.7 Released 06/08/05
//
//	Bug Fixes:
//
//		Issue:			Microphone Input channel swapped.
//		Fix:			The new DSP was looking for microphone input on the
//						opposite side than what most cards use.  This was
//						causing TX to have no modulation.  This has been
//						swapped back to the previous setting.
//		Fix Coded By:	N4HY
//
//		Issue:			Phase display not working.
//		Fix:			The new DSP had the phase display hooked to a dead
//						end.  This has been fixed.
//		Reported By:	KC2LFI
//		Fix Coded By:	N4HY
//		References:		Email 06/07/05 Subject: Re: [Flexradio] PowerSDR Beta
//							v1.3.6 is released 
//
//		Issue:			HRD blue screen with N8VB's vCOM driver.
//		Fix:			The CAT source has been updated to help prevent this
//						problem.  Please see Phil's webpage for more info on
//						the latest vCOM driver.
//		Reported By:	KD5TFD
//		Fix Coded By:	N8VB
//		References:		http://www.philcovington.com/SDR.html
//
//		Issue:			Changing Preamp while in TX changes output power.
//		Fix:			The Preamp should not be changed while in TX and has
//						been disabled to prevent this issue.
//		Reported By:	W4DWG
//		Fix Coded By:	KE5DTO
//		References:		Email 06/07/05 Subject: Re: [Flexradio] (no subject)
//
//		Issue:			TX ALC meter causing console Fatal exception
//		Fix:			Unitialized variable in digitalagc being read by
//						meter before the dsp got to it the first time.
//		Fix Coded By:	N4HY
//		References:		Email 06/07/05 Subject: Re: [Flexradio] (no subject)
#endregion

#region Beta 1.3.6 Released 06/07/05
//Beta 1.3.6 Released 06/07/05
//
//	Bug Fixes:
//
//		Issue:			Digitalagc performing badly if strong signal is present
//						in the beginning of the buffer.
//		Fix:			Digitalagc now computes	a gain from looking 48 samples
//						into the future.  This introduces a 48 sample lag
//						(0.1 ms) but improves the AGC dynamics.  Look for more
//						changes coming soon.
//		Reported By:	VK6APH, VK6APH, VK6APH, and finally VK6APH.
//		Fix Coded By:	AB2KT and N4HY
//
//		Issue:			Speechprocessor causes console to throw a fatal error.
//		Fix:			A nasty bug in a two year old routine in bufvec.c was
//						uncovered as well as a pointer writing beyond the end
//						of the speechprocessor buffer.
//		Reported By:	AB1DO
//		Fix Coded By:	N4HY
//		References:		http://mail.flex-radio.biz/pipermail/flexradio_flex-radio.biz/2005-June/000481.html
//
//		Issue:			Can not use double digit COM port (eg COM10).
//		Fix:			This was apparently a well known MS bug.  See the
//						reference below for more information.
//		Reported By:	James C Samuels
//		Fix Coded By:	KD5TFD
//		References:		Email 06/06/05 Subject: Re: [Flexradio] Double Digit 
//							COMPort Support
//						http://support.microsoft.com/default.aspx?scid=kb;%5BLN%5D;115831
//
//		Issue:			AF and MUT do not work with Delta 44 in Beta 1.3.5.
//		Fix:			This was a result of consolidating some of the new
//						audio handling for the Delta 44.  This is now fixed.
//		Reported By:	W5GI, K5SDR, K5KDN
//		Fix Coded By:	KE5DTO
//		References:		Email 05/27/05 Subject: Re: [Flexradio] PowerSDR Beta
//							v1.3.5 is released.
//
//		Issue:			Not having an ASIO device causes an error on startup.
//		Fix:			A logic flaw in the initialization code caused this.
//						A better object oriented design fixes this issue and
//						is a more robust solution that will support future
//						hosts as well.
//		Reported By:	KC5NRA
//		Fix Coded By:	KC5NRA
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=7720#7720
//
//		Issue:			ATU not being bypassed when changing bands.
//		Fix:			The new hardware code was being exercised and no hardware
//						control was being sent while the changes took place when
//						changing bands.  Unfortunately, this meant that the ATU
//						was not working properly in that instance.  This has been
//						fixed.
//		Reported By:	K5SDR
//		Fix Coded By:	KE5DTO
//
//	Modifications:
//
//		Feature:		Rewrite of DSP.
//		Description:	In order to allow for remoting, resize, smaller latency,
//						and to gain further streamlining, a rewrite was done.
//						Not all features are enabled in this release as the
//						Console must be modified to accomodate these changes.
//						Use of Named Pipes is included (we will test sockets later)
//						for the remoted transmission of commands, power spectra,
//						meter data, and scope data.  The current DSP can accept
//						local .dll function calls OR remoted commands. A new
//						compressor is built in (wscompand) as well as all new
//						spectrum and meter calculation.  Spot tones are enabled
//						and need to be enabled in the console to function (soon)
//						A better dcblocking module has been added to fix up the
//						nasty DC offset in some sound cards on TX.  It is not yet
//						enabled in the console but will be in the next release.
//		Coded By:		AB2KT, N4HY
//
//		WARNING:		Release 1.3.6 is a major change in the DSP and console
//						algorithms.  Much can go wrong.  We are using you as
//						alpha testers to help us get to 1.4.0 quickly.  BEWARE
//						when using this code to drive high power amplifiers
//						and transverters!  NO MAJOR NEW DSP features are enabled
//						in 1.3.6.  This is a debugging release to stabilize the
//						code before we turn on all the new features.
//
//		Feature:		Pthreads-Win32 Release 2.6.0.
//		Description:	Continued progress has been made in pthreads by Red Hat.
//						Increased use of critical sections speeds up the semaphores,
//						mutexes, etc.  and bug fixes.
//		Reference:		http://sources.redhat.com/pthreads-win32/
//
//		Feature:		Smoothed Average Tuning.
//		Description:	Tuning while display averaging was turned on has always
//						given a strange effect until now.  This was because the
//						existing data needed to be shifted to coincide with the
//						VFO changes.
//		Coded By:		KD5TFD
//
//	New Features:
//
//		Feature:		DC Blocking Filter.
//		Description:	The DSP now supports a DC blocking filter for those
//						folks who have very poor response and/or 
//						noise/spurious signals below 100Hz on their sound 
//						cards.  To enable this,	go into Setup/Transmit and
//						check the DC Block check box in the Equalizer section.
//		Coded By:		AB2KT, N4HY
//
//		Feature:		VFO Lock
//		Description:	This is a handy feature that prevents you (and others)
//						from accidentally changing the VFO frequency and
//						ruining a QSO.
//		Suggested By:	N9VV
//		Coded By:		KD5TFD
//		References:		Email 06/05/05 Subject: [Flexradio] do you use VFO-LOCK?
//						http://www.flex-radio.com/forum/viewtopic.php?p=5135#5135
//						http://www.flex-radio.com/forum/viewtopic.php?p=4966#4966
//						http://www.flex-radio.com/forum/viewtopic.php?p=437#437
#endregion

#region Beta 1.3.5 Released 05/27/05
//Beta 1.3.5 Released 05/27/05
//
//	Bug Fixes:
//
//		Issue:			Frequency gets lost when switching to or from CW modes.
//		Fix:			When the new hardware code was implemented, some of the
//						logic that kept the CW modes in order was lost in the
//						shuffle.  This has been corrected.
//		Reported By:	N9VV
//		Fix Coded By:	KE5DTO
//		References:		Email 05/22/05 Subject: [Flexradio] [Q] 1.3.4 CW
//						problems?
//
//		Issue:			Setting PWR to 0 with the Delta 44 mutes the output of
//						the transmitter until the software is restarted.
//		Fix:			This was leftover code from the single sound card setup
//						where the output was simply muted when the PWR was set
//						to 0.  This has been fixed for Delta 44 cards and
//						2 sound card setups.
//		Reported By:	K0PFX, W5GI
//		Fix Coded By:	KE5DTO
//		References:		Email 05/17/05 Subject: [Flexradio] Fail to recover
//						from "0" pwr setting
//
//		Issue:			CW sidetone in v1.3.2+ has artifacts with the Delta 44.
//		Fix:			Some of the logic used to copy the buffers to both the
//						monitor and the radio channels had a flaw in it.  This
//						has been fixed and the sidetone is clean again.
//		Fix Coded By:	KE5DTO
//
//	New Features:
//
//		Feature:		Large Font Support.
//		Description:	The PowerSDR software now supports display modes with
//						dpi	settings other than 96dpi.  This will allow those
//						running	larger screens to keep the resolution high and
//						run large fonts (120dpi on XP) or even custom dpi 
//						settings without the errors previously seen in the
//						PowerSDR software. 
//		Requested By:	W5ZL
//		Coded By:		KE5DTO
//
//		Feature:		Wave File Tuning -- All 48kHz.
//		Description:	While we have had the ability to playback from recorded
//						IF wave files for quite some time, the tuning ability
//						has been limited to about 3kHz due to the way spur
//						reduction was implemented (and the way it interacts
//						with the wave file playback).  This version will now
//						tune through the entire 48kHz of the recorded wave 
//						file.  Wider sampling rate tuning will be supported as
//						soon as the sampling rate is unlocked in the new DSP.
//		Requested By:	W0VB, W5ZL, K5SDR (among others)
//		Coded By:		KE5DTO
//
//	Modifications:
//
//		Feature:		Increased Keyboard shortcut support.
//		Description:	In light of the recent flurry of ShuttlePro sales, it
//						is clear that we need to offer more keyboard shortcuts
//						for controlling various aspects of the PowerSDR
//						software.  Support for manipulating XIT, RIT, and CW
//						dot and dash are now available on the Keyboard tab of
//						the Setup Form.  Expect many more options to be 
//						available in the future	as we attempt to bring the 
//						ShuttlePro controller to full console functionality
//						with default layouts for all the typical operating 
//						modes (CW, DX, VHF, etc).  Please weigh in on the email
//						reflector on what you would like to see available for
//						control via the ShuttlePro. 
//		Requested By:	W5ZL
//		Coded By:		KE5DTO
//
//		Feature:		Increased maximum Mic Preamp gain.
//		Description:	After performing tests in our lab, it was determined
//						that increasing the software Mic Preamp gain to ~55dB,
//						the	Delta 44 no longer requires a separate preamp when
//						using a consumer type microphone.  The output 
//						characteristics	are actually better when doing this 
//						than when running the microphone through an on-board
//						sound card to bring	the voltage up to line level.  The
//						Delta 44 diagram will be changed shortly.
//		Suggested By:	N4HY
//		Coded By:		KE5DTO
#endregion

#region Beta 1.3.4 Released 05/09/05
//Beta 1.3.4 Released 05/09/05
//
//	Bug Fixes:
//
//		Issue:			PA Bias on during RX.
//		Fix:			Due to inverse logic, the PA Bias was left low (on) on
//						start up in Beta 1.3.3.  This is now fixed.
//		Reported By:	WA8SRA
//		Fix Coded By:	KE5DTO
//		References:		Email 05/06/05 Subject: [Flexradio] Higher P/S current 
//						w/ ver. 1.3.3
//
//		Issue:			X2 TR sequencing not working.
//		Fix:			While rearranging the hardware code in a more object 
//						oriented fashion, the code that switched the X2-7 pin
//						was dropped in the shuffle.  This code has been added
//						in the appropriate place to fix this issue.
//		Reported By:	W5GI, WA8SRA, AA5XE, G4BBY
//		Fix Coded By:	KE5DTO
//		References:		Email 05/07/05 Subject: [Flexradio] 1.3.3
//						http://www.flex-radio.com/forum/viewtopic.php?p=7602#7602
//
//		Issue:			PowerSDR crashes on startup if ASIO4ALL is not installed.
//		Fix:			More generally, the program would crash if an ASIO driver
//						was not found.  This was caused by removing the ASIO 
//						devices for the second sound card selection without
//						checking first to see if there was one.
//		Reported By:	KC2LFI
//		Fix Coded By:	KE5DTO
//		References:		Email 05/07/05 Subject: Re: Demo Disk
//
//		Issue:			Mute relay set incorrectly on transition from TX->RX.
//		Fix:			Converting the Mute relay property in the hardware code
//						to positive logic caused this bug.  This is now fixed.
//		Reported By:	KD5TFD
//		Fix Coded By:	KD5TFD
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=7588#7588
//
//		Issue:			Turning Spur Reduction off while in SPEC mode loses
//						signals.	
//		Fix:			The four properties Spur Reduction, IFShift, DDSFreq, and
//						CurrentDSPMode work in tandem.  They must be aware of a 
//						state change in any of these properties.  In some cases
//						changes were not reflected in one or more of these 
//						properties.  This is now fixed.
//		Reported By:	N4HY
//		Fix Coded By:	KE5DTO
//		References:		Email 05/07/05 Subject: Procedure to find bug
//
//		Issue:			Relays come up in wrong state on startup.					
//		Fix:			The algorithm used in Beta 1.3.3 to get the hardware to a
//						known state had a design flaw that caused only certain
//						relays to be set on startup.  This has been corrected and
//						the startup code now sets all the relays to a known state.
//		Reported By:	N4HY, WA8SRA 
//		Fix Coded By:	KE5DTO
//		References:		Email 05/06/05 Subject: [Flexradio] ver. 1.3.3 gain issue
//
//	Modifications:
//
//		Feature:		Delta 44 Max VRMS Change.
//		Description:	Previously the max VRMS for the Delta 44 was set at 3.8V.
//						However, this was measured on the +4dBu output setting.
//						This poses a serious risk of overdriving the amplifier if
//						the PA is not calibrated correctly as the PA needs less
//						than 1 Volt to drive it to 100W.  For this reason, we are
//						changing the voltage for the Delta 44 to read 0.98V.  It 
//						is imperative that the output be changed to match this
//						new voltage setting on the Delta 44 control panel.  Use
//						the -10dBv setting for the output.  Note that the new
//						voltage may get reset if importing a database from an
//						older version.  I will get with N9VV and have him update
//						the screenshots on his site.
//		Reported By:	KE5DTO
#endregion

#region Beta 1.3.3 Released 05/06/05
//Beta 1.3.3 Released 05/06/05
//
//	General Bug Fixes:
//
//		Issue:			TX filter settings only get saved if done in TX mode.
//		Fix:			There was a logic error that was preventing the TX filter
//						from updating unless MOX was active.  Thanks to Bill
//						KD5TFD for pointing out where to fix this in the code.
//		Reported By:	W5GI, KD5TFD
//		Fix Coded By:	KD5TFD, KE5DTO
//		References:		Email 4/27/05 9:18PM Subject: A buglet? 
//
//		Issue:			RX Image Rejection Calibration sometimes does not work.
//		Fix:			In some situations, the image peak of the signal was not
//						being located correctly.  The algorithm now sets the 
//						controls in a way that should magnify the image in order
//						to find it initially.  This should allow the algorithm
//						to more easily find the peak and calculate the minimum
//						correctly.
//		Reported By:	K5SDR
//		Fix Coded By:	KE5DTO
//
//		Issue:			Unsupported Card Warning comes up on first run.
//		Fix:			While unsupported card is the default card (for
//						compatibility reasons), this warning no longer shows up
//						unless Unsupported Card is explicitly chosen.
//		Fix Coded By:	KE5DTO
//
//		Issue:			Configs w/o RFE have wrong cal info on startup.
//		Fix:			This issue was a result of a change made to the level
//						calibration routine for quicker and more accurate
//						values.  Unfortunately, the three board stacks were
//						being initialized correctly.  This has been fixed.
//		Reported By:	DK7XL, EB4APL, WK0J, OH2RZ
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=7491#7491
//
//	New Features:
//
//		Feature:		Expanded Wave File Controls.
//		Description:	We have added a more complete Wave File Playback
//						interface including features such as: Stop, Play,
//						Pause, A playlist for queueing up files, quick
//						switching between files in the playlist using intuitive
//						Previous (Prev) and Next buttons.  Looping works for
//						single files or for an entire list.  Along with this
//						interface comes the ability to record directly from a
//						wave file that is playing back.  This allows easy
//						editing (splitting, combining, etc.) or	existing wave
//						files.  Along with the new interface, we have changed the
//						reader and writer classes to use ring buffer code in order
//						to improve performance.  Playback is now possible all the
//						way down to 64 sample buffers.
//		Coded By:		KE5DTO
//
//		Feature:		Hardware Code Rewrite.
//		Description:	We have completely rewritten the hardware code interface
//						with two main goals in mind.  First, we wanted to allow
//						updates to be written in an atomic manner with respect to
//						each of the 6 8-bit registers (2 on PIO board, 4 on RFE).
//						This should give a significant improvement when compared
//						to accessing each bit on each register atomically which
//						requires as many as 8 times as many writes to the hardware.
//						Second, the code was in bad need of being reorganized to
//						incorporate all of the additions to the original 3 board
//						stack in a coherent style.  The older hardware code had
//						become somewhat *cough* confusing due to just tacking on
//						new code to support new products.
//		Coded By:		KE5DTO
//
//	Modifications:
//
//		Feature:		ALC Meter.
//		Description:	The ALC meter now reads a fictitious positive value when
//						not limiting in order to help give the user a sense of
//						how close they are getting to compression when setting up
//						for transmit audio.  The analog meter has been adjusted
//						appropriately.  
//		Reported By:	W5GI, K2WS, K5SDR
//		Coded By:		KE5DTO
//
//  CAT Bug Fixes 
//
//		Issue:			CAT BitBangPTT settings not save 
//		Fix:			GroupBox for BitBangPTT control was not of the TS flavor.
//						Changing to TS version fixed this issue.
//		Reported By:	AA5XE
//		Fix Coded By:	KD5TFD 
//
//		Issue:			Disabling BitBangPTT does not close comm port.
//		Fix:			This was collateral damage from a prior CAT fix. We now
//						use close_pending when port is used for BitBang mode.
//		Reported By:	KD5TFD 
//		Fix Coded By:	KD5TFD 
//
//		Issue:			Thetis.exe not closing all the way.
//		Fix:			CAT read and/or write threads not closing. Fixed by tagging 
//						them as background. 
//		Reported By:	K5KDN
//		Fix Coded By:	KE5DTO, KD5TFD 
//
//      Issue:          Disabling CAT on a hardware port hangs the CAT threads.
//		Fix:            Combined read and xmit serial threads into a single thread.
//						Cleaned up init and termination.  Cause of the hang
//						apparently read and write threads deadlocking on the comm
//						port. 
//      Reported by:    KD5TFD 
//      Fixed by:       KD5TFD 
#endregion

#region Beta 1.3.2 Released 04/29/05
//Beta 1.3.2 Released 04/29/05
//
//	Bug Fixes:
//
//		Issue:			CAT controls are not visible.
//		Fix:			Somehow the controls for the CAT Control tab had their
//						Visible property set to false in the previous version.
//						This is now fixed.
//		Reported By:	KC2LFI, KD5TFD
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=7433#7433
//
//	Modifications:
//
//		Feature:		DSP Upgrades.
//		Description:	The digital signal processing available for cvs download
//						as on the reference contains many new capabilities.  Not
//						all of these new capabilities are enabled since they need
//						to be enabled and recognized in the GUI, but major new 
//						features are coming.
//		Coded By:		N4HY, AB2KT
//		References:		http://dttsp.sourceforge.net/cvs.html
#endregion

#region Beta 1.3.1 Released 04/25/05
//Beta 1.3.1 Released 04/25/05
//
//	Bug Fixes:
//
//		Issue:			VFO A Band text shows up in VFO B.
//		Fix:			This was a simple typo error from the conversion to the
//						thread safe controls.
//		Reported By:	W0VB, KD5TFD, OH2RZ, AA5XE
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=7373#7373
//
//		Issue:			No receive audio with Delta 44.
//		Fix:			There was a bug in the audio processing algorithm that
//						was causing the receive audio to be muted.  This is now
//						fixed.
//		Reported By:	W9AD, VE3BGE, W5GI
//		Fix Coded By:	KE5DTO
//		References:		Email 4/23/05 10:40AM Subject: delta 44set-up  on 1.3.0??
//
//		Issue:			Console locks up on startup if serial port settings are
//						enabled without having the virtual serial port installed.
//		Fix:			This error is now handled in software.  The source was 
//						also updated and the controls moved from the Test tab to
//						its own tab called CAT Control.
//		Reported By:	NJ1H
//		Fix Coded By:	KD5TFD
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=7394#7394
//
//		Issue:			Keyboard controls are empty.
//		Fix:			The code to fill the keyboard controls used type sensitive
//						constructs which needed to be changed to work with the new
//						thread safe controls.
//		Reported By:	VK6APH
//		Fix Coded By:	KE5DTO
//		References:		Email 4/23/05 6:38AM Subject: V1.3.0
//
//		Issue:			Appearance control changes do not take effect immediately.
//		Fix:			The code which checks the color scheme when appearance
//						controls are changed uses type sensitive constructs which
//						which needed to be changed to work with the new thread
//						safe controls.
//		Reported By:	VK6APH
//		Fix Coded By:	KE5DTO
//		References:		Email 4/23/05 6:38AM Subject: V1.3.0
//
//		Issue:			CW controls are not saved.
//		Fix:			The code which saves the cw controls automatically uses
//						type sensitive constructs which needed to be changed to
//						work with the new thread safe controls.
//		Reported By:	AA5XE
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=7380#7380
//
//		Issue:			Fractional part of Image Reject controls is not restored.
//		Fix:			A ordering problem (slider before updown) on startup was 
//						causing this issue.  This is now fixed.
//		Reported By:	OH2RZ
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=7408#7408
//
//	Modifications:
//
//		Feature:		Speech Processor (transmit compressor) upgraded.
//		Fix:			Thanks to Phil Harman VK6APH, we have made changes to the
//						speech processor code to enhance performance and eliminate
//						low frequency attenuation.  Please test as we are looking
//						for feedback.  
//		Reported By:	VK6APH
//		Fix Coded By:	N4HY
//		References:		Private conversations between N4HY & VK6APH.
#endregion

#region Beta 1.3.0 Released 04/22/05
//Beta 1.3.0 Released 04/22/05
//
//	Bug Fixes:
//
//		Issue:			Multiple distortion, noise, and performance issues with
//						TX compression
//		Fix:			When we moved the ALC to the end of the transmitter, it
//						revealed an already existing fault in the TX Compressor.
//						The compressor was not thresholded properly and when it
//						did threshold it was set to MinGain rather than MaxGain. 
//						This debugging led to clean up and a reduction in CPU cycles
//						by 50% in this routine.
//		Reported By:	VK6APH, N4HY
//		Fix Coded By:	N4HY
//
//	New Features:
//
//		Feature:		Second sound card support. 
//		Description:	The long awaited dual sound card solution is implemented
//						in this version.  Note that it is necessary to configure
//						details of the second sound card configuration using the
//						new controls on the Audio Tab of the Setup Form.  Select
//						to show details from Card #2 and then fill out the Driver,
//						Input, Output, Mixer, Receive, and Transmit selections.
//						ASIO may only be used for one device, so MME is recommended
//						for Card #2.  The voltage for Card #2 _MUST_ be entered if
//						using the FlexRadio 100W PA in order to control the audio
//						drive correctly.  In order to use the AF control for monitor
//						volume and the PWR control for transmit power, it is necessary
//						to run the Second Sound Card Calibration by pushing the button
//						located directly to the right of the enable checkbox.  Make
//						sure the wiring is correct and the power is on before running
//						this test.  The wiring setup is below:
//
//							Card 1:
//								Line In  -->  SDR-1000 "To Line In"
//								Mic		 -->  No Connection
//								Line Out -->  Y-splitter -- Card 2 Line In & Speakers
//							Card 2:
//								Line In  --> Card 1 Line Out (as above)
//								Mic		 --> SDR-1000 "To Mic In" (or just PC Mic)
//								Line Out --> SDR-1000 "To Line Out"
//
//		Suggested by:	Implied by numerous posts and K5SDR, N4HY
//		Coded by:		KE5DTO, N4HY
//
//		Feature:		M-Audio Delta 44 Support. 
//		Description:	This high performance card is now a supported card offering
//						a really nice advantage.  It allows independant output power
//						and monitor volume.  Use the PWR control for output power
//						and the AF control for monitor volume.  The audio will be
//						muted during transmit if the monitor is not active.  Note that 
//						the output is a whopping 3.8VRMS.  Please use caution when
//						using this with 1W configurations as there is currently
//						nothing keeping you from running the output full blast
//						(configurations with the PA take the Max VRMS into account
//						and are therefore protected).  The inputs and outputs are
//						described below:
//
//							Input 1 & 2: SDR-1000 "To Line In"
//							Input 3: Amplified Microphone (can use 2nd card for preamp)
//							Input 4: Line Out from 2nd card (optional -- for digital modes)
//							
//							Output 1 & 2: Speakers/Monitor (use Y-splitter for digital modes
//								-- Connect to 2nd card Line In)	
//							Output 3 & 4: SDR-1000 "To Line Out"
//
//		Suggested by:	Implied by numerous posts and K5SDR, N4HY
//		Coded by:		KE5DTO, N4HY
//
//		Feature:		Virtual Serial Port
//		Description:	Use the controls on the Test Tab of the Setup Form to setup the
//						virtual serial port.  Note that in order to use the virtual 
//						serial port programming, it is necessary to install the virtual
//						serial port program available at the following address supplied
//						by Bill, KD5TFD:
//						http://ewjt.com/kd5tfd/sdr1k-notebook/catctrl.html
//						Programs verified to work with this beta version are:
//						MixW, DXLabs, Commander, 
//		Coded By:		K5KDN, KD5TFD
//
//		Feature:		Transmit Image/Opposite Sideband Null
//		Description:	With second sound card support, it has become more important 
//						that we allow for nulling the opposite sideband on TX.  We are   
//						legal with almost all sound cards but we have the ability to be  
//						the "best" with this nulling.  The primary limitation is that 
//						there is no second IF and no feedback mechanism.  We are limited  
//						to nulling using a second receiver. An attempt was made to use the
//						high end sound card to null the gain imbalance from the second  
//						(or TX) sound card.  This works just beautifully but it is
//						 not the whole picture. The SDR is a system and this did not 
//						include gain imbalance in the TX hardware (QSE) in the SDR-1000.
//						As such, we found the automatic null using only the sound card
//						was worse than doing nothing on some cards.  The sliders for 
//						this nulling are in the same place as the image rejection
//						sliders for RX on the Setup/DSP panel. There is a new 
//						combination box that allows you to choose between TX and RX
//						nulling.
//		Suggested by:	DK7XL, N4HY, K5SDR, W5GI
//		Coded by:		N4HY, KE5DTO
//
//		Feature:		ThreadSafe Controls
//		Description:	This is more of a "behind-the-scenes" feature, but it is worth noting.
//						Rather than continue to add as much as 8 lines per property of each
//						control we would like to be able to control from any thread, we
//						decided to approach this in a much more object oriented sense and
//						just create custom controls that are in themselves threadsafe.  This
//						solution is not only cleaner (savings of >1000 lines of code
//						immediately), but also a better ongoing solution as we will not have
//						to worry about whether someone has actually checked if their control
//						properties are safe.
//		Coded By:		KE5DTO
//
//	Modifications:
//
//		Feature:		Image Null controls are now floats (were ints).
//		Description:	The Image Null controls were previously limited to integer
//						values and for that reason were missing the optimal null in
//						between	the available control values.  This has been changed
//						so that the DSP will accept floating point numbers and the
//						updown controls send fractional information.  In addition to
//						this, we are rewriting the Automatic Image Null routine to be
//						more consistent and to run faster.
//		Suggested By:	Multiple reports from WA8SRA
//		Coded By:		KE5DTO, N4HY
//
//		Feature:		Display Code Optimization
//		Fix:			Rather than creating and releasing a graphics object each time 
//						through the graphics routine, we now create this object whenever
//						the background bitmap is created.
//		Reported By:	KC9FOL
//		Fix Coded By:	KC9FOL
#endregion

#region 1.2.0 RC2 Released 04/20/05
//1.2.0 RC2 Released 04/20/05
//
//	Bug Fixes:
//
//		Issue:			On first startup, RFE is not enabled in the wizard.
//		Fix:			In the latest wizard cleanup, this was inadvertantly changed
//						and is now returned to default that the RFE is present.
//		Reported By:	N4HY
//		Fix Coded By:	KE5DTO
//
//		Issue:			2m Band database has "holes" in it.
//		Fix:			The frequencies in the range from 144.1 to 144.3MHz now have
//						an appropriate label.
//		Reported By:	W0VB
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=7179#7179
//
//		Issue:			Hitting MOX button when out of band powers the software off.
//		Fix:			While trying to prevent the PTT from putting the software
//						in an unrecoverable state (due to no radio being present),
//						this bug was introduced.  We changed the logic to detect
//						when PTT is being used and now only power off in that case.
//		Reported By:	VK6APH
//		Fix Coded By:	KE5DTO
//		References:		Email: 4/10/2005 7:29AM 
//
//		Issue:			CW keying stability.
//		Fix:			Due to customer feedback, we are reversing a change made to
//						catch elements while in the SPACE state of the keyer machine.
//		Reported By:	W5ZL, KB9YIG
//		Fix Coded By:	KE5DTO
#endregion

#region 1.2.0 RC1 Released 04/08/05
//1.2.0 RC1 Released 04/08/05
//
//	Bug Fixes:
//
//		Issue:			3 board stack (no RFE) will not calibrate correctly.
//		Fix:			There were two problems causing this issue.  First, the
//						rfe_present variable was not being set correctly if only
//						the 3 boards were present.  Second, the calibration routine
//						was not calculating the offset correctly given that the
//						attenuator is not present on the 3 board stack.  Both of
//						these issues are now fixed.
//		Reported By:	VK6APH
//		Fix Coded By:	KE5DTO
//		References:		Email: 4/8/2005 8:27AM 
//
//		Issue:			CW misses dashes if dots are held and dash is tapped.
//		Fix:			A state was not accounted for in the CW state machine 
//						that creates the output samples.  This has been addressed
//						and the tapped inputs are now caught.
//		Reported By:	AB2KT
//		Fix Coded By:	AB2KT
//
//		Issue:			Transmit and Receive Overlap Save filter not completely
//						initialized.
//		Fix:			Flushed residual contents of the overlap save buffers 
//						when transitioning between receive and transmit (and
//						vice versa).
//		Reported By:	N4HY, AB2KT
//		Fix Coded By:	N4HY, AB2KT
//
//		Issue:			DC Filter not being reset during rx<->tx transition.
//		Fix:			When transitioning between transmit to receive, the DC
//						offset removal is no longer called.  This was causing
//						zeroed buffers to become non-zero during the transition.
//		Reported By:	N4HY, AB2KT
//		Fix Coded By:	N4HY, AB2KT	
//
//		Issue:			Incremental steps for NR/ANF are too large (100).
//		Fix:			Marino was kind enough to point out that our default
//						step sizes for the NR/ANF controls were too large.  This
//						is now fixed and all of these controls now step +/-1.
//		Reported By:	I2KBO
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=7064#7064
//
//		Issue:			Import Database from Wizard does not load PA Gain by Band
//						settings.
//		Fix:			The wizard had a flow control problem in which the values
//						would be read into the program from the database and then
//						would be overwritten by the wizard when pressing the 
//						Finish button.  This has been fixed.  The wizard now closes
//						when a database is successfully imported.
//		Reported By:	W5ZL
//		Fix Coded By:	KE5DTO
//
//		Issue:			Tune Power gets set high whenever trying to TUN out of band.
//		Fix:			When the TUN function was used out of band, the failure of
//						the MOX handler caused the current output power to be saved
//						in the Tune Power.  We now change the power level as soon
//						as the TUN button is hit.
//		Reported By:	K2WS
//		Fix Coded By:	KE5DTO
//		References:		Phone Call 04/01/05
//
//		Issue:			No min/max for VFO B.
//		Fix:			Setting the minimum and maximum for VFO A was implemented
//						after copying the code for VFO B.  Thanks to an observant
//						user/source code user, this is now fixed.
//		Reported By:	DG8CAA
//		Fix Coded By:	DG8CAA
//		References:		Private Msg: 04/01/05 Subject: Re: Beta 1.1.9 tune frequency
//
//		Issue:			RX buffers getting into TX and vice versa.
//		Fix:			We have added a state machine to the audio system in order
//						to minimize the effects of audio buffers crossing from
//						TX to RX (or vice versa).  While this does not completely
//						eliminate the problem for systems with ASIO4ALL, it improves
//						the situation dramatically on SSB and is at least as good
//						as before on CW.  The eventual solution will be realized
//						only after we rewrite the hardware code and do away with
//						ASIO4ALL (likely with our own WDM-KS interface).
//		Fix Coded By:	N4HY
//
//		Issue:			NR and ANF default values sound "watery."
//		Fix:			The defaults were set for the complex version of these
//						functions which are not yet complete.  The defaults are
//						now 65/50/50 for Taps/Delay/Gain.
//		Reported by:	WO0Z, WA8SRA
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6856#6856
//						http://www.flex-radio.com/forum/viewtopic.php?p=6696#6696
//
//		Issue:			Display average is not initialized correctly when buffer
//						sizes other than 2048 are used.
//		Fix:			This comes as a result of using the buffer size specified
//						rather than 2048 which is hard coded into the DSP for now.
//						The calculation is now fixed.
//		Reported by:	WA8SRA
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6696#6696
//
//		Issue:			ATU Present on startup without PA Present checked.
//		Fix:			This was an oversight and is now fixed.
//		Fix Coded By:	KE5DTO
//
//	Modifications:
//
//		Feature:		AGC attack lengthened to 3ms.
//		Description:	We have lengthened the AGC attack time in order to
//						smooth the onset of large signals.  
//		Reported By:	N4HY, AB2KT
//		Fix Coded By:	N4HY, AB2KT
//
//		Feature:		ATU power out is sometimes too low.
//		Description:	Using PWR = 10 for the ATU tuning will sometimes cause
//						the tune to fail due to low power on higher loads.  We
//						have adjusted the routine to use PWR = 16 to fix this
//						issue.
//		Suggested By:	K5SDR, K5BOT
//		Coded By:		KE5DTO
//
//		Feature:		Default Line In Gain set for some cards.
//		Description:	The Line In Gain is now defaulted to the ideal position
//						for the Santa Cruz and the MP3+.  The spectrum display and
//						multimeter should be fairly close upon first running the
//						console when using these cards (without importing the
//						database).
//		Coded By:		KE5DTO
//
//		Feature:		Added USB Adapter and ATU to the wizard options.
//		Description:	The newly released USB to Parallel adapter as well as the
//						LDG Z-100 Antenna Tuning Unit are now in the wizard and
//						can be configured just like the RFE and PA options.
//		Suggested By:	N9VV
//		Coded By:		KE5DTO
#endregion

#region Beta 1.1.9 Released 03/29/05
//Beta 1.1.9 Released 03/29/05
//
//	Bug Fixes:
//
//		Issue:			Audio, XVTR and other settings not being retrieved
//						correctly from the database.
//		Fix:			This was the result of a late addition to Beta 1.1.8
//						involving a new initialization routine.  We have fixed
//						this problem.  This was also causing the issue resulting
//						in a reference to the USB dll.  Having this dll is no
//						longer necessary unless actually using the USB adapter.
//		Reported by:	N9DG, K3TL, WA2N, AA5XE, N8VB, NJ1H, K3IB, AA8YI, W0VB
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6763#6763
//						http://www.flex-radio.com/forum/viewtopic.php?p=6766#6766
//						http://www.flex-radio.com/forum/viewtopic.php?p=6776#6776
//						http://www.flex-radio.com/forum/viewtopic.php?p=6800#6800
//						http://www.flex-radio.com/forum/viewtopic.php?p=6806#6806
//
//		Issue:			New snap-to 1kHz tuning gets stuck on 1.001MHz.
//		Fix:			This problem came as a result of floating point errors
//						when rounding small numbers.  An additional round operation
//						keeps this from happening now.
//		Reported by:	EB4APL
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6802#6802
//
//		Issue:			Voltage spike on TX -> RX transition.
//		Fix:			Due to an ordering issue and old code (3 board stack days),
//						there was a problem with a voltage spike getting into the
//						RX audio path.  This has been mitigated using a different
//						relay switch ordering when coming back from transmit into
//						receive mode.
//		Reported by:	N9VV, KB9YIG, N4HY
//		Fix Coded By:	KE5DTO (aided by K5SDR, K5BOT)
//
//		Issue:			Preamp menu selection has duplicate values.
//		Fix:			This was caused an initialization problem that was resolved
//						in the first note above.  However, further precautions have
//						been taken to ensure the integrity of the preamp and 
//						multimeter selection menus.
//		Reported by:	NJ1H, NG6B, N4HY
//		Fix Coded By:	KE5DTO (aided by K5SDR, K5BOT)
//
//		Issue:			Band buttons call ATU Bypass multiple times.
//		Fix:			This was an issue with Event handlers calling each other
//						multiple times.  This was fixed with a flag so the ATU
//						is only sent the bypass command once.
//		Reported by:	K5SDR, K5BOT
//		Fix Coded By:	KE5DTO
//
//	Modifications:
//
//		Feature:		Default PA Gain set to 48.0dB.
//		Description:	Previously the default was 50.2dB, but this low value was
//						causing some people to have trouble calibrating with sound
//						cards that have lower output on 160m and 80m bands.
//		Suggested By:	K5SDR
//		Coded By:		KE5DTO
#endregion

#region Beta 1.1.8 Released 03/25/05
//Beta 1.1.8 Released 03/25/05
//
//	Bug Fixes:
//
//		Issue:			DRM display is wrong on Histogram and Waterfall.
//		Fix:			The offset that put the display up 10kHz is now
//						fixed and centered around zero.
//		Reported by:	N4HY
//		Fix Coded By:	KE5DTO
//
//		Issue:			Analog multimeter shows nothing on 1W configs.
//		Fix:			The 1W TX meter is now aligned correctly with
//						the analog "bar graph" meter.
//		Reported by:	VK6APH
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6668#6668
//
//		Issue:			Changing Setup controls while in MOX can cause problems.
//		Fix:			While this is not customary, we have taken precautions to
//						ensure that controls that would affect transmit are disabled
//						whenever MOX is active.
//		Reported by:	N9VV
//		Fix Coded By:	KE5DTO
//		References:		Email: Subject: 1.1.5 bug? 03/05/05 5:21PM
//
//		Issue:			SWR Protection does not work in CW mode.
//		Fix:			This was actually a larger problem.  The only part of the SWR
//						protection algorithm that was working was the visible text
//						when in non-CW modes.  For testing purposes, we added the 
//						ability to disable the SWR protection.  When this was added,
//						a variable was introduced that effectively disabled SWR
//						protection until this test was run.  This is now fixed.
//		Reported by:	AC5OG
//		Fix Coded By:	KE5DTO
//
//		Issue:			Unsupported Card mixers do not get initialized.
//		Fix:			In the process of adding all of the specialized initialization
//						routines for the supported cards, the code that setup
//						unsupported cards got lost in the shuffle.  There is now
//						a standard set of Mixer setups that will run on startup if
//						Unsupported Card is selected and the selected mixer is valid.
//						These settings are:
//							Main Volume = Max
//							Main Mute = false
//							Wave Out Volume = Max
//							Wave Out Mute = false
//							Line In Mute = true
//							Microphone Mute = true
//		Reported by:	AC5OG
//		Fix Coded By:	KE5DTO
//
//		Issue:			Level cal and Image Null cal only work if Preamp is set
//						to Med.
//		Fix:			There was a bug in the calibration algorithm using a 
//						while loop that should have been a do/while.  This has
//						been fixed, and the Level cal has been improved to
//						detect exact gain differences when the 26dB gain and/or
//						attentuator is active.  This should give the multimeter
//						accuracy down to +/- 0.1dB with respect to the source.
//		Reported by:	WA8SRA, VK6APH
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6547#6547
//
//		Issue:			ALC meter shows negative numbers.
//		Fix:			Several meter modes including ALC, Peak Pow, and Fwd Pow
//						(w/o PA) are not valid during CW or TUN modes.  They will
//						now read 0 when this is the case (rather than numbers that
//						do not make sense).
//		Reported by:	KC2LFI
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6565#6565
//
//		Issue:			ALC meter shows negative numbers.
//		Fix:			Several meter modes including ALC, Peak Pow, and Fwd Pow
//						(w/o PA) are not valid during CW or TUN modes.  They will
//						now read 0 when this is the case (rather than numbers that
//						do not make sense).
//		Reported by:	KC2LFI
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6565#6565
//
//		Issue:			CW memory buffer carrying over from last transmit.
//		Fix:			The buffers are now cleared whenever the Send button is
//						depressed.
//		Reported by:	WO0Z
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6589#6589
//
//		Issue:			Slow display during busy signal activity.
//		Fix:			A bug in the display calculation was causing the display
//						thread to take longer than expected.  This resulted in
//						slower refreshes due to its lower priority.  The calculation
//						is fixed and now the screen display is more consistent.					
//		Reported by:	VK6APH
//		Fix Coded By:	N4HY
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6546#6546
//
//		Issue:			Panadapter display looks noisy.
//		Fix:			The optimized display code did not take into account cases
//						where wide filters would have multiple bins per pixel.  In
//						these cases, we need to take the peak bin for the display.
//						This is fixed and the display appears as it used to.					
//		Reported by:	DJ9CS
//		Fix Coded By:	N4HY
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6566#6566
//
//	Modifications:
//
//		Feature:		Mouse Wheel Tuning @ 1kHz snaps to nearest kHz.
//		Description:	As requested, we have modified the interface so that when
//						using the mouse wheel to tune with 1kHz steps, the first
//						step will snap to the nearest kHz frequency.  Note that this
//						does not apply to hover tuning over the VFO.
//		Suggested By:	WA8SRA, WO0Z
//		Coded By:		KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6433#6433
//
//	New Features:
//
//		Feature:		Receive Only Option. 
//		Description:	As requested by many people, we bring to you a receive only
//						checkbox accessible through the Setup Form -> General Tab.
//		Suggested by:	AA4SW
//		Coded by:		KE5DTO
//		References:		Teamspeak forum.
#endregion

#region Beta 1.1.7 Released 03/18/05
//Beta 1.1.7 Released 03/18/05
//
//	Bug Fixes:
//
//		Issue:			Mixer settings change on startup.
//		Fix:			Even when NONE was selected as the mixer, the onboard
//						sound card mixer settings or other random mixer settings
//						would change but mostly microphone would be moved to
//						line in.  The incredible mess in Windows Mixer ID's
//						has been sorted out and this should no longer happen
//						when reading a database that once had sound cards
//						with mixers or different mixers from the primary system
//						sound card.
//		Reported by:	N4HY
//		Fix Coded By:	KE5DTO
//
//		Issue:			Performance issues with display.
//		Fix:			Several instances of buffers being allocated within the
//						drawing loop were eliminated.  This should drop the CPU
//						load and yield a more responsive display.
//		Reported by:	N4HY
//		Fix Coded By:	KE5DTO
//
//		Issue:			CW Crashing.
//		Fix:			In going from interleaved to non-interleaved sample
//						buffers, there was a memory error that was causing
//						a pointer to run past it's allocated memory.  This
//						is now fixed. 
//		Reported by:	KC2LFI, SM6OMH, VE6IV, K5SDR, N9VV, AA5XE, WO0Z, KD5TFD
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6292#6292
//
//		Issue:			CW missing/clipped first dot or dash.
//		Fix:			This is a classic case of retrogression of code.  The code
//						that we implemented to prevent this was commented out. It
//						is back in commission now.
//		Reported by:	VE6IV, WO0Z
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4973#4973
//
//	Modifications:
//
//		Feature:		Preamp Controls.
//		Description:	Due to customer feedback, we have decided to eliminate
//						the Gain and ATT controls from the console.  We are 
//						replacing these controls with one called Preamp that is
//						closer to what hams are used to seeing on traditional
//						rigs.  There are 4 settings as described below.  Note
//						that without the RFE board, only the Med and High options
//						are available (with high using 40dB gain).  Also note that
//						the High and Low options are not available when in the
//						2m band.
//
//						Setting		ATT		Gain
//						Off			On		0dB
//						Low			Off		0dB
//						Med			On		26dB
//						High		Off		26dB 
//						
//		Suggested By:	K5SDR, W5GI
//		Coded By:		KE5DTO
//
//		Issue:			DSP wasted computation for Power Spectrum
//		Description:	A legacy holdover from the very first alpha 0.0.1 console
//						was a 32768 sample buffer for spectral display.  This
//						buffer was interpreted by the console for the power
//						spectrum,  scope, panadapter, etc.  The console now
//						takes the power spectrum fft output buffer, irrespective
//						of size and interpolates to produce smooth data for
//						display.  This will soon enable users to set an FFT
//						size to match their preference given system performance.
//		Suggested by:	N4HY
//		Coded by:		N4HY
//
//	New Features:
//
//		Feature:		Power Readings from DSP. 
//		Description:	Both RMS average power as well as peak power is determined
//						in each buffer and returned for display by the console.
//						With a calibrated amplifier, this will likely be more
//						accurate than the hardware power meter.  The peak power
//						will show the peak voltage hits the value required to
//						drive the amplifier or transverter or the OPA (QRP settings)
//						to full power.
//		Suggested by:	Implied by numerous posts and K5SDR, N4HY
//		Coded by:		KE5DTO, N4HY
#endregion

#region Beta 1.1.6 Released 03/14/05
//Beta 1.1.6 Released 03/14/05
//
//	Bug Fixes:
//
//		Issue:			Crashing/Sluggishness/Responsiveness.
//		Fix:			Some sluggishness in responsiveness of console and some 
//						anomalies noticed when making changes or rapid TX<->RX 
//						transitions.   When we were fixing the threading issues,  
//						we disabled the protection for the dsp chain from updates.
//						These have been enabled again and now the dsp and updates 
//						to the dsp do not.
//		Fix Coded By:	N4HY
//
//		Issue:			Low power output/distorted output.
//		Fix:			The transmit audio chain had several design/code bugs  
//						which introduced distortion and other anomalies which  
//						rendered power control useless, and allowed peak values 
//						that drove the amplifier into distortion.  A new usage 
//						of the agc system to provide a soft limiter in the dsp 
//						chain.  This moves digitalagc from do_tx_pre to the first 
//						thing in do_tx_post and sets the the maximum gain to 0dB  
//						and the minimum gain to -10 dB.  This should prevent 
//						overdrive of the amplifier.
//		Suggested By:	AC5OG, KE5DTO, N4HY
//		Fix Coded By:	N4HY
//
//		Issue:			Changing frequency in TX while in AM mode makes
//						the transmitter lose the frequency.
//		Fix:			This was a bug related to the fact that we use
//						an 11kHz IF when transmitting AM/FM.  We have
//						corrected this issue and tuning while in those
//						modes, while not recommended, will work as
//						expected.
//		Reported By:	W9AD
//		Fix Coded By:	KE5DTO
//		Reference:		Phone Call 3/10/05 Afternoon
//
//		Issue:			TUN does does not work while CW form is open.
//		Fix:			There were conflicting settings when the CW Form
//						was open and TUN was turned on.  This has been
//						fixed and TUN now works even if the form is open.
//		Reported By:	AC5OG
//		Fix Coded By:	KE5DTO
//
//		Issue:			Scanner gets stuck on if you power off while it is
//						running.
//		Fix:			The scanner is now stopped if the power button is
//						depressed while it is running.
//		Reported By:	AC5OG
//		Fix Coded By:	KE5DTO
//
//		Issue:			TX AGC Correction.
//		Fix:			When changing from transmit to receive, the digital agc
//						is set to fast response to minimize the capture of the
//						agc by the pulse.  The need for this will soon be
//						eliminated.
//		Suggested By:	VK6APH
//		Fix Coded By:	VK6APH (adapted by N4HY)
//
//		Issue:			Pops on transition
//		Fix:			A nasty bug in the transition from RX->TX and back was
//						introduced while we were attempting to mitigate other
//						transition anomalies.  It made the problem worse!
//		Reported by:	MANY
//		Fix Coded By:	N4HY
//
//		Issue:			PA ALC controls are visible when PA is not enabled.
//		Fix:			This oversight is now fixed.
//		Reported By:	VK6APH
//		Fix Coded By:	KE5DTO
//		References:		Email: 02/11/05 Subject: ALC
//
//		Issue:			Longer Delay in the Audio buffering
//		Fix:			When diagnosing the threading problems, to protect the
//						ring buffers, an increase delay/latency was introduced.
//						Not only is it no longer needed, it is in fact detrimental
//						to ring buffer performance under load.
//		Reported by:	WA8SRA
//		Fix Coded By:	N4HY
//		References:		http://www.flex-radio.com/forum/viewtopic.php?t=1339
//
//		Issue:			Receive (and Transmit) Filters have different gains.
//		Fix:			In early days when Phil Harmann, VK6APH reported this, the
//						filters were normalized.  Regression occurred and this
//						normalization disappeared. Phil reported RX filters,
//						N4HY determined it was true with TX filters as well.
//		Reported By:	VK6APH
//		Fix Coded By:	AB2KT and N4HY
//		References:		http://www.flex-radio.com/forum/viewtopic.php?t=630
//
//	Modifications:
//
//		Feature:		Tune Power.
//		Description:	Due to customer feedback, we have added a control that
//						sets the tune power on the Setup Form, Transmit Tab.
//						When using the TUN button on the front panel, it will
//						start at this new value.  Any changes to the PWR control
//						will be saved in the new control when the TUN button
//						is turned off.
//		Suggested By:	WO0Z
//		Coded By:		KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6201#6201
//
//		Issue:			Calibration Power Point.
//		Description:	Due to customer feedback, we have added a control that
//						allows the user to set the tune power target for the PA
//						calibration procedure.  This control can be used to
//						calibrate to a power lower than 75 watt.  Note that 
//						calibrating at a lower power level will likely give a
//						less accurate calibration.
//		Suggested By:	WO0Z
//		Coded By:		KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6201#6201
//
//		Issue:			Analog multimeter peak hold time.
//		Description:	There is now a control in the Display Tab on the Setup
//						Form that allows the user to set the peak hold time for
//						the analog meter.
//		Suggested By:	AC5OG
//		Coded By:		KE5DTO
//
//		Issue:			Digital multimeter text peak hold time.
//		Description:	There are now controls in the Display Tab on the Setup
//						Form that allows the user to set the peak hold time for
//						the digital meter text.
//		Suggested By:	AC5OG
//		Coded By:		KE5DTO
//
//		Issue:			ASIO4ALL v1.8 gives best results.
//		Description:	After much testing, we have concluded that ASIO4ALL gives
//						the best results when using v1.8 (as opposed to the more
//						recent v2.2, 2.3 and 2.4).  Until further notice please 
//						use v1.8.  The software will now look for v1.8 and use it
//						if it is installed (before it would use the latest version).
//		Coded By:		KE5DTO
//
//	New Features:
//
//		Feature:		Update Notifications (work in progress).
//		Description:	The software will now attempt to connect to our website
//						and check a text file to see if a newer release is out.
//						If no internet is present (or if a firewall blocks the
//						check), this step is simply skipped.
//		Coded By:		KE5DTO
//
//		Feature:		ALC Meter
//		Description:	The TX meter now has an option to view the ALC in dB of
//						compression.  This meter can be used in conjunction with
//						the MIC control on the front panel and the new Pre-Gain
//						control on the Transmit Tab of the Setup Form to set your
//						input correctly for microphone and/or digital modes.
//		Coded By:		N4HY
#endregion

#region Beta 1.1.5 Released 03/01/05
//Beta 1.1.5 Released 03/01/05
//
//	Bug Fixes:
//
//		Issue:			TX output level is low.
//		Fix:			The fix for the AM/FMN TX problem introduced lower signal
//						levels on the order of 1/Sqrt(2).  This has been adjusted
//						to be consistent with previous versions.
//		Fix Coded By:	N4HY
//
//		Issue:			Ring buffer data is not protected.
//		Fix:			We have added protection to the ring buffer so that data
//						can only be read out in 4 byte floating point increments.
//						Other protections are also now in place.
//		Fix Coded By:	N4HY
//
//		Issue:			White noise, instability, etc.
//		Fix:			A serious threading issue that has been present since
//						the introduction of the new DSP was found and eliminated
//						in this version.  Thanks to everyone who confirmed,
//						reported, and helped debug this issue.
//		Reported By:	Nearly everyone.  :)
//		Fix Coded By:	N4HY
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6118#6118
//
//		Issue:			Trying to playback from a wave file marked as read
//						only causes the console to crash.
//		Fix:			The wave file was originally being opened in read/write
//						mode.  This is now changed to only be read mode.
//		Reported By:	W0VB, N4HY
//		Fix Coded By:	KE5DTO
//
//		Issue:			After each time tuning the ATU, an error message appears.
//		Fix:			A logic error in the tuning algorithm was causing the
//						error.  This is now fixed.
//		Reported By:	K5SDR
//		Fix Coded By:	KE5DTO
//
//		Issue:			Multimeter peak lingers over RX/TX transition.
//		Fix:			The peak is now reset when MOX is pressed.
//		Fix Coded By:	KE5DTO
//
//		Issue:			When in SPEC or DRM modes, changing the filter is
//						possible using the keyboard shortcuts.
//		Fix:			This was simply an oversight.  The keyboard shortcuts now
//						do nothing if the filter is set to Filter.NONE (as it is
//						in those modes).
//		Reported By:	VK3AAW
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6051#6051
//
//		Issue:			When in SPEC or DRM modes, copying from VFO A to B
//						causes a filter to be set when copying back.
//		Fix:			This was a problem with setting the Filter to Filter.NONE
//						when in these modes.  We have corrected this issue and
//						now force the filter to Filter.NONE when in SPEC or DRM
//						modes.
//		Reported By:	WA2N
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6049#6049
//
//		Issue:			ATT is left on when going to 2m (even though the control
//						is disabled.
//		Fix:			We have corrected this to turn off the ATT when switching
//						to 2m.
//		Reported By:	K7MDL
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6002#6002
//
//		Issue:			Using the PowerMate (or Ctrl + Arrow keys), the step
//						frequency sometimes does not show 10MHz as it should.
//		Fix:			This was a bug.  When 10MHz was added as a step option,
//						it was not added everywhere it needed to be.  This is
//						now fixed.
//		Reported By:	VK3AAW
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6007#6007
//
//		Issue:			CW Speed (WPM) is not being restored on startup.
//		Fix:			This was simply a this before that issue with the startup
//						initialization.  Once the ordering was corrected, this
//						problem went away.
//		Reported By:	VE6IV
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=6062#6062
//
//	Modifications:
//
//		Feature:		TX bandwidth limit was changed to 20kHz from 3.5kHz.
//		Description:	This change did not come about without much thought and
//						planning.  As pointed out to us though, the responsibility
//						for good transmitting practice is in the hands of the
//						control operator.  We encourage those who use these new
//						limits to do so with adherence to the warning and to use
//						good operating practice.
//		Suggested By:	W9AD
//		Coded By:		KE5DTO
//		References:		Email 2/23/05 9:20a Subject: Re: My order
//
//		Feature:		Filter Shift is no longer reset when changing filters.
//		Description:	After viewing the poll results on this issue, the Filter
//						Shift control is now only reset when changing modes (not
//						when changing filters).
//		Suggested By:	SM6OMH, VK6APH 
//		Coded By:		KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?t=1281
//
//	New Features:
//
//		Feature:		FFTW Wisdom.
//		Description:	FFTW has a function where it will run test FFTs on your
//						machine in order to optimize itself to run the fastest
//						methods given your operating environment.  In order to
//						use this feature, simply run the fftw_wisdom.exe file
//						round in the PowerSDR application folder.  Make sure
//						to run it while using your machine as you usually would
//						when running the radio software (but do not actually
//						run PowerSDR while running the test).
//		Coded By:		FFTW & N4HY
//
//		Feature:		USB Adapter.
//		Description:	This version has been completely tested with our USB to
//						parallel adapter.  This includes testing on the RFE, the
//						XVTR, PA, and ATU.  Public release will follow this
//						release in the near future.
//		Coded By:		OH2BFO (integrated by KE5DTO).
#endregion

#region Beta 1.1.4 Released 02/18/05
//Beta 1.1.4 Released 02/18/05
//		
//	Bug Fixes:
//
//		Issue:			MP3+ Supported card selection causes problems due
//						to ASIO4ALL v2.2 issues.
//		Fix:			Now that the implications of the ASIO4ALL v2.2 problems
//						with the MP3+ are completely understood, we have changed
//						the software to look for either ASIO4ALL v1.8 & the
//						creative drivers ("Sound Blaster") -OR- ASIO4ALL v2.x &
//						the windows drivers ("USB Audio").  This will enable
//						either configuration to work with the supported card
//						MP3+ selection.
//		Reported By:	DJ9CS, DL2JA, N8VB
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=5356#5356
//
//		Issue:			When restoring XIT and RIT after being left on, the
//						background color is not set appropriately.
//		Fix:			The event handlers were set for the Click event rather
//						than the CheckedChanged event as is customary for checkbox
//						type controls.  Once corrected, the background works as
//						it should.
//		Reported By:	KD5TFD, EB4APL
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=5916#5916
//						http://www.flex-radio.com/forum/viewtopic.php?p=5865#5865
//
//		Issue:			CW Max WPM limit is 15 when the buffers are set to 256.
//		Fix:			This was an oversight as the 256 size buffers were a new
//						addition.  This has been fixed.
//		Reported By:	N9VV
//		Fix Coded By:	KE5DTO
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=5888#5888
//
//		Issue:			Reset All button on the Setup Form gives an error when
//						pressed.
//		Fix:			When changing the fixed gain and max gain from scalar 
//						values to dB, the reset values were not changed.  This
//						has been addressed and the button now operates as intended.
//		Fix Coded By:	KE5DTO
//
//		Issue:			AM and FM transmit do not work or are distorted.
//		Fix:			The microphone audio was left as complex in stereo and not
//						made a real signal in the software.
//		Reported By:	N8VB, K3XF
//		Fix Coded By:	N4HY
//		References:		http://www.flex-radio.com/forum/viewtopic.php?t=1280
//						http://www.flex-radio.com/forum/viewtopic.php?t=1111
//						http://www.flex-radio.com/forum/viewtopic.php?t=1280
#endregion

#region Beta 1.1.3 Released 02/10/05
//Beta 1.1.3 Released 02/10/05
//
//	New Features:
//	
//		Feature:		Automatic Level Control (for PA Only).
//		Description:	We have included a very early Beta form of an ALC system
//						we are working on for the PA.  Enable it using the controls
//						on the Setup Form -> PA Settings tab.  Feedback is
//						appreciated.
//		Coded By:		Eric Wachsmann KE5DTO
//		
//	Bug Fixes:
//
//		Issue:			Squelch level does not match signal.
//		Fix:			Squelch being computed in the wrong place in the signal
//						chain.  Moved to the correct place.
//		Reported By:	K7RSB
//		Fix Coded By:	N4HY
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4889#4889
//
//		Issue:			Changing the TX Filter High Cut while in TX did
//						not update the display.
//		Fix:			This was a simple oversight.  It is now fixed.
//		Reported By:	N4HY
//		Fix Coded By:	Eric Wachsmann
//
//
//		Issue:			CW feels delayed/does not sound right.
//		Fix:			This problem is related to the changes we made to the
//						audio latency.  For this reason, we have added a new
//						control on the Setup->Audio form to control the latency.
//						Use the manual option to use something other than the
//						0ms that was customary until the last release (Beta 1.1.2).
//		Reported By:	N9VV, VE3MM, K3IB
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=5608#5608
//
//	Modifications:
//
//		Feature:		TX EQ now has 12 bands.
//		Description:	We added 3 new bands to the TX EQ: 80Hz, 160Hz, and
//						3.5kHz.  At the same time we evaluated them to be sure
//						they were centered at their respective frequencies.
//		Suggested By:	W5GI
//		Coded By:		Eric Wachsmann & N4HY
#endregion

#region Beta 1.1.2 Released 02/02/05
//Beta 1.1.2 Released 02/02/05
//
//	Bug Fixes:
//
//		Issue:			TxFC will not turn off after being turned on.
//		Fix:			This was a simple fix in the DSP code.  A passed
//						parameter was not being used in a function call.
//		Reported By:	WA8SRA
//		Fix Coded By:	Bob McGwier N4HY
//		References:		Phone call on 1/31/05 @ 2PM.
//
//		Issue:			ATU stays engaged after changing bands.
//		Fix:			This was an oversight.  The ATU is now put in Bypass mode
//						if you change bands after tuning.
//		Fix Coded By:	Eric Wachsmann
//
//	Modifications:
//
//		Feature:		Display Average control now uses millisecond values.
//		Description:	Before the control allowed the user to specify over how
//						many samples they would like to average.  This is a better
//						way of expressing the average.  Note that the ms value is
//						rounded to the nearest buffer quantization (41.6ms for 2048
//						sample buffers).
//		Coded By:		Eric Wachsmann
#endregion

#region Beta 1.1.1 Released 01/21/05
//Beta 1.1.1 Released 01/21/05
//
//	Bug Fixes:
//
//		Issue:			Mixer Mux switches to the wrong setting on TX.
//		Fix:			This was a simple typo.  MixerMuxTXID1 referenced the 
//						wrong local variable.  This is fixed.
//		Reported By:	KD5TFD, DK7XL
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=5319#5319
//
//		Issue:			Using ASIO4ALL v1.8 gives a PortAudio Invalid IO error.
//		Fix:			This was another typo.  AudioInput1 referenced the 
//						wrong local variable.  This is fixed.
//		Reported By:	KC9FOL, WA2N
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=5318#5318
//
//		Issue:			Channel scanning with the first entry marked Scan = false
//						locks up Windows XP.
//		Fix:			This was a case of poor logic.  The condition above 
//						caused the code to get into a tight infinite loop.  This
//						is now fixed.
//		Reported By:	KC9FOL, WA2N
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=5318#5318
//
//	New Features:
//
//		Feature:		New DSP Code!
//		Description:	The code Bob McGwier (N4HY) and Frank Brickle (AB2KT) have
//						working on has come to fruition in this version.  There is
//						one audio bug we are still working on, so bug reports are
//						valued.  The bug seems to present itself only on buffer
//						sizes smaller than 2048 samples.
//		Coded By:		N4HY, AB2KT
//
//	Modifications:
//
//		Feature:		Image Reject Algorithm Improvement.
//		Description:	The Image Reject routine used to scan the whole passband
//						on each setting looking for the peak signal in order to
//						find a global minimum.  This has been simplified to
//						finding the peak initially, and using that same frequency
//						(bin) to find a minimum.
//		Coded By:		Eric Wachsmann
#endregion

#region Beta 1.1.0 Released 01/14/05
//Beta 1.1.0 Released 01/14/05
//
//	**New Features
//
//		Feature:		ATU Support.
//		Description:	The PowerSDR software now supports the LDG Z-100
//						Automatic Antenna Tuning unit.  The front panel button
//						works with a switch to offer Bypass, Memory, and Full
//						tuning modes.  Note that there is still a hardware
//						component to be completed before this will go live.
//		Coded By:		Eric Wachsmann w/debug help from AC5OG, K5BOT
//
//		Feature:		2nd Sound Card Pipe Support (work in progress).
//		Description:	This is the first stage in supporting 2 sound card setups
//						as discussed at length on the forums and in TS.  This
//						first step allows software piping through a second sound
//						card for transmit filtering.
//		Coded By:		Eric Wachsmann
#endregion

#region 1.0.6 Released 02/02/05
//1.0.6 Released 02/02/05
//
//	Bug Fixes:
//
//		Issue:			WWV and 60m band buttons do not save changes to mode/filter.
//		Description:	While being careful not to change the frequencies, changes
//						to mode and filter are now saved to WWV (filter changes only
//						to 60m).
//		Coded By:		Eric Wachsmann
//		Reference:		TS Town Hall meeting 1/29/05.
//
//		Issue:			Keyboard Filter up/down does not wrap properly.
//		Fix:			When this function was rewritten recently, two values
//						were reversed.  This has been corrected.
//		Reported By:	WB3IAL
//		Fix Coded By:	Eric Wachsmann
//		References:		Email 1/30/05 Subject: 1.0.5 Console
//
//		Issue:			CW Break-In value is not saved/restored.
//		Fix:			The value was being set to false before saving the
//						databse to avoid a threading issue.  The threading issue
//						has since been corrected and the Break-In value is now
//						saved in the database.
//		Reported By:	W0VB
//		Fix Coded By:	Eric Wachsmann
//		References:		AIM comments 1/24/05
//
//		Issue:			CW sometimes misses the first dit or dah.
//		Fix:			While coding for the new feature listed below, a new
//						idea for a method of not losing the first touch on transmit
//						was implemented and tested.
//		Reported By:	N4HY, AC5OG, W5ZL
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			TX freq is wrong if XIT is turned on when switching MOX
//						on.
//		Fix:			There was one piece of code that was overlooked when 
//						converting the XIT to Hz from kHz.  This has been
//						resolved.
//		Reported By:	KD5TFD
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=5240#5240
//
//		Issue:			FMN and AM do not work on transmit.
//		Fix:			Filters and IF offsets had gotten confused over time
//						and were not in agreement.  They are now working as intended.
//		Reported By:	K3XF
//		Fix Coded By:	Bob McGwier
//		References:		Email 1/24/05 4:45PM Subject: RE: forum access
//
//		Issue:			CPU loading unusually high.
//		Fix:			We found that the default latency for ASIO on portaudio
//						was using extremely small buffers.  This was causing
//						all kinds of issues including overflow/underflow errors
//						as well as overloading the CPU unnecessarily.  This was
//						corrected to use a more reasonable value (0.043s).
//		Reported By:	N4HY
//		Fix Coded By:	Bob McGwier
//
//	New Features:
//
//		Feature:		Joystick mode for CW.
//		Description:	As discussed extensively in the forums, we know have a 
//						working CW joystick interface thanks to Bob, K5KDN.
//		Coded By:		Paddle interface by K5KDN, Interface mods by Eric Wachsmann
//		Reference:		http://www.flex-radio.com/forum/viewtopic.php?p=5402#5402
//
//	Modifications:
//
//		Feature:		CW Manual Send.
//		Description:	Similar to the way MOX works on the front panel, you can
//						now use Break-In and still be able to do a manual send
//						(i.e. Break In will not immediately unkey the radio if
//						you click the button rather than using the input mode).
//		Coded By:		Eric Wachsmann
//
//		Issue:			CW Break-In Delay is now in ms for all modes.
//		Description:	Previously, the Break-In delay was in ms only for Ext Key
//						mode and was in 100ms values otherwise.  Now they are all
//						measured in ms (e.g. a value of 4 would be 4ms rather
//						than 400ms).  Using a value larger than 10 is recommended.
//		Coded By:		Eric Wachsmann
#endregion

#region 1.0.5 Released 01/17/05
//Beta 1.0.5 Released 01/17/05
//
//	Bug Fixes:
//
//		Issue:			PA Calibration sometimes overruns ADC.
//		Fix:			We now reset the gain for all bands to 50.2dB before
//						running the calibration routine.
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			RFE relays switch randomly after adding ATU support.
//		Fix:			Bugs were found in the hardware code that did not
//						affect operation until the ATU was installed.  These
//						bugs have been fixed.
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			TX freq is wrong if XIT is turned on when switching MOX
//						on.
//		Fix:			There was one piece of code that was overlooked when 
//						converting the XIT to Hz from kHz.  This has been
//						resolved.
//		Reported By:	KD5TFD
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=5240#5240
//
//		Issue:			Setup Form will not save non standard LPT Address (B800).
//		Fix:			There was a check from way back in Beta to prevent combobox
//						data from being loaded incorrectly.  This has been fixed
//						and tested to work with B800.
//		Reported By:	KC9FOL
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=5235#5235
//
//		Issue:			Sometimes frequency calibration gives an error about
//						needing a 30dB above noise floor signal when it has one.
//		Fix:			On faster machines, the calibration routine was running
//						so fast that the frequency change was not taking effect
//						in time to be measured.  We have added a pause to ensure
//						the frequency change takes effect before the calibration
//						test takes place.
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			BandStack will not save Mode or Filter changes if a 
//						frequency is already in the stack.
//		Fix:			In past versions, you had to change the frequency of a
//						BandStacking memory in order to change the filter or
//						mode.  This is fixed in this version.
//		Reported By:	AC5OG, N4HY, W0VB
//		Fix Coded By:	Eric Wachsmann
//		References:		Email 1/13/05 Subject: Bob's "Newer" Code On it's way
#endregion

#region 1.0.4 Released 01/05/05
//Beta 1.0.4 Released 01/05/05
//
//	**Bug Fixes
//
//		Issue:			If ASIO4ALL v2 is installed, automatic sound card
//						selection chokes asking to install ASIO4ALL.
//		Fix:			This was an error in the sound card setup
//						algorithm.  This has been fixed to use the latest
//						version of ASIO4ALL installed on the machine.
//		Reported By:	AH6JR, AA5XE
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=5196#5196
#endregion

#region 1.0.3 Released 01/04/05
//Beta 1.0.3 Released 01/04/05
//
//	**Bug Fixes
//
//		Issue:			MARS frequencies will not TX above 1W with PA installed.
//		Fix:			This was another unintentional bug caused by our
//						gain-by-band implementation.  This was fixed with the
//						issue listed below.
//		Reported By:	K3ZYK
//		Fix Coded By:	Eric Wachsmann
//		References:		Email 12/28/04 Subject: 100WPA Installation, three items...
//						Phone 12/28/04 1PM
//
//		Issue:			2m XVTR and 6m will not TX with the PA Enabled.
//		Fix:			This was an unintended by-product of our gain-by-band 
//						implementation.  We added support for using the older
//						gain calculation for 2m operation with the XVTR and
//						PA installed.
//		Reported By:	DL2JA
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=5090#5090
//
//		Issue:			Slow machines have trouble running image null calibration.
//		Fix:			Now the code checks to see if the display mode is set to
//						"Off" before switching to "Spectrum" to save CPU ticks on
//						slower machines.
//		Reported By:	N9VV
//		Fix Coded By:	Eric Wachsmann
//		References:		Email 12/27/04 Subject: RE: Audigy 2 ZS  **SUCCESS**
//
//		Issue:			Line In is not muted after TX with Santa Cruz.
//		Fix:			Fixed logic flaw in code cleanup done in 1.0.2.
//		Reported By:	N9VV, WA8SRA, VK6APH
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=5049#5049
//
//	**Modifications
//
//		Feature:		Sound Card selection now detects ASIO4ALL v2 and uses it
//						instead of v1.8 (shows up as Wuschel's ASIO4ALL in the
//						device list).
//		Coded By:		Eric Wachsmann
//
//	**New Features
//
//		Feature:		Reset for XIT/RIT.
//		Description:	There is now a Reset button labeled '0' for the RIT and
//						XIT controls.
//		Suggested By:	VK6APH
//		Coded By:		VK6APH
//		References:		Email 12/18/04 Subject: RIT Control
//						Email 12/28/04 Subject: RIX/XIT zero controls
#endregion

#region 1.0.2 Released 12/23/04
//1.0.2 Released 12/23/04
//
//	**Bug Fixes
//
//		Issue:			Bandstack registers will not recognize extra entries
//						manually entered in the database.
//		Fix:			When recoding the register routines, the values were hard
//						coded.  This has been fixed to read how many of registers
//						are in each band from the database. 
//		Reported By:	W0IQ
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4913#4913
//
//		Issue:			2m Bandstack registers now working.
//		Fix:			Corrected error in band limit equation from (144-54) to
//						(144-146).
//		Reported By:	W0VB
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4918#4918
//
//		Issue:			CW Form Cleanup.
//		Fix:			Reorganized a few options and changed around some of the
//						logic for enabling/disabling options.
//		Fix Coded By:	Eric Wachsmann
//
//	**New Feature
//
//		Feature:		Hotkeys for CW memories.
//		Description:	Added hotkeys to map F1-F5 to the 5 memory selections for
//						sending text using the CW keyer.
//		Suggested By:	AA5XE
//		Coded By:		Eric Wachsmann
//		References:		Telephone call 12/20/04 3PM.
//
//		Feature:		300kHz Low Filter Option.
//		Description:	Added a control to enable use of the custom filter bank
//						on the RFE for 300kHz operation.
//		Suggested By:	AC5OG
//		Coded By:		Eric Wachsmann

#endregion

#region 1.0.1 Released 12/14/04
//Beta 1.0.1 Released 12/14/04
//
//	**Bug Fixes
//
//		Issue:			Console crashes on first run after installation.
//		Fix:			The changes to the multimeter necessitated a startup
//						value change that was overlooked.  This has been corrected. 
//		Reported By:	WA2N, KD5TFD, DJ9CS
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4790#4790
//
//		Issue:			VFO Digit highlight happens even when the form is not focused.
//		Fix:			The code now checks the forms ContainsFocus property to
//						prevent this from happening. 
//		Reported By:	WA2N
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4863#4863
//
//		Issue:			Multimeter sometimes shows wrong scale.
//		Fix:			Changing the RX/TX meter mode only changes the scale now
//						when in RX or TX respectively. 
//		Reported By:	WA8SRA
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4877#4877
//
//		Issue:			Console gives Dynamic Database Creation Failed error after
//						importing a database.
//		Fix:			The code was referencing the wrong database file after
//						importing.  The code now uses the application directory
//						explicitly. 
//		Reported By:	WA8SRA
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4860#4860
//						http://www.flex-radio.com/forum/viewtopic.php?p=4879#4879
//
//		Issue:			Sometimes VFO highlight digit is not the one that is tuned.
//		Fix:			Corrected an offset miscalculation to take care of this. 
//		Reported By:	EB4APL
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4880#4880
//
//	**Modifications
//
//		Feature:		TX IMD Test now writes the front panel power back to the
//						Setup Form control when finished with the test.
//		Suggested By:	DK7XL
//		Coded By:		Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4866#4866		
//
//		Feature:		Classical Blackman Harris Window
//		Suggested By:	W3IP
//		Coded By:		N4HY
//		References:		http://forum.allaboutcircuits.com/newsgroups/viewtopic.php?t=32630&start=0
//						http://www.flex-radio.com/forum/viewtopic.php?p=4690#4690
#endregion

#region 1.0.0 Released 12/13/04
//1.0.0 Released 12/13/04
//
//	**Bug Fixes
//
//		Issue:			ATT causes unexpected results on 2m band.			
//		Fix:			The 10dB attenuator is not in the circuit if using
//						the 2m XVTR, so the ATT button is now disabled when
//						in the 2m band.
//		Reported By:	DL2JA
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4790#4790
//
//		Issue:			Inadvertant clicking on the Setup Reset button causes
//						lost settings.		
//		Fix:			The Reset button now has a confirmation dialog to 
//						prevent unintentional clearing of the data.  The button
//						was also renamed to indicate that it defaults all of 
//						the data on each tab of the Setup form.
//		Reported By:	WD4EGF
//		Fix Coded By:	Eric Wachsmann
//		References:		Stress Testing Email 12/6
//
//		Issue:			Squelch is not saved or restored in the memory database.			
//		Fix:			The squelch value is now carried from the front console
//						when saving a memory and restored when recalling one.
//		Reported By:	WD4EGF
//		Fix Coded By:	Eric Wachsmann
//		References:		Stress Testing Email 12/6
//
//		Issue:			The new X2-11 PTT does not work with the CW Form.			
//		Fix:			This was added as a late feature of 0.1.17 and the
//						implementation was not well thought out or tested.
//						The new implementation has been tested with SSB and
//						CW.  Note that the CW form must be open to use an
//						external key.
//		Reported By:	WA8SRA
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4748#4748
//
//		Issue:			The Tune button and CW Send button cause problems when
//						used at the same time.				
//		Fix:			Each button disables the other while it is active.
//		Reported By:	WA8SRA
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4756#4756
//
//		Issue:			After doing a Channel Scan, the Recall button no longer
//						works.					
//		Fix:			Forced selected position for the datagrid back to the top
//						after the scan is finished.  Also found/corrected a bug
//						with scanning if the last entries Scan is set to false.
//		Reported By:	DJ7MGQ
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4708#4708
//
//		Issue:			Frequency calibration is off if RIT is on during cal.					
//		Fix:			Freq. cal now forces to 0 and turns the control off
//						while running the routine (previous values are restored
//						once the calibration is finished).
//		Reported By:	KC2LFI, W0VB
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4612#4612
//
//		Issue:			Split mode does not work right when in CW mode.					
//		Fix:			VFO B now uses the CW Pitch offset when in Split mode
//						just like VFO A does.
//		Reported By:	AA5XE
//		Fix Coded By:	Eric Wachsmann
//		References:		Stress Test Email 12/3
//
//	**New Features
//
//		Feature:		Highlighted digit for mousewheel tuning.
//		Description:	Using the mousewheel over the VFO now shows an underline
//						to indicate the digit which will be modified.
//		Suggested By:	N9VV
//		Coded By:		Eric Wachsmann
//		Reference:		http://www.flex-radio.com/forum/viewtopic.php?p=4338#4338
//
//		Feature:		Clicking on the CW Menu option while in SSB modes now 
//						switches the mode to CW.
//		Description:	If LSB is active, it is switched to CWL. USB to CWU.
//		Suggested By:	W0VB
//		Coded By:		Eric Wachsmann
//		Reference:		IM discussion 12/3 4:32PM
//
//		Feature:		The TX IMD test now has an associated Power control.
//		Description:	The TX IMD was running at 100W before, and now runs
//						at the selected power setting.
//		Suggested By:	DK7XL
//		Coded By:		Eric Wachsmann
//		Reference:		http://www.flex-radio.com/forum/viewtopic.php?p=4727#4727
//
//		Feature:		RX/TX independant Multimeter selections.
//		Description:	The meter interface has been modified to have the RX and
//						TX modes separated.  Whichever mode is not in use will
//						show up in gray (RX when in TX and vice versa).
//		Suggested By:	AC5OG
//		Coded By:		Eric Wachsmann
//
//		Feature:		RIT/XIT now increment based on filter width.
//		Description:	The RIT/XIT controls now read out in Hz and increment
//						5Hz for filters less than 250Hz and 50Hz for filters
//						greater than or equal to 250Hz.
//		Suggested By:	SM6OMH
//		Coded By:		VK6APH
//		Reference:		http://www.flex-radio.com/forum/viewtopic.php?p=4707#4707
#endregion

#region Beta 0.1.17 Released 12/01/04
//Beta 0.1.17 Released 12/01/04
//
//	**New Features
//
//		Feature:		Shift key modifies wheel tuning rate by multiple of 10.
//		Description:	In an effort to simplify tuning and homing in on signals
//						with the mouse wheel, holding the Shift key will now cause
//						the tune rate to be divided by 10 (ie. 100Hz becomes 10Hz).
//		Suggested By:	WA8SRA
//		Coded By:		Eric Wachsmann
//		Reference:		http://www.flex-radio.com/forum/viewtopic.php?p=4592#4592
//
//		Feature:		External PTT connection separate from Dot/Dash.
//		Description:	CW operators have complained that there is no way to
//						do a manual PTT as you can in SSB.  This is because the
//						Dot line is shared with the current PTT line.  We have
//						added another PTT on the X2 connector (X2-11) that can
//						be used to cycle the PTT.  This is ideal for a footswitch. 
//		Suggested By:	KM0T, N4HY
//		Coded By:		Eric Wachsmann
//
//		Feature:		Process Priority Selection.
//		Description:	Under Setup->General, there is now a combobox that
//						allows the user to select their preferable process
//						priority.  This value is saved in the database
//						restored on startup.
//		Suggested By:	DK7XL
//		Coded By:		Eric Wachsmann
//		Reference:		http://www.flex-radio.com/forum/viewtopic.php?p=3995#3995
//
//		Feature:		Iambic CW Option.
//		Description:	The CW Form has always used Iambic B keying when the
//						keyer was active (though it never worked right before
//						this version.  This new control allows you to turn off
//						iambic keying (default is off).
//		Suggested By:	W5ZL, N4HY, AC5OG
//		Coded By:		Eric Wachsmann
//		Reference:		http://www.flex-radio.com/forum/viewtopic.php?p=3995#3995
//
//	**Bug Fixes
//
//		Issue:			Recalling a memory with DRM or SPEC mode causes the
//						filter to be in a strange state.						
//		Fix:			Added a "None" option to the filters for the memories.
//						Also now force None to be selected and disable the combobox
//						when DRM or SPEC is selected.
//		Reported By:	DJ9CS
//		Fix Coded By:	Eric Wachsmann
//		Reference:		http://www.flex-radio.com/forum/viewtopic.php?p=4620#4620
//
//		Issue:			After using the Semi Break In option in the Ext Key
//						mode, you can not do a "manual" Send.						
//		Fix:			This was a problem with leaving the Semi Break In 
//						in a strange state while keying.  Unchecking the 
//						Semi Break In option now sets the state to prevent
//						this from happening.
//		Reported By:	W5ZL, N4HY
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			When using the CW keyer, the iambic dot gets cut off.
//		Fix:			Two similarly named variables (dokeyer_instate &
//						getkeyerlevel_instate) were inadvertantly switched in
//						a critical section of the iambic code.  Once this was
//						fixed, the dot sounds normal in iambic mode.
//		Reported By:	W5ZL, AC5OG, N4HY
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			The channel scanner gets stuck after the last memory.
//		Fix:			Eliminated an unwanted event handler from interacting
//						and causing this issue.
//		Reported By:	DJ7MGQ
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4624#4624
//
//		Issue:			The CW Form attempts to respond to Mic key in LSB mode
//						after having used semi break in. 
//		Fix:			Fixed the polling thread for CW to detect when the CW
//						form has been closed.  When it detects this condition,
//						it stops polling and waits for the CW form to reopen.
//		Reported By:	SM6OMH
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4611#4611
//
//		Issue:			Multimeter thread terminates when in CW TX.
//		Fix:			Changed code that handles the non-DSP cases to simply
//						wait rather than terminate the thread.
//		Reported By:	DJ9CS, K3IB
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4607#4607
//						http://www.flex-radio.com/forum/viewtopic.php?p=4619#4619
//
//		Issue:			TXEQ is enabled on startup (despite Enable checkbox not
//						being checked.
//		Fix:			Moved the forced event handler for the enable checkbox
//						to just after the TX Low/High filter controls.
//		Reported By:	WA8SRA
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4586#4586
//
//		Issue:			Bandstack registers do not save 1Hz digit if the frequency
//						is larger than 10MHz.
//		Fix:			Corrected an error when recalling database values and
//						casting a double as a float (now using doubles).
//		Reported By:	SM6OMH
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4596#4596
//
//		Issue:			Clock minutes do not advance.
//		Fix:			Corrected ToString() function to use "m" for minutes 
//						instead of "M" for month.  :)
//		Reported By:	W0IQ, DJ9CS, LA6XJ, WB0SOK, K3IB
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4579#4579
//						http://www.flex-radio.com/forum/viewtopic.php?p=4595#4595
//
//		Issue:			If you click the Power button while the CW Send button is
//						engaged, the console crashes.
//		Fix:			Added a statement to the Power button handler to switch
//						the DSP before stopping the CW to prevent the crash.
//		Reported By:	AC5OG
//		Fix Coded By:	Eric Wachsmann
#endregion

#region Beta 0.1.16 Released 11/23/04
//Beta 0.1.16 Released 11/23/04
//
//	**Bug Fixes
//
//		Issue:			Clicking Var1 while in DSB mode gives a ChangeFilter
//						error.
//		Fix:			Corrected a typo of -2600 to 2600 for the high filter
//						cut on DSB Var1 default.
//		Reported By:	WK0J
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4519#4519
//
//		Issue:			Panadapter display mode is messed up when in DRM mode.
//		Fix:			Made adjustment to panadapter to use 0-20kHz limits for
//						DRM mode to correct display.
//		Reported By:	Phil Harman VK6APH
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4522#4522
//
//		Issue:			Scrolling to tune the VFO after using the Filter Shift
//						control causes both tuning and the Filter Shift to change.
//		Fix:			After scolling the Filter Shift control, the focus is
//						passed back to the main form.
//		Reported By:	Bob McGwier N4HY
//		Fix Coded By:	Eric Wachsmann
//		References:		email: Subject: First bug in IF shift Received 11/21/04 2:34PM
//
//		Issue:			Clicking Reset button in PA Settings Gain Calibration causes
//						an unhandled exception to occur.
//		Fix:			Fixed typo of 150.2dB to 50.2dB in Reset function.
//		Reported By:	Gerald Youngblood AC5OG
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			Semi-Breakin delay control does not work when using an external
//						keyer.
//		Fix:			Corrected the algorithm for ext. keyers to use the delay value
//						in ms.
//		Reported By:	W5ZL, AC5OG
//		Fix Coded By:	Eric Wachsmann
//
//	**New Features
//	
//		Feature:		Date/Time Display.
//		Description:	Added date and time display including local and UTC time.
//		Suggested By:	Eric Ellison AA4SW
//		Coded By:		Eric Wachsmann
#endregion

#region Beta 0.1.15 Released 11/19/04
//Beta 0.1.15 Released 11/19/04
//
//	**Bug Fixes
//
//		Issue:			CW Form crashes intermittantly.
//		Fix:			Added a check after creating each sample for a quick
//						exit from TX mode.
//		Reported By:	AC5OG
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			Squelch had a very harsh onset and sounded like a click.
//		Fix:			Added a soft onset.  After Frank reported we needed a
//						soft release for CW, this was added.
//		Reported By:	SM6OMH, AB2KT
//		Fix Coded By:	Bob N4HY
//		References:		http://www.flex-radio.com/forum/viewtopic.php?t=850
//
//		Issue:			Filter display is offset after using IF Shift control.
//		Fix:			Fixed the background redraw to occur after the filter
//						values have been changed instead of before.
//		Reported By:	AA1VG, SM6OMH
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4422#4422
//						http://www.flex-radio.com/forum/viewtopic.php?p=4399#4399
//
//		Issue:			Changing filter from a Var filter to a predefined one
//						causes unexpected results.
//		Fix:			IF Shift is now reset whenever the filter is changed.
//						If using a Var filter, the pre-shifted values are saved.
//		Reported By:	Phil Harman VK6APH
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4470#4470
//
//		Issue:			SPEC and DRM modes do not display correctly on startup.
//		Fix:			Since the Filter Shift control doesn't apply to these 
//						modes, it is now disabled when these modes are active.
//		Reported By:	Eric Wachsmann, N4HY
//		Fix Coded By:	Eric Wachsmann

//		Issue:			Color selection controls not working.
//		Fix:			Rebuilt handler connections in the VS2003 IDE.
//		Reported By:	VK6APH, DK7XL
//		Fix Coded By:	Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4410#4410
//
//	**New Features:
//
//		Feature:		Tune Power is saved.
//		Description:	The Tune power setting is now saved when the button is
//						turned off and is restored from the database on startup.
//		Suggested By:	Phil Harman VK6APH
//		Coded By:		Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4411#4411
//
//		Feature:		CW Semi Break-In Variable Delay.
//		Description:	The delay between when the key is released and when the
//						console returns to RX is now variable.
//		Suggested By:	W1CG, AC5OG
//		Coded By:		Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4262#4262
//
//	**Modifications:
//
//		Feature:		Wave recording buffer sizes.
//		Description:	The Wave recorder now supports buffer sizes of 512 and
//						1024 in addition to the previous 2048 settings.  These
//						are set using the Buffers control in Setup->Audio.
//		Suggested By:	K3IB
//		Coded By:		Eric Wachsmann
//		References:		http://www.flex-radio.com/forum/viewtopic.php?p=4442#4442
//
//		Feature:		PortAudio build updated.
//		Description:	The PortAudio v19 build we were using was over 6 mos. out 
//						of date.  We update the build which included quite a few
//						bug fixes in their code.  Please see www.portaudio.com
//						for more information.
//		Implemented By:	Eric Wachsmann
#endregion

#region Beta 0.1.14 Released 11/12/04
//Beta 0.1.14 Released 11/12/04
//
//	Bug Fixes:
//
//		Issue:			Squelch Popping when turning on and off.
//		Fix:			Smoothed squelch transitions using 1ms growth/decay.
//		Reported By:	SM6OMH
//		Fix Coded By:	Bob McGwier N4HY
//		Reference:		http://www.flex-radio.com/forum/viewtopic.php?t=850
//
//		Issue:			Transmit drive output low due to multithreading issues.
//		Fix:			Added Mutexes to hardware code to prevent the ADC
//						and DDS from having negative interactions.
//		Reported By:	AC5OG and KC1SX
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			Audigy 2 Sound Card selection fails initialization.
//		Fix:			Separated Audigy 2 and Audigy 2 ZS initialization
//						routines.
//		Reported By:	Clint Herron / HCJB
//		Fix Coded By:	Eric Wachsmann
//		Reference:		http://www.flex-radio.com/forum/viewtopic.php?p=4240#4240
//
//		Issue:			CW speed limited to 15WPM using software keyer.
//		Fix:			Corrected cw buffer size after changing from 2048.  Note
//						that 15WPM IS the limit if using 2048 sample buffers.
//						Use 1024 or 512 to get above 15WPM.
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			Center line of display is one pixel off to the right.
//		Fix:			Instead of drawing the line at x-pos Width/2 with width 2,
//						we now draw at x-pos Width/2-1 with width 2.
//		Reported By:	Bob McGwier N4HY
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			Console crashes if you click the power button off while
//						sending CW.
//		Fix:			Check CW Send when turning the power off.
//		Reported By:	Gerald Youngblood AC5OG
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			Extremely high power output causes PA Gain Calibration
//						to fail.
//		Fix:			Modified Calibration calibration routine to set the gain
//						to 50.2dBm once.  The next range error will fail the test.
//		Reported By:	AC5OG and KC1SX.
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			Playback from WAV files not created with the PowerSDR
//						software could cause the console to crash.
//		Fix:			Improved Wave File reading routine.  Now checks sample
//						rate, number of channels and wave file format.
//		Reported By:	Eric Wachsmann
//		Fix Coded By:	Eric Wachsmann
//
//		Issue:			Progress Abort button is unresponsive for up to 4 seconds
//						during the PA Gain Calibration routine.
//		Fix:			Rather than calling Sleep(4000), we now sleep for only 100ms
//						and then check the progress bar for input.  This results in
//						a much more responsive Abort.
//		Reported By:	Gerald Youngblood AC5OG
//		Fix Coded By:	Eric Wachsmann
//
//	New Features:
//
//		Feature:		Filter Shift Control
//		Description:	Easily shifts the high and low filter cuts.  Ideal for
//						panadapter display mode.
//		Suggested By:	Phil Harman VK6APH
//		Coded By:		Phil Harman VK6APH
//
//		Feature:		TX IMD Test (added to Setup->Tests)
//		Description:	Sends a 2 tone signal signal at the frequency shown.
//		Suggested By:	Gerald Youngblood AC5OG
//		Coded By:		Eric Wachsmann
//
//		Feature:		Front panel Tune button.
//		Description:	Outputs CW tone at 10%/10W for 1W/100W configurations.			 
#endregion

#region Beta 0.1.13
//Beta 0.1.13 - 11/08/04
//	Eric Wachsmann
//		- Fixed bug with 6kHz Filter on LSB (was doing USB).
//		- Fixed bug in BandStack algorithm causing strange
//			values to be saved to the database.
//		- Fixed bug when saving calibration values to database
//			causing error about field too small.
//		- Fixed more bugs in the CW code.
//		- Modified PWR control to be in Watts when using the 
//			FlexRadio 100W PA.
//		- Added PA fwd/rev power and SWR to the multimeter when
//			the FlexRadio 100W PA is enabled.
//		- Added PA 100W Calibration Routine.
//		- Added "High SWR" warning on display if reverse
//			power is more than 11W.
//		- Added "None" option to Mixer selection.
//		- Modified Mixer get Mux function to fail if the 
//			sound card doesn't have 2 or more Mux lines.
//		- Added X2 Enable and Delay controls.
//		- Modified sequencing to match Phil's suggestion.
//			Link: www.flex-radio.com/forum/viewtopic.php?p=4285
//		- Prevented PA Bias from going active if outside the
//			spec of the PA (1.8-29.7MHz)
//		- Fixed problem with band showing up as GEN sometimes 
//			when on the edge of a band.
//		- Modified Audigy 2 and Audigy 2 ZS to mute the Line In
//			when in TX to prevent RF feedback.
#endregion

#region Beta 0.1.12
//Beta 0.1.12 - 10/29/04
//	Bob McGwier N4HY
//		- Fixed bug with TX EQ in wrong state on startup.
//		- Fixed bug using PWR control for TX DSP volume.
//			Issue reported by: DK7XL.
//	Eric Wachsmann
//		- Revamped the routines that change the Bands,	Modes,
//			and Filters.  Cleaner code -- UI unchanged.
//		- Added PA Power and SWR to the multimeter.
//		- Fixed ATT getting enabled after TX without RFE.
//		- Modified Image and Level calibration to force Spectrum
//			display mode while running.
//		- Fixed 6dB offset between Spectrum and Panadapter
//			display modes.
//		- Eliminated picture files from the installation
//			(now using embedded resources).
//		- Added PA controls for gain by band (not yet enabled).
#endregion

#region Beta 0.1.11
//Beta 0.1.11 - 10/20/04
//	Eric Wachsmann
//		- Modified PWR and AF settings to use DSP code instead
//			of mixer (Main and Wave volume set to max). 
//		- Fixed bug where console crashes on started due to
//			sound card selection (Audigy cards).
//		- Consolidated Sound Card options (all Audigy cards 
//			together now).
//		- TX EQ settings are now saved on exit.
//		- Fixed Peak Text and Cursor Position Text when in CW
//			modes (was off by CW Pitch).
//		- Modified mode change.  Whenever switching to or from
//			CW, the tone is preserved.
//		- Fixed problem with Audio Devices with a '/' in the
//			the name.
//		- Fixed problem where 60m frequencies were showing
//			up in the GEN band rather than 60m band.
//	Phil Harman VK6APH
//		- Fixed low power output problem in configurations
//			without the RFE (ATTN relay in MOX transition).  
#endregion

#region Beta 0.1.10
//Beta 0.1.10 - 10/14/04
//	Eric Wachsmann
//		- Fixed problem with Band buttons crashing the console
//			caused by changing database datatypes in 0.1.9.
//		- Fixed CW Form transmitting in CWL even when in
//			CWU mode.
//		- Fixed problem when using ',' for the decimal delimiter
//			causing database to not save fractional information.
//		- Changed the way the current band is detected.  Now
//			frequencies outside the bands for which there are
//			buttons will show up in the GEN (General) band.
#endregion

#region Beta 0.1.9
//Beta 0.1.9 - 10/12/04
//	Bob McGwier N4HY
//		- Fixed Squelch Control value.
//	Eric Wachsmann
//		- Added Enable button for TX EQ.
//		- Fixed Sound Card Selection using Audigy 2 or Audigy
//			2 ZS.
//		- Fixed Image Null Calibration progress going to 200%.
//		- Fixed ATT - now turned off (and disabled) if RFE is
//			unchecked.
//		- Fixed Import Database values not taking affect right
//			away.
//		- Fixed Database - frequencies are now saved as a
//			numeric datatype (instead of text).
#endregion

#region Beta 0.1.8
//Beta 0.1.8 - 10/11/04
//	Bob McGwier N4HY
//		- Enabled TX Equalizer.
//	Eric Wachsmann
//		- Added progress display to calibration routines.
//		- Enabled feedfoward compression.
//		- Added Delete button to Memory form.
//		- Fixed VFO Swap problem (frequency set last instead of 
//			first).
//		- Reorganized Audio Tab in Setup form.
//		- Added Sound Card selection to simplify setup.
//		- Added setup code for supported sound cards.
//		- Added code to support External Reference Clock.
//		- Removed Test Form.
//		- Fixed ATT not getting switched off during TX.
//		- Fixed problem where Level Calibration in Panadapter
//			display mode would be 6dB off.
//		- Added Tooltips to many of the controls (hover over
//			the control for descriptive information).
#endregion

#region Beta 0.1.7
//Beta 0.1.7 - 10/01/04
//	Bob McGwier N4HY
//		- Modified Scope DSP to show only the modulated portion
//			of the signal (and not the carrier).
//		- Enabled TX Feedback and Feedforward Compression.
//		- Fixed FM detection (in progress -- FM modulation not 
//			tested).
//		- Zeroed old RX or TX buffer on TX switching.
//		- Added update control for TX equalization (in progress).
//	Eric Wachsmann
//		- Added option to disable software gain/att compensation.
//		- Added more color options to the Appearance menu.
//		- Added function to import previous database (See Setup
//			Form bottom left).
//		- Added Mutex synchronization to prevent calibration 
//			threads from crashing.
//		- Increased the thread priority for the calibration
//			routines to prevent starvation/deadlocks on
//			slower/busier machines.
//		- Fixed BandStacking problem where changing the filter
//			would not update the database.
//		- Fixed Peak Text bug showing wrong values in Panadapter
//			display mode.
//		- Set main mute off on startup.
//		- Enabled display during calibration routines.
//		- Sped up calibration routines by slowing the display.
//		- Improved Image Reject Calibration algorithm.
//		- Clicking the Apply button in the Setup Form now saves
//			the options to the database.
#endregion

#region Beta 0.1.6
//Beta 0.1.6 - 9/22/04
//	Bob McGwier N4HY
//		- Fixed SAM demodulation coherent detection error.
//	Eric Wachsmann
//		- Added wizard to simply the setup process.
//		- Added TR sequencing to X2-7 (as it was in VB).
//		- Added Auto Frequency calibration (Setup -> General).
//		- Added Auto Image Null calibration (Setup -> General).
//		- Added Support for 100W PA (TR relay, band selection,
//			PWR ADC, Bias Relay).
//		- Removed FIRST from the Save Memory Form AGC List.
#endregion

#region Beta 0.1.5
//Beta 0.1.5 - 9/15/04
//	Eric Wachsmann
//		- Removed PA_Skeleton host API.  Possible source
//			of crashing on startup.
//		- Simplified calibration for sound cards without a
//			typical windows mixer.
//		- Fixed AGC Mode list in memory form.  
//		- DisplayMode, AGCType and DSP Window references now
//			all refer to the enumeration for easy modification.
//		- Modified Waterfall DisplayMode: Now directly accesses
//			bitmap memory rather than coloring pixels.
//		- Fixed problem with 3kHz jump when using clock
//			correction.
//		- Clock correction now works without spur reduction.
//		- Fixed problem where signal would move when turning
//			Spur Reduction on and off.
//		- Fixed problem with display when Max Spectrum was not
//			zero.
//		- Modified Panadapter display mode: horizontal line
//			labels are now moved to the left when in USB or 
//			CWU modes for better signal visibility.
#endregion

#region Beta 0.1.4
//Beta 0.1.4 - 9/08/04
//	Bob McGwier N4HY 
//		- Fixed problem with Clock Offset in hardware code. 
//	Eric Wachsmann
//		- MON button now works while in TX.
//		- While MON is still automatically enabled in CW mode,
//			it now returns to the mode it was in before CW
//			(instead of just turning the monitor off each time).
//		- Fixed problem where sometimes mixer selections were
//			not cleared when switching devices.
//		- Added Quick Memory to list of saved variables to store
//			in the database upon exit.
//		- Pressing 'Enter' when in a TextBox, ComboBox or
//			NumericUpDown control now returns focus to the 
//			main window.
//		- QS (Quick Save) now saves mode and filter values.
//			QR (Quick Restore) restores them.
//		- Modified portaudio code to use 0.2 as the suggested
//			latency if the Driver Type is MME.
#endregion

#region Beta 0.1.3
//Beta 0.1.3 - 8/31/04
//	Bob McGwier, N4HY
//		- Modified DSP synchronization to improve efficiency.
//		- Modified hardware code to disable unnecessary DDS
//			functions (saves ~100mA).
//	Eric Wachsmann
//		- Spur Reduction is now enabled the first time you run
//			the console.
//		- Added Portaudio error message translator (from number
//			to text).
//		- Enabled Keydown in CW form.
//		- Averaging buffer is now reset when first enabled,on
//			power on, and on changed filter.
//		- Fixed problem remembering XVTR Present setting.
//		- Warning about 25W XVTR no longer displays on startup.
//		- Window locations are now checked against screen
//			resolution.
//		- DDS Multiplier maximum is now 20 (was 10).
//		- DDS Correction now has the label "Clock Offset."
#endregion

#region Beta 0.1.2
//Beta 0.1.2 - 8/18/04
//	Bob McGwier, N4HY
//		- Added Max Gain to AGC controls (DSP tab).
//		- Power normalized band pass filters (250Hz and up).
//	Eric Wachsmann
//		- Added Quick Memory Save (QS) and Restore (QR).
//		- Added support for the 25W XVTR from DEMI.
//		- Modified Waterfall Display.  Now uses controls
//			on the Display form in the Display tab.
//		- Modified calibration to support all IF settings,
//			0-100 (previously cal was only valid for one IF
//			value).
//		- Fixed BandStacking problem with trying to add a
//			frequency that was already saved.
//		- Fixed ATT not staying on return from TX mode.
//		- Fixed CW Pitch - changes take effect immediately 
//			(used to have to change filters).
//		- Fixed CW Filters - now centered around CW Pitch.
//		- Fixed meter bug with international delimiters.
//		- Reorganized code for dynamic creation of the database.
//		- AF, IF, PWR and MIC controls are no longer disabled
//			after a mixer error.
//		- Choosing SPEC while in Panadapter display mode changes
//			the display mode to Spectrum.
//		- Modified Panadapter display to be closer to calibrated 
//			spectrum values.
#endregion

#region Beta 0.1.1
//Beta 0.1.1 - 8/13/04
//	Eric Wachsmann
//		- Added support for MARS & CAP frequencies (requires 
//			license, contact FlexRadio for more info).
//		- Added Up/Down controls for Gain, Phase, Feedforward,
//			and Feedback sliders.
//		- Added delay controls for the CPU meter and the Peak
//			Display	Text.
//		- Added long cursor support (right-click).
//		- Modified waterfall display to support 3 colors (low,
//			mid, high).
//		- Modified click-tuning to work only when in long cursor
//			mode.
//		- Fixed Peak Display Text.  Now turned off if not in
//			Spectrum, Histogram, Waterfall or Panadater modes.
//		- Fixed display cursor location text.  Now clears when
//			mouse is not within display.  Also only displays
//			in Spectrum, Histogram, Waterfall or Panadater modes.
//		- Fixed 25Hz filter in AM, SAM, FMN, and DSB.  Values 
//			are now -613, 613 (was 587, 613).
//		- Fixed backwards offset when changing from LSB, USB,
//			or DSB to CWL or CWU.
//		- Fixed problem with using non-standard LPT Addresses.
//		- Fixed Database failing dynamic creation.
#endregion

#region Beta 0.1.0
//Beta 0.1.0 - 8/11/04
//	Bob McGwier, N4HY
//		- Modified Hang times on all AGC modes.
//		- Added Squelch support.
//	Eric Wachsmann
//		- Added Push To Talk (PTT) functionality.
//		- Added digital meter for AGC and ADC meter modes.
//		- Added left and right meter colors to appearance
//			options.
//		- Added Button Selected color to appearance options.
//		- Added Squelch (SQL) controls.
//		- Modified AGC list to be Fixd, Long, Slow, Med, and Fast.
//		- Fixed problem with display not updating on changes to
//			the Spectrum Min value.
//		- Fixed bug in Calibration that caused crashing and 
//			limited resolution to 1.0dB (instead of 0.1dB).
//		- Fixed bug with CW speed increasing with decreasing 
//			buffer size setting.
#endregion

#region Alpha Release Notes
//Alpha 0.0.27 - 8/04/04
//	Bob McGwier, N4HY
//		- Added support for different buffers sizes (work in
//			progress).
//		- Added DSP support for meters.
//		- Fixed spectrum flutter/strangeness.
//	Eric Wachsmann
//		- Added Memory channel scanner support.
//		- Fixed Arrow key control in comboboxes.
//		- Fixed CW crashing or slow when coming out of TX with
//			some configurations.
//		- Implemented digital meters.

//Alpha 0.0.26 - 7/19/04
//	Bob McGwier, N4HY
//		- Added AM, SAM and FMN TX modulation.
//	Eric Wachsmann
//		- Fixed problem where Saving a memory while in SAM,	SPEC
//			or DRM mode would cause the save to fail.
//		- Fixed problem where sometimes the group setting would
//			not show up under Memory form.
//		- Fixed problem where trying to enter a blank frequency
//			in VFO A or B blank would cause the console to crash.
//		- Added Frequency scanner function.
//		- Implemented display averaging.
//		- Removed Phase and Scope display options when in SPEC 
//			mode.

//Alpha 0.0.25 - 7/16/04
//	Bob McGwier, N4HY
//		- Added SSB TX.
//	Eric Wachsmann
//		- Fixed reversed sideband on CW keyer.
//		- Improved memory form.
//		- Added save memory option.

//Alpha 0.0.24 - 7/12/04
//	Bob McGwier, N4HY
//		- Added DSP to display in TX mode.
//		- Added support for TX filter control.
//	Eric Wachsmann
//		- Added TX Equalizer controls (Not implemented).
//		- Added TX Filter controls.
//		- Added TX Compression controls (Not implemented).
//		- Fixed leading decimal frequency entry (eg.  ".590").
//		- Added support for ',' as the decimal separator.
//		- Added controls for multimeter (Not implemented).
//		- Improved display issues related to CW keyer.
//		- Added Memory and Group tables to database.
//		- Added basic Memory form.

//Alpha 0.0.23 - 6/29/04
//	Eric Wachsmann
//		- Front panel state is now saved on exit.
//		- CW form options are now saved on exit.
//		- The database is checked on startup for all tables and
//			adds any that are missing.
//		- Fixed 500Hz filter in DSB modes.
//		- Added Semi Break In mode for CW (Manual keyboard and
//			mouse).
//		- Fixed bug where audio/display would stop coming out
//			of CW send mode.

//Alpha 0.0.22 - 6/25/04
//	Eric Wachsmann
//		- Fixed CW Keyer.  All CW modes are now functional.

//Alpha 0.0.21 - 6/24/04
//  Eric Wachsmann
//		- Improved CW support.  Various bugs eliminated.

//Alpha 0.0.20 - 6/17/04
//	Eric Wachsmann
//		- The VFO controls (A > B, A < B, A <> B) now include
//			the filter (in addition to mode and frequency).
//		- The VFO controls including Split are now disabled
//			during transmit.
//		- Filters below 1kHz are no longer disabled in modes
//			other than CW.
//		- CW Keyer implemented (several known bugs still exist).

//Alpha 0.0.19 - 6/15/04
//	Eric Wachsmann
//		- Changing display controls now redraws the background
//			in Panadapter and Histogram modes (in addition to 
//			Spectrum mode).
//		- Added Colorbutton to control filter overlay display
//			color in Panadapter display mode.
//		- Highlighted band button now turns Olive (like mode
//			and filter) when in TX mode.
//		- Changed Phase and Gain increment values to 1/5 for
//			small/large increment (was 10/50).
//		- Added click tuning to Panadapter display mode.

//Alpha 0.0.18 - 6/11/04
//	Eric Wachsmann
//		- Fixed problem with keypad entry.
//		- Implemented Panadapter display mode.
//		- Eliminated the Up 5 button (use XIT control instead).
//		- VFO swap (A <> B) now swaps the mode and freq.

//Alpha 0.0.17 - 6/10/04
//	Eric Wachsmann
//		- Fixed 60m band by moving each channel down 1.5kHz.
//		- Added yellow color to selected mode/filter.
//		- Added highlight for current band based on band text.
//		- Added mousewheel tuning for 10MHz digit.
//		- Removed Mixer tab from Setup.  Combined with Audio.
//		- Fixed display bug on startup in large fonts mode.
//		- Fixed colorbutton control bug in startup.
//		- Mixer Device is now selected to match the Audio Input.
//		- Removed Sample Depth as it is no longer relevant
//			(ie. MME/DS = 16-bit, ASIO = 24-bit)
//		- Added CW Form interface (not functional yet).

//Alpha 0.0.16 - 6/08/04
//	Eric Wachsmann
//		- Implemented VFO functions (XIT, RIT, Split, Up 5).
//		- Implemented Tone Test for CW TX using 128 samples.
//		- All push buttons now turn yellow when active (pushed).
//		- Fixed mixer issue - Mux optionsdisabled if not found.
//		- Fixed CW Offset when in CWL or CWU Modes.
//		- Added version number to the database.

//Alpha 0.0.15 - 6/07/04
//  Eric Wachsmann
//		- Added hook for CW form on main form.
//		- Implemented transmit interface (no DSP).
//		- Redesigned front panel to make room for more features.

//Alpha 0.0.14 - 6/03/04
//	Bob McGwier, N4HY
//		- Redesigned spectrum display DSP routines.
//		- Added selectable windowing modes.
//		- Reworked DSP process loop to increase both efficiency
//			and readability.
//		- Added Panadapter DSP functions.
//	Eric Wachsmann
//		- Added window mode selection to setup form.

//Alpha 0.0.13 - 6/02/04
//	Eric Wachsmann
//		- Added long click function for Powermate (ctrl+right).
//		- Clock Correction is now implemented (in setup).
//		- RFE Control bug found and fixed (constants for IC9 and
//			IC10 swapped).
//		- Modified BandText table in database to add a field to
//			determine if transmit is allowed.
//		- Fixed attentuator display offset (10dB).

//Alpha 0.0.12 - 5/27/04
//	Bob McGwier, N4HY
//		- Fixed Binaural mode.
//		- Limited audio values to the range -1.0 < x < 1.0
//		- Fixed DSP error causing mixing products in audio.
//	Eric Wachsmann
//		- Added init code for slider values in setup form.
//		- Added support for the Griffen Powermate.
//		- Added function keys and home, end, etc to key map. 

//Alpha 0.0.11 - 5/26/04
//	Bob McGwier, N4HY
//		- Added Binaural (BIN) audio mode.
//	Eric Wachsmann
//		- Corrected splash screen for large fonts
//		- Added DDS controls to the setup form (not implemented).
//		- Fixed click tuning in CW modes (pitch offset).
//		- Spur reduction is no longer always on after startup.

//Alpha 0.0.10 - 5/25/04
//	Bob McGwier, N4HY
//		- Code cleanup in DSP - AM and SAM modes.
//		- Fixed bug in DSP - DRM mode.
//		- Corrected DRM VFO offset problem.
//	Eric Wachsmann
//		- Added Histogram display mode.
//		- Waterfall display mode now scrolls (instead of wrap)

//Alpha 0.0.9 - 5/20/04
//	Bob McGwier, N4HY
//		- Corrected reversed gain values.
//	Eric Wachsmann
//		- Added Waterfall display mode.
//		- Fixed broken AGC, NR, ANF and NB (handlers gone after
//			moving them into DSP groupbox).
//		- PWR, AF, IF and MIC controls now affect the mixer.
//		- Added version to title bar.

//Alpha 0.0.8 - 5/19/04
//	Eric Wachsmann
//		- Error handling for variable filters added (disallow
//			low cut being higher than high cut and vice versa).
//		- Added controls to front panel for functionality
//			comparable to the VB version (RIT and XIT not yet
//			implemented).
//		- Added variable wheel tuning rates (use middle mouse
//			button to cycle through them).

//Alpha 0.0.7 - 5/18/04
//	Bob McGwier, N4HY
//		- Corrected NB threshold values in setup form.
//		- Modified DSP closeup routine to correct memory error.
//	Eric Wachsmann
//		- Added splash screen with startup info/countdown.
//		- Added icons to shortcuts and windows.
//		- Forms locations are now saved on exit and restored on
//			on startup.
//		- Eliminated all direct access to the database to
//			startup and exit (using static datasets).
//		- Added error catching for mixer init on startup.

//Alpha 0.0.6 - 5/12/04
//	Eric Wachsmann
//		- Modified Mixer code to fix reversed Mux list issue.
//		- Fixed issue with dragging a window over the display.
//		- Modified Setup project to allow multiple versions
//			(from this version on).

//Alpha 0.0.5 - 5/11/04
//	Bob McGwier, N4HY
//		- Various DSP code cleanup.
//	Eric Wachsmann
//		- Added appearance settings in Setup Form.
//		- Implemented Key Map for Mode, Filter and Band.
//		- Fixed bandstacking database issue (caused by auto-
//			creation not creating primary key on BandStack).
//		- Added additional delay to display thread when mode is
//			Off (to lower CPU usage).

//Alpha 0.0.4 - 5/10/04
//	Bob McGwier, N4HY
//		- Added controls to Setup for DSP functions.
//		- Cleaned up DSP code and removed more unnecessary
//			buffer copying.
//	Eric Wachsmann
//		- Added support for auto-creation of the database.
//		- Changed default PortAudio Driver to ASIO if available
//			(Then DirectSound, then MME)
//		- Removed unused controls from Setup form.
//		- Cleaned up Mixer initialization code (to prevent
//			the index 0 error).

//Alpha 0.0.3 - 5/7/04
//	Bob McGwier, N4HY
//		- Fixed offset problem in SPEC mode.
//		- Added offset to VFO in DRM mode.
//		- Code cleanup in SetDDS.
//	Eric Wachsmann
//		- Added mousewheel tuning support for large fonts.
//		- Completed Bandstacking feature (saves entries).

//Alpha 0.0.2 - 5/6/04
//	Bob McGwier, N4HY
//		- Implemented spur reduction for all modes except SPEC
//		- Added transmit capability to the DSP (not implemented
//			in GUI yet)
//		- Implemented IQ correction in DSP and tied to trackbars
//			in the setup form. (image rejection)
//		- Fixed slHang and Hang time calculation bugs in digital
//			AGC.
//		- Modified several buffer copies to	memcpy from loops.
//	Eric Wachsmann
//		- Implemented read-only Band Stacking buttons on front
//			panel.
//		- Fixed Mixer problem causing the error when pressing
//			Apply or OK in setup in some circumstances.

//Alpha 0.0.1b - 5/6/04
//	Eric Wachsmann
//		- Fixed NB button handler.
//		- Save/Restore variable filter settings from the database
//			for each mode.
//		- Disabled filters below 1000Hz for all modes except CW.

//Alpha 0.0.1 - 5/5/04
//		- Alpha testing begins.
#endregion
