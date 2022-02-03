//-----------------------------------------------------------------------
// <copyright file="SimplygonHelper.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public static class SimplygonHelper
    {
        static SimplygonHelper()
        {
            #if !USING_SIMPLYGON
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.Contains("Simplygon.Unity.EditorPlugin"))
                {
                    ProjectDefinesHelper.AddDefineToProject("USING_SIMPLYGON");
                }
            }
            #endif
        }

        public static GameObject Reduce(List<GameObject> gameObjects, float quality)
        {
            #if USING_SIMPLYGON
            
            // TODO [bgish]: Make sure to debug log the time it took to create the LODs
            return ReduceInternal(gameObjects, quality);
            
            #else
            
            SetupSimplygon();
            return null;
            
            #endif
        }

        private static void SetupSimplygon()
        {
            var installDirectory = GetInstallDirectory();

            if (installDirectory != null)
            {
                ImportSimplygonDLL(installDirectory);
            }

            static string FindSimplygonInstallFolders()
            {
                var installFolders = new List<string>
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    Environment.ExpandEnvironmentVariables("%ProgramW6432%"),
                    Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%"),
                };

                foreach (var installFolder in installFolders)
                {
                    var simplygonFolder = Path.Combine(installFolder, "Simplygon/9/Unity/bin").Replace("\\", "/");

                    if (Directory.Exists(simplygonFolder))
                    {
                        return simplygonFolder;
                    }
                }

                return null;
            }

            static string GetInstallDirectory()
            {
                var simplygonDirectory = FindSimplygonInstallFolders();
                if (simplygonDirectory == null)
                {
                    bool result = EditorUtility.DisplayDialog(
                        "Install Simplygon?",
                        "Unable to find Simplygon install folder, would you like to download it now?",
                        "yes",
                        "no");

                    if (result)
                    {
                        Application.OpenURL("https://www.simplygon.com/Downloads");
                    }
                }

                return simplygonDirectory;
            }

            static void ImportSimplygonDLL(string simplygonInstallDirectory)
            {
                bool result = EditorUtility.DisplayDialog(
                        "Import Simplygon?",
                        "Found Simplygon, but hasn't been added to Unity yet.  Do that now?",
                        "yes",
                        "no");

                if (result)
                {
                    var simplygonEditorDllFileName = "Simplygon.Unity.EditorPlugin.dll";
                    var simplygonDllFilePath = Path.Combine(simplygonInstallDirectory, simplygonEditorDllFileName);

                    var pluginsDirectory = "./Assets/Plugins";
                    var editorDirectory = Path.Combine(pluginsDirectory, "Editor");
                    var simplygonDirectory = Path.Combine(editorDirectory, "Simplygon");
                    var simplygonOutputFilePath = Path.Combine(simplygonDirectory, simplygonEditorDllFileName);

                    if (Directory.Exists(pluginsDirectory) == false)
                    {
                        Directory.CreateDirectory(pluginsDirectory);
                    }

                    if (Directory.Exists(editorDirectory) == false)
                    {
                        Directory.CreateDirectory(editorDirectory);
                    }

                    if (Directory.Exists(simplygonDirectory) == false)
                    {
                        Directory.CreateDirectory(simplygonDirectory);
                    }

                    File.WriteAllBytes(simplygonOutputFilePath, File.ReadAllBytes(simplygonDllFilePath));
                }
            }
        }

#if USING_SIMPLYGON

        private static GameObject ReduceInternal(List<GameObject> gameObjects, float quality)
        {
            // if so, initialize Simplygon
            using (Simplygon.ISimplygon simplygon = global::Simplygon.Loader.InitSimplygon(out Simplygon.EErrorCodes simplygonErrorCode, out string simplygonErrorMessage))
            {
                // if Simplygon handle is valid, loop all selected objects
                // and call Reduce function.
                if (simplygonErrorCode == Simplygon.EErrorCodes.NoError)
                {
                    return ReduceTest(simplygon, 0.5f, gameObjects);
                }

                // if invalid handle, output error message to the Unity console
                else
                {
                    Debug.Log("Initializing failed!");
                    return null;
                }
            }
        }

        private static GameObject ReduceTest(Simplygon.ISimplygon simplygon, float quality, List<GameObject> gameObjects)
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

                    return importedGameObjects.FirstOrDefault();

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
        
#endif
    }
}
