using System.Reflection;
using UnityEditor;

namespace WaynGroup.Mgm.Ability.Editor
{
    internal class DotsScriptTemplates
    {
        [MenuItem("Assets/Create/DOTS/IAspect")]
        public static void CreateIAspect()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"Assets/Editor/DotsScriptTemplates/IAspect.txt",
                "IAspect.cs");
        }

        [MenuItem("Assets/Create/DOTS/Unmanaged System")]
        public static void CreateUnmanagedSystem()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"Assets/Editor/DotsScriptTemplates/UnmanagedSystem.txt",
                "UnmanagedSystem.cs");
        }

        [MenuItem("Assets/Create/DOTS/Authoring Component")]
        public static void CreateAuthoringComponent()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"Assets/Editor/DotsScriptTemplates/AuthoringComponent.txt",
                "AuthoringComponent.cs");
        }

        [MenuItem("Assets/Create/DOTS/IComponentData")]
        public static void CreateIComponentData()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"Assets/Editor/DotsScriptTemplates/IComponentData.txt",
                "IComponentData.cs");
        }
        [MenuItem("Assets/Create/DOTS/IBufferElementData")]
        public static void CreateIBufferElementData()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"Assets/Editor/DotsScriptTemplates/IBufferElementData.txt",
                "IBufferElementData.cs");
        }
        [MenuItem("Assets/Create/DOTS/Hybrid Component")]
        public static void CreateHybridComponent()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                $"Assets/Editor/DotsScriptTemplates/HybridComponent.txt",
                "HybridComponent.cs");
        }

    }
}