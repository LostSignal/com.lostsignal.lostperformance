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
        [SerializeField] private OptimizerSettings settings;
        
        [ReadOnly] [SerializeField] private bool isOptimized;
        [ReadOnly] [SerializeField] private List<MeshRenderer> combinedMeshRenderers;
        [ReadOnly] [SerializeField] private List<MeshFilter> combinedMeshFilters;
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
            this.combinedMeshRenderers = this.GetMeshRenderersToCombine(0);
            this.combinedMeshFilters = this.combinedMeshRenderers.Select(x => x.GetComponent<MeshFilter>()).ToList();
            MeshCombiner.CreateLODs(this.transform, this.settings.LODSettings, this.combinedMeshRenderers, generateLODGroup: true);

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
            MeshCombiner.DestoryLODs(this.transform);

            // TODO [bgish]: Delete LODS parent GameObject
            // TODO [bgish]: Delete BoxColliders parent GameObject
            // TODO [bgish]: Delete MeshColliders parent GameObject

            this.combinedMeshRenderers.ForEach(x => x.enabled = true);
            this.combinedBoxColliders.ForEach(x => x.enabled = true);
            this.combinedMeshColliders.ForEach(x => x.enabled = true);

            this.combinedMeshRenderers.Clear();
            this.combinedMeshFilters.Clear();
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
            if (Application.isPlaying)
            {
                this.CleanUp();
            }
        }
        #endif

        private void OnValidate()
        {
            if (this.combinedMeshRenderers == null)
            {
                this.combinedMeshRenderers = new List<MeshRenderer>();
            }

            if (this.combinedMeshFilters == null)
            {
                this.combinedMeshFilters = new List<MeshFilter>();
            }

            if (this.combinedBoxColliders == null)
            {
                this.combinedBoxColliders = new List<BoxCollider>();
            }

            if (this.combinedMeshColliders == null)
            {
                this.combinedMeshColliders = new List<MeshCollider>();
            }
        }

        private List<MeshRenderer> GetMeshRenderersToCombine(int lodLevel)
        {
            return this.GetComponentsInChildren<MeshRenderer>(true)
                .Where((x) =>
                {
                    var meshFilter = x.GetComponent<MeshFilter>();
                    var ignore = x.GetComponentInParent<ObjectOptimizerIgnore>();

                    bool isMeshFilterValid = meshFilter != null && meshFilter.sharedMesh != null;
                    bool isIgnoreValid = ignore == null || lodLevel < (int)ignore.IgnoreLOD;

                    return isMeshFilterValid && isIgnoreValid;
                })
                .ToList();
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

            this.combinedMeshRenderers.ForEach(x => Destory(x));
            this.combinedMeshFilters.ForEach(x => Destory(x));
            this.combinedBoxColliders.ForEach(x => Destory(x));
            this.combinedMeshColliders.ForEach(x => Destory(x));

            this.combinedMeshRenderers.Clear();
            this.combinedMeshFilters.Clear();
            this.combinedBoxColliders.Clear();
            this.combinedMeshColliders.Clear();

            this.combinedMeshRenderers = null;
            this.combinedMeshFilters = null;
            this.combinedBoxColliders = null;
            this.combinedMeshColliders = null;

            DeleteEmptyOrDisabledGameObjects(this.transform);
            Destroy(this);

            void Destory(Component component)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(component);
                }
                else
                {
                    GameObject.DestroyImmediate(component);
                }
            }

            void DeleteEmptyOrDisabledGameObjects(Transform childTransform, bool isRoot = true)
            {
                for (int i = 0; i < childTransform.childCount; i++)
                {
                    DeleteEmptyOrDisabledGameObjects(childTransform.GetChild(i), false);
                }

                bool isNotActive = childTransform.gameObject.activeInHierarchy == false;
                bool hasNoComponents = childTransform.GetComponentsInChildren<Component>().Length == 1;

                if (isRoot == false && (isNotActive || hasNoComponents))
                {
                    GameObject.DestroyImmediate(childTransform);
                }
            }
        }
    }
}
