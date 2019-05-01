
Latest Release v2.6.7 April 29, 2019
# 2.6.7 (2019-4-29)
- fixed bug where the VOX/DEXP LookAhead feature was enabled when VOX/DEXP was not.
- corrected compatiblity issue with the ANAN-10E. This requires new firmare to be flashed. v10.3
- corrected the Spectrum and Histogram diplay during transmit

# 2.6.6 (2019-4-21)
- corrects issue with EU region using commas
- corrects issue with having out of band frequency on startup
- fixed transmit filter not being displayed when using split

# 2.6.5 (2019-4-18)
- corrected issue with console remaining open after exiting Thetis
- fixed problem of program crashing when recording while transmitting
- fixed problem with program crashing when receiving a bad packet

# 2.6.4 (2019-4-13)
- improved VOX/DEXP features and performance
- QSK cabibility for the ANAN-200D, 7000DLE, and 8000DLE models
- fixed VAC1 startup problem
- fixed VAC2 resampler problem
- added option to use VAC2 on split
- improved TX-RX and RX-TX transistion on voice modes
- transverter T/R relay bug fixed
- added control for BYPS-EXT1-XVTR RX ANT for 7000DLE

  * see "Release Notes for 2-6-4.docx" for detailed information.

# 2.6.0 (2018-4-10)
- added diagnostic LED array
- divided open collector controls into 3 groups (HF-VHF-SWL)
- bug fix for step tune using MIDI

# 2.5.9 (2018-3-29)
- changed "MDECAY" constant to 0.99 in netInterface.c
- added 2Hz step tune choice
- corrected duplicate db import dialogs
- modified behavior of sequence errors so that sequence errors are ignored for seq 0
- changes to VAC includes tooltips for various controls, fix for the Output Ringbuffer latency Monitor control not working, and added the ability to reset the diagnostics
- forced BPF1 into ByPass during transmit if PureSignal is enabled for Orion MkII boards only

# 2.5.8 (2018-3-25)
- changed "MDECAY" constant to 0.9 in netInterface.c
- fixes for VFO A&B Lock 
- NB/NB2 is turned OFF while transmitting when DUP is enabled
- Added 2kHz Tune Step
- Changed ANF behavior so that it is disabled when in CW mode
- Removed the 750Hz CW filter and added a 150Hz CW filter
- Increased display buffer to support larger than 4k displays

# 2.5.7 (2018-3-25)
- spectrum roll-off adjusted to clip 4%
- calls to PeakFwdPower(…) and PeakRevPower(…) moved from netInterface.c to network.c
- skin graphics added for chkRxAnt and chkVFOBLock controls

# 2.5.6 (2018-3-25)
- added MIDI/CAT updates
- added independent VFO Locks

# 2.5.5 (2018-3-24)
- added support for ANAN-7000DLE
- added 'Rx Ant' support

# 2.5.4 (2018-3-22)
- added Audio Adaptive Variable Resampler with monitor tools
