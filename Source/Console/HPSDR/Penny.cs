/*
*
* Copyright (C) 2008 Bill Tracey, KD5TFD, bill@ewjt.com 
* Copyright (C) 2010-2013  Doug Wigley
* This program is free software; you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation; either version 2 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

//
// this module contains code to support the Penelope Transmitter board 
// 
// 



namespace Thetis
{
	/// <summary>
	/// Summary description for Penny.
	/// </summary>
	public class Penny
	{
		private static Penny theSingleton = null; 
		
		public  static Penny getPenny() 
		{ 
			lock ( typeof(Penny) ) 
			{
				if ( theSingleton == null ) 
				{ 
					theSingleton = new Penny(); 
				} 
			}
			return theSingleton; 
		} 

		private Penny()
		{		
		}


		private byte[] TXABitMasks = new byte[25]; 
		private byte[] RXABitMasks = new byte[25];
        private byte[] TXBBitMasks = new byte[25];
        private byte[] RXBBitMasks = new byte[25]; 


		public void setBandABitMask(Band band, byte mask, bool tx) 
		{ 
			int idx = (int)band - (int)Band.B160M; 
			if ( tx ) 
			{ 
				TXABitMasks[idx] = mask;
			} 
			else 
			{ 
				RXABitMasks[idx] = mask;
			} 
			return; 

		}

        public void setBandBBitMask(Band band, byte mask, bool tx)
        {
            int idx = (int)band - (int)Band.B160M;
            if (tx)
            {
                TXBBitMasks[idx] = mask;
            }
            else
            {
                RXBBitMasks[idx] = mask;
            }
            return;

        }
        
        public void ExtCtrlEnable(bool enable, Band band, Band bandb, bool tx) 
		{
			if ( !enable ) 
			{
				NetworkIO.SetOCBits(0); 
			}
			else 
			{
				UpdateExtCtrl(band, bandb, tx);
			}
		}

        public int RxABitMask = 0xf; // 4x3 split
        public bool SplitPins = false;
        public bool VFOTBX = false;
		public void UpdateExtCtrl(Band band, Band bandb, bool tx) 
		{
			if (!tx && (int)band < 12)  // if !tx ignore given band and round off to nearest band based on freq 
			{ 
				band = Alex.AntBandFromFreq();
			}

            if (!tx && (int)bandb < 12)
            {
                bandb = Alex.AntBandFromFreqB();
            }
            
            int idx = (int)band - (int)Band.B160M;
            int idxb = (int)bandb - (int)Band.B160M;
			int bits; 
			if ( (idx < 0 || idx > 26) || (idxb < 0 || idxb > 26) )
			{ 
				bits = 0; 
			} 
			else 
			{
                if (SplitPins)
                {
                    bits = tx ? (TXABitMasks[idx] & RxABitMask) | TXBBitMasks[idxb] : (RXABitMasks[idx] & RxABitMask) | RXBBitMasks[idxb];
                }
                else
                {
                    if (tx && VFOTBX)
                        bits = TXABitMasks[idxb];
                    else if (tx)
                        bits = TXABitMasks[idx];
                    else bits = RXABitMasks[idx];
                }
			}
            System.Console.WriteLine("Bits: " + bits + " Band: " + (int)band + " Band: " + (int)bandb); 
			NetworkIO.SetOCBits(bits);
		}
	}
}
