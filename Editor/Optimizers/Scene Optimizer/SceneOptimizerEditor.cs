//-----------------------------------------------------------------------
// <copyright file="SceneOptimizerEditor.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.EditorCoroutines.Editor;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    //// TODO [bgish]: Add button to toggle gizmo for this class on/off
    //// TODO [bgish]: Update Gizmo to be transparent boxes with the color being green to red based on tri count

    [CustomEditor(typeof(SceneOptimizer), true)]
    public class SceneOptimizerEditor : Editor
    {
        private string meshRendererCountText;
        private List<GameObject> all = new List<GameObject>();
        private List<GameObject> lod1RecalculateList = new List<GameObject>();
        private List<GameObject> lod2RecalculateList = new List<GameObject>();

        public SceneOptimizer SceneOptimizer => (SceneOptimizer)this.target;

        //// TODO [bgish]: Add setting for now combining MeshCombineChildren if size > X
        //// TODO [bgish]: How should we handle making LODS for volumes when it has objects that already have LODS?  Should
        ////               NOT send those LODs to Simplygon (make 2 gameobjects, LODed objects and NonLODed objects?

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // TODO [bgish]: Add Dropdown for how to color the bound boxes in editor

            GUILayout.Space(25);
            GUILayout.Label("Mesh Combine Volumes");

            if (GUILayout.Button("Calculate Octree Volumes"))
            {
                this.SceneOptimizer.CalculateOctree();
                SceneView.RepaintAll();
                this.UpdateMeshRendererCount();
                EditorUtility.SetDirty(this.target);
            }

            if (GUILayout.Button("Generate LODs"))
            {
                using (new TimingLogger("Generate LODs"))
                {
                    this.SceneOptimizer.GenerateLODs();
                    this.UpdateMeshRendererCount();
                    this.SaveAll();
                }
            }

            //// // TODO [bgish]: Hard coding LOD1/LOD2 right here, but should make this programatic
            //// if (this.lod1RecalculateList.Count > 0 && GUILayout.Button($"Calculate LOD1s ({this.lod1RecalculateList.Count})"))
            //// {
            ////     SimplygonHelper.ReduceGameObjects(this.lod1RecalculateList, this.SceneOptimizer.Settings.LODSettings[1].Quality, 1);                
            ////     this.UpdateRecalculateLists();
            //// }
            //// 
            //// // TODO [bgish]: Hard coding LOD1/LOD2 right here, but should make this programatic
            //// if (this.lod2RecalculateList.Count > 0 && GUILayout.Button($"Calcuate LOD2s ({this.lod2RecalculateList.Count})"))
            //// {
            ////     SimplygonHelper.ReduceGameObjects(this.lod2RecalculateList, this.SceneOptimizer.Settings.LODSettings[2].Quality, 2);
            ////     this.UpdateRecalculateLists();
            //// }

            if (GUILayout.Button($"Recalculate All Simplygon LODs ({this.all.Count})"))
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(this.SimplygonAll());
            }

            if (GUILayout.Button("Delete LODs"))
            {
                this.SceneOptimizer.DeleteLODs();
                this.UpdateMeshRendererCount();
            }

            //// GUILayout.Label("Mesh Combine Children");
            //// 
            //// if (GUILayout.Button("Build MeshCombine Children LOD0"))
            //// {
            ////     foreach (var meshCombineChildren in GameObject.FindObjectsOfType<MeshCombineChildren>())
            ////     {
            ////         meshCombineChildren.CreateLOD(0);
            ////     }
            //// 
            ////     this.UpdateMeshRendererCount();
            //// }
            //// 
            //// if (GUILayout.Button("Split up meshes by material"))
            //// {
            ////     foreach (var meshCombiner in GameObject.FindObjectsOfType<MeshCombiner>())
            ////     {
            ////         meshCombiner.SeperateMesh(0);
            ////     }
            //// 
            ////     this.UpdateMeshRendererCount();
            //// }

            if (Application.isPlaying)
            {
                GUILayout.Label("Misc");
                
                if (GUILayout.Button("Unload Unused Assets"))
                {
                    Resources.UnloadUnusedAssets();
                }

                //// if (GUILayout.Button("Simulate Build Step"))
                //// {
                ////     this.OnBuild();
                ////     this.DeleteDisabledMeshRenderers();
                ////     this.UpdateMeshRendererCount();
                ////     Resources.UnloadUnusedAssets();
                //// }
            }

            //// NOTE [bgish]: There is a very good chance that this should only affect the Octree ones
            //// TODO [bgish]: Add button "Create Octree MeshCombiners LOD0"
            //// TODO [bgish]: Add button "Create Octree MeshCombiners LOD1+"
            //// TODO [bgish]: Add button "Revert All MeshCombiners"
            ///
            //// TODO [bgish]: Add button "Create MeshCombiner Meshes"
            //// TODO [bgish]: Add button "Expand all MeshCombiner Meshes"
            //// TODO [bgish]: Add button "Colapse all MeshCombiner Meshes"

            //// TODO [bgish]: Export LOD Meshes (specific folder next to scene file)
            //// TODO [bgish]: Delete Unused LOD Meshes (Show Count of Unused)
            //// TODO [bgish]: Delete Temp Simplygon Meshes (Show MB of temp files)
            //// TODO [bgish]: Optimize Meshes (Show Count of MeshCombineLOD's that need optimizing).
            ////               Should bring up progress bar, and see if we can get Simplygon to do more
            ////               than 1 at a time.

            //// Have a button for "STOP MESH COMBINE", disables all components and creates hash codes
            //// Now that you have all your renderers back, make your changes
            //// Press "START MESH COMBINE" to bring the optimization back and should have a list of "what chagned"
            //// with the option to update those changed areas.
        }

        private IEnumerator SimplygonAll()
        {
            // https://docs.unity3d.com/2021.2/Documentation/ScriptReference/Progress.html
            // int progressId = Progress.Start("Running one task", 0);

            var startTime = DateTime.Now;

            this.all = this.all.OrderBy(x => x.GetFullName()).ToList();

            for (int i = 0; i < this.all.Count; i++)
            {
                var gameObject = this.all[i];
                var meshFilter = gameObject.GetComponent<MeshFilter>();

                // Progress.Report(progressId, "any progress update or null if it hasn't changed", frame / 1000.0f);
                
                int lod = this.lod1RecalculateList.Contains(gameObject) ? 1 : 2;
                
                SimplygonHelper.ReduceGameObjects(new List<GameObject> { gameObject }, this.SceneOptimizer.Settings.LODSettings[lod].Quality, lod);
                
                //// this.ReduceGameObject(gameObject, this.SceneOptimizer.Settings.LODSettings[lod].Quality);

                if (i > 1 && i % 50 == 0)
                {
                    this.SaveAll();
                }

                int completedJobs = i + 1;
                int jobsRemaining = this.all.Count - completedJobs;
                var totalSeconds = DateTime.Now.Subtract(startTime).TotalSeconds;
                var averageSecondsPerJob = totalSeconds / completedJobs;
                var secondsRemaining = averageSecondsPerJob * jobsRemaining;
                var timeRemaining = TimingLogger.GetTimeAsString(TimeSpan.FromSeconds(secondsRemaining));

                Debug.Log($"Finished Simplygon {i + 1} of {this.all.Count} - {meshFilter.sharedMesh.name}.  {timeRemaining} remaining...");

                yield return null;

                if (i == 1)
                {
                    break;
                }
            }

            // Progress.Remove(progressId);
            this.UpdateRecalculateLists();
        }

        private void SaveAll()
        {
            // Saving assets and the current scene
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(this.SceneOptimizer.gameObject.scene);
        }

        private void OnBuild()
        {
            // Need MeshCombineLOD class
            //    bool IsExpanded
            //    bool NeedsOptimizing
            //    void Optimize();
            //    ExpandMesh();
            //    CollapseMesh();

            // Need MeshCombineLODs class
            //    Responsible for determining making an LODGroup or to stream LODS in with Addressables
            //    ExpandAll();
            //    CollapseAll();

            // Generate MeshCombineVolumes
            // Generate MeshCombineChildren

            // If has a Material Combiner
            //     Expand All MeshCombiners Meshes
            //     Replace All Materials
            //     Collapse All MeshCombiners (update UVs)

            // If using LODGroups
            //    Generate LODGroups
            //    Delete All MeshCombineLOD and MeshCombineLODs

            // Delete All MeshCombineChildren GameObjects
            // Delete All MeshCombineList GameObjects
            // Delete All MeshCombineManager Components
        }

        private void DeleteDisabledMeshRenderers()
        {
            foreach (var meshRenderer in GameObject.FindObjectsOfType<MeshRenderer>())
            {
                var meshGameObject = meshRenderer.gameObject;
                var meshFilter = meshGameObject.GetComponent<MeshFilter>();

                if (meshRenderer.enabled == false)
                {
                    GameObject.DestroyImmediate(meshRenderer);

                    if (meshFilter != null)
                    {
                        GameObject.DestroyImmediate(meshFilter);
                    }

                    // If this GameObject is empty, then delete the whole object too
                    if (meshGameObject.GetComponents<Component>().Length == 1)
                    {
                        GameObject.DestroyImmediate(meshGameObject);
                    }
                }
            }
        }

        private void OnEnable()
        {
            this.UpdateRecalculateLists();
            this.UpdateMeshRendererCount();
            SceneView.duringSceneGui += this.OnSceneGUI;
        }

        private void OnDisable()
        {
            this.lod1RecalculateList.Clear();
            this.lod2RecalculateList.Clear();

            SceneView.duringSceneGui -= this.OnSceneGUI;
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

        private void UpdateRecalculateLists()
        {
            this.all.Clear();
            this.lod1RecalculateList.Clear();
            this.lod2RecalculateList.Clear();

            foreach (var lodGroup in (this.target as SceneOptimizer).GetComponentsInChildren<LODGroup>())
            {
                var lods = lodGroup.GetLODs();

                var lod0 = lods[0].renderers.FirstOrDefault().GetComponent<MeshFilter>();
                var lod1 = lods[1].renderers.FirstOrDefault().GetComponent<MeshFilter>();
                var lod2 = lods[2].renderers.FirstOrDefault().GetComponent<MeshFilter>();

                if (lod1.sharedMesh.triangles.Length >= lod0.sharedMesh.triangles.Length)
                {
                    this.lod1RecalculateList.Add(lod1.gameObject);
                }

                if (lod2.sharedMesh.triangles.Length >= lod1.sharedMesh.triangles.Length)
                {
                    this.lod2RecalculateList.Add(lod2.gameObject);
                }

                this.all.Add(lod1.gameObject);
                this.all.Add(lod2.gameObject);
            }
        }

        private void ReduceGameObject(GameObject gameObject, float quality)
        {
            var sourceMeshFilter = gameObject.GetComponent<MeshFilter>();
            var sourceMesh = sourceMeshFilter.sharedMesh;
            var sourceMeshName = sourceMesh.name;

            var options = UnityMeshSimplifier.SimplificationOptions.Default;
            options.PreserveBorderEdges = true;
            options.PreserveUVSeamEdges = true;
            options.PreserveSurfaceCurvature = true;

            //// options.VertexLinkDistance = 0.0001;
            //// options.PreserveUVFoldoverEdges = true;
            //// options.Agressiveness = 7.0f * 5;
            //// options.MaxIterationCount = 100 * 5;

            // TODO [bgish]: Need to wrap in USING_UNITY_MESH_SIMPLIFIER and print error (not installed if it fails)
            // Also, add "com.whinarn.unitymeshsimplifier": "https://github.com/Whinarn/UnityMeshSimplifier.git#v3.0.0", to your manifest
            var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
            meshSimplifier.SimplificationOptions = options;
            meshSimplifier.Initialize(sourceMesh);
            meshSimplifier.SimplifyMesh(quality);

            var newMesh = meshSimplifier.ToMesh();
            newMesh.RecalculateNormals();
            newMesh.Optimize();
            newMesh.name = sourceMeshName;

            EditorUtility.CopySerialized(newMesh, sourceMesh);            
        }
    }
}
