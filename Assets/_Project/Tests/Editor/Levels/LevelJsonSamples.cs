namespace WaterSortPuzzle.Tests.EditMode.Levels
{
    internal static class LevelJsonSamples
    {
        internal const string ValidLevel = @"
        {
          ""levelNumber"": 7,
          ""bottleCapacity"": 4,
          ""bottles"": [
            {
              ""liquidIdsBottomToTop"": [""red"", ""blue""],
              ""hiddenLiquidIndices"": [1]
            },
            {
              ""liquidIdsBottomToTop"": [],
              ""hiddenLiquidIndices"": []
            }
          ]
        }";
    }
}
