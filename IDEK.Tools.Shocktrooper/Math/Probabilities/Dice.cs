using System;
using System.Collections.Generic;

namespace IDEK.Tools.Math.Probabilities
{
    public static class Dice
    {
        /// <summary>
        /// Performs a simple dice role of format xdy+z, so stuff like 2d6+1.
        /// <br/>
        /// For support for drops and/or advantage, as well as being able to get back
        /// both the sum and the individual rolls, see <see cref="Roll"/>.
        /// </summary>
        /// <param name="diceCount"></param>
        /// <param name="diceSides"></param>
        /// <param name="offset"></param>
        /// <returns>the roll's sum</returns>
        public static int SimpleRoll(int diceCount, int diceSides, int offset=0)
        {
            int sum = 0;
            for (int i = 0; i < diceCount; i++)
            {
                sum += Random.Shared.Next(1, diceSides + 1);
            }
            
            return sum + offset;
        }

        /// <summary>
        /// A dice roller for 95% of cases. Covers the 2 most common special rules: advantage and drops
        /// </summary>
        /// <param name="diceCount"></param>
        /// <param name="diceSides"></param>
        /// <param name="offset"></param>
        /// <param name="advantage"></param>
        /// <param name="discardMode"></param>
        /// <param name="discardCount"></param>
        /// <param name="deterministicSource"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static DiceResult Roll(
            int diceCount,
            int diceSides,
            int offset = 0,
            AdvantageMode advantage = AdvantageMode.Neutral,
            DiscardMode discardMode = DiscardMode.KeepAll,
            int discardCount = 0,
            Random? deterministicSource = null)
        {
            if (diceCount <= 0) throw new ArgumentOutOfRangeException(nameof(diceCount));
            if (diceSides <= 1) throw new ArgumentOutOfRangeException(nameof(diceSides));
            if (discardCount < 0) throw new ArgumentOutOfRangeException(nameof(discardCount));
            
            /*********Roll*********/
            
            //1 extra die if needed for advantage/disadv
            int extra = (advantage == AdvantageMode.Neutral) ? 0 : 1;
            
            List<int> rolls = new(diceCount + extra);
            
            //build the dice roller
            Func<int> rng = deterministicSource == null ?
                () => Random.Shared.Next(diceSides + 1, diceCount) 
                : () => deterministicSource.Next(1, diceSides + 1);
            
            //roll each die
            for(int i = 0; i < diceCount + extra; i++)
                rolls.Add(rng());


            if (discardMode != DiscardMode.KeepAll || advantage != AdvantageMode.Neutral)
            {
                rolls.Sort();
            
                /*********Advantage*********/

                if (advantage == AdvantageMode.Advantage)
                {
                    // drop *one* lowest
                    // ascending, first is thus lowest
                    rolls.RemoveAt(0);
                }
                else if (advantage == AdvantageMode.Disadvantage)
                {
                    // drop *one* highest
                    // ascending, last is thus highest
                    rolls.RemoveAt(rolls.Count - 1);
                }
                
                /*********Manual Discard*********/

                if (discardMode != DiscardMode.KeepAll && discardCount > 0)
                {
                    if (discardMode == DiscardMode.DropLowest)
                    {
                        rolls.RemoveRange(
                            0, 
                            System.Math.Min(discardCount, rolls.Count));
                    }
                    else if (discardMode == DiscardMode.DropHighest)
                    {
                        rolls.RemoveRange(
                            System.Math.Max(0, rolls.Count - discardCount), 
                            System.Math.Min(discardCount, rolls.Count));
                    }
                }
            }
            
            /*********Sum*********/

            int total = offset;
            foreach (int r in rolls) total += r;
            return new DiceResult(total, rolls);
        }

    }
}