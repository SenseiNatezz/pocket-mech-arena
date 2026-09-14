using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace PocketMech.Editor {
 public static class WebBuild {
  public static void Build() {
   PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
   PlayerSettings.WebGL.decompressionFallback = true;
   PlayerSettings.WebGL.template = "PROJECT:PocketMech";
   PlayerSettings.WebGL.initialMemorySize = 128;
   PlayerSettings.WebGL.maximumMemorySize = 1024;
   PlayerSettings.WebGL.dataCaching = true;
   PlayerSettings.runInBackground = false;
            PlayerSettings.bundleVersion = "0.7.0";
   var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
    scenes = new[] {"Assets/PocketMech/Scenes/ArenaA1.unity"},
    locationPathName = "../PocketMechArena-Web", target = BuildTarget.WebGL,
    options = BuildOptions.None
   });
   if(report.summary.result != BuildResult.Succeeded) throw new System.Exception("Web build failed: " + report.summary.result);
   Debug.Log("PMA WEB BUILD PASSED: " + report.summary.totalSize);
  }
 }
}
