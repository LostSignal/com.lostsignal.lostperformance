//-----------------------------------------------------------------------
// <copyright file="ObjectOptimizer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class ObjectOptimizer : MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] private ObjectOptimizerSettings settings;
        
        [ReadOnly] [SerializeField] private bool isOptimized;
        [ReadOnly] [SerializeField] private List<MeshRendererInfo> meshRendererInfos;
        [ReadOnly] [SerializeField] private List<BoxCollider> combinedBoxColliders;
        [ReadOnly] [SerializeField] private List<MeshCollider> combinedMeshColliders;
        #pragma warning restore 0649

        public bool IsOptimized => this.isOptimized;

        public OptimizerSettings Settings => this.settings;

        public void Optimize()
        {
            if (this.isOptimized)
            {
                this.Revert();
            }

            this.isOptimized = true;

            // Collecting and merging MeshRenderers
            this.meshRendererInfos = MeshRendererInfo.GetMeshRendererInfos(new List<GameObject> { this.gameObject });
            MeshCombiner.CreateLODs(this.transform, this.settings.LODSettings, this.meshRendererInfos, generateLODGroup: true);

            // TODO [bgish]: Collect Box Colliders
            // TODO [bgish]: Optimize Box Colliders

            // TODO [bgish]: Collect Mesh Colliders
            // TODO [bgish]: Optimize Mesh Colliders

            this.combinedMeshColliders.ForEach(x => x.enabled = false);
            this.combinedBoxColliders.ForEach(x => x.enabled = false);
            this.combinedMeshColliders.ForEach(x => x.enabled = false);
        }

        public void Revert()
        {
            if (this.isOptimized == false)
            {
                return;
            }

            // Destroying LODs
            MeshCombiner.DestoryLODs(this.transform, this.meshRendererInfos);

            // TODO [bgish]: Delete BoxColliders parent GameObject
            // TODO [bgish]: Delete MeshColliders parent GameObject

            this.combinedBoxColliders.ForEach(x => x.enabled = true);
            this.combinedMeshColliders.ForEach(x => x.enabled = true);

            this.meshRendererInfos.Clear();
            this.combinedBoxColliders.Clear();
            this.combinedMeshColliders.Clear();
            
            this.isOptimized = false;
        }

        [EditorEvents.OnProcessScene]
        private static void CleanupObjectOptimizers(Scene scene)
        {
            if (Application.isEditor && Application.isPlaying == false && Application.isBatchMode && UnityEditor.BuildPipeline.isBuildingPlayer)
            {
                Debug.Log($"ObjectOptimizer.CleanupObjectOptimizers({scene.name})");

                foreach (var objectOptimizer in GameObject.FindObjectsOfType<ObjectOptimizer>().Where(x => x.gameObject.scene == scene))
                {
                    objectOptimizer.CleanUp(true);
                }
            }
        }

        #if UNITY_EDITOR
        private void Awake()
        {
            if (Application.isPlaying && this.isOptimized)
            {
                this.CleanUp();
            }
        }
        #endif

        private void OnValidate()
        {
            if (this.meshRendererInfos == null)
            {
                this.meshRendererInfos = new List<MeshRendererInfo>();
            }

            if (this.combinedBoxColliders == null)
            {
                this.combinedBoxColliders = new List<BoxCollider>();
            }

            if (this.combinedMeshColliders == null)
            {
                this.combinedMeshColliders = new List<MeshCollider>();
            }

            if (this.settings == null)
            {
                // TODO [bgish]: Get the default settings from project settings, not hard coded
                this.settings = EditorUtil.GetAssetByGuid<ObjectOptimizerSettings>("9026b0cfb0d3e9a4d95fd0c8697ec701");
            }
        }

        private void CleanUp(bool unpackPrefabsCompletely = false)
        {
            if (this.isOptimized == false)
            {
                return;
            }

            #if UNITY_EDITOR
            if (unpackPrefabsCompletely && UnityEditor.PrefabUtility.GetPrefabInstanceStatus(this.gameObject) != UnityEditor.PrefabInstanceStatus.NotAPrefab)
            {
                UnityEditor.PrefabUtility.UnpackPrefabInstance(this.gameObject, UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
            }
            #endif

            foreach (var meshRendererInfo in this.meshRendererInfos)
            {
                if (meshRendererInfo.IsIgnored == false)
                {
                    GameObject.DestroyImmediate(meshRendererInfo.MeshRenderer);
                    GameObject.DestroyImmediate(meshRendererInfo.MeshFilter);
                }
            }

            this.combinedBoxColliders.ForEach(x => GameObject.DestroyImmediate(x));
            this.combinedMeshColliders.ForEach(x => GameObject.DestroyImmediate(x));

            MeshCombiner.DeleteEmptyOrDisabledGameObjects(this.transform);

            Destroy(this);
        }
    }
}
