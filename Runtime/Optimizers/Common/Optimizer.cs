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
        #if UNITY_EDITOR

        #pragma warning disable 0649
        [Header("Optimizer")]
        [ReadOnly] [SerializeField] private List<int> lodsToCalculate;
        [ReadOnly] [SerializeField] private List<MeshRendererInfo> meshRendererInfos;
        [ReadOnly] [SerializeField] private List<GameObject> lods;
        [ReadOnly] [SerializeField] private OptimizerSettings settings;
        [ReadOnly] [SerializeField] private bool isOptimized;

        [SerializeField] private bool overwriteUnityMeshSimplifierSettings;
        [SerializeField] private UnityMeshSimplifierSettings unityMeshSimplifierSettings;

        [SerializeField] private bool overwriteSimplygonSettings;
        [SerializeField] private SimplygonSettings simplygonSettings;
        #pragma warning restore 0649

        public enum Method
        {
            Unoptimized,
            Simplygon,
            UnityMeshSimplifier,
        }

        public int LODSToCalculateCount => this.lodsToCalculate == null ? 0 : this.lodsToCalculate.Count;

        public List<int> LODSToCalculate 
        { 
            get => this.lodsToCalculate;
            protected set => this.lodsToCalculate = value;
        }
        
        public List<GameObject> LODs => this.lods;

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

            // Generating unoptimized verions of each LOD
            for (int i = 0; i < settings.LODSettings.Count; i++)
            {
                OptimizeLOD(i, Method.Unoptimized);
            }

            // Updating LODs List
            this.lods.Clear();
            
            var lods = MeshCombiner.GetLODsTransform(this.transform, false);
            for (int i = 0; i < lods.childCount; i++)
            {
                this.lods.Add(lods.GetChild(i).gameObject);
            }
            
            this.isOptimized = true;
        }

        public void OptimizeLOD(int lod, Method method)
        {
            var lodTransform = MeshCombiner.GetLODTransform(this.transform, this.settings.LODSettings, lod);

            if (method == Method.Unoptimized || this.lodsToCalculate.Contains(lod) == false)
            {
                MeshCombiner.CreateLOD(this.transform, this.settings.LODSettings, this.meshRendererInfos, this.settings.GenerateLODGroup, this.GetMeshName(), this.GetMeshDirectory(), lod);
                
                if (lod != 0)
                {
                    this.lodsToCalculate.Add(lod);
                }
            }
            
            if (method == Method.UnityMeshSimplifier)
            {
                var settings = this.overwriteUnityMeshSimplifierSettings ? 
                    this.unityMeshSimplifierSettings : 
                    this.settings.UnityMeshSimplifierSettings;

                ReduceGameObjectWithUnityMeshSimplifier(lodTransform.gameObject, this.GetQuality(lod), settings);
                this.lodsToCalculate.Remove(lod);
            }
            else if (method == Method.Simplygon)
            {
                var settings = this.overwriteSimplygonSettings ? 
                    this.simplygonSettings : 
                    this.settings.SimplygonSettings;

                SimplygonHelper.ReduceGameObject(lodTransform.gameObject, this.GetQuality(lod), lod, settings);
                this.lodsToCalculate.Remove(lod);
            }
            else if (method != Method.Unoptimized)
            {
                Debug.LogError($"Unknown Method {method} found!");
            }
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

        private void ReduceGameObjectWithUnityMeshSimplifier(GameObject gameObject, float quality, UnityMeshSimplifierSettings settings)
        {
            #if USING_UNITY_MESH_SIMPLIFIER
            var existingMeshFilter = gameObject.GetComponent<MeshFilter>();
            var existingMesh = existingMeshFilter.sharedMesh;
            var existingMeshName = existingMesh.name;

            var options = new UnityMeshSimplifier.SimplificationOptions
            {
                Agressiveness = settings.Agressiveness,
                EnableSmartLink = settings.EnableSmartLink,
                ManualUVComponentCount = settings.ManualUVComponentCount,
                MaxIterationCount = settings.MaxIterationCount,
                PreserveBorderEdges = settings.PreserveBorderEdges,
                PreserveSurfaceCurvature = settings.PreserveSurfaceCurvature,
                PreserveUVFoldoverEdges = settings.PreserveUVFoldoverEdges,
                PreserveUVSeamEdges = settings.PreserveUVSeamEdges,
                UVComponentCount = settings.UVComponentCount,
                VertexLinkDistance = settings.VertexLinkDistance,
            };

            var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
            meshSimplifier.SimplificationOptions = options;
            meshSimplifier.Initialize(existingMesh);
            meshSimplifier.SimplifyMesh(quality);

            var newMesh = meshSimplifier.ToMesh();
            newMesh.RecalculateNormals();
            newMesh.Optimize();
            newMesh.name = existingMeshName;

            existingMesh.Clear();
            UnityEditor.EditorUtility.CopySerialized(newMesh, existingMesh);

            #else
            
            var add = UnityEditor.EditorUtility.DisplayDialog(
                "Add Unity Mesh Simplifier Package?", 
                "Could not find the Unity Mesh Simplifier Package.\nWould you like to add that now?", 
                "Yes", 
                "No");

            if (add)
            {
                PackageManagerUtil.AddGitPackage("com.whinarn.unitymeshsimplifier", "https://github.com/Whinarn/UnityMeshSimplifier.git#v3.0.1");
            }

            #endif
        }
        
        #endif
    }
}
