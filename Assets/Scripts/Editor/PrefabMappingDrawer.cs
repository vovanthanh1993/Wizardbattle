using UnityEngine;
using UnityEditor;
using Fusion;

[CustomPropertyDrawer(typeof(PrefabMapping))]
public class PrefabMappingDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var nameProperty = property.FindPropertyRelative("characterName");
        var prefabProperty = property.FindPropertyRelative("prefabRef");

        Rect nameRect = new Rect(position.x, position.y, position.width * 0.4f, position.height);
        Rect prefabRect = new Rect(position.x + position.width * 0.4f + 5, position.y, position.width * 0.6f - 5, position.height);

        EditorGUI.PropertyField(nameRect, nameProperty, GUIContent.none);
        EditorGUI.PropertyField(prefabRect, prefabProperty, GUIContent.none);

        EditorGUI.EndProperty();
    }
}

[System.Serializable]
public class PrefabMapping
{
    public string characterName;
    public NetworkPrefabRef prefabRef;
}
