//-----------------------------------------------------------------------
// <copyright file="Optimizer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class Optimizer : MonoBehaviour
    {
        #pragma warning disable 0649
        [Header("Optimizer")]
        [ReadOnly] [SerializeField] private List<int> lodsToCalculate;
        [ReadOnly] [SerializeField] private List<MeshRendererInfo> meshRendererInfos;
        [ReadOnly] [SerializeField] private List<GameObject> lods;
        [ReadOnly] [SerializeField] private bool isOptimized;
        #pragma warning restore 0649

        #if UNITY_EDITOR

        public enum CalculateMethod
        {
            UntiyMeshSimplifier,
            Simplygon,
        }

        public int LODSToCalculateCount => this.lodsToCalculate == null ? 0 : this.lodsToCalculate.Count;

        public List<int> LODSToCalculate 
        { 
            get => this.lodsToCalculate;
            protected set => this.lodsToCalculate = value;
        }
        
        public List<GameObject> LODs => this.lods;

        public bool IsOptimized => this.isOptimized;

        public virtual void Optimize(List<MeshRendererInfo> meshRendererInfos, List<LODSetting> settings, bool generateLODGroup)
        {
            if (this.isOptimized)
            {
                this.Revert();
            }

            this.OptimizeMeshRenderers(meshRendererInfos, settings, generateLODGroup);
            this.isOptimized = true;
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
            }
        }

        protected virtual void Awake()
        {
            if (Application.isPlaying)
            {
                this.CleanUp();
            }
        }

        protected virtual void OnValidate()
        {
            if (this.lodsToCalculate == null)
            {
                this.lodsToCalculate = new List<int>();
            }

            if (this.meshRendererInfos == null)
            {
                this.meshRendererInfos = new List<MeshRendererInfo>();
            }

            if (this.lods == null)
            {
                this.lods = new List<GameObject>();
            }
        }

        protected virtual string GetMeshName() => this.name;

        protected abstract string GetMeshDirectory();

        private void OptimizeMeshRenderers(List<MeshRendererInfo> meshRendererInfos, List<LODSetting> settings, bool generateLODGroup)
        {
            // Setting the mesh renderers we'll be optimizing
            this.meshRendererInfos = meshRendererInfos;

            // Creating the LOD children
            MeshCombiner.CreateLODs(this.transform, settings, this.meshRendererInfos, generateLODGroup, GetMeshName(), GetMeshDirectory());

            // Updating LODS
            this.lods.Clear();

            var lods = this.transform.Find("LODS");
            for (int i = 0; i < lods.childCount; i++)
            {
                var child = lods.GetChild(i);
                this.lods.Add(child.gameObject);
            }

            // Updating LODs to Calculate
            this.lodsToCalculate.Clear();

            for (int i = 1; i < settings.Count; i++)
            {
                this.lodsToCalculate.Add(i);
            }
        }

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
