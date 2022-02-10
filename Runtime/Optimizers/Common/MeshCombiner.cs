//-----------------------------------------------------------------------
// <copyright file="MeshCombiner.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

//// TODO [bgish]: Track all mesh renders that have been combined and on build, delete them and remove reference
//// TODO [bgish]: Add check box to delete empty game objects as well
//// TODO [bgish]: Make sure we also clean up the temp directory after done importing simplygon assets

#if UNITY_EDITOR

namespace Lost
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public static class MeshCombiner
    {
        public static void DeleteEmptyOrDisabledGameObjects(Transform childTransform, bool isRoot = true)
        {
            for (int i = 0; i < childTransform.childCount; i++)
            {
                DeleteEmptyOrDisabledGameObjects(childTransform.GetChild(i), false);
            }

            // Determining if we have an unused LODGroup
            var lodGroup = childTransform.GetComponent<LODGroup>();

            if (lodGroup != null && lodGroup.enabled == false)
            {
                GameObject.DestroyImmediate(lodGroup);
            }

            Component[] components = childTransform.GetComponentsInChildren<Component>();
            bool hasNoComponents = components.Length == 1;
            bool isNotActive = childTransform.gameObject.activeInHierarchy == false;

            if (childTransform.name == "LOD1")
            {
                Debug.Log("LOD1", childTransform);
            }

            if (isRoot == false && (isNotActive || hasNoComponents))
            {
                GameObject.DestroyImmediate(childTransform.gameObject);
            }
        }

        public static void CreateLOD(Transform transform,
            List<LODSetting> lodSettings,
            List<MeshRendererInfo> meshRendererInfos,
            bool generateLODGroup,
            string meshName,
            string assetDirectory,
            int lod = -1)
        {
            // Early out if no settings
            if (lodSettings.Count == 0)
            {
                return;
            }

            // Creating LOD and making sure has needed components
            var lodTransform = GetLODTransform(transform, lodSettings, lod);
            lodTransform.gameObject.GetOrAddComponent<MeshFilter>();
            lodTransform.gameObject.GetOrAddComponent<MeshRenderer>();

            // Creating the combined mesh
            var newMeshName = meshName == null ? $"LOD{lod}" : $"{meshName}_LOD{lod}";
            CreateCombinedMeshGameObject(lodTransform, meshRendererInfos, lod, newMeshName, assetDirectory);

            // Disabling all affected LODGroups
            UpdateLODGroups(meshRendererInfos, false);

            GenerateLODGroup();
            
            void GenerateLODGroup()
            {
                if (generateLODGroup == false)
                {
                    return;
                }

                var lodsTransform = GetLODsTransform(transform, true);
                var lods = new List<LOD>();

                for (int lodIndex = 0; lodIndex < lodSettings.Count; lodIndex++)
                {
                    lods.Add(new LOD
                    {
                        screenRelativeTransitionHeight = lodSettings[lodIndex].ScreenPercentage,
                        renderers = GetLODTransform(transform, lodSettings, lodIndex).GetComponentsInChildren<MeshRenderer>().ToArray(),
                    });
                }

                var lodGroup = lodsTransform.gameObject.GetOrAddComponent<LODGroup>();
                lodGroup.SetLODs(lods.ToArray());

                EditorUtil.SetDirty(lodsTransform.gameObject);
            }
        }

        public static void DestoryLODs(Transform transform, List<MeshRendererInfo> meshRendererInfos)
        {
            var lodsTransform = GetLODsTransform(transform, false);
            
            // Destroying LOD Transforms
            if (lodsTransform != null)
            {
                if (Application.isPlaying == false)
                {
                    GameObject.DestroyImmediate(lodsTransform.gameObject);
                }
                else
                {
                    GameObject.Destroy(lodsTransform.gameObject);
                }
            }

            // Enabling all turned off MeshRenderers
            foreach (var meshRendererInfo in meshRendererInfos)
            {
                meshRendererInfo.MeshRenderer.enabled = true;
            }

            // Enabling all turned off LODGroups
            UpdateLODGroups(meshRendererInfos, true);
        }

        public static Transform GetLODTransform(Transform transform, List<LODSetting> settings, int lodIndex)
        {
            var lodsTransform = GetLODsTransform(transform, true);
            var lodName = settings[lodIndex].Name;
            var lod = transform.Find($"LODS/{lodName}");

            if (lod == null)
            {
                lod = new GameObject(lodName).transform;
                lod.SetParent(lodsTransform);
                lod.Reset();
            }

            if (lod.GetSiblingIndex() != lodIndex)
            {
                lod.SetSiblingIndex(lodIndex);
            }

            return lod;
        }

        //// public static void CalculateLOD(Transform transform, List<LODSetting> settings, int lodIndex)
        //// {
        ////     if (lodIndex >= settings.Count)
        ////     {
        ////         return;
        ////     }
        //// 
        ////     var lodTransform = GetLODTransform(settings, transform, lodIndex);
        ////     var finalMeshFilter = lodTransform.gameObject.GetOrAddComponent<MeshFilter>();
        ////     var finalMeshRenderer = lodTransform.gameObject.GetOrAddComponent<MeshRenderer>();
        ////     this.SetCombinedMesh(finalMeshFilter, finalMeshRenderer, lodIndex);
        //// 
        ////     var lodSetting = this.settings.LODSettings[lodIndex];
        //// 
        ////     if (lodSettings.Quality == 1.0f)
        ////     {
        ////         return;
        ////     }
        //// 
        ////     foreach (var meshFilter in lodTransform.GetComponentsInChildren<MeshFilter>())
        ////     {
        ////         var newMesh = this.CreateNewMesh(lodSetting, meshFilter);
        //// 
        ////         if (newMesh != null)
        ////         {
        ////             meshFilter.mesh = newMesh;
        ////         }
        ////     }
        //// }

        ////
        //// Took a lot of inspiration from this blog post http://projectperko.blogspot.com/2016/08/multi-material-mesh-merge-snippet.html
        ////
        private static void CreateCombinedMeshGameObject(Transform transform, List<MeshRendererInfo> meshRendererInfos, int lodIndex, string meshName, string assetDirectory)
        {
            var finalMeshFilter = transform.gameObject.GetOrAddComponent<MeshFilter>();
            var finalMeshRenderer = transform.gameObject.GetOrAddComponent<MeshRenderer>();

            // Filting out all mesh renderers that are meant to be ignored at this LOD level
            meshRendererInfos = meshRendererInfos.Where(x => x.IgnoreMeshCombine == null || lodIndex < (int)x.IgnoreMeshCombine.IgnoreLOD).ToList();

            //// BUG [bgish]: These are some serious bugs invovling how we treat mesh renderers that are already apart of an LODGroup
            //// // Filtering out all MeshRenderes that aren't apart of this LOD
            // meshRendererInfos = meshRendererInfos.Where(x => x.LodGroup != null && x.LODLevel == lodIndex).ToList();
            // REALLY need to combine all meshes that aren't apart of an LODGroup, Calculate New Mesh, then combines those back in

            var meshRenderers = meshRendererInfos.Select(x => x.MeshRenderer).ToList();
            var meshFilters = meshRendererInfos.Select(x => x.MeshFilter).ToList();
            var materials = new List<Material>();

            // Making sure our arrays are value
            if (meshRenderers.Count != meshFilters.Count)
            {
                Debug.LogError("SOMETHING WHEN HORRIBLY WRONG");
                return;
            }

            // Collecting all unique materials
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                foreach (Material material in meshRenderer.sharedMaterials)
                {
                    materials.AddIfNotNullAndUnique(material);
                }
            }

            var submeshes = new List<Mesh>();
            var vertexCount = 0;

            foreach (Material material in materials)
            {
                // Make a combiner for each (sub)mesh that is mapped to the right material.
                var combiners = new List<CombineInstance>();

                for (int i = 0; i < meshRenderers.Count; i++)
                {
                    var meshRenderer = meshRenderers[i];
                    var meshFilter = meshFilters[i];
                    int materialIndex = Array.IndexOf(meshRenderer.sharedMaterials, material);

                    if (materialIndex != -1 && meshFilter.sharedMesh != null)
                    {
                        vertexCount += meshFilter.sharedMesh.vertexCount;

                        combiners.Add(new CombineInstance
                        {
                            mesh = meshFilter.sharedMesh,
                            subMeshIndex = materialIndex,
                            transform = transform.worldToLocalMatrix * meshRenderer.transform.localToWorldMatrix,
                        });
                    }
                }

                // Flatten into a single mesh.
                Mesh mesh = new Mesh();
                mesh.indexFormat = vertexCount >= ushort.MaxValue ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
                mesh.CombineMeshes(combiners.ToArray(), true);
                submeshes.Add(mesh);
            }

            int vertCount = 0;

            // The final mesh: combine all the material-specific meshes as independent submeshes.
            var finalCombiners = new List<CombineInstance>();
            foreach (Mesh mesh in submeshes)
            {
                vertCount += mesh.vertexCount;

                finalCombiners.Add(new CombineInstance
                {
                    mesh = mesh,
                    subMeshIndex = 0,
                    transform = Matrix4x4.identity,
                });
            }

            // Disabling all the MeshRenderers (MeshRenderers and MeshFilters will be destoryed on build)
            for (int i = 0; i < meshRenderers.Count; i++)
            {
                meshRenderers[i].enabled = false;
            }

            var finalMesh = new Mesh();
            finalMesh.indexFormat = vertCount >= ushort.MaxValue ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            finalMesh.CombineMeshes(finalCombiners.ToArray(), false);

            if (string.IsNullOrWhiteSpace(meshName) == false)
            {
                finalMesh.name = meshName;
            }

            finalMeshFilter.sharedMesh = finalMesh;
            finalMeshRenderer.sharedMaterials = materials.ToArray();

            // Saving this mesh to disk
            DirectoryUtil.CreateFolder(assetDirectory);
            
            var meshAssetPath = Path.Combine(assetDirectory, meshName + ".asset").Replace("\\", "/");
            
            if (File.Exists(meshAssetPath) == false)
            {
                AssetDatabase.CreateAsset(finalMeshFilter.sharedMesh, meshAssetPath);
            }
            else
            {
                var finalSharedMesh = finalMeshFilter.sharedMesh;
                var existingMesh = AssetDatabase.LoadMainAssetAtPath(meshAssetPath) as Mesh;
                existingMesh.Clear();
            
                EditorUtility.CopySerialized(finalSharedMesh, existingMesh);
                existingMesh.name = meshName;
                finalMeshFilter.sharedMesh = existingMesh;
            
                EditorUtil.SetDirty(existingMesh);
            }

            EditorUtil.SetDirty(finalMeshFilter.gameObject);
            EditorUtil.SetDirty(finalMeshRenderer.gameObject);
        }

        public static Transform GetLODsTransform(Transform transform, bool createIfDoesNotExist)
        {
            var lods = transform.Find("LODS");

            if (lods == null && createIfDoesNotExist)
            {
                lods = new GameObject("LODS").transform;
                lods.SetParent(transform);
                lods.Reset();
            }

            return lods;
        }

        private static void UpdateLODGroups(List<MeshRendererInfo> meshRendererInfos, bool enable)
        {
            foreach (var lodGroup in meshRendererInfos.Where(x => x.LodGroup != null && x.IsIgnored == false).Select(x => x.LodGroup).Distinct())
            {
                lodGroup.enabled = enable;

                var lods = lodGroup.GetLODs();

                for (int j = 0; j < lods.Length; j++)
                {
                    Array.ForEach(lods[j].renderers, x =>
                    {
                        if (x is MeshRenderer)
                        {
                            x.enabled = enable;
                        }
                    });
                }
            }
        }

        //// private static Mesh CreateNewMesh(OptimizerSettings settings, MeshFilter meshFilter)
        //// {
        ////     #if UNITY_EDITOR
        //// 
        //// 
        ////     #if !USING_SIMPLYGON
        ////     if (settings.)
        ////     #endif
        //// 
        ////     #if USING_UNITY_MESH_SIMPLIFIER
        //// 
        //// 
        //// 
        //// 
        ////     if (lodSettings.Simplifier == LODSettings.MeshSimplifier.UnityMeshSimplifier)
        ////     {
        ////         // TODO [bgish]: Need to wrap in USING_UNITY_MESH_SIMPLIFIER and print error (not installed if it fails)
        ////         // Also, add "com.whinarn.unitymeshsimplifier": "https://github.com/Whinarn/UnityMeshSimplifier.git#v3.0.1", to your manifest
        ////         var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
        ////         meshSimplifier.SimplificationOptions = lodSettings.Options;
        ////         meshSimplifier.Initialize(meshFilter.mesh);
        ////         meshSimplifier.SimplifyMesh(lodSettings.Quality);
        //// 
        ////         var newMesh = meshSimplifier.ToMesh();
        ////         newMesh.RecalculateNormals();
        ////         newMesh.Optimize();
        ////         return newMesh;
        ////     }
        ////     else if (lodSettings.Simplifier == LODSettings.MeshSimplifier.Simplygon)
        ////     {
        ////         // if so, initialize Simplygon
        ////         using (Simplygon.ISimplygon simplygon = global::Simplygon.Loader.InitSimplygon(out Simplygon.EErrorCodes simplygonErrorCode, out string simplygonErrorMessage))
        ////         {
        ////             // if Simplygon handle is valid, loop all selected objects
        ////             // and call Reduce function.
        ////             if (simplygonErrorCode == Simplygon.EErrorCodes.NoError)
        ////             {
        ////                 Reduce(meshFilter, simplygon, lodSettings.Quality);
        ////             }
        //// 
        ////             // if invalid handle, output error message to the Unity console
        ////             else
        ////             {
        ////                 Debug.Log("Initializing failed!");
        ////             }
        ////         }
        //// 
        ////         return null;
        ////     }
        ////     else
        ////     {
        ////         Debug.LogError($"Unknown Mesh Simplifier {lodSettings.Simplifier} found!");
        ////         return null;
        ////     }
        ////     #else
        //// 
        ////     return null;
        //// 
        ////     #endif
        //// }
        //// 
        //// public void SeperateMesh(int lodIndex)
        //// {
        ////     if (lodIndex >= this.lodSettings.Count)
        ////     {
        ////         return;
        ////     }
        //// 
        ////     var lod = this.transform.Find($"LODS/{this.lodSettings[lodIndex].Name}");
        //// 
        ////     if (lod != null)
        ////     {
        ////         this.SeperateMesh(lod);
        ////     }
        //// }
        //// 
        //// private void SeperateMesh(Transform meshTransfrom)
        //// {
        ////     var originalMeshFilter = meshTransfrom.GetComponent<MeshFilter>();
        ////     var originalMeshRenderer = meshTransfrom.GetComponent<MeshRenderer>();
        ////     var meshRendererCopies = new List<MeshRenderer>();
        //// 
        ////     for (int i = 0; i < originalMeshRenderer.sharedMaterials.Length; i++)
        ////     {
        ////         var subMesh = new GameObject(originalMeshRenderer.sharedMaterials[i].name);
        ////         subMesh.transform.SetParent(meshTransfrom);
        ////         subMesh.transform.Reset();
        //// 
        ////         #if UNITY_EDITOR
        ////         UnityEditorInternal.ComponentUtility.CopyComponent(originalMeshFilter);
        ////         UnityEditorInternal.ComponentUtility.PasteComponentAsNew(subMesh);
        //// 
        ////         UnityEditorInternal.ComponentUtility.CopyComponent(originalMeshRenderer);
        ////         UnityEditorInternal.ComponentUtility.PasteComponentAsNew(subMesh);
        ////         #endif
        //// 
        ////         meshRendererCopies.Add(subMesh.GetComponent<MeshRenderer>());
        ////     }
        //// 
        ////     // Making sure each copy only has 1 material that it's concered about
        ////     for (int i = 0; i < meshRendererCopies.Count; i++)
        ////     {
        ////         meshRendererCopies[i].sharedMaterials = new Material[] { originalMeshRenderer.sharedMaterials[i] };
        //// 
        ////         // Now removing all sub meshes that aren't needed for this new mesh
        ////         var meshFilter = meshRendererCopies[i].GetComponent<MeshFilter>();
        ////         var newMesh = Mesh.Instantiate(meshFilter.sharedMesh);
        ////         newMesh.triangles = meshFilter.sharedMesh.GetTriangles(i);
        ////         newMesh.Optimize();
        //// 
        ////         meshFilter.mesh = newMesh;
        ////     }
        //// 
        ////     // Destorying the Originals
        ////     GameObject.DestroyImmediate(originalMeshRenderer);
        ////     GameObject.DestroyImmediate(originalMeshFilter);
        //// }
    }
}

#endif
