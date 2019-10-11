using System;

namespace nanoFramework.Hardware.Esp32.Rmt.Helpers
{
    /// <summary>
    /// A class that represents a Phillips Pronto infra-red control code.
    /// </summary>
    public class ProntoCode
    {
        private const int preambleLength = 4;
        private const double frequencyMultiplier = 0.241246;

        private string _code { get; set; }

        private int[] _prontoNumbers { get; set; }

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
        public int[] OnceSequence {get; private set; }

        /// <summary>
        /// The number of burst pairs in <c>OnceSequence</c>.
        /// </summary>        
        public int OnceSequenceBurstPairs { get; private set; }
        
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

        /// <summary>
        /// Initializes a new instance of the <c>ProntoCode</c> class.
        /// </summary>
        /// <param name="code">A string that contains the code data.</param>
        public ProntoCode(string code)
        {
            _code = code;

            // Convert Pronto string to an array of numbers.
            // A Pronto string is a sequence of hexadecimal numbers, 
            // separated by spaces.
            var prontoSegments = _code.Split(' ');
            _prontoNumbers = new int[prontoSegments.Length];
            for (int i = 0; i < prontoSegments.Length; i++)
            {
                _prontoNumbers[i] = Convert.ToInt32(prontoSegments[i], 16);
            }

            // Get code information from the preamble
            Type = _prontoNumbers[0];            
            Frequency = 1 / (_prontoNumbers[1] * frequencyMultiplier);            
            OnceSequenceBurstPairs = _prontoNumbers[2];
            RepeatSequenceBurstPairs = _prontoNumbers[3];

            // Validate the string
            if (_prontoNumbers.Length != (preambleLength + 2 * (OnceSequenceBurstPairs + RepeatSequenceBurstPairs)))
            {
                throw new ArgumentException("Number of pulse widths does not equal the preamble");
            }

            // Decode the string into burst sequences.
            if (Type == 0)
            {
                learnedCodeToBurstPairSequences();
            }
            else
            {
                throw new NotImplementedException("Only codes with Type '0000' can currently be parsed.");
            }            
        }

        private void learnedCodeToBurstPairSequences()
        {
            var onceSequenceCount = (OnceSequenceBurstPairs * 2);
            var repeatSequenceCount = (RepeatSequenceBurstPairs * 2);

            // Convert code sequence segments to pulse lengths in microseconds
            if (onceSequenceCount > 0)
            {                
                OnceSequence = new int[onceSequenceCount];
                for (var j =0; j < onceSequenceCount; j++)
                {
                    var codeIndex = j + preambleLength;
                    OnceSequence[j] = (int)Math.Round((double)_prontoNumbers[codeIndex] / Frequency);
                }
            }

            // Convert repeat sequence numbers to pulse lengths in microseconds
            if (repeatSequenceCount > 0)
            {                
                RepeatSequence = new int[repeatSequenceCount];                
                for (var k = 0; k < repeatSequenceCount; k++)
                {
                    var codeIndex = k + preambleLength + onceSequenceCount;
                    RepeatSequence[k] = (int)Math.Round((double)_prontoNumbers[codeIndex] / Frequency);
                }
            }
            
        }        
    }
}