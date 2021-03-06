//-----------------------------------------------------------------------
// <copyright file="ObjectOptimizerEditor.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Collections.Generic;
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
            bool multiSelect = this.targets?.Length > 1;

            if (multiSelect == false)
            {
                base.OnInspectorGUI();
            }
            
            // Drawing UI            
            GUILayout.Space(20.0f);
                
            if (multiSelect)
            {
                this.DrawMultiSelectUI();
            }                
            else
            {
                this.DrawSingleSelectUI();
            }

            // Collecting Optimized LODs
            var optimzedLODs = new List<OptimizedLOD>();

            foreach (var target in this.targets.Select(x => x as ObjectOptimizer).Where(x => x.IsOptimized))
            {
                optimzedLODs.AddRange(target.GetComponentsInChildren<OptimizedLOD>());
            }

            // Drawing Buttons
            GUILayout.Space(20.0f);
            OptimizerEditorUtil.DrawLODButtons(optimzedLODs, multiSelect);
            
            // Special UI for play mode
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
