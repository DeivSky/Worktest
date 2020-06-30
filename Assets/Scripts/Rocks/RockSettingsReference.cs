using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Rock Settings", menuName = "Worktest/Rock Settings", order = 12)]
public sealed class RockSettingsReference : DataReferenceAsset<RockSettings> { }

#if UNITY_EDITOR
[CustomEditor(typeof(RockSettingsReference))]
[CanEditMultipleObjects]
public class RockThrowerSettingsEditor : Editor
{
    private RockSettingsReference prop;

    private void OnEnable() => prop = (RockSettingsReference) serializedObject.targetObject;

    public override void OnInspectorGUI()
    {
        prop.Value.RockCount = EditorGUILayout.IntSlider(
            "Cantidad de rocas", 
            prop.Value.RockCount, 
            1, 
            100);
        
        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.MinMaxSlider(
                "Tiempo entre lanzamientos",
                ref prop.Value.MinTimeBetweenThrows, 
                ref prop.Value.MaxTimeBetweenThrows, 
                0.5f, 
                10f);
            prop.Value.MinTimeBetweenThrows = EditorGUILayout.FloatField(
                prop.Value.MinTimeBetweenThrows, 
                GUILayout.MaxWidth(40f));
            prop.Value.MaxTimeBetweenThrows = EditorGUILayout.FloatField(
                prop.Value.MaxTimeBetweenThrows, 
                GUILayout.MaxWidth(40f));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.MinMaxSlider(
                "Velocidad de lanzamiento",
                ref prop.Value.MinRockVelocity, 
                ref prop.Value.MaxRockVelocity, 
                0.1f, 
                50f);
            prop.Value.MinRockVelocity = EditorGUILayout.FloatField(
                prop.Value.MinRockVelocity, 
                GUILayout.MaxWidth(40f));
            prop.Value.MaxRockVelocity = EditorGUILayout.FloatField(
                prop.Value.MaxRockVelocity, 
                GUILayout.MaxWidth(40f));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.MinMaxSlider(
                "Ángulo de lanzamiento",
                ref prop.Value.MinThrowAngle, 
                ref prop.Value.MaxThrowAngle, 
                10f, 80f);
            prop.Value.MinThrowAngle = Mathf.Clamp(
                EditorGUILayout.FloatField(
                    prop.Value.MinThrowAngle, 
                    GUILayout.MaxWidth(40f)), 
                10f, 
                prop.Value.MaxThrowAngle);
            prop.Value.MaxThrowAngle = Mathf.Clamp(
                EditorGUILayout.FloatField(
                    prop.Value.MaxThrowAngle, 
                    GUILayout.MaxWidth(40f)), 
                prop.Value.MinThrowAngle, 
                80f);
        EditorGUILayout.EndHorizontal();
        
        prop.Value.Gravity = -Mathf.Clamp(
            Mathf.Abs(
                EditorGUILayout.FloatField(
                    "Gravedad", 
                    prop.Value.Gravity)), 
            0.1f, 
            50f);
        
        prop.Value.BounceMultiplier = EditorGUILayout.Vector2Field(
            "Multiplicador de rebote", 
            prop.Value.BounceMultiplier);
        prop.Value.BounceMultiplier.x = Mathf.Clamp(
            prop.Value.BounceMultiplier.x,
            0.01f, 
            2f);
        prop.Value.BounceMultiplier.y = Mathf.Clamp(
            prop.Value.BounceMultiplier.y, 
            0.01f, 
            2f);
        
        prop.Value.RockPrefab = (GameObject) EditorGUILayout.ObjectField(
            "Prefab de roca", 
            prop.Value.RockPrefab, 
            typeof(GameObject), 
            false);
    }
}
#endif