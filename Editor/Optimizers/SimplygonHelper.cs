//-----------------------------------------------------------------------
// <copyright file="SimplygonHelper.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class SimplygonHelper
    {
        public static void Test(List<GameObject> gameObjects)
        {
            // if so, initialize Simplygon
            using (Simplygon.ISimplygon simplygon = global::Simplygon.Loader.InitSimplygon(out Simplygon.EErrorCodes simplygonErrorCode, out string simplygonErrorMessage))
            {
                // if Simplygon handle is valid, loop all selected objects
                // and call Reduce function.
                if (simplygonErrorCode == Simplygon.EErrorCodes.NoError)
                {
                    ReduceTest(simplygon, 0.5f, gameObjects);
                }

                // if invalid handle, output error message to the Unity console
                else
                {
                    Debug.Log("Initializing failed!");
                }
            }
        }

        private static void ReduceTest(Simplygon.ISimplygon simplygon, float quality, List<GameObject> gameObjects)
        {
            string exportTempDirectory = Simplygon.Unity.EditorPlugin.SimplygonUtils.GetNewTempDirectory();
            Debug.Log(exportTempDirectory);

            using (Simplygon.spScene sgScene = Simplygon.Unity.EditorPlugin.SimplygonExporter.Export(simplygon, exportTempDirectory, gameObjects))
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

                    string assetFolderGuid = UnityEditor.AssetDatabase.CreateFolder(baseFolder, "Simplygon Output");
                    string assetFolderPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetFolderGuid);

                    List<GameObject> importedGameObjects = new List<GameObject>();
                    int startingLodIndex = 0;
                    Simplygon.Unity.EditorPlugin.SimplygonImporter.Import(simplygon, reductionPipeline, ref startingLodIndex, assetFolderPath, "Simplygon Output", importedGameObjects);

                    int count = importedGameObjects.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Debug.Log($"{(i + 1)} of {count}: " + importedGameObjects[i].name);
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


        //// 
        //// #if UNITY_EDITOR
        //// 
        //// public static void Reduce(MeshFilter originalMeshFilter, Simplygon.ISimplygon simplygon, float quality)
        //// {
        ////     var gameObject = originalMeshFilter.gameObject;
        ////     List<GameObject> selectedGameObjects = new List<GameObject>();
        ////     selectedGameObjects.Add(gameObject);
        //// 
        ////     string exportTempDirectory = Simplygon.Unity.EditorPlugin.SimplygonUtils.GetNewTempDirectory();
        ////     Debug.Log(exportTempDirectory);
        //// 
        ////     using (Simplygon.spScene sgScene = Simplygon.Unity.EditorPlugin.SimplygonExporter.Export(simplygon, exportTempDirectory, selectedGameObjects))
        ////     {
        ////         using (Simplygon.spReductionPipeline reductionPipeline = simplygon.CreateReductionPipeline())
        ////         using (Simplygon.spReductionSettings reductionSettings = reductionPipeline.GetReductionSettings())
        ////         {
        ////             reductionSettings.SetReductionTargets(Simplygon.EStopCondition.All, true, false, false, false);
        ////             reductionSettings.SetReductionTargetTriangleRatio(quality);
        //// 
        ////             reductionPipeline.RunScene(sgScene, Simplygon.EPipelineRunMode.RunInThisProcess);
        //// 
        ////             string baseFolder = "Assets/SimpleReductions";
        ////             if (UnityEditor.AssetDatabase.IsValidFolder(baseFolder) == false)
        ////             {
        ////                 UnityEditor.AssetDatabase.CreateFolder("Assets", "SimpleReductions");
        ////             }
        //// 
        ////             string assetFolderGuid = UnityEditor.AssetDatabase.CreateFolder(baseFolder, gameObject.name);
        ////             string assetFolderPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetFolderGuid);
        //// 
        ////             List<GameObject> importedGameObjects = new List<GameObject>();
        ////             int startingLodIndex = 0;
        ////             Simplygon.Unity.EditorPlugin.SimplygonImporter.Import(simplygon, reductionPipeline, ref startingLodIndex, assetFolderPath, gameObject.name, importedGameObjects);
        //// 
        ////             if (importedGameObjects.Count > 0)
        ////             {
        ////                 var importedMeshFilter = importedGameObjects[0].GetComponent<MeshFilter>();
        //// 
        ////                 if (importedMeshFilter != null && importedMeshFilter.sharedMesh != null)
        ////                 {
        ////                     var assetPath = UnityEditor.AssetDatabase.GetAssetPath(importedMeshFilter.sharedMesh);
        ////                     var newMesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
        ////                     var meshCopy = GameObject.Instantiate(newMesh);
        //// 
        ////                     UnityEditor.EditorApplication.delayCall += () =>
        ////                     {
        ////                         originalMeshFilter.sharedMesh = meshCopy;
        ////                         UnityEditor.EditorUtility.SetDirty(originalMeshFilter.gameObject);
        ////                     };                            
        ////                 }
        ////             }
        //// 
        ////             foreach (var importedObject in importedGameObjects)
        ////             {
        ////                 GameObject.DestroyImmediate(importedObject);
        ////             }
        ////         }
        ////     }
        //// }
        //// 
        //// #endif
    }
}
