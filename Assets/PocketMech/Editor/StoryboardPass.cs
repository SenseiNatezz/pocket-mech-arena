using UnityEditor;
using UnityEngine;
namespace PocketMech.Editor
{
    public static class StoryboardPass
    {
        public static void Build()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PocketMech/Resources/Prefabs/Rush.prefab");
            var elite = Object.Instantiate(source); elite.name = "Assault Striker";
            PrefabUtility.SaveAsPrefabAsset(elite, "Assets/PocketMech/Resources/Prefabs/Elite.prefab"); Object.DestroyImmediate(elite);
            PlayerSettings.bundleVersion = "0.4.0"; AssetDatabase.SaveAssets(); ProjectBuilder.BuildWindows();
        }
    }
}
