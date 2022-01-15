//-----------------------------------------------------------------------
// <copyright file="ObjectOptimizerEditor.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ObjectOptimizer))]
    public class ObjectOptimizerEditor : Editor
    {
        private string meshRendererCountText;

        private void OnEnable()
        {
            this.UpdateMeshRendererCount();
            SceneView.duringSceneGui += this.OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (this.targets?.Length > 1)
            {
                this.DrawMultiSelectUI();
            }
            else
            {
                this.DrawSingleSelectUI();
            }

            if (Application.isPlaying)
            {
                GUILayout.Space(20);
                if (GUILayout.Button("Unload Unused Assets"))
                {
                    Resources.UnloadUnusedAssets();
                }
            }
        }

        private void DrawMultiSelectUI()
        {
            if (GUILayout.Button("Optimize"))
            {
                foreach (var optimizer in this.targets.Cast<ObjectOptimizer>())
                {
                    optimizer.Optimize();
                }

                this.UpdateMeshRendererCount();
            }

            if (GUILayout.Button("Revert"))
            {
                foreach (var optimizer in this.targets.Cast<ObjectOptimizer>())
                {
                    optimizer.Revert();
                }

                this.UpdateMeshRendererCount();
            }
        }

        private void DrawSingleSelectUI()
        {
            var objectOptimizer = this.target as ObjectOptimizer;

            if (objectOptimizer.IsOptimized)
            {
                if (GUILayout.Button("Revert"))
                {
                    objectOptimizer.Revert();
                    this.UpdateMeshRendererCount();
                }

                GUILayout.Space(20);

                for (int i = 1; i < objectOptimizer.Settings.LODSettings.Count; i++)
                {
                    var setting = objectOptimizer.Settings.LODSettings[i];

                    // setting.Simplifier == ObjectOptimizerSettings.LODSetting.MeshSimplifier.UnityMeshSimplifier
                    if (GUILayout.Button($"Calculate LOD{i}"))
                    {
                        Debug.LogError("Not Implemented Yet");
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Optimize"))
                {
                    objectOptimizer.Optimize();
                    this.UpdateMeshRendererCount();
                }
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            var pixelPosition = new Vector3(5, 20, 1.0f);
            var worldPosition = sceneView.camera.ScreenToWorldPoint(pixelPosition);
            Handles.Label(worldPosition, this.meshRendererCountText);
        }

        private void UpdateMeshRendererCount()
        {
            this.meshRendererCountText = $"Mesh Render Count: {GameObject.FindObjectsOfType<MeshRenderer>().Where(x => x.enabled).Count()}";
        }
    }
}
