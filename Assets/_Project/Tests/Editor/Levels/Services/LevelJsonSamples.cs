namespace WaterSortPuzzle.Tests.EditMode.Levels
{
    internal static class LevelJsonSamples
    {
        internal const string ValidLevel = @"
        {
          ""levelNumber"": 7,
          ""difficulty"": 1,
          ""bottleCapacity"": 4,
          ""bottles"": [
            {
              ""liquidIdsBottomToTop"": [""red"", ""blue""],
              ""hiddenLiquidIndices"": [0]
            },
            {
              ""liquidIdsBottomToTop"": [],
              ""hiddenLiquidIndices"": []
            }
          ]
        }";
    }
}
