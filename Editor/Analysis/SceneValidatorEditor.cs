//-----------------------------------------------------------------------
// <copyright file="SceneValidatorEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using Lost.EditorGrid;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SceneValidator))]
    public class SceneValidatorEditor : Editor
    {
        private static readonly int FoldoutId = nameof(SceneValidator).GetHashCode();

        public override void OnInspectorGUI()
        {
            GUILayout.Space(5);

            this.serializedObject.Update();
            var sceneValidator = this.target as SceneValidator;

            int foldoutId = FoldoutId;
            for (int i = 0; i < sceneValidator.Validators.Count; i++)
            {
                var validator = sceneValidator.Validators[i];

                using (new FoldoutScope(foldoutId++, validator?.DisplayName ?? "NULL", out bool visible, out Rect position))
                {
                    if (validator == null)
                    {
                        continue;
                    }

                    // Process Validator Button
                    Rect buttonRect = default;
                    buttonRect.x = position.width - 20;
                    buttonRect.y = position.y + 3;
                    buttonRect.width = 35;
                    buttonRect.height = 18;

                    if (GUI.Button(buttonRect, "Run"))
                    {
                        sceneValidator.ForceProcessIndex(sceneValidator.gameObject.scene, i);
                    }

                    // Validator IsActive Toggle
                    Rect activeToggleRect = position;
                    activeToggleRect.x = position.x + 20;
                    activeToggleRect.y = position.y + 3;
                    activeToggleRect.width = 15;
                    activeToggleRect.height = 15;

                    bool newIsActive = GUI.Toggle(activeToggleRect, validator.IsActive, string.Empty);
                    if (validator.IsActive != newIsActive)
                    {
                        validator.IsActive = newIsActive;
                    }

                    // Show the list of game objects to ignore if visible
                    if (visible)
                    {
                        using (new IndentLevelScope(2))
                        {
                            var objectsToIgnore = this.serializedObject.FindProperty($"validators.Array.data[{i}].objectsToIgnore");

                            GUILayout.Space(5);
                            EditorGUILayout.PropertyField(objectsToIgnore);
                            GUILayout.Space(5);
                        }
                    }
                }

                GUILayout.Space(5);
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Run All"))
            {
                sceneValidator.ProcessAll();
            }

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
