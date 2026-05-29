using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneAttribute))]
public class SceneDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.HelpBox(position, "[Scene] attribute requires a string field.", MessageType.Error);
            return;
        }

        var scenes = EditorBuildSettings.scenes;
        if (scenes.Length == 0)
        {
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.HelpBox(position, "No scenes in Build Settings.", MessageType.Warning);
            return;
        }

        // 씬 이름 목록 (경로에서 파일명만 추출)
        string[] sceneNames = new string[scenes.Length + 1];
        sceneNames[0] = "None";
        for (int i = 0; i < scenes.Length; i++)
            sceneNames[i + 1] = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);

        // 현재 값의 인덱스 찾기
        string current = property.stringValue;
        int selectedIndex = 0;
        for (int i = 1; i < sceneNames.Length; i++)
        {
            if (sceneNames[i] == current)
            {
                selectedIndex = i;
                break;
            }
        }

        EditorGUI.BeginChangeCheck();
        int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, sceneNames);
        if (EditorGUI.EndChangeCheck())
            property.stringValue = newIndex == 0 ? "" : sceneNames[newIndex];
    }
}
