using UnityEngine;

[System.Serializable]
public class ShapeData
{
    public string name;
    public Color color;
    public int[] matrix; // Flat 2D array representation. 1 = filled, 0 = empty
    public int width;
    public int height;
    public int minPhase = 1;

    public int GetCell(int x, int y)
    {
        return matrix[y * width + x];
    }

    public ShapeData Clone()
    {
        ShapeData clone = new ShapeData();
        clone.name = this.name;
        clone.color = this.color;
        clone.width = this.width;
        clone.height = this.height;
        clone.minPhase = this.minPhase;
        clone.matrix = (int[])this.matrix.Clone();
        return clone;
    }

    public void RotateClockwise()
    {
        int[] newMatrix = new int[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // To rotate clockwise: newX = height - 1 - y, newY = x
                int newX = height - 1 - y;
                int newY = x;
                newMatrix[newY * height + newX] = GetCell(x, y);
            }
        }
        matrix = newMatrix;
        int oldWidth = width;
        width = height;
        height = oldWidth;
    }
}
