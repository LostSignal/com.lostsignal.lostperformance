//-----------------------------------------------------------------------
// <copyright file="Optimizer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public enum OptimizeState
    {
        Unoptimized,
        Simplygon,
        UnityMeshSimplifier,
    }

    // When we bake a mesh, save hash value (meshRenderer.count + positions + rotations + scales)

    // Need some sort of a Hide/Show and Calculate Dirty
    // Hide Scene Optimizer (disbables all Volumes and puts scene mesh renderes back to normal)
    // Make your edits
    // Show Scene Optimizer (disable scene volumes, calculate bake hash, if it's changed, then move state back to needs calculating)

    // VolumeOptizer Editor
    //   LOD0/1/2 Stats
    //   Calculate LOD1 [none] [simplygon] [ums]
    //   Calculate LOD2 [none] [simplygon] [ums]

    // OptimizedLOD Editor
    //   [none] [simplygon] [ums]
    //   Show Unoptimzed Stats vs Optimized
    //   Show Mesh Preview

    public abstract class Optimizer : MonoBehaviour
    {
        #if UNITY_EDITOR

        #pragma warning disable 0649
        [Header("Optimizer")]
        [ReadOnly] [SerializeField] private List<MeshRendererInfo> meshRendererInfos;
        [ReadOnly] [SerializeField] private OptimizerSettings settings;
        [ReadOnly] [SerializeField] private bool isOptimized;
        #pragma warning restore 0649

        public OptimizerSettings Settings => this.settings;

        public List<MeshRendererInfo> MeshRendererInfos => this.meshRendererInfos;

        public bool IsOptimized => this.isOptimized;

        public float GetQuality(int lod) => this.settings.LODSettings[lod].Quality;

        public virtual void Optimize(List<MeshRendererInfo> meshRendererInfos, OptimizerSettings settings)
        {
            if (this.isOptimized)
            {
                this.Revert();
            }

            this.meshRendererInfos = meshRendererInfos;
            this.settings = settings;

            this.OnValidate();

            this.isOptimized = true;
        }

        protected virtual void OnValidate()
        {
            if (this.settings == null || this.meshRendererInfos == null)
            {
                return;
            }

            // Creating all LODs
            var validLODs = new List<GameObject>();
            var lods = this.transform.FindOrCreateChild("LODS", typeof(OptimizedLODGroup));
            lods.gameObject.GetOrAddComponent<OptimizedLODGroup>();

            foreach (var lodSettings in this.settings.LODSettings)
            {
                var lodTransform = lods.FindOrCreateChild(lodSettings.Name, typeof(MeshFilter), typeof(MeshRenderer), typeof(OptimizedLOD));
                var optimizedLOD = lodTransform.gameObject.GetOrAddComponent<OptimizedLOD>();
                optimizedLOD.Initialize();

                validLODs.Add(optimizedLOD.gameObject);
            }

            // Removing any non-valid lod children
            for (int i = lods.childCount - 1; i >= 0; i--)
            {
                var child = lods.GetChild(i).gameObject;

                if (validLODs.Contains(child) == false)
                {
                    GameObject.DestroyImmediate(child);
                }
            }

            // Updating the LODGroup
            lods.GetComponent<OptimizedLODGroup>().UpdateLODGroup();
        }

        //// public void OptimizeLOD(int lod, Method method)
        //// {
        ////     var lodTransform = MeshCombiner.GetLODTransform(this.transform, this.settings.LODSettings, lod);
        //// 
        ////     if (method == Method.Unoptimized || this.lodsToCalculate.Contains(lod) == false)
        ////     {
        ////         MeshCombiner.CreateLOD(this.transform, this.settings.LODSettings, this.meshRendererInfos, this.settings.GenerateLODGroup, this.GetMeshName(), this.GetMeshDirectory(), lod);
        ////         
        ////         if (lod != 0)
        ////         {
        ////             this.lodsToCalculate.Add(lod);
        ////         }
        ////     }
        ////     
        ////     if (method == Method.UnityMeshSimplifier)
        ////     {
        ////         var settings = this.overwriteUnityMeshSimplifierSettings ? 
        ////             this.unityMeshSimplifierSettings : 
        ////             this.settings.UnityMeshSimplifierSettings;
        //// 
        ////         ReduceGameObjectWithUnityMeshSimplifier(lodTransform.gameObject, this.GetQuality(lod), settings);
        ////         this.lodsToCalculate.Remove(lod);
        ////     }
        ////     else if (method == Method.Simplygon)
        ////     {
        ////         var settings = this.overwriteSimplygonSettings ? 
        ////             this.simplygonSettings : 
        ////             this.settings.SimplygonSettings;
        //// 
        ////         SimplygonHelper.ReduceGameObject(lodTransform.gameObject, this.GetQuality(lod), lod, settings);
        ////         this.lodsToCalculate.Remove(lod);
        ////     }
        ////     else if (method != Method.Unoptimized)
        ////     {
        ////         Debug.LogError($"Unknown Method {method} found!");
        ////     }
        //// }

        public virtual void Revert()
        {
            if (this.isOptimized)
            {
                this.RevertMeshRendererOptimization();
                this.isOptimized = false;
            }
        }

        public virtual void CleanUp()
        {
            if (this.isOptimized)
            {
                this.CleanUpOptimizedMeshRenderers();
                MeshCombiner.DeleteEmptyOrDisabledGameObjects(this.transform);
                DestroyImmediate(this);
            }
        }

        protected virtual void Awake()
        {
            if (Application.isPlaying)
            {
                this.CleanUp();
            }
        }

        public virtual string GetMeshName() => this.name;

        public abstract string GetMeshDirectory();

        private void RevertMeshRendererOptimization()
        {
            MeshCombiner.DestoryLODs(this.transform, this.meshRendererInfos);
            this.meshRendererInfos.Clear();
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

#endif
    }
}
