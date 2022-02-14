//-----------------------------------------------------------------------
// <copyright file="Optimizer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public enum OptimizeState
    {
        None,
        Unoptimized,
        Simplygon,
        UnityMeshSimplifier,
    }

    public abstract class Optimizer : MonoBehaviour
    {
        #if UNITY_EDITOR

        #pragma warning disable 0649
        [Header("Optimizer")]
        [ReadOnly] [SerializeField] private List<MeshRendererInfo> meshRendererInfos;
        [ReadOnly] [SerializeField] private bool isOptimized;
        #pragma warning restore 0649

        public abstract OptimizerSettings Settings { get; }

        public List<MeshRendererInfo> MeshRendererInfos => this.meshRendererInfos;

        public bool IsOptimized => this.isOptimized;

        public virtual void Optimize(List<MeshRendererInfo> meshRendererInfos)
        {
            if (this.isOptimized)
            {
                this.Revert();
            }

            this.meshRendererInfos = meshRendererInfos;

            this.OnValidate();

            this.isOptimized = true;
        }

        protected virtual void OnValidate()
        {
            if (this.meshRendererInfos == null)
            {
                return;
            }

            // Creating all LODs
            var validLODs = new List<GameObject>();
            var lods = this.transform.FindOrCreateChild("LODS", typeof(OptimizedLODGroup));
            lods.gameObject.GetOrAddComponent<OptimizedLODGroup>();

            foreach (var lodSettings in this.Settings.LODSettings)
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

                Array.ForEach(this.GetComponentsInChildren<OptimizedLOD>(), x => GameObject.DestroyImmediate(x));
                Array.ForEach(this.GetComponentsInChildren<OptimizedLODGroup>(), x => GameObject.DestroyImmediate(x));
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
