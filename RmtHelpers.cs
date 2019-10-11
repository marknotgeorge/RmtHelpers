using System;
using System.Collections;
using System.Collections.Generic;
using nanoFramework.Hardware.Esp32.Rmt;

namespace nanoFramework.Hardware.Esp32.Rmt.Helpers
{
    public static class RmtHelpers
    {
        public static RmtItem[] Pronto2RmtItems(string code, int repeats = 1, byte clockDivider = 80)
        {            
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }

            if (repeats < 1)
            {
                throw new ArgumentOutOfRangeException("repeats", "Must be at least 1.");
            }
            
            int clockFrequency = 80_000_000;
            

            var ticksPerMicroSecond = clockFrequency / clockDivider / 1_000_000;

            var pronto = new ProntoCode(code);

            var itemList = new ArrayList();            
            if (pronto.CodeSequenceBurstPairs > 0)
            {
                for (var i = 0; i < ((pronto.CodeSequenceBurstPairs * 2) - 1); i = i + 2)
                {
                    var highBurst = pronto.CodeSequence[i] / ticksPerMicroSecond;
                    var lowBurst = pronto.CodeSequence[i+1] / ticksPerMicroSecond;

                    var newItem = CreateBurstPair((UInt16)highBurst, (UInt16)lowBurst);
                    itemList.Add(newItem);
                }
            }
            else if (pronto.RepeatSequenceBurstPairs > 0)
            {
                for (var i = 0; i < ((pronto.RepeatSequenceBurstPairs * 2) - 1); i = i + 2)
                {
                    var highBurst = pronto.RepeatSequence[i] / ticksPerMicroSecond;
                    var lowBurst = pronto.RepeatSequence[i+1] / ticksPerMicroSecond;

                    var newItem = CreateBurstPair((UInt16)highBurst, (UInt16)lowBurst);
                    itemList.Add(newItem);
                }
            }

            if (repeats > 1 )
            {
                for (var k = 1; k < repeats; k++)
                {
                    for (var i = 0; i < ((pronto.RepeatSequenceBurstPairs * 2) - 1); i = i + 2)
                    {
                        var highBurst = pronto.RepeatSequence[i] / ticksPerMicroSecond;
                        var lowBurst = pronto.RepeatSequence[i+1] / ticksPerMicroSecond;

                        var newItem = CreateBurstPair((UInt16)highBurst, (UInt16)lowBurst);
                        itemList.Add(newItem);
                    }
                }
            }

            var itemArray = new RmtItem[itemList.Count];

            for (var i = 0; i < itemList.Count; i++)
            {
                itemArray[i] = (RmtItem)itemList[i];
            }

            return itemArray;
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