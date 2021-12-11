//-----------------------------------------------------------------------
// <copyright file="MeshCombinerEditor.cs" company="Lost Signal">
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

    [CanEditMultipleObjects]
    [CustomEditor(typeof(MeshCombiner), true)]
    public class MeshCombinerEditor : Editor
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

            if (GUILayout.Button("TEST"))
            {
                foreach (var meshCombiner in this.targets.Cast<MeshCombineChildren>())
                {
                    meshCombiner.Test();
                }
            }

            int maxLodCout = 3;
            for (int i = 0; i < maxLodCout; i++)
            {
                GUILayout.Label($"LOD {i}");

                using (new IndentLevelScope(2))
                {
                    if (GUILayout.Button("Create LOD"))
                    {
                        foreach (var meshCombiner in this.targets.Cast<MeshCombiner>())
                        {
                            meshCombiner.CreateLOD(i);
                        }

                        this.UpdateMeshRendererCount();
                    }

                    if (GUILayout.Button("Simplify Mesh"))
                    {
                        foreach (var meshCombiner in this.targets.Cast<MeshCombiner>())
                        {
                            meshCombiner.SimplifyLODMesh(i);
                        }

                        this.UpdateMeshRendererCount();
                    }

                    if (GUILayout.Button("Separate Mesh"))
                    {
                        foreach (var meshCombiner in this.targets.Cast<MeshCombiner>())
                        {
                            meshCombiner.SeperateMesh(i);
                        }

                        this.UpdateMeshRendererCount();
                    }
                }
                
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Generate LODGroup Component"))
            {
                foreach (var meshCombiner in this.targets.Cast<MeshCombiner>())
                {
                    meshCombiner.GenerateLODGroup();
                }

                this.UpdateMeshRendererCount();
            }

            if (GUILayout.Button("Select All Objects"))
            {
                List<GameObject> newSelection = new List<GameObject>();

                foreach (var meshCombiner in this.targets.Cast<MeshCombiner>())
                {
                    newSelection.AddRange(meshCombiner.GetGameObjectsToCombine(0));
                }

                Selection.objects = newSelection.Cast<UnityEngine.Object>().ToArray();
            }

            if (GUILayout.Button("Hide Selected Objects in Editor"))
            {
                SceneVisibilityManager.instance.Hide(Selection.objects.Where(x => x is GameObject).Cast<GameObject>().ToArray(), false);
            }

            if (GUILayout.Button("Revert"))
            {
                foreach (var meshCombiner in this.targets.Cast<MeshCombiner>())
                {
                    meshCombiner.Revert();
                }

                this.UpdateMeshRendererCount();
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
