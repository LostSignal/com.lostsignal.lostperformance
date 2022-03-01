//-----------------------------------------------------------------------
// <copyright file="RemoteInspectorManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    //// ### Remote Inspector Tool
    //// * Take Screenshot (also records level name and xyz position)
    //// * Use static function Attributes to expose buttons that can be called
    //// * Have list of all Scene and option to load them with a click of a button
    //// * Ability to change "LOD Bias" in the Quality Settings
    //// * Functions
    ////   * Visualize NavMesh (gets nav mesh data, makes an actual mesh out of it and puts a material on it)
    ////   * Change Physics Time Step
    ////   * Turn occlusion culling on/off
    ////   * Enable/Disable logging (Debug.unityLogger.logEnabled)
    ////
    ////
    ////
    ////
    ////
    ////
    ////
    ////
    ////
    ////
    ////

    public class RemoteInspectorManager : SingletonMonoBehaviour<RemoteInspectorManager>, IName
    {
        public string Name => "Remote Inspector Manager";
    }
}

#endif
