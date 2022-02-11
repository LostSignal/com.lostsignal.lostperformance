//-----------------------------------------------------------------------
// <copyright file="OptimizedLOD.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Linq;
    using UnityEngine;

    public class OptimizedLOD : MonoBehaviour
    {
        #pragma warning disable 0649
        [ReadOnly] [SerializeField] private int lodIndex;
        [ReadOnly] [SerializeField] private OptimizeState state;
        [ReadOnly] [SerializeField] private bool isInitialized;

        [SerializeField] private bool overrideSimplygonSettings;
        [SerializeField] private bool overrideUnityMeshSimplifierSettings;

        [ShowIf("overrideSimplygonSettings", true)]
        [SerializeField] private SimplygonSettings simplygonSettings;

        [ShowIf("overrideUnityMeshSimplifierSettings", true)]
        [SerializeField] private UnityMeshSimplifierSettings unityMeshSimplifierSettings;
        #pragma warning restore 0649

        public int LODIndex => this.lodIndex;

        public Optimizer Optimizer => this.GetComponentInParent<Optimizer>();

        public OptimizerSettings OptimizerSettings => this.Optimizer.Settings;

        public UnityMeshSimplifierSettings UnityMeshSimplifierSettings => this.overrideUnityMeshSimplifierSettings ? this.unityMeshSimplifierSettings : this.OptimizerSettings.UnityMeshSimplifierSettings;

        public float Quality => this.OptimizerSettings.LODSettings[this.lodIndex].Quality;

        public OptimizeState State
        {
            get => this.state;
            set => this.state = value;
        }

        public void Initialize()
        {
            if (this.isInitialized == false)
            {
                this.isInitialized = true;
                this.Unoptimize();
            }
        }

        public void Unoptimize()
        {
            MeshCombiner.CreateLOD(this);
            this.state = OptimizeState.Unoptimized;
        }

        private void OnValidate()
        {
            var settings = this.OptimizerSettings;
            var lodSetting = settings.LODSettings.FirstOrDefault(x => x.Name == this.name);

            if (lodSetting != null)
            {
                this.lodIndex = settings.LODSettings.IndexOf(lodSetting);
            }
            else
            {
                Debug.LogError("Unable to find LOD Index", this);
            }
        }
    }
}
