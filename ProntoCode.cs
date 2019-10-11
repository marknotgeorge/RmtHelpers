using System;

namespace nanoFramework.Hardware.Esp32.Rmt.Helpers
{
    public class ProntoCode
    {
        private const int preambleLength = 4;
        private const double frequencyMultiplier = 0.241246;

        private string _code { get; set; }

        /// <summary>
        /// The frequency of the carrier wave in KiloHertz
        /// </summary>       
        public double Frequency { get; private set; }
        
        /// <summary>
        /// The sequence of burst pairs when the code is sent once.
        /// </summary>
        /// <remarks>
        /// An even index is the first segment of a brst pair, and is the 
        /// length of time in microseconds the IR emitter is turned on. 
        /// An odd index is the length of time in microseconds the emitter is off.
        /// </remarks>
        public int[] CodeSequence {get; private set; }

        /// <summary>
        /// The number of burst pairs in <c>CodeSequence</c>.
        /// </summary>        
        public int CodeSequenceBurstPairs { get; private set; }
        
        /// <summary>
        /// The sequence of burst pairs when the code is repeated.
        /// </summary>
        /// <remarks>
        /// An even index is the first segment of a brst pair, and is the 
        /// length of time in microseconds the IR emitter is turned on. 
        /// An odd index is the length of time in microseconds the emitter is off.
        /// </remarks>
        public int[] RepeatSequence {get; private set;}

        /// <summary>
        /// The number of burst pairs in <c>RepeatSequence</c>.
        /// </summary>
        /// <value></value>
        public int RepeatSequenceBurstPairs { get; private set; }
        
        /// <summary>
        /// The type of the Pronto code. A value of 0 means the code is Learned
        /// </summary>        
        public int Type { get;  private set; }

        public ProntoCode(string code)
        {
            _code = code;

            // Convert Pronto string to an array of numbers.
            // A Pronto string is a sequence of hexadecimal numbers, 
            // separated by spaces.
            var prontoSegments = _code.Split(' ');
            var prontoNumbers = new int[prontoSegments.Length];
            for (int i = 0; i < prontoSegments.Length; i++)
            {
                prontoNumbers[i] = Convert.ToInt32(prontoSegments[i], 16);
            }

            // Get code information from the preamble
            Type = prontoNumbers[0];            
            Frequency = 1 / (prontoNumbers[1] * frequencyMultiplier);            
            CodeSequenceBurstPairs = prontoNumbers[2];
            RepeatSequenceBurstPairs = prontoNumbers[3];

            // Validate the string
            if (prontoNumbers.Length != (preambleLength + 2 * (CodeSequenceBurstPairs + RepeatSequenceBurstPairs)))
            {
                throw new ArgumentException("Number of pulse widths does not equal the preamble");
            }

            var codeSequenceNumbers = (CodeSequenceBurstPairs * 2);
            var repeatSequenceNumbers = (RepeatSequenceBurstPairs * 2);

            // Convert code sequence segments to pulse lengths in microseconds
            if (codeSequenceNumbers > 0)
            {                
                CodeSequence = new int[codeSequenceNumbers];
                for (var j =0; j < codeSequenceNumbers; j++)
                {
                    var codeIndex = j + preambleLength;
                    CodeSequence[j] = (int)Math.Round((double)prontoNumbers[codeIndex] / Frequency);
                }
            }

            // Convert repeat sequence numbers to pulse lengths in microseconds
            if (repeatSequenceNumbers > 0)
            {                
                RepeatSequence = new int[repeatSequenceNumbers];                
                for (var k = 0; k < repeatSequenceNumbers; k++)
                {
                    var codeIndex = k + preambleLength + codeSequenceNumbers;
                    RepeatSequence[k] = (int)Math.Round((double)prontoNumbers[codeIndex] / Frequency);
                }
            }
        }        
    }
}