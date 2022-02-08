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
        protected override string GetMeshDirectory()
        {
            var meshName = this.GetMeshName();
            var volumeNumber = int.Parse(meshName.Split('_')[1]);
            var volumeDir = ((volumeNumber / 10) * 10).ToString("000");
            var sceneOptimizer = this.GetComponentInParent<SceneOptimizer>();

            return Path.Combine(sceneOptimizer.GetOuputFolder(), volumeDir).Replace("\\", "/");
        }
    }
}
