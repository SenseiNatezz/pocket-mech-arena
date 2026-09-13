using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PocketMech.Editor
{
    public static class ProjectBuilder
    {
        const string Root = "Assets/PocketMech/";
        [MenuItem("Pocket Mech/Rebuild Placeholder Assets and Scene")]
        public static void Prepare()
        {
            Directory.CreateDirectory(Root + "Resources/Art"); Directory.CreateDirectory(Root + "Resources/Prefabs"); Directory.CreateDirectory(Root + "Scenes");
            MakeArt("Square", false); MakeArt("Disc", true); AssetDatabase.Refresh();
            Visuals.Square = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Resources/Art/Square.png"); Visuals.Disc = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Resources/Art/Disc.png");
            var b = AssetDatabase.LoadAssetAtPath<Balance>(Root + "Resources/RunBalance.asset");
            if (b == null) { b = ScriptableObject.CreateInstance<Balance>(); AssetDatabase.CreateAsset(b, Root + "Resources/RunBalance.asset"); }
            var player = new GameObject("VX-01 Ranger"); player.AddComponent<PlayerMech>(); Visuals.Mech(player.transform); SavePrefab(player, "PlayerMech");
            foreach (EnemyKind kind in System.Enum.GetValues(typeof(EnemyKind))) { var e = new GameObject(kind.ToString()); e.AddComponent<Enemy>(); Visuals.Enemy(e.transform, kind); SavePrefab(e, kind.ToString()); }
            var bolt = Visuals.Shape("Beam Bolt", null, Vector2.zero, new Vector2(.12f, .6f), Visuals.Blue, 6); bolt.AddComponent<Projectile>(); SavePrefab(bolt, "Projectile");
            var xp = Visuals.Shape("XP Shard", null, Vector2.zero, Vector2.one * .23f, new Color(.35f, 1, .9f), 2); xp.AddComponent<XpPickup>(); SavePrefab(xp, "XpPickup");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var arena = new GameObject("Arena A-1 / Placeholder Environment");
            Visuals.Shape("Industrial Floor", arena.transform, Vector2.zero, new Vector2(26, 40), new Color(.055f, .085f, .105f), -10);
            for (int x = -12; x <= 12; x += 2) Visuals.Shape("Tile Seam", arena.transform, new Vector2(x, 0), new Vector2(.035f, 38), new Color(.1f, .15f, .18f), -9);
            for (int y = -18; y <= 18; y += 2) Visuals.Shape("Tile Seam", arena.transform, new Vector2(0, y), new Vector2(24, .035f), new Color(.1f, .15f, .18f), -9);
            foreach (int side in new[] { -1, 1 })
            {
                Visuals.Shape("Arena Boundary", arena.transform, new Vector2(side * 12, 0), new Vector2(.15f, 38), new Color(.2f, .65f, .72f), -7);
                Visuals.Shape("Arena Boundary", arena.transform, new Vector2(0, side * 19), new Vector2(24, .15f), new Color(.2f, .65f, .72f), -7);
                for (int y = -16; y <= 16; y += 4)
                {
                    Visuals.Shape("Wall Pylon", arena.transform, new Vector2(side * 12.5f, y), new Vector2(.7f, 1.5f), new Color(.2f, .28f, .32f), -5);
                    Visuals.Shape("Lane Light", arena.transform, new Vector2(side * 9.8f, y), new Vector2(.08f, 1.2f), new Color(.2f, .38f, .4f), -8);
                }
            }
            for (int y = -16; y <= 16; y += 8) { Visuals.Shape("Landing Stripe", arena.transform, new Vector2(0, y), new Vector2(3, .09f), new Color(.25f, .32f, .3f), -8); }
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraRig)); camera.tag = "MainCamera";
            var c = camera.GetComponent<Camera>(); c.orthographic = true; c.orthographicSize = 10.8f; c.clearFlags = CameraClearFlags.SolidColor; c.backgroundColor = new Color(.03f, .05f, .07f); camera.transform.position = new Vector3(0, -2, -10);
            var systems = new GameObject("Pocket Mech / Run Systems"); systems.AddComponent<Game>().balance = b;
            EditorSceneManager.SaveScene(scene, Root + "Scenes/ArenaA1.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Root + "Scenes/ArenaA1.unity", true) };
            PlayerSettings.companyName = "Pocket Mech Prototype"; PlayerSettings.productName = "Pocket Mech Arena"; PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultScreenWidth = 540; PlayerSettings.defaultScreenHeight = 960; PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true; PlayerSettings.runInBackground = true; PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false; PlayerSettings.allowedAutorotateToLandscapeRight = false; PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.pocketmech.arena.prototype");
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS, "com.pocketmech.arena.prototype");
            QualitySettings.vSyncCount = 0; QualitySettings.antiAliasing = 0;
            AssetDatabase.SaveAssets(); Debug.Log("PMA: scene, balance and nine prefabs generated.");
        }
        static void MakeArt(string name, bool circle)
        {
            string path = Root + "Resources/Art/" + name + ".png";
            var tex = new Texture2D(64, 64); var pixels = new Color[4096];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++) pixels[y * 64 + x] = !circle || Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) < 31.5f ? Color.white : Color.clear;
            tex.SetPixels(pixels); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); Object.DestroyImmediate(tex); AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 64; importer.filterMode = FilterMode.Bilinear; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
        }
        static void SavePrefab(GameObject go, string name) { PrefabUtility.SaveAsPrefabAsset(go, Root + "Resources/Prefabs/" + name + ".prefab"); Object.DestroyImmediate(go); }
        [MenuItem("Pocket Mech/Build Windows Prototype")]
        public static void BuildWindows()
        {
            Directory.CreateDirectory("../PocketMechArena-Windows");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { Root + "Scenes/ArenaA1.unity" }, locationPathName = "../PocketMechArena-Windows/PocketMechArena.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new System.Exception("PMA build failed: " + report.summary.result);
            Debug.Log("PMA BUILD PASSED: " + report.summary.totalSize + " bytes.");
        }
        public static void PrepareAndBuild() { Prepare(); BuildWindows(); }
    }
}
