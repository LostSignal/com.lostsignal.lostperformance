//-----------------------------------------------------------------------
// <copyright file="MeshCombineManager.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    
    ////
    //// TODO [bgish]: Figure out what's better, have 1 mesh renderer with multiple materials, or multiple mesh renderers with 1 material
    ////               Make my Remote Method Call (RMC) tool with the ability to find all MeshCombiner scripts and collapse/expand
    ////
    //// TODO [bgish]: Go through eveIry octreeMeshRenderers after calculating and make sure they were assigned an OctreeVolumeIndex
    ////
    
    public class SceneOptimizer : MonoBehaviour
    {
        [SerializeField] private List<GameObject> meshRenderersGameObjects = new List<GameObject>();
        [SerializeField] private List<OctreeVolume> octreeVolumes = new List<OctreeVolume>();
        [SerializeField] private List<MeshRendererInfo> meshRendererInfos = new List<MeshRendererInfo>();
        [SerializeField] private int maxTriangleCount = 150000;
        [SerializeField] private float maxBoundsSize = 2000;
        [SerializeField] private float minBoundsSize = 30;
        [SerializeField] private Bounds octreeBounds;
    
        [ReadOnly] [SerializeField] private Transform volumesTransform;
        [ReadOnly] [SerializeField] private List<VolumeOptimizer> volumesMeshCombiners;
    
        public List<OctreeVolume> OctreeVolumes => this.octreeVolumes;
    
        // #if UNITY_EDITOR
    
        public void OnDrawGizmosSelected()
        {
            foreach (var octreeVolume in this.octreeVolumes)
            {
                var bounds = octreeVolume.CenterBounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
    
            //// Debug
            //// Gizmos.DrawWireCube(this.octreeBounds.center, this.octreeBounds.size);
        }
        
        public void CalculateOctree()
        {
            this.octreeVolumes.Clear();
            this.meshRendererInfos.Clear();
    
            DateTime startMeshRendererCollection = DateTime.Now;
            this.meshRendererInfos = this.GetMeshRendererInfos();
            DateTime endMeshRendererCollection = DateTime.Now;
            double meshCollctionInMillis = endMeshRendererCollection.Subtract(startMeshRendererCollection).TotalMilliseconds;
            Debug.Log($"Mesh Renderer Collection took {meshCollctionInMillis} milliseconds");
    
            if (this.meshRendererInfos.Count == 0)
            {
                Debug.LogError("No valid MeshRenders found!");
                return;
            }
    
            var octreeMeshRenderers = this.meshRendererInfos.Where(x => x.IsIgnored == false).ToList();
            this.octreeBounds = this.GetMaxBounds(octreeMeshRenderers);
    
            // Making sure the octree bounds is a perfect square
            float max = Math.Max(Math.Max(this.octreeBounds.extents.x, this.octreeBounds.extents.y), this.octreeBounds.extents.z);
            this.octreeBounds = new Bounds(this.octreeBounds.center, new Vector3(max, max, max));
    
            DateTime startOctreeGeneration = DateTime.Now;
            this.CalculateOctree(octreeMeshRenderers, this.octreeBounds);
            DateTime endOctreeGeneration = DateTime.Now;
            double octreeGenerationInMillis = endOctreeGeneration.Subtract(startOctreeGeneration).TotalMilliseconds;
            Debug.Log($"Octree Generation took {octreeGenerationInMillis} milliseconds");
    
            Debug.Log("Mesh Renderers: " + this.meshRendererInfos.Count);
            Debug.Log("Octree Volume: " + this.octreeVolumes.Count);
    
            int missingFromOctreeCount = octreeMeshRenderers.Sum(x => x.OctreeVolumeIndex == -1 ? 1 : 0);
            if (missingFromOctreeCount > 0)
            {
                UnityEditor.Selection.objects = octreeMeshRenderers.Where(x => x.OctreeVolumeIndex == -1).Select(x => x.MeshRenderer.gameObject).ToArray();
    
                Debug.LogError($"Mesh Renderers Without Octree Volume: {missingFromOctreeCount}");
            }
    
            //// Debug.Log("Average MeshRender per Volume: Not Implemented");
            //// Debug.Log("Average Triangles per Volume: Not Implemented");
        }
    
        public void DeleteVolumesMeshCombine()
        {
            if (this.volumesMeshCombiners == null)
            {
                this.volumesMeshCombiners = new List<VolumeOptimizer>();
            }
    
            foreach (var meshCombine in this.volumesMeshCombiners)
            {
                //meshCombine.Revert();
            }
    
            this.volumesMeshCombiners.Clear();
            this.volumesTransform.DestroyAllChildrenImmediate();
        }
    
        public void GenerateVolumesMeshCombine()
        {
            this.DeleteVolumesMeshCombine();
    
            int count = 0;
            foreach (var volume in this.octreeVolumes)
            {
                GameObject volumeMeshCombine = new GameObject($"Volume {count++}", typeof(VolumeOptimizer));
                volumeMeshCombine.transform.SetParent(this.volumesTransform);
                volumeMeshCombine.transform.position = volume.CenterBounds.center;
                var meshCombineList = volumeMeshCombine.GetComponent<VolumeOptimizer>();
                meshCombineList.SetList(volume.MeshRendererInfos.Select(x => x.MeshRenderer).ToList());
                //meshCombineList.CreateLOD(0);
            }
        }
    
        private void CalculateOctree(List<MeshRendererInfo> meshRendererInfos, Bounds bounds)
        {
            if (meshRendererInfos.Count == 0)
            {
                return;
            }
    
            float boundsSize = GetBoundsSize(bounds);
    
            if (boundsSize >= this.maxBoundsSize)
            {
                Split(meshRendererInfos, bounds);
            }
            else if (boundsSize <= this.minBoundsSize)
            {
                AddOctreeVolume(meshRendererInfos, bounds);
            }
            else if (meshRendererInfos.Sum(x => x.TriCount) > this.maxTriangleCount)
            {
                Split(meshRendererInfos, bounds);
            }
            else
            {
                // If we got here then we have a valid volume
                AddOctreeVolume(meshRendererInfos, bounds);
            }
    
            void Split(List<MeshRendererInfo> meshRendererInfos, Bounds bounds)
            {
                foreach (var newBounds in GetSplitBounds(bounds))
                {
                    var newMeshRendererInfos = meshRendererInfos.Where(x => newBounds.Contains(x.Bounds.center)).ToList();
    
                    if (newMeshRendererInfos.Count > 0)
                    {
                        this.CalculateOctree(newMeshRendererInfos, newBounds);
                    }
                }
            }
    
            Bounds[] GetSplitBounds(Bounds bounds)
            {
                Vector3 center = bounds.center;
                Vector3 e = bounds.extents;
    
                return new Bounds[]
                {
                    NewBounds(-e.x, -e.y, -e.z),
                    NewBounds(-e.x, -e.y, +e.z),
                    NewBounds(+e.x, -e.y, +e.z),
                    NewBounds(+e.x, -e.y, -e.z),
                    NewBounds(-e.x, +e.y, -e.z),
                    NewBounds(-e.x, +e.y, +e.z),
                    NewBounds(+e.x, +e.y, +e.z),
                    NewBounds(+e.x, +e.y, -e.z),
                };
    
                Bounds NewBounds(float x, float y, float z)
                {
                    Bounds bounds = new Bounds(center, Vector3.zero);
                    bounds.Encapsulate(center + new Vector3(x, y, z));
                    return bounds;
                }
            }
    
            void AddOctreeVolume(List<MeshRendererInfo> meshRendererInfos, Bounds bounds)
            {
                int octreeVolumeIndex = this.octreeVolumes.Count;
    
                this.octreeVolumes.Add(new OctreeVolume
                {
                    Bounds = bounds,
                    MeshRendererInfos = meshRendererInfos,
                    CenterBounds = this.GetCenterBounds(meshRendererInfos),
                    TriCount = meshRendererInfos.Sum(x => x.TriCount),
                });
    
                foreach (var meshRendererInfo in meshRendererInfos)
                {
                    if (meshRendererInfo.OctreeVolumeIndex != -1)
                    {
                        string errorMessage = $"MeshRendererInfo {meshRendererInfo.MeshRenderer.name} can't set OctreeVolumeIndex" +
                                              $" to {octreeVolumeIndex} because it's already {meshRendererInfo.OctreeVolumeIndex}";
    
                        Debug.LogError(errorMessage, meshRendererInfo.MeshRenderer);
                    }
                    else
                    {
                        meshRendererInfo.OctreeVolumeIndex = octreeVolumeIndex;
                    }
                }
            }
    
            float GetBoundsSize(Bounds bounds)
            {
                return Vector3.Magnitude(bounds.extents * 2);
            }
        }
    
        private List<MeshRendererInfo> GetMeshRendererInfos()
        {
            List<MeshRendererInfo> results = new List<MeshRendererInfo>();
    
            foreach (var meshRenderersGameObject in this.meshRenderersGameObjects)
            {
                foreach (var meshRenderer in meshRenderersGameObject.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (meshRenderer.enabled == false)
                    {
                        continue;
                    }
    
                    var meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                    var lodGroup = meshRenderer.gameObject.GetComponentInParent<LODGroup>();
                    var ignore = meshRenderer.gameObject.GetComponentInParent<ObjectOptimizerIgnore>();
    
                    if (meshFilter == null || meshFilter.sharedMesh == null)
                    {
                        continue;
                    }
    
                    results.Add(new MeshRendererInfo
                    {
                        MeshRenderer = meshRenderer,
                        MeshFilter = meshFilter,
                        LodGroup = lodGroup,
                        IgnoreMeshCombine = ignore,
                        TriCount = meshFilter.sharedMesh.triangles.Length,
                        VertCount = meshFilter.sharedMesh.vertexCount,
                        LODLevel = GetLODLevel(meshRenderer, lodGroup),
                        IsIgnored = ignore != null && ignore.IgnoreLOD == IgnoreLOD.LOD0AndAbove,
                        Bounds = meshRenderer.bounds,
                    });
                }
            }
    
            return results;
    
            int GetLODLevel(MeshRenderer meshRenderer, LODGroup lodGroup)
            {
                int lodLevel = 0;
    
                if (lodGroup != null)
                {
                    var lods = lodGroup.GetLODs();
    
                    for (int i = 0; i < lods.Length; i++)
                    {
                        if (lods[i].renderers.Contains(meshRenderer))
                        {
                            lodLevel = i;
                            break;
                        }
                    }
                }
    
                return lodLevel;
            }
        }
    
        private Bounds GetMaxBounds(List<MeshRendererInfo> meshRendererInfos)
        {
            Bounds totalBounds = meshRendererInfos[0].Bounds;
    
            foreach (var meshRendererInfo in meshRendererInfos)
            {
                totalBounds.Encapsulate(meshRendererInfo.Bounds.min);
                totalBounds.Encapsulate(meshRendererInfo.Bounds.max);
            }
    
            return totalBounds;
        }
    
        private Bounds GetCenterBounds(List<MeshRendererInfo> meshRendererInfos)
        {
            Bounds firstBounds = meshRendererInfos[0].Bounds;
            Bounds totalBounds = new Bounds(firstBounds.center, Vector3.zero);
    
            foreach (var meshRendererInfo in meshRendererInfos)
            {
                totalBounds.Encapsulate(meshRendererInfo.Bounds.center);
            }
    
            if (totalBounds.size == Vector3.zero)
            {
                totalBounds.size = Vector3.one * 5.0f;
            }
    
            return totalBounds;
        }
    
        private void OnValidate()
        {
            if (this.volumesTransform == null)
            {
                this.volumesTransform = new GameObject("Volumes").transform;
                this.volumesTransform.SetParent(this.transform);
                this.volumesTransform.Reset();
            }
        }
    
        // #endif
    
        [Serializable]
        public class OctreeVolume
        {
            [SerializeField] private List<MeshRendererInfo> meshRendererInfos;
            [SerializeField] private Bounds bounds;
            [SerializeField] private Bounds centerBounds;
            [SerializeField] private int triCount;
    
            public List<MeshRendererInfo> MeshRendererInfos
            {
                get => this.meshRendererInfos;
                set => this.meshRendererInfos = value;
            }
    
            public Bounds Bounds
            {
                get => this.bounds;
                set => this.bounds = value;
            }
    
            public Bounds CenterBounds
            {
                get => this.centerBounds;
                set => this.centerBounds = value;
            }
    
            public int TriCount
            {
                get => this.triCount;
                set => this.triCount = value;
            }
        }
    
        [Serializable]
        public class MeshRendererInfo
        {
            [SerializeField] private MeshRenderer meshRenderer;
            [SerializeField] private MeshFilter meshFilter;
            [SerializeField] private LODGroup lodGroup;
            [SerializeField] private ObjectOptimizerIgnore ignoreMeshCombine;
            [SerializeField] private int lodLevel;
            [SerializeField] private int vertCount;
            [SerializeField] private int triCount;
            [SerializeField] private bool isIgnored;
            [SerializeField] private Bounds bounds;
            [SerializeField] private int octreeVolumeIndex = -1;
    
            public MeshRenderer MeshRenderer
            {
                get => this.meshRenderer;
                set => this.meshRenderer = value;
            }
    
            public MeshFilter MeshFilter
            {
                get => this.meshFilter;
                set => this.meshFilter = value;
            }
    
            public LODGroup LodGroup
            {
                get => this.lodGroup;
                set => this.lodGroup = value;
            }
    
            public ObjectOptimizerIgnore IgnoreMeshCombine
            {
                get => this.ignoreMeshCombine;
                set => this.ignoreMeshCombine = value;
            }
    
            public int LODLevel
            {
                get => this.lodLevel;
                set => this.lodLevel = value;
            }
    
            public int VertCount
            {
                get => this.vertCount;
                set => this.vertCount = value;
            }
    
            public int TriCount
            {
                get => this.triCount;
                set => this.triCount = value;
            }
    
            public bool IsIgnored
            {
                get => this.isIgnored;
                set => this.isIgnored = value;
            }
    
            public Bounds Bounds
            {
                get => this.bounds;
                set => this.bounds = value;
            }
    
            public int OctreeVolumeIndex
            {
                get => this.octreeVolumeIndex;
                set => this.octreeVolumeIndex = value;
            }
        }
    }
}
