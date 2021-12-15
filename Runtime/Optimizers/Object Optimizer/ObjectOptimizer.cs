//-----------------------------------------------------------------------
// <copyright file="ObjectOptimizer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ObjectOptimizer : MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] private ObjectOptimizerSettings settings;
        
        [Header("Read Only")]
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

            this.OptimizeMeshRenderers();
            this.OptimizeBoxColliders();
            this.OptimizeMeshColliders();
        }

        public void Revert()
        {
            if (this.isOptimized)
            {
                this.RevertMeshRendererOptimization();
                this.RevertBoxColliderOptimization();
                this.RevertMeshColliderOptimization();
                this.isOptimized = false;
            }
        }

        public void CleanUp()
        {
            if (this.isOptimized)
            {
                this.CleanUpOptimizedMeshRenderers();
                this.CleanUpOptimizedBoxColliders();
                this.CleanUpOptimizedMeshColliders();

                MeshCombiner.DeleteEmptyOrDisabledGameObjects(this.transform);

                DestroyImmediate(this);
            }
        }

        #if UNITY_EDITOR
        private void Awake()
        {
            if (Application.isPlaying)
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

        private void OptimizeMeshRenderers()
        {
            this.meshRendererInfos = MeshRendererInfo.GetMeshRendererInfos(new List<GameObject> { this.gameObject });
            MeshCombiner.CreateLODs(this.transform, this.settings.LODSettings, this.meshRendererInfos, generateLODGroup: true);
        }

        private void OptimizeBoxColliders()
        {
            //// this.combinedBoxColliders = this.GetComponentsInChildren<BoxCollider>().ToList();
            //// 
            //// // TODO [bgish]: Optimize Box Colliders
            //// 
            //// this.combinedBoxColliders.ForEach(x => x.enabled = false);
        }

        private void OptimizeMeshColliders()
        {
            //// this.combinedMeshColliders = this.GetComponentsInChildren<MeshCollider>().ToList();
            //// 
            //// // TODO [bgish]: Optimize Mesh Colliders
            //// 
            //// this.combinedMeshColliders.ForEach(x => x.enabled = false);
        }

        private void RevertMeshRendererOptimization()
        {
            MeshCombiner.DestoryLODs(this.transform, this.meshRendererInfos);
            this.meshRendererInfos.Clear();
        }

        private void RevertBoxColliderOptimization()
        {
            // TODO [bgish]: Delete BoxColliders parent GameObject
            this.combinedBoxColliders.ForEach(x => x.enabled = true);
            this.combinedBoxColliders.Clear();
        }

        private void RevertMeshColliderOptimization()
        {
            // TODO [bgish]: Delete MeshColliders parent GameObject
            this.combinedMeshColliders.ForEach(x => x.enabled = true);
            this.combinedMeshColliders.Clear();
        }

        private void CleanUpOptimizedMeshRenderers()
        {
            foreach (var meshRendererInfo in this.meshRendererInfos)
            {
                if (meshRendererInfo.IsIgnored == false)
                {
                    GameObject.DestroyImmediate(meshRendererInfo.MeshRenderer);
                    GameObject.DestroyImmediate(meshRendererInfo.MeshFilter);
                }
            }
        }

        private void CleanUpOptimizedBoxColliders()
        {
            //// TODO [bgish]: Implement...
            this.combinedBoxColliders.ForEach(x => GameObject.DestroyImmediate(x));
        }

        private void CleanUpOptimizedMeshColliders()
        {
            //// TODO [bgish]: Implement...
            this.combinedMeshColliders.ForEach(x => GameObject.DestroyImmediate(x));
        }
    }
}
