using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Player Settings", menuName = "Worktest/Player Settings", order = 11)]
public sealed class PlayerSettingsReference : DataReferenceAsset<PlayerSettings> { }

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerSettingsReference))]
[CanEditMultipleObjects]
public class PlayerSettingsEditor : Editor
{
    private PlayerSettingsReference prop;

    private void OnEnable() => prop = (PlayerSettingsReference) serializedObject.targetObject;

    public override void OnInspectorGUI()
    {
        prop.Value.Speed = EditorGUILayout.Slider(
            "Velocidad de movimiento", 
            prop.Value.Speed, 
            0.1f, 
            50f);

        prop.Value.InertiaDuration = EditorGUILayout.Slider(
            "Duración de inerrcia", 
            prop.Value.InertiaDuration, 
            0f, 
            10f);
    }
}
#endif