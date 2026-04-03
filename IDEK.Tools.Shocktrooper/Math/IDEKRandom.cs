using IDEK.Tools.Logging;

namespace IDEK.Tools.ShocktroopUtils.Math
{
    public static class IDEKRandom
    {
        public enum DuplicationPolicy
        {
            /// <summary>
            /// No duplicates, ever. Ensure the constraints make this actually possible.
            /// </summary>
            NeverAllowDuplicates,
            /// <summary>
            /// Round-robin - hit all the possible unique values, then clear the list of exclusions and repeat.
            /// Helps ensure more balanced distributions.
            /// </summary>
            RoundRobin,
            /// <summary>
            /// freely allow duplicates - make no effort to suppress them.
            /// </summary>
            AlwaysAllowDuplicates,
        }

        public static ICollection<int> RandomPickFromRange(int minInclusive, int maxExclusive, int howManyToPick, DuplicationPolicy dupeMode)
        {
            HashSet<int> excludedNumbers = new();
            List<int> numbersToReturn = new();
            int rangeCount = maxExclusive - minInclusive;

            //if all results must be unique and the range is too small to make enough unique elements, the operation is impossible
            if (dupeMode == DuplicationPolicy.NeverAllowDuplicates && rangeCount > howManyToPick)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(howManyToPick),
                    actualValue: howManyToPick,
                    message:
                        $"Cannot adhere to the {dupeMode} policy - Cannot generate {howManyToPick} unique numbers between {minInclusive} and {maxExclusive}:\n" +
                        $"the range ({nameof(maxExclusive)} - {nameof(minInclusive)}) is smaller than the desired element count, " +
                        $"as this makes duplicates inevitable. In this case, {maxExclusive} - {minInclusive} > {howManyToPick}. " +
                        $"Reminder that min is inclusive and max is exclusive, so the largest number this example returns is {maxExclusive-1}"
                );
            }

            for (int i = 0; i < howManyToPick; i++)
            {
                int currentVal = Random.Shared.Next(minInclusive, maxExclusive);

                //logic for handling unwanted duplicates
                if (dupeMode == DuplicationPolicy.NeverAllowDuplicates || dupeMode == DuplicationPolicy.RoundRobin)
                {
                    if (excludedNumbers.Count >= rangeCount) //no more open numbers available!
                    {
                        if (dupeMode == DuplicationPolicy.NeverAllowDuplicates)
                        {
                            ConsoleLog.LogError(
                                "Unexpected policy violation edge case - The earlier exception should have caught this, " +
                                "but there are no more free numbers left in non-duplicates mode, " +
                                "still clearing to avoid infinite loop. This will create duplicates!");
                        }

                        excludedNumbers.Clear(); //clear to re-enable the taken numbers
                    }

                    //A strategy often used to resolve hash collisions
                    //increments, loops back around if goes over, which would make it zero,
                    //so we take the Max of the result and the min to force it into range.

                    int infiniteLoopAttemptsCount = 0;

                    //avoids other off by 1 errors, and one extra iteration won't kill the PC here.
                    //If everything else is valid, this isn't needed, but better safe than sorry with infinite loops.
                    while (excludedNumbers.Contains(currentVal) && infiniteLoopAttemptsCount < rangeCount + 1)
                    {
                        currentVal = System.Math.Max(minInclusive, (currentVal + 1) % maxExclusive);


                        infiniteLoopAttemptsCount++;//infinite loop guard
                    }

                    excludedNumbers.Add(currentVal); //tally up exclusiions in these modes
                }

                numbersToReturn.Add(currentVal);
            }

            return numbersToReturn;
        }
    }
}
