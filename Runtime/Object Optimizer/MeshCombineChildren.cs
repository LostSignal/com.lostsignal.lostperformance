//-----------------------------------------------------------------------
// <copyright file="MeshCombineChildren.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class MeshCombineChildren : MeshCombiner
    {
        public List<GameObject> testObjects;

        public override List<GameObject> GetGameObjectsToCombine(int lodLevel)
        {
            return this.GetComponentsInChildren<MeshRenderer>(true)
                .Where((x) =>
                {
                    var meshFilter = x.GetComponent<MeshFilter>();
                    var ignore = x.GetComponentInParent<IgnoreMeshCombine>();

                    bool isMeshFilterValid = meshFilter != null && meshFilter.sharedMesh != null;
                    bool isIgnoreValid = ignore == null || lodLevel < (int)ignore.IgnoreLOD;

                    return isMeshFilterValid && isIgnoreValid;
                })
                .Select(x => x.gameObject)
                .ToList();
        }

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

                    string baseFolder = "Assets/SimpleReductions";
                    if (UnityEditor.AssetDatabase.IsValidFolder(baseFolder) == false)
                    {
                        UnityEditor.AssetDatabase.CreateFolder("Assets", "SimpleReductions");
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
