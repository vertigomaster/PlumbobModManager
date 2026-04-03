using IDEK.Tools.Logging;
using System.Collections.Generic;
using System.Linq;
using System;
using IDEK.Tools.ShocktroopExtensions;

namespace IDEK.Tools.ShocktroopUtils
{
    [Serializable]
    public class BasicRoomCodeGenerator : IRoomCodeGenerator
    {
        private const char ROOMCODE_SEPARATOR = '-';
        public int roomCodeLength = 6;

        public string validRoomCodeChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private int roomCodeHyphenEveryXChars = 3;

        public char[] ValidRoomCodeChars => validRoomCodeChars.ToCharArray();

        public int RoomCodeHyphenEveryXChars => roomCodeHyphenEveryXChars;

        public bool ShouldForceUppercase() => shouldForceUppercase;

        public bool shouldForceUppercase;

        /// <summary>
        /// Given a proper decent source (like a userGUID), generates a corresponding room code hash.
        /// This room code is directly usable.
        /// </summary>
        /// <param name="userGUID"></param>
        /// <returns></returns>
        public string HashIntoRoomCode(string userGUID)
        {
            long sum = 0;

            foreach(char c in userGUID.ToCharArray())
            {
                sum += c * 101;
                sum *= 7;
            }

            return NumIntoDisplayableRoomCode(sum);
        }


        /// <summary>
        /// Goes straight from number to displayble room code with whatever separators or things are involved.
        /// The output of this should be a directly-usable valid room code.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public string NumIntoDisplayableRoomCode(long num)
        {
            //reclamp to positive if overflowed
            if(num <= 0) num += long.MaxValue;

            string roomCodeString = new string(NumToRawRoomCode(num));

            //add hyphens
            return HyphenateRoomCode(roomCodeString);
        }


        /// <summary>
        /// Adds the given number to the room code
        /// 
        /// </summary>
        /// <param name="roomCode"></param>
        /// <param name="numericalOffset"></param>
        /// <returns></returns>
        public string OffsetRoomCode(string roomCode, int numericalOffset)
        {
            long codeSum = RoomCodeToNumber(roomCode);

            //apply offset (alterantive is manually handling place value, and no thanks)
            codeSum += numericalOffset;

            //NumToRawRoomCode(codeSum);
            return NumIntoDisplayableRoomCode(codeSum);
        }


        /// <summary>
        /// Compiles a room code (which is essentially a an arbitrary base representation of a given number) down to a numeric type (long)
        /// Happens to work with both raw and display-able room codes.
        /// Throws error if given string contains invalid characters.
        /// </summary>
        /// <param name="roomCode"></param>
        /// <returns></returns>
        public long RoomCodeToNumber(string roomCode)
        {
            long codeSum = 0;
            List<char> validChars = ValidRoomCodeChars.ToList();
            for(int i = 0; i < roomCode.Length; i++)
            {
                if(roomCode[i] == ROOMCODE_SEPARATOR)
                    continue;

                if(!validChars.TryGetIndexOf(roomCode[i], out int validCharIndex))
                {
                    ConsoleLog.LogError($"Invalid room code \"{roomCode}\" given, cannot offset hash! Must be made out of valid characters only\n Valid characters:\n" +
                        string.Join(",", ValidRoomCodeChars) + $" (and separator {ROOMCODE_SEPARATOR})");

                    return -1;
                }

                codeSum += validCharIndex * (long)System.Math.Pow(validChars.Count, i);
            }

            return codeSum;
        }

        public bool IsValidRoomCode(string roomCode)
        {
            var validCharHash = ValidRoomCodeChars.ToHashSet();
            return roomCode.All(x => validCharHash.Contains(x));
        }


        /// <summary>
        /// Inserts hyphens into a RAW room code string, per settings
        /// </summary>
        /// <param name="rawRoomCodeString"></param>
        /// <returns></returns>
        private string HyphenateRoomCode(string rawRoomCodeString)
        {
            if(roomCodeHyphenEveryXChars > 0)
            {
                for(int i = rawRoomCodeString.Length - 1; i > 0; i--)
                {
                    if(i % roomCodeHyphenEveryXChars == 0)
                        rawRoomCodeString = rawRoomCodeString.Insert(i, ROOMCODE_SEPARATOR.ToString());
                }
            }

            return rawRoomCodeString;
        }


        /// <summary>
        /// Generates a room code string without any hyphens or bells and whistles, just a pure "raw" code
        /// </summary>
        /// <param name="sum"></param>
        /// <returns></returns>
        private char[] NumToRawRoomCode(long sum)
        {
            char[] newRoomCodeCharArray = new char[roomCodeLength];
            char[] validChars = ValidRoomCodeChars;

            long shiftedSum = sum;

            //shifted sumcheck handles cases where the sum is actually smaller than what is needed for a full room code
            for(int i = 0; i < newRoomCodeCharArray.Length; i++)
            {
                //char is UTF-16 by default, but we have a particular subset
                int digitValue = (int)(shiftedSum % validChars.Length);

                //this seems to happen implicitly, no need for case
                if(digitValue >= validChars.Length)
                {
                    ConsoleLog.LogError($"Math Error: choppedSum % validCharsTotal = {digitValue}, " +
                        $"max allowed is validRoomCodeChars.Length - 1 ({validChars.Length - 1})");

                    return null;
                }

                newRoomCodeCharArray[i] = validChars[digitValue];

                //"bitshift" right, but by the base instead of 2
                shiftedSum /= validChars.Length;
            }

            return newRoomCodeCharArray;
        }

    }
}
