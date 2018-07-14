
Latest Beta Release v2.6.0 April 10, 2018
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
