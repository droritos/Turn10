using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShapeDatabase", menuName = "ZenGrid/ShapeDatabase")]
public class ShapeDatabase : ScriptableObject
{
    public List<ShapeData> shapes = new List<ShapeData>();
}
