//-----------------------------------------------------------------------
// <copyright file="ObjectOptimizerEditor.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Collections.Generic;
    using System.Linq;
    using Lost.EditorGrid;
    using UnityEditor;
    using UnityEngine;

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
                        // 
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

            if (objectOptimizer.IsOptimized)
            {

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
