//-----------------------------------------------------------------------
// <copyright file="StreamingLODGroup.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    // Make sure SceneOptimizer applies LOD0/1/2 to all Volumes and creates a LODGroup component on the LODS object
    // On Startup/Build, if ScneeOptizer "StreamLODs" flag is on, then get all LODGroups and add StreamingLODGroup component to them

    public class StreamingLODGroup : MonoBehaviour
    {
        public LODGroup lodGroup;
        public MeshFilter[] meshFilters;
        public string[] meshGuids;

        private void OnValidate()
        {
            if (this.lodGroup == null)
            {
                this.lodGroup = this.GetComponentInChildren<LODGroup>();
            }

            if (this.meshFilters == null || this.meshFilters.Length == 0)
            {
                this.meshFilters = this.GetComponentsInChildren<MeshFilter>();
                this.meshGuids = new string[this.meshFilters.Length];

                #if UNITY_EDITOR
                for (int i = 0; i < this.meshFilters.Length; i++)
                {
                    string path = UnityEditor.AssetDatabase.GetAssetPath(this.meshFilters[i].sharedMesh);
                    this.meshGuids[i] = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                }
                #endif
            }
        }

        //// [SerializeField] private List<LOD> lods;
        //// [SerializeField] private bool isStreamingLods;
        //// 
        //// [EditorEvents.OnProcessSceneBuild]
        //// private static void OnProcessSceneBuild(Scene scene)
        //// {
        ////     foreach (var lostLodGroup in GameObject.FindObjectsOfType<LostLODGroup>(true))
        ////     {
        ////         lostLodGroup.UpdateGameObjects();
        ////         
        ////         if (lostLodGroup.isStreamingLods)
        ////         {
        ////             lostLodGroup.BuildTimeSetActiveLOD(2);
        ////         }
        ////     }
        //// }
        //// 
        //// #if UNITY_EDITOR
        //// private void Awake()
        //// {
        ////     if (this.isStreamingLods)
        ////     {
        ////         this.BuildTimeSetActiveLOD(2);
        ////     }
        //// }
        //// #endif
        //// 
        //// private void OnValidate()
        //// {
        ////     this.UpdateGameObjects();
        //// }

        //// public bool DoesLODNeedBaking(int index)
        //// {
        ////     if (index < 0)
        ////     {
        ////         // Error
        ////     }
        ////     else if (index == 0)
        ////     {
        ////         return false;
        ////     }
        ////     else
        ////     {
        ////         var previousLod = 
        //// 
        //// 
        ////     }
        //// }
        //// 
        //// public bool DoesLODNeedSaving(int index)
        //// {
        //// 
        //// }

        public void SetLODs(List<Mesh> meshes)
        {
            var lodGroup = this.GetOrAddComponent<LODGroup>();
            var lods = lodGroup.GetLODs();
            bool recalculateLODGroup = false;

            for (int i = 0; i < meshes.Count; i++)
            {
                var lodGameObject = this.gameObject.GetOrCreateChild($"LOD{i}", typeof(MeshFilter), typeof(MeshRenderer));
                var meshRenderer = lodGameObject.GetComponent<MeshRenderer>();
                var meshFilter = lodGameObject.GetComponent<MeshFilter>();
                meshFilter.sharedMesh = meshes[i];

                if (i >= lods.Length || lods[i].renderers.Contains(meshRenderer))
                {
                    recalculateLODGroup = true;
                }
            }

            if (recalculateLODGroup)
            {

            }

            // if (this.lods == null)
            // {
            //    this.lods = new List<LOD>(meshes.Count);
            // }
            // 
            // this.lods.Clear();
            // 
            // foreach (var mesh in meshes)
            // {
            //     this.lods.Add(new LOD { LODMesh = mesh });
            // }
        }

        //// private void UpdateGameObjects()
        //// {
        ////     if (this.isStreamingLods)
        ////     {
        ////         // Delete all child objects
        ////         // Delete LODGroup component if present
        ////         // Make sure has MeshFilter and MeshRenderer component
        ////     }
        ////     else
        ////     {
        ////         // Delete MeshFilter / MeshRender if present
        ////         // Make sure all children objects exist and have MeshFilter / MeshRender components
        ////         // Make sure this component has LODGroup
        ////         // Make sure all children have
        ////     }
        //// 
        ////     // Clean Up
        ////     //    Destory MeshFilter / MeshRenderer
        //// 
        //// 
        //// 
        ////     // Generates Children Objects for each LOD with LODGroup component (saves reference to that LODGroup)
        //// }
        //// 
        //// public void BuildTimeSetActiveLOD(int index)
        //// {
        ////     if (this.isStreamingLods && this.lods != null && index >= 0 && index < this.lods.Count)
        ////     {
        ////         var meshFilter = this.GetComponent<MeshFilter>();
        ////         
        ////         for (int i = 0; i < this.lods.Count; i++)
        ////         {
        ////             if (i == index)
        ////             {
        ////                 meshFilter.sharedMesh = this.lods[i].LODMesh;
        ////                 this.lods[i].IsActiveLod = true;
        ////                 this.lods[i].IsDefaultLod = true;
        ////             }
        ////             else
        ////             {
        ////                 this.lods[i].LODMesh = null;
        ////                 this.lods[i].IsDefaultLod = false;
        ////                 this.lods[i].IsActiveLod = false;
        ////             }
        ////         }
        ////     }
        ////     else
        ////     {
        ////         Debug.LogError($"Unable to set Active LOD for obejct {this.name}", this);
        ////     }
        //// }
        //// 
        //// public class LOD
        //// {
        ////     [SerializeField] private string lodGuid;
        ////     [SerializeField] private bool isActiveLod;
        ////     [SerializeField] private bool isDefaultLod;
        //// 
        ////     #if UNITY_EDITOR
        ////     [SerializeField] private Mesh lodMesh;   // OnBuild, null our lodMesh?
        ////     [SerializeField] private bool needsSimplifying;
        ////     [SerializeField] private bool needsSaving;
        ////     #endif
        //// 
        ////     public string LODGuid => this.lodGuid;
        //// 
        ////     public bool IsActiveLod
        ////     {
        ////         get => this.isActiveLod;
        ////         set => this.isActiveLod = value;
        ////     }
        //// 
        ////     public bool IsDefaultLod
        ////     {
        ////         get => this.isDefaultLod;
        ////         set => this.isDefaultLod = value;
        ////     }
        //// 
        ////                 
        ////     #if UNITY_EDITOR
        //// 
        ////     public Mesh LODMesh
        ////     {
        ////         get => this.lodMesh;
        //// 
        ////         set 
        ////         {
        ////             this.lodMesh = value;
        ////             var path = UnityEditor.AssetDatabase.GetAssetPath(this.lodMesh);
        ////             this.lodGuid = string.IsNullOrWhiteSpace(path) ? null : UnityEditor.AssetDatabase.GUIDFromAssetPath(path).ToString();
        ////         }
        ////     }
        //// 
        ////     public bool NeedsSimplifying
        ////     {
        ////         get => this.needsSimplifying;
        ////         set => this.needsSimplifying = value;
        ////     }
        //// 
        ////     public bool NeedsSaving
        ////     {
        ////         get => this.needsSaving;
        ////         set => this.needsSaving = value;
        ////     }
        ////     
        ////     #endif
        //// }
    }
}
