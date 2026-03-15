#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Veresk.World.Core;

namespace Veresk.World.EditorTools
{
    [CustomEditor(typeof(WorldGenerator))]
    public class WorldGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            WorldGenerator generator = (WorldGenerator)target;

            serializedObject.Update();

            DrawReferencesSection();
            DrawRuntimeSection(generator);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Generation Controls", EditorStyles.boldLabel);

            GUI.enabled = !Application.isPlaying;
            if (GUILayout.Button("Generate World"))
            {
                generator.GenerateWorld();
                EditorUtility.SetDirty(generator);
            }
            GUI.enabled = true;

            EditorGUILayout.Space();

            if (generator.Settings != null)
            {
                EditorGUILayout.HelpBox(
                    $"Configured Seed: {generator.Settings.seed}\n" +
                    $"Last Generated Seed: {generator.LastGeneratedSeed}\n" +
                    $"Generated: {generator.HasGenerated}\n\n" +
                    $"Менять нужно seed в asset WS_World_Main, а не поле Last Generated Seed.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Assign WorldSettings. Менять seed нужно в asset WorldSettings.",
                    MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawReferencesSection()
        {
            EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("worldSettings"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("oceanMaterial"));
        }

        private void DrawRuntimeSection(WorldGenerator generator)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime State", EditorStyles.boldLabel);

            GUI.enabled = false;
            EditorGUILayout.IntField("Last Generated Seed", generator.LastGeneratedSeed);
            EditorGUILayout.Toggle("Has Generated", generator.HasGenerated);
            GUI.enabled = true;
        }
    }
}
#endif