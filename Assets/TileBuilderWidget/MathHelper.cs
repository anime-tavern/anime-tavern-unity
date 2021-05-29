using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MathHelper
{

    public static float standardDeviationOfVertexData(Dictionary<VertexLocation2D, VertexLocation3D> vertexCollection)
    {
        float sum = 0.0f;
        float mean = 0.0f;
        float standardDeviation = 0.0f;

        foreach (VertexLocation2D location2D in vertexCollection.Keys)
        {
            sum += vertexCollection[location2D].y;
        }

        mean = sum / vertexCollection.Keys.Count;
        foreach (VertexLocation2D location2D in vertexCollection.Keys)
        {
            standardDeviation += (float)Math.Pow((double)vertexCollection[location2D].y - mean, 2);
        }

        float coefficient = (1.0f / (float)(vertexCollection.Keys.Count - 1));

        return Mathf.Sqrt(coefficient * standardDeviation);
    }

    public static float gaussianBlurOfNumber(float value, float standardDeviation)
    {
        float eulerAnswer = (float)(Math.Exp(-(Math.Pow((double)value, 2) / (2 * (Math.Pow((double)standardDeviation, 2))))));
        float coefficient = 1 / Mathf.Sqrt(2 * (float)Math.PI * (float)Math.Pow((double)standardDeviation, 2));
        return coefficient * eulerAnswer;
    }

    /**
     * This function didn't work right and I never fixed it. Ignore it. The gaussian calculation for the number wasn't right.
     */
    public static void applyGaussianBlurToVertexData(ref Dictionary<VertexLocation2D, VertexLocation3D> vertexCollection)
    {
        float standardDeviation = MathHelper.standardDeviationOfVertexData(vertexCollection);
        Debug.Log("Standard deviation: " + standardDeviation);
        foreach(VertexLocation2D location2D in vertexCollection.Keys)
        {
            Debug.Log("Old " + vertexCollection[location2D].y);
            vertexCollection[location2D].y = MathHelper.gaussianBlurOfNumber(vertexCollection[location2D].y, standardDeviation);
            Debug.Log("New " + vertexCollection[location2D].y);
        }
    }

    public static void applyBoxFilterToVertexData(ref Dictionary<VertexLocation2D, VertexLocation3D> vertexCollection)
    {

        float strengthRange = TileBuilderWindow.MAX_SMOOTHEN_BRUSH_STRENGTH - TileBuilderWindow.MIN_SMOOTHEN_BRUSH_STRENGTH;
        float sliderPercent = (float)TileBuilderWindow.instance.brushStrengthAsInteger / (float)TileBuilderWindow.MAX_BRUSH_STRENGTH_SLIDER_VALUE;
        float strength = TileBuilderWindow.MIN_SMOOTHEN_BRUSH_STRENGTH + (sliderPercent) * strengthRange;

        foreach (VertexLocation2D location2D in vertexCollection.Keys)
        {
            float totalValue = 0.0f;

            // Sample all adjacent vertices
            // This samples in a + pattern (top, bottom, left, right)
            List<VertexLocation2D> adjacentVertices = WorldGrid.getAdjacentVertices(location2D);
            foreach(VertexLocation2D adjacentVertex in adjacentVertices)
            {
                VertexLocation3D location3D = WorldGrid.getVertexAt2DLocation(adjacentVertex);
                totalValue += location3D.y;
            }

            // Set this location's 3D vertex value to the average
            if (adjacentVertices.Count > 0)
            {
                float originalValue = vertexCollection[location2D].y;
                float newValue = totalValue / (float)adjacentVertices.Count;
                float difference = newValue - originalValue;
                float interpolatedNewValue = difference * strength;
                vertexCollection[location2D].y += interpolatedNewValue;
            }
        }
    }
}
