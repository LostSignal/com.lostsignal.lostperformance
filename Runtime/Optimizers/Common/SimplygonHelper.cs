//-----------------------------------------------------------------------
// <copyright file="SimplygonHelper.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR

namespace Lost
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    #if USING_SIMPLYGON
    using Simplygon.Unity.EditorPlugin;
    #endif

    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public static class SimplygonHelper
    {
        static SimplygonHelper()
        {
            ProjectDefinesHelper.AddOrRemoveDefine("Simplygon.Unity.EditorPlugin", "USING_SIMPLYGON");
        }

        public static void ReduceGameObject(GameObject gameObject, float quality, int lod, SimplygonSettings settings)
        {
            #if !USING_SIMPLYGON
            SetupSimplygon();
            #else

            //// TODO [bgish]: Filter out and print errors if find Meshes with zero verts/tris

            //// // Creating a single object that will hold copies of the given gameObjects
            //// string batchObjectName = $"Simplygon LOD{lod} Batch";
            //// var batchObject = GameObject.Find(batchObjectName);
            //// if (batchObject == null)
            //// {
            ////     batchObject = new GameObject(batchObjectName);
            ////     batchObject.transform.Reset();
            //// }
            //// 
            //// batchObject.DestroyChildren();
            //// 
            //// var objectsToReduce = new List<GameObject>();
            //// 
            //// foreach (var gameObject in gameObjects)
            //// {
            ////     var copy = GameObject.Instantiate(gameObject, batchObject.transform);
            ////     copy.name = gameObject.GetInstanceID().ToString();
            ////     copy.transform.position = gameObject.transform.position;
            ////     copy.transform.rotation = gameObject.transform.rotation;
            ////     copy.transform.localScale = gameObject.transform.localScale;
            ////     
            ////     objectsToReduce.Add(copy);
            //// }

            // Sending the batch to Simplygon
            GameObject simplygonOutput = null;
            
            simplygonOutput = SimplygonReduce(gameObject, quality, settings);
            
            if (simplygonOutput == null)
            {
                Debug.LogError("Unexcpected error when reducing game objects with Simplygon.");
                return;
            }

            var originalMeshFilter = gameObject.GetComponent<MeshFilter>();
            var originalSharedMesh = originalMeshFilter.sharedMesh;
            var originalMeshName = originalSharedMesh.name;
            var originalMeshAssetPath = AssetDatabase.GetAssetPath(originalSharedMesh);
            var originalMeshFullPath = Path.GetFullPath(originalMeshAssetPath);
            Debug.Log("originalMeshFullPath = " + originalMeshFullPath);

            var simplygonMeshFilter = simplygonOutput.GetComponent<MeshFilter>();
            var simplygonSharedMesh = simplygonMeshFilter.sharedMesh;
            var simplygonMeshName = simplygonSharedMesh.name;
            var simplygonMeshAssetPath = AssetDatabase.GetAssetPath(simplygonSharedMesh);
            var simplygonMeshFullPath = Path.GetFullPath(simplygonMeshAssetPath);
            Debug.Log("simplygonMeshFullPath = " + simplygonMeshFullPath);

            string simplygonText = File.ReadAllText(simplygonMeshFullPath)
                .Replace(simplygonMeshName, originalMeshName);

            File.WriteAllText(originalMeshFullPath, simplygonText);
            AssetDatabase.ImportAsset(originalMeshAssetPath);
                        
            //// // Updating the original game objects list with the new meshes
            //// for (int i = 0; i < batchObject.transform.childCount; i++)
            //// {
            ////     var batchChild = batchObject.transform.GetChild(i);
            ////     var originalInstanceId = int.Parse(batchChild.name);
            ////     var original = (GameObject)EditorUtility.InstanceIDToObject(originalInstanceId);
            //// 
            ////     List<GameObject> simplygonTransforms = new List<GameObject>();
            //// 
            ////     // Speical case if there is only 1 object, it's the root
            ////     if (batchObject.transform.childCount == 1)
            ////     {
            ////         simplygonTransforms.Add(simplygonOutput);
            ////     }
            ////     else
            ////     {
            ////         simplygonTransforms = FindChildGameObject(simplygonOutput.transform, batchChild.name);
            ////     }
            //// 
            ////     if (simplygonTransforms.Count == 0)
            ////     {
            ////         Debug.LogError($"Unable to find Simplygon Version of {original.name} with instance id {originalInstanceId}", original);
            ////         continue;
            ////     }
            //// 
            ////     if (simplygonTransforms.Count > 1)
            ////     {
            ////         Debug.LogError($"Found too many matching Simplygon Version of {original.name} with instance id {originalInstanceId}", original);
            ////         continue;
            ////     }
            //// 
            ////     var simplygon = simplygonTransforms.FirstOrDefault();
            ////     var originalMeshFilter = original.GetComponent<MeshFilter>();
            ////     var simplygonMeshFilter = simplygon.GetComponent<MeshFilter>();
            //// 
            ////     if (simplygonMeshFilter == null)
            ////     {
            ////         Debug.LogError($"Skipping Object {original.name}, simplygon did not produce a mesh filter for this object.", original);
            ////         continue;
            ////     }
            //// 
            ////     // Copying the simplygon version 
            ////     var simplygonMesh = simplygonMeshFilter.sharedMesh;
            ////     var originalMesh = originalMeshFilter.sharedMesh;
            ////     var originalName = originalMesh.name;
            //// 
            ////     EditorUtility.CopySerialized(simplygonMesh, originalMesh);
            ////     
            ////     originalMesh.name = originalName;
            //// 
            ////     //// EditorUtil.SetDirty(originalMesh);
            ////     //// 
            ////     //// 
            ////     //// var newMesh = AssetDatabase.LoadAssetAtPath<Mesh>(simplygonMesh);
            ////     //// var meshCopy = GameObject.Instantiate(newMesh);
            ////     //// meshCopy.name = originalMeshFilter.sharedMesh.name;
            ////     //// 
            ////     //// EditorApplication.delayCall += () =>
            ////     //// {
            ////     ////     originalMeshFilter.sharedMesh = meshCopy;
            ////     ////     EditorUtility.SetDirty(originalMeshFilter.gameObject);
            ////     //// };
            //// }

            // Destorying generated game objects
            //// GameObject.DestroyImmediate(batchObject);
            GameObject.DestroyImmediate(simplygonOutput);

            //// List<GameObject> FindChildGameObject(Transform parent, string childToFind)
            //// {
            ////     List<GameObject> children = new List<GameObject>();
            //// 
            ////     for (int i = 0; i < parent.childCount; i++)
            ////     {
            ////         var child = parent.GetChild(i);
            //// 
            ////         if (childToFind.EndsWith(child.name.Replace("_", string.Empty)))
            ////         {
            ////             children.Add(child.gameObject);
            ////         }
            ////     }
            //// 
            ////     return children;
            //// }
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

        private static GameObject SimplygonReduce(GameObject gameObject, float quality, SimplygonSettings settings)
        {
            using (Simplygon.ISimplygon simplygon = global::Simplygon.Loader.InitSimplygon(out Simplygon.EErrorCodes simplygonErrorCode, out string simplygonErrorMessage))
            {
                if (simplygonErrorCode == Simplygon.EErrorCodes.NoError)
                {
                    return Reduce(simplygon, quality, gameObject, settings);
                }
                else
                {
                    Debug.Log($"Simplygon Initializing failed with error code {simplygonErrorCode}");
                    return null;
                }
            }
        }

        private static GameObject Reduce(Simplygon.ISimplygon simplygon, float quality, GameObject gameObject, SimplygonSettings settings)
        {
            var exportTempDirectory = SimplygonUtils.GetNewTempDirectory();
            var gameObjects = new List<GameObject> { gameObject };

            using (var sgScene = SimplygonExporter.Export(simplygon, exportTempDirectory, gameObjects))
            using (var reductionPipeline = simplygon.CreateReductionPipeline())
            using (var reductionSettings = reductionPipeline.GetReductionSettings())
            using (var repairSettings = reductionPipeline.GetRepairSettings())
            {
                reductionSettings.SetReductionTargets(Simplygon.EStopCondition.All, true, false, false, false);
                reductionSettings.SetReductionTargetTriangleRatio(quality);

                reductionSettings.SetCurvatureImportance(settings.CurvatureImportance);
                reductionSettings.SetEdgeSetImportance(settings.EdgeSetImportance);
                reductionSettings.SetGeometryImportance(settings.GeometryImportance);
                reductionSettings.SetGroupImportance(settings.GroupImportance);
                reductionSettings.SetMaterialImportance(settings.MaterialImportance);
                reductionSettings.SetShadingImportance(settings.ShadingImportance);
                reductionSettings.SetSkinningImportance(settings.SkinningImportance);
                reductionSettings.SetTextureImportance(settings.TextureImportance);
                reductionSettings.SetVertexColorImportance(settings.VertexColorImportance);

                repairSettings.SetProgressivePasses(settings.ProgressivePasses);
                repairSettings.SetUseWelding(settings.UseWelding);
                repairSettings.SetWeldDist(settings.WeldDist);
                repairSettings.SetUseTJunctionRemover(settings.UseTJunctionRemover);
                repairSettings.SetTJuncDist(settings.TJuncDist);
                repairSettings.SetWeldOnlyBetweenSceneNodes(settings.WeldOnlyBetweenSceneNodes);
                repairSettings.SetWeldOnlyBorderVertices(settings.WeldOnlyBorderVertices);
                repairSettings.SetWeldOnlyWithinMaterial(settings.WeldOnlyWithinMaterial);
                repairSettings.SetWeldOnlyWithinSceneNode(settings.WeldOnlyWithinSceneNode);

                reductionPipeline.RunScene(sgScene, Simplygon.EPipelineRunMode.RunInThisProcess);

                string folderName = "Simplygon Temp Assets";
                string baseFolder = $"Assets/{folderName}";

                if (AssetDatabase.IsValidFolder(baseFolder) == false)
                {
                    AssetDatabase.CreateFolder("Assets", folderName);
                }

                string assetFolderGuid = AssetDatabase.CreateFolder(baseFolder, "Simplygon Output");
                string assetFolderPath = AssetDatabase.GUIDToAssetPath(assetFolderGuid);

                List<GameObject> importedGameObjects = new List<GameObject>();
                int startingLodIndex = 0;
                SimplygonImporter.Import(simplygon, reductionPipeline, ref startingLodIndex, assetFolderPath, "Simplygon Output", importedGameObjects);

                // Cleaning up the temp export folder simplygon generated
                DirectoryUtil.DeleteDirectory(exportTempDirectory);

                //// TODO [bgish]: Need to delete the asset folder created (but might be tricky because we need them to live for a few ticks before destroying them)

                return importedGameObjects.FirstOrDefault();
            }
        }

        public static string GetTempSimplygonAssetsFolder() => $"Assets/Simplygon Temp Assets";

        public static void DeleteTempSimplygonAssetsFolder()
        {
            var folder = GetTempSimplygonAssetsFolder();

            if (AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.DeleteAsset(folder);
            }
        }
#endif
    }
}

#endif
