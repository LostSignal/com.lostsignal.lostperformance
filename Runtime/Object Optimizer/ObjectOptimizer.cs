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
        [SerializeField] private ObjectOptimizerSettings settings;
        
        [ReadOnly] [SerializeField] private bool isOptimized;
        [ReadOnly] [SerializeField] private List<MeshRenderer> combinedMeshRenderers;
        [ReadOnly] [SerializeField] private List<MeshFilter> combinedMeshFilters;
        [ReadOnly] [SerializeField] private List<BoxCollider> combinedBoxColliders;
        [ReadOnly] [SerializeField] private List<MeshCollider> combinedMeshColliders;
        #pragma warning restore 0649

        public bool IsOptimized => this.isOptimized;

        public ObjectOptimizerSettings Settings => this.settings;

        public void Optimize()
        {
            this.combinedMeshRenderers = this.GetMeshRenderersToCombine(0);
            this.combinedMeshFilters = this.combinedMeshRenderers.Select(x => x.GetComponent<MeshFilter>()).ToList();

            // TODO [bgish]: Collect Box Colliders
            // TODO [bgish]: Collect Mesh Colliders

            // Creating all LODs
            for (int i = 0; i < this.settings.LODSettings.Count; i++)
            {
                MeshCombiner.CreateLOD(this.combinedMeshRenderers, i, this.transform);
            }
            
            this.isOptimized = true;

            this.combinedMeshColliders.ForEach(x => x.enabled = false);
            this.combinedBoxColliders.ForEach(x => x.enabled = false);
            this.combinedMeshColliders.ForEach(x => x.enabled = false);
        }

        public List<MeshRenderer> GetMeshRenderersToCombine(int lodLevel)
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

        public void Revert()
        {
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

        private void Awake()
        {
            if (Application.isPlaying)
            {
                this.CleanUp();
            }
        }

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

        private void CleanUp(bool unpackPrefabsCompletely = false)
        {
            if (this.isOptimized == false)
            {

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

            DeleteEmptyGameObjects();
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

            void DeleteEmptyGameObjects()
            {
                // TODO [bgish]: Go through every child game object and if it has only 1 transform component and no children
                //               then delete it, also if it is just disabled, then delete it.
            }
        }

        public List<GameObject> testObjects;

        #if UNITY_EDITOR

        public void Test()
        {
            // if so, initialize Simplygon
            using (Simplygon.ISimplygon simplygon = global::Simplygon.Loader.InitSimplygon(out Simplygon.EErrorCodes simplygonErrorCode, out string simplygonErrorMessage))
            {
                simplygon.SetGlobalEnableLogSetting(true);
                simplygon.SetGlobalLogToFileSetting(true);
                simplygon.SetThreadLocalLogFileNameSetting(@"C:\Users\User\Desktop\Simplygon.txt");

                // if Simplygon handle is valid, loop all selected objects
                // and call Reduce function.
                if (simplygonErrorCode == Simplygon.EErrorCodes.NoError)
                {
                    this.ReduceTest(simplygon, 0.5f);
                }

                // if invalid handle, output error message to the Unity console
                else
                {
                    Debug.Log("Initializing failed!");
                }
            }
        }

        public void ReduceTest(Simplygon.ISimplygon simplygon, float quality)
        {
            string exportTempDirectory = Simplygon.Unity.EditorPlugin.SimplygonUtils.GetNewTempDirectory();
            Debug.Log(exportTempDirectory);

            using (Simplygon.spScene sgScene = Simplygon.Unity.EditorPlugin.SimplygonExporter.Export(simplygon, exportTempDirectory, this.testObjects))
            {
                using (Simplygon.spReductionPipeline reductionPipeline = simplygon.CreateReductionPipeline())
                using (Simplygon.spReductionSettings reductionSettings = reductionPipeline.GetReductionSettings())
                {
                    reductionSettings.SetReductionTargets(Simplygon.EStopCondition.All, true, false, false, false);
                    reductionSettings.SetReductionTargetTriangleRatio(quality);
                    reductionPipeline.RunScene(sgScene, Simplygon.EPipelineRunMode.RunInThisProcess);

                    string folderName = "Simplygon Temp Assets";
                    string baseFolder = $"Assets/{folderName}";
                    
                    if (UnityEditor.AssetDatabase.IsValidFolder(baseFolder) == false)
                    {
                        UnityEditor.AssetDatabase.CreateFolder("Assets", folderName);
                    }

                    string assetFolderGuid = UnityEditor.AssetDatabase.CreateFolder(baseFolder, gameObject.name);
                    string assetFolderPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetFolderGuid);

                    List<GameObject> importedGameObjects = new List<GameObject>();
                    int startingLodIndex = 0;
                    Simplygon.Unity.EditorPlugin.SimplygonImporter.Import(simplygon, reductionPipeline, ref startingLodIndex, assetFolderPath, gameObject.name, importedGameObjects);

                    int count = importedGameObjects.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Debug.Log($"{(i+1)} of {count}: " + importedGameObjects[i].name);
                    }
                    
                    // if (importedGameObjects.Count > 0)
                    // {
                    //     var importedMeshFilter = importedGameObjects[0].GetComponent<MeshFilter>();
                    // 
                    //     if (importedMeshFilter != null)
                    //     {
                    //         var assetPath = AssetDatabase.GetAssetPath(importedMeshFilter.sharedMesh);
                    //         var newMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                    //         var meshCopy = GameObject.Instantiate(newMesh);
                    // 
                    //         EditorApplication.delayCall += () =>
                    //         {
                    //             originalMeshFilter.sharedMesh = meshCopy;
                    //             EditorUtility.SetDirty(originalMeshFilter.gameObject);
                    //         };
                    //     }
                    // }
                    // 
                    // foreach (var importedObject in importedGameObjects)
                    // {
                    //     GameObject.DestroyImmediate(importedObject);
                    // }
                }
            }
        }

        #endif
    }
}
