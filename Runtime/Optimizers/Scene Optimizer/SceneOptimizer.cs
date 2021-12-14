//-----------------------------------------------------------------------
// <copyright file="SceneOptimizer.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class SceneOptimizer : MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] private SceneOptimizerSettings settings;
        [SerializeField] private List<GameObject> objectOptimizationList = new List<GameObject>();

        [Header("Editor Visuals Only")]
        [SerializeField] private ColorBy colorBy;

        [Header("Read Only")]
        [ReadOnly] [SerializeField] private List<OctreeVolume> octreeVolumes = new List<OctreeVolume>();
        [ReadOnly] [SerializeField] private List<MeshRendererInfo> meshRendererInfos = new List<MeshRendererInfo>();
        [ReadOnly] [SerializeField] private Bounds octreeBounds;
        [ReadOnly] [SerializeField] private Transform volumesTransform;
        #pragma warning restore 0649
        
        private enum ColorBy
        {
            None,
            TriangleCount,
            MeshRendererCount,
        }

        public void CalculateOctree()
        {
            this.octreeVolumes.Clear();
            this.meshRendererInfos.Clear();
    
            DateTime startMeshRendererCollection = DateTime.Now;
            this.meshRendererInfos = MeshRendererInfo.GetMeshRendererInfos(this.objectOptimizationList);
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

            //// Debug.Log("Average MeshRender per Volume: Not Implemented");
            //// Debug.Log("Average Triangles per Volume: Not Implemented");

            #if UNITY_EDITOR
            int missingFromOctreeCount = octreeMeshRenderers.Sum(x => x.OctreeVolumeIndex == -1 ? 1 : 0);
            if (missingFromOctreeCount > 0)
            {
                UnityEditor.Selection.objects = octreeMeshRenderers.Where(x => x.OctreeVolumeIndex == -1).Select(x => x.MeshRenderer.gameObject).ToArray();
                Debug.LogError($"Mesh Renderers Without Octree Volume: {missingFromOctreeCount}");
            }
            #endif
        }
    
        public void DeleteLODs()
        {
            if (this.octreeVolumes == null)
            {
                return;
            }

            foreach (var volume in this.octreeVolumes)
            {
                volume.Revert();
            }
        }
    
        public void GenerateLODs()
        {
            this.DeleteLODs();
    
            int count = 0;
            foreach (var volume in this.octreeVolumes)
            {
                GameObject volumeGameObject = new GameObject($"Volume {count++}");
                volumeGameObject.transform.SetParent(this.volumesTransform);
                volumeGameObject.transform.position = volume.CenterBounds.center;
                volume.GameObject = volumeGameObject;

                MeshCombiner.CreateLODs(
                    volumeGameObject.transform, 
                    this.settings.LODSettings, 
                    volume.MeshRendererInfos, 
                    false);
            }
        }

        public void CleanUp()
        {
            foreach (var volume in this.octreeVolumes)
            {
                foreach (var meshRendererInfo in volume.MeshRendererInfos)
                {
                    if (meshRendererInfo.IsIgnored == false)
                    {
                        GameObject.DestroyImmediate(meshRendererInfo.MeshRenderer);
                        GameObject.DestroyImmediate(meshRendererInfo.MeshFilter);
                    }
                }
            }

            foreach (var gameObject in this.objectOptimizationList)
            {
                MeshCombiner.DeleteEmptyOrDisabledGameObjects(gameObject.transform);
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

        private void CalculateOctree(List<MeshRendererInfo> meshRendererInfos, Bounds bounds)
        {
            if (meshRendererInfos.Count == 0)
            {
                return;
            }
    
            float boundsSize = GetBoundsSize(bounds);
    
            if (boundsSize >= this.settings.MaxVolumeBoundsSize)
            {
                Split(meshRendererInfos, bounds);
            }
            else if (boundsSize <= this.settings.MinVolumeBoundsSize)
            {
                AddOctreeVolume(meshRendererInfos, bounds);
            }
            else if (meshRendererInfos.Sum(x => x.TriCount) > this.settings.MaxTrianglesPerVolume)
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

            if (this.settings == null)
            {
                // TODO [bgish]: Get the default settings from project settings, not hard coded
                this.settings = EditorUtil.GetAssetByGuid<SceneOptimizerSettings>("622478ab99818ea45b7e1cd8fc290196");
            }
        }

        private void OnDrawGizmosSelected()
        {
            //// TODO [bgish]: Actually color these by this.colorBy

            foreach (var octreeVolume in this.octreeVolumes)
            {
                var bounds = octreeVolume.CenterBounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }

        [Serializable]
        private class OctreeVolume
        {
            [SerializeField] private List<MeshRendererInfo> meshRendererInfos;
            [SerializeField] private Bounds bounds;
            [SerializeField] private Bounds centerBounds;
            [SerializeField] private int triCount;
            [SerializeField] private GameObject gameObject;

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

            public GameObject GameObject
            {
                get => this.gameObject;
                set => this.gameObject = value;
            }

            public void CraeteLODs()
            {

            }

            public void Revert()
            {
                if (this.gameObject)
                {
                    MeshCombiner.DestoryLODs(this.gameObject.transform, this.meshRendererInfos);

                    if (Application.isPlaying == false)
                    {
                        GameObject.DestroyImmediate(this.GameObject);
                    }
                    else
                    {
                        GameObject.Destroy(this.GameObject);
                    }
                }
            }
        }
    }
}
