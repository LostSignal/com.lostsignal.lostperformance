//-----------------------------------------------------------------------
// <copyright file="MeshCombineManagerEditor.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    //// TODO [bgish]: Add button to toggle gizmo for this class on/off
    //// TODO [bgish]: Update Gizmo to be transparent boxes with the color being green to red based on tri count

    [CustomEditor(typeof(SceneOptimizer), true)]
    public class MeshCombineManagerEditor : Editor
    {
        private string meshRendererCountText;

        public SceneOptimizer SceneOptimizer => (SceneOptimizer)this.target;

        //// TODO [bgish]: Add setting for now combining MeshCombineChildren if size > X
        //// TODO [bgish]: How should we handle making LODS for volumes when it has objects that already have LODS?  Should
        ////               NOT send those LODs to Simplygon (make 2 gameobjects, LODed objects and NonLODed objects?

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);

            GUILayout.Label("Mesh Combine Volumes");

            if (GUILayout.Button("Calculate Octree Volumes"))
            {
                this.SceneOptimizer.CalculateOctree();
                SceneView.RepaintAll();
                this.UpdateMeshRendererCount();
            }

            if (GUILayout.Button("Generate MeshCombineVolume Meshes"))
            {
                this.SceneOptimizer.GenerateVolumesMeshCombine();
                this.UpdateMeshRendererCount();
            }

            if (GUILayout.Button("Delete MeshCombineVolume Meshes"))
            {
                this.SceneOptimizer.DeleteVolumesMeshCombine();
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
            }

            if (Application.isPlaying && GUILayout.Button("Simulate Build Step"))
            {
                this.OnBuild();
                this.DeleteDisabledMeshRenderers();
                this.UpdateMeshRendererCount();
                Resources.UnloadUnusedAssets();
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
            this.UpdateMeshRendererCount();
            SceneView.duringSceneGui += this.OnSceneGUI;
        }

        private void OnDisable()
        {
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
    }
}
