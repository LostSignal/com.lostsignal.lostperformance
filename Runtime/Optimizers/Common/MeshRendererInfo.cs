//-----------------------------------------------------------------------
// <copyright file="MeshRendererInfo.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [Serializable]
    public class MeshRendererInfo
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private LODGroup lodGroup;
        [SerializeField] private OptimizerIgnore ignoreMeshCombine;
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

        public OptimizerIgnore IgnoreMeshCombine
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

        public static List<MeshRendererInfo> GetMeshRendererInfos(List<GameObject> gameObjects)
        {
            List<MeshRendererInfo> results = new List<MeshRendererInfo>();

            foreach (var gameObject in gameObjects)
            {
                foreach (var meshRenderer in gameObject.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (meshRenderer.enabled == false)
                    {
                        continue;
                    }

                    var meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                    var lodGroup = meshRenderer.gameObject.GetComponentInParent<LODGroup>();
                    var ignore = meshRenderer.gameObject.GetComponentInParent<OptimizerIgnore>();

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
    }
}
