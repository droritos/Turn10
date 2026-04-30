using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShapeData))]
public class ShapeDataDrawer : PropertyDrawer
{
    private const float padding = 2f;
    private const float lineHeight = 18f;
    private const float boxSize = 24f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return lineHeight;

        SerializedProperty widthProp = property.FindPropertyRelative("width");
        SerializedProperty heightProp = property.FindPropertyRelative("height");

        int width = widthProp.intValue;
        int height = heightProp.intValue;

        float h = lineHeight; // foldout
        h += (lineHeight + padding) * 5; // 5 fields: name, color, minPhase, width, height

        if (width > 0 && height > 0)
        {
            h += padding + (height * boxSize) + padding; // matrix
        }
        else
        {
            h += padding;
        }

        return h;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect foldoutRect = new Rect(position.x, position.y, position.width, lineHeight);
        string name = property.FindPropertyRelative("name").stringValue;
        string labelText = string.IsNullOrEmpty(name) ? label.text : name;
        
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, new GUIContent(labelText), true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            Rect currentRect = new Rect(position.x, position.y + lineHeight + padding, position.width, lineHeight);

            SerializedProperty nameProp = property.FindPropertyRelative("name");
            SerializedProperty colorProp = property.FindPropertyRelative("color");
            SerializedProperty widthProp = property.FindPropertyRelative("width");
            SerializedProperty heightProp = property.FindPropertyRelative("height");
            SerializedProperty minPhaseProp = property.FindPropertyRelative("minPhase");
            SerializedProperty matrixProp = property.FindPropertyRelative("matrix");

            EditorGUI.PropertyField(currentRect, nameProp);
            currentRect.y += lineHeight + padding;

            EditorGUI.PropertyField(currentRect, colorProp);
            currentRect.y += lineHeight + padding;

            EditorGUI.PropertyField(currentRect, minPhaseProp);
            currentRect.y += lineHeight + padding;

            EditorGUI.BeginChangeCheck();
            int newWidth = EditorGUI.IntField(currentRect, "Width", widthProp.intValue);
            currentRect.y += lineHeight + padding;
            int newHeight = EditorGUI.IntField(currentRect, "Height", heightProp.intValue);
            currentRect.y += lineHeight + padding;

            if (EditorGUI.EndChangeCheck())
            {
                newWidth = Mathf.Max(1, newWidth);
                newHeight = Mathf.Max(1, newHeight);

                int[] oldMatrix = new int[matrixProp.arraySize];
                for (int i = 0; i < matrixProp.arraySize; i++)
                {
                    oldMatrix[i] = matrixProp.GetArrayElementAtIndex(i).intValue;
                }

                int oldWidth = widthProp.intValue;
                int oldHeight = heightProp.intValue;

                widthProp.intValue = newWidth;
                heightProp.intValue = newHeight;
                matrixProp.arraySize = newWidth * newHeight;

                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        int oldVal = 0;
                        if (x < oldWidth && y < oldHeight)
                        {
                            int index = y * oldWidth + x;
                            if (index < oldMatrix.Length)
                            {
                                oldVal = oldMatrix[index];
                            }
                        }
                        matrixProp.GetArrayElementAtIndex(y * newWidth + x).intValue = oldVal;
                    }
                }
            }

            int width = widthProp.intValue;
            int height = heightProp.intValue;

            if (width > 0 && height > 0)
            {
                currentRect.height = boxSize;
                currentRect.y += padding;

                float startX = currentRect.x + EditorGUIUtility.labelWidth;

                Color oldColor = GUI.backgroundColor;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Rect boxRect = new Rect(startX + x * boxSize, currentRect.y + y * boxSize, boxSize - 1, boxSize - 1);
                        
                        int index = y * width + x;
                        if (index < matrixProp.arraySize)
                        {
                            SerializedProperty cellProp = matrixProp.GetArrayElementAtIndex(index);
                            bool isFilled = cellProp.intValue == 1;
                            
                            GUI.backgroundColor = isFilled ? colorProp.colorValue : Color.gray;
                            
                            if (GUI.Button(boxRect, GUIContent.none))
                            {
                                cellProp.intValue = isFilled ? 0 : 1;
                            }
                        }
                    }
                }

                GUI.backgroundColor = oldColor;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }
}
