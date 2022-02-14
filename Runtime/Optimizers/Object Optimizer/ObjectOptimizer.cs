//-----------------------------------------------------------------------
// <copyright file="ObjectOptimizer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class ObjectOptimizer : Optimizer
    {
        #if UNITY_EDITOR
        
        #pragma warning disable 0649
        [Header("Object Optimizer")]
        [SerializeField] private ObjectOptimizerSettings objectOptimizerSettings;
        [SerializeField] private DefaultAsset outputFolder;

        [ReadOnly] [SerializeField] private List<BoxCollider> combinedBoxColliders;
        [ReadOnly] [SerializeField] private List<MeshCollider> combinedMeshColliders;
        #pragma warning restore 0649
                
        public override OptimizerSettings Settings => this.objectOptimizerSettings;

        public void Optimize()
        {
            var meshRendererInfos = MeshRendererInfo.GetMeshRendererInfos(new List<GameObject> { this.gameObject });
            this.Optimize(meshRendererInfos);
        }

        public override void Optimize(List<MeshRendererInfo> meshRendererInfos)
        {
            base.Optimize(meshRendererInfos);
            this.OptimizeBoxColliders();
            this.OptimizeMeshColliders();
        }

        public override void Revert()
        {
            if (this.IsOptimized)
            {
                this.RevertBoxColliderOptimization();
                this.RevertMeshColliderOptimization();
            }

            base.Revert();
        }

        public override void CleanUp()
        {
            if (this.IsOptimized)
            {
                this.CleanUpOptimizedBoxColliders();
                this.CleanUpOptimizedMeshColliders();
            }

            base.CleanUp();
        }

        protected override void OnValidate()
        {
            if (this.objectOptimizerSettings == null)
            {
                // TODO [bgish]: Get the default settings from project settings, not hard coded
                this.objectOptimizerSettings = EditorUtil.GetAssetByGuid<ObjectOptimizerSettings>("9026b0cfb0d3e9a4d95fd0c8697ec701");
            }

            if (this.outputFolder == null)
            {
                var fileNameNoExtension = Path.GetFileNameWithoutExtension(this.gameObject.scene.path);
                var directory = Path.GetDirectoryName(this.gameObject.scene.path);
                var outputPath = Path.Combine(directory, $"{fileNameNoExtension}_Meshes", "Objects").Replace("\\", "/");
                DirectoryUtil.CreateFolder(outputPath);
                this.outputFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(outputPath);
            }

            if (this.combinedBoxColliders == null)
            {
                this.combinedBoxColliders = new List<BoxCollider>();
            }

            if (this.combinedMeshColliders == null)
            {
                this.combinedMeshColliders = new List<MeshCollider>();
            }

            base.OnValidate();
        }

        public override string GetMeshDirectory() => AssetDatabase.GetAssetPath(this.outputFolder);

        private void OptimizeBoxColliders()
        {
            //// this.combinedBoxColliders = this.GetComponentsInChildren<BoxCollider>().ToList();
            //// 
            //// // TODO [bgish]: Optimize Box Colliders
            //// 
            //// this.combinedBoxColliders.ForEach(x => x.enabled = false);
        }

        private void OptimizeMeshColliders()
        {
            //// this.combinedMeshColliders = this.GetComponentsInChildren<MeshCollider>().ToList();
            //// 
            //// // TODO [bgish]: Optimize Mesh Colliders
            //// 
            //// this.combinedMeshColliders.ForEach(x => x.enabled = false);
        }

        private void RevertBoxColliderOptimization()
        {
            // TODO [bgish]: Delete BoxColliders parent GameObject
            this.combinedBoxColliders.ForEach(x => x.enabled = true);
            this.combinedBoxColliders.Clear();
        }

        private void RevertMeshColliderOptimization()
        {
            // TODO [bgish]: Delete MeshColliders parent GameObject
            this.combinedMeshColliders.ForEach(x => x.enabled = true);
            this.combinedMeshColliders.Clear();
        }

        private void CleanUpOptimizedBoxColliders()
        {
            //// TODO [bgish]: Implement...
            this.combinedBoxColliders.ForEach(x => GameObject.DestroyImmediate(x));
        }

        private void CleanUpOptimizedMeshColliders()
        {
            //// TODO [bgish]: Implement...
            this.combinedMeshColliders.ForEach(x => GameObject.DestroyImmediate(x));
        }
        
        #endif
    }
}
