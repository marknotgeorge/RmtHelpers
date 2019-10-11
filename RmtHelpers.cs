using System;
using System.Collections;
using System.Collections.Generic;
using nanoFramework.Hardware.Esp32.Rmt;

namespace nanoFramework.Hardware.Esp32.Rmt.Helpers
{
    /// <summary>
    /// A class of helper functions for the nanoFramework.Hardware.Esp32.Rmt library.
    /// </summary>
    public static class RmtHelpers
    {
        /// <summary>
        /// Creates an array of RmtItems from a Pronto code.
        /// </summary>
        /// <param name="code">A string containing the Pronto code</param>        
        /// <param name="sequence">The code sequence to return. Defaults to <c>Once</c>.</param>
        /// <param name="clockDivider">The RMT clock divider, used to determine the number of RMT ticks per microsecond. Defaults to 80.</param>
        /// <returns></returns>
        public static RmtItem[] Pronto2RmtItems(string code, CodeSequence sequence = CodeSequence.Once, byte clockDivider = 80)
        {            
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }            
            
            int clockFrequency = 80_000_000;
            

            var ticksPerMicroSecond = clockFrequency / clockDivider / 1_000_000;

            var pronto = new ProntoCode(code);

            RmtItem[] returnArray = null;

            switch (sequence)
            {
                case CodeSequence.Once:
                    returnArray =  getRmtItems(pronto.OnceSequenceBurstPairs, pronto.OnceSequence, ticksPerMicroSecond);
                    break;
                case CodeSequence.Repeat:
                    returnArray =  getRmtItems(pronto.RepeatSequenceBurstPairs, pronto.RepeatSequence, ticksPerMicroSecond);
                    break;                
            }

            return returnArray;                
        }

           

            
        

        private static RmtItem[] getRmtItems(int burstPairCount, int[] burstPairArray, int ticksPerMicroSecond)
        {
            var itemList = new ArrayList();
            for (var i = 0; i < ((burstPairCount * 2) - 1); i = i + 2)
            {
                var highBurst = burstPairArray[i] / ticksPerMicroSecond;
                var lowBurst = burstPairArray[i+1] / ticksPerMicroSecond;

                var newItem = CreateBurstPair((UInt16)highBurst, (UInt16)lowBurst);
                itemList.Add(newItem);
            }

                var itemArray = new RmtItem[itemList.Count];

            for (var i = 0; i < itemList.Count; i++)
            {
                itemArray[i] = (RmtItem)itemList[i];
            }

            return itemArray;
        }

        /// <summary>
        /// The code sequence to return.
        /// </summary>
        public enum CodeSequence
        {
            /// <summary>
            /// The code to send when the code is sent once, or the first code when the code is sent repeatedly.
            /// </summary>
            Once,
            /// <summary>
            /// The code to send after <c>Once</c> when the code is sent repeatedly.
            /// </summary>            
            Repeat
        }

        public static RmtItem CreateBurstPair(UInt16 highDuration, UInt16 lowDuration, bool highFirst=true)
        {
            const UInt16 maxDuration = 32_767;

            if (highDuration > maxDuration)
            {
                throw new ArgumentOutOfRangeException("highDuration", $"Must be less than {maxDuration}");
            }

            if (lowDuration > maxDuration)
            {
                throw new ArgumentOutOfRangeException("lowDuration", $"Must be less than {maxDuration}");
            }

            var duration0 = (highFirst)? highDuration : lowDuration;
            var duration1 = (highFirst)? lowDuration : highDuration;

            return new RmtItem()
            {
                Duration0 = duration0,
                Level0 = highFirst,
                Duration1 = duration1,
                Level1 = !highFirst
            };
        }
    }
}