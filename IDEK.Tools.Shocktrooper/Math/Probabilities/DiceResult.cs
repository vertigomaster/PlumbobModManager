using System;
using System.Collections.Generic;

namespace IDEK.Tools.Math.Probabilities
{
    [Serializable]
    public readonly struct DiceResult
    {
        public int Sum { get; }

        public IReadOnlyList<int> Rolls { get; }

        public DiceResult(int sum, IReadOnlyList<int> rolls)
        {
            Sum = sum;
            Rolls = rolls;
        }

        public void Deconstruct(out int sum, out IReadOnlyList<int> rolls)
        {
            sum = Sum;
            rolls = Rolls;
        }

        public override string ToString() => $"{Sum} [{string.Join(",", Rolls)}]";
    }
}