using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

/// <summary>
/// Custom editor for SoundData that allows auto-populating entries from enum types.
/// This makes it much easier to set up sound mappings.
/// </summary>
[CustomEditor(typeof(SoundDataSO))]
public class SoundDataEditor : Editor
{
    private Type[] availableEnumTypes;
    private string[] enumTypeNames;
    private int selectedEnumIndex = 0;

    private void OnEnable()
    {
        // Find only sound event enums (those ending with "SoundEvent")
        // This filters out all irrelevant enums from your project
        availableEnumTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => 
            {
                string name = assembly.GetName().Name;
                // Only include user assemblies (Assembly-CSharp and plugins)
                return name.StartsWith("Assembly-CSharp") || 
                       (!name.StartsWith("Unity") && 
                        !name.StartsWith("System") && 
                        !name.StartsWith("mscorlib") &&
                        !name.StartsWith("netstandard") &&
                        !name.Contains("Editor") &&
                        !name.StartsWith("Mono."));
            })
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsEnum && 
                          type.IsPublic && 
                          type.Name.EndsWith("SoundEvent")) // Only show *SoundEvent enums
            .OrderBy(type => type.Name)
            .ToArray();

        enumTypeNames = availableEnumTypes.Select(t => t.Name).ToArray();
        
        // If no enums found, add a helpful message
        if (enumTypeNames.Length == 0)
        {
            enumTypeNames = new string[] { "No SoundEvent enums found - check SoundEventEnums.cs" };
        }
    }

    public override void OnInspectorGUI()
    {
        SoundDataSO soundData = (SoundDataSO)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Sound Data Setup Helper", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Select an enum type below and click 'Populate from Enum' to automatically create entries for each enum value. " +
            "This is useful when setting up sound data for characters, weapons, levels, etc.",
            MessageType.Info
        );

        EditorGUILayout.BeginHorizontal();
        selectedEnumIndex = EditorGUILayout.Popup("Enum Type", selectedEnumIndex, enumTypeNames);
        
        if (GUILayout.Button("Populate from Enum", GUILayout.Width(150)))
        {
            if (selectedEnumIndex >= 0 && selectedEnumIndex < availableEnumTypes.Length)
            {
                Undo.RecordObject(soundData, "Populate Sound Data from Enum");
                soundData.PopulateFromEnum(availableEnumTypes[selectedEnumIndex]);
                EditorUtility.SetDirty(soundData);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Sound Entries", EditorStyles.boldLabel);

        // Draw the default inspector for the sound entries
        DrawDefaultInspector();
    }
}





// using UnityEngine;
// using UnityEditor;
// using System;
// using System.Linq;
//
// /// <summary>
// /// Custom editor for SoundData that allows auto-populating entries from enum types.
// /// This makes it much easier to set up sound mappings.
// /// </summary>
// [CustomEditor(typeof(SoundDataSO))]
// public class SoundDataEditor : Editor
// {
//     private Type[] availableEnumTypes;
//     private string[] enumTypeNames;
//     private int selectedEnumIndex = 0;
//
//     private void OnEnable()
//     {
//         // Find all enum types in the project
//         availableEnumTypes = AppDomain.CurrentDomain.GetAssemblies()
//             .SelectMany(assembly => assembly.GetTypes())
//             .Where(type => type.IsEnum && type.IsPublic)
//             .OrderBy(type => type.Name)
//             .ToArray();
//
//         enumTypeNames = availableEnumTypes.Select(t => t.Name).ToArray();
//     }
//
//     public override void OnInspectorGUI()
//     {
//         SoundDataSO soundData = (SoundDataSO)target;
//
//         EditorGUILayout.Space();
//         EditorGUILayout.LabelField("Sound Data Setup Helper", EditorStyles.boldLabel);
//         EditorGUILayout.HelpBox(
//             "Select an enum type below and click 'Populate from Enum' to automatically create entries for each enum value. " +
//             "This is useful when setting up sound data for characters, weapons, levels, etc.",
//             MessageType.Info
//         );
//
//         EditorGUILayout.BeginHorizontal();
//         selectedEnumIndex = EditorGUILayout.Popup("Enum Type", selectedEnumIndex, enumTypeNames);
//         
//         if (GUILayout.Button("Populate from Enum", GUILayout.Width(150)))
//         {
//             if (selectedEnumIndex >= 0 && selectedEnumIndex < availableEnumTypes.Length)
//             {
//                 Undo.RecordObject(soundData, "Populate Sound Data from Enum");
//                 soundData.PopulateFromEnum(availableEnumTypes[selectedEnumIndex]);
//                 EditorUtility.SetDirty(soundData);
//             }
//         }
//         EditorGUILayout.EndHorizontal();
//
//         EditorGUILayout.Space();
//         EditorGUILayout.LabelField("Sound Entries", EditorStyles.boldLabel);
//
//         // Draw the default inspector for the sound entries
//         DrawDefaultInspector();
//     }
// }
