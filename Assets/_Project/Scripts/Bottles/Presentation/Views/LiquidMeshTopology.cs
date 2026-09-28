using UnityEngine.UI;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    internal static class LiquidMeshTopology
    {
        public static void AddPolygonTriangles(
            VertexHelper vertexHelper,
            int firstVertexIndex,
            int vertexCount)
        {
            for (int vertexOffset = 1; vertexOffset < vertexCount - 1; vertexOffset++)
            {
                vertexHelper.AddTriangle(
                    firstVertexIndex,
                    firstVertexIndex + vertexOffset,
                    firstVertexIndex + vertexOffset + 1);
            }
        }

        public static void AddTriangleStripTriangles(
            VertexHelper vertexHelper,
            int firstVertexIndex,
            int vertexCount)
        {
            int sampleCount = vertexCount / 2;

            for (int sampleIndex = 1; sampleIndex < sampleCount; sampleIndex++)
            {
                int currentLowerVertex = firstVertexIndex + sampleIndex * 2;
                int currentUpperVertex = currentLowerVertex + 1;
                int previousLowerVertex = currentLowerVertex - 2;
                int previousUpperVertex = currentUpperVertex - 2;

                vertexHelper.AddTriangle(
                    previousLowerVertex,
                    currentUpperVertex,
                    previousUpperVertex);

                vertexHelper.AddTriangle(
                    previousLowerVertex,
                    currentLowerVertex,
                    currentUpperVertex);
            }
        }
    }
}
