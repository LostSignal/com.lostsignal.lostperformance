//-----------------------------------------------------------------------
// <copyright file="SceneOptimizerEditor.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using Lost.EditorGrid;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    //// TODO [bgish]: Add button to toggle gizmo for this class on/off
    //// TODO [bgish]: Update Gizmo to be transparent boxes with the color being green to red based on tri count

    [CustomEditor(typeof(SceneOptimizer), true)]
    public class SceneOptimizerEditor : Editor
    {
        private string meshRendererCountText;

        public SceneOptimizer SceneOptimizer => (SceneOptimizer)this.target;

        //// TODO [bgish]: Add setting for now combining MeshCombineChildren if size > X
        //// TODO [bgish]: How should we handle making LODS for volumes when it has objects that already have LODS?  Should
        ////               NOT send those LODs to Simplygon (make 2 gameobjects, LODed objects and NonLODed objects?

        public override void OnInspectorGUI()
        {
            //// TODO [bgish]: Add Dropdown for how to color the bound boxes in editor

            using (new FoldoutScope(354821, "Base Variables", out bool visible))
            {
                if (visible)
                {
                    using (new IndentLevelScope(1))
                    { 
                        base.OnInspectorGUI();
                    }

                    GUILayout.Space(25);
                }
            }

            this.DrawButtons();
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

        private void DrawButtons()
        {
            bool octreeExists = this.SceneOptimizer.OctreeExists;

            GUILayout.Label("Mesh Combine Volumes");

            if (GUILayout.Button(octreeExists ? "Re-calculate Octree Volumes" : "Calculate Octree Volumes"))
            {
                this.SceneOptimizer.DeleteLODs();
                this.SceneOptimizer.CalculateOctree();
                SceneView.RepaintAll();
                this.UpdateMeshRendererCount();
                EditorUtility.SetDirty(this.target);
            }

            bool volumeOptimizersExist = this.SceneOptimizer.VolumeOptimizers?.Count > 0;

            if (octreeExists && GUILayout.Button(volumeOptimizersExist ? "Regenerate LODs" : "Generate LODs"))
            {
                using (new TimingLogger("Generate LODs"))
                {
                    this.SceneOptimizer.GenerateLODs();
                    this.UpdateMeshRendererCount();
                    EditorUtil.SaveProjectAndScene(this.SceneOptimizer);
                }
            }

            if (octreeExists && volumeOptimizersExist && GUILayout.Button("Delete LODs"))
            {
                this.SceneOptimizer.DeleteLODs();
                this.UpdateMeshRendererCount();
                EditorUtil.SaveProjectAndScene(this.SceneOptimizer);
            }

            GUILayout.Space(20.0f);

            if (octreeExists && volumeOptimizersExist)
            {
                OptimizerEditorUtil.DrawLODButtons(this.SceneOptimizer.GetComponentsInChildren<OptimizedLOD>().ToList(), true);
            }

            if (Application.isPlaying)
            {
                GUILayout.Label("Misc");

                if (GUILayout.Button("Unload Unused Assets"))
                {
                    Resources.UnloadUnusedAssets();
                }
            }
        }

        private void UpdateMeshRendererCount()
        {
            this.meshRendererCountText = $"Mesh Render Count: {GameObject.FindObjectsOfType<MeshRenderer>().Where(x => x.enabled).Count()}";
        }
    }
}
