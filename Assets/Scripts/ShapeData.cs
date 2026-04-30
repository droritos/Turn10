using UnityEngine;

[System.Serializable]
public class ShapeData
{
    public string name;
    public Color color;
    public int[] matrix; // Flat 2D array representation. 1 = filled, 0 = empty
    public int width;
    public int height;

    public int GetCell(int x, int y)
    {
        return matrix[y * width + x];
    }
}
