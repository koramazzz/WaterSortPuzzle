using System;
using WaterSortPuzzle.Configuration;

namespace WaterSortPuzzle.Levels.Rewards
{
    public sealed class LevelRewardCalculator
    {
        public int CalculateGoldReward(LevelDifficulty difficulty)
        {
            return CalculateGoldReward(
                GameBalance.BaseGoldReward,
                difficulty);
        }

        public int CalculateGoldReward(
            int baseGoldReward,
            LevelDifficulty difficulty)
        {
            if (baseGoldReward <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseGoldReward));
            }

            int multiplier = GetMultiplier(difficulty);
            return checked(baseGoldReward * multiplier);
        }

        public int GetMultiplier(LevelDifficulty difficulty)
        {
            if (!Enum.IsDefined(typeof(LevelDifficulty), difficulty))
            {
                throw new ArgumentOutOfRangeException(nameof(difficulty));
            }

            return (int)difficulty;
        }
    }
}
