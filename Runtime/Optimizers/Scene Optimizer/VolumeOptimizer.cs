//-----------------------------------------------------------------------
// <copyright file="VolumeOptimizer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.IO;

    public class VolumeOptimizer : Optimizer
    {
        #if UNITY_EDITOR
        
        public override OptimizerSettings Settings => this.GetComponentInParent<SceneOptimizer>().Settings;

        public override string GetMeshDirectory()
        {
            var meshName = this.GetMeshName();
            var volumeNumber = int.Parse(meshName.Split('_')[1]);
            var volumeDir = ((volumeNumber / 10) * 10).ToString("000");
            var sceneOptimizer = this.GetComponentInParent<SceneOptimizer>();

            return Path.Combine(sceneOptimizer.GetOuputFolder(), volumeDir).Replace("\\", "/");
        }

        #endif
    }
}
