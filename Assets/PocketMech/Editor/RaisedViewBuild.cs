using UnityEditor;
namespace PocketMech.Editor
{
    public static class RaisedViewBuild
    {
        public static void Build(){PlayerSettings.bundleVersion="0.7.0";AssetDatabase.SaveAssets();ProjectBuilder.BuildWindows();}
    }
}
