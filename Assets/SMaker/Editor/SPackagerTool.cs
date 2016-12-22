using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SPackagerTool : MonoBehaviour {
	// path config
    public static readonly string projectSvnPath = "004_Sgame_Tools";

    static string RootSvnPath
    {
        get
        {
            return Application.dataPath.Substring(0, Application.dataPath.IndexOf(projectSvnPath));
        }
    }

    // hero
	public static readonly string heroPkgSvnPath = "008_Sgame_Res/GameRes/Model/";
    public static readonly string heroFbxPath = "Assets/_Art/Models/";
	public static readonly string heroPrefabPath = "Assets/Resources/Models/";

    public static bool IsHeroModel (string assetPath) {
        return assetPath.StartsWith (heroFbxPath, true, null);
    }

	[MenuItem("Assets/Sgame - Package Model")]
    static void PackageModel () {
        CreatePrefabAndPackage (heroFbxPath, heroPrefabPath, heroPkgSvnPath);
    }

	public static void CheckAndCreatePath (string dir) {
		string rootPath = Application.dataPath.Substring (
			0,
			Application.dataPath.LastIndexOf ("/Assets")
		);
		string realPath = System.IO.Path.Combine (rootPath, dir);
		if (System.IO.Directory.Exists (realPath)) {
			return;
		}
		System.IO.Directory.CreateDirectory (realPath);
	}

    static GameObject CreatePrefab (string prefabPath, GameObject fbx) {
        GameObject prefab = PrefabUtility.CreatePrefab (prefabPath, fbx,ReplacePrefabOptions.ConnectToPrefab);
        AssetDatabase.SaveAssets();
        EditorApplication.SaveAssets();
        return prefab;
    }

	static void CreatePrefabAndPackage (string fbxPath, string prefabPath, string pkgSvnPath) {
		if (Selection.gameObjects.Length <= 0) {
			return;
		}
		foreach (GameObject fbx in Selection.gameObjects) {
			if (! AssetDatabase.GetAssetPath (fbx).StartsWith (fbxPath)) {
				ShowErrorDialog ("invalid path of fbx, should place in:" + fbxPath);
				continue;
			}
            CheckAndCreatePath(prefabPath);
            string prefab = prefabPath + fbx.name + ".prefab";
            string pkgPath = Path.Combine(RootSvnPath, pkgSvnPath);
			try {
				ExportPrefabToPkg (fbxPath, CreatePrefab (prefab, fbx), pkgPath);
			} catch (System.Exception e) {
				ShowErrorDialog (e.Message);
				Debug.LogWarning (e.StackTrace);
				return;
			}
		}
	}
	
	static bool CheckHeroModelPathRule (string fbxPath, string path, string goName, out string correctPath) {
		string materialPath  = Path.Combine (fbxPath, "Materials");
		string texturePath = Path.Combine (fbxPath, "Textures");
		correctPath = "";
		UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath (path, typeof(UnityEngine.Object));
		System.Type type = asset.GetType ();
		if (type == typeof (Material)) {
			correctPath = materialPath;
			return path.StartsWith (materialPath);
		}
        if (type == typeof (Texture) || type == typeof (Texture2D)) {
			correctPath = texturePath;
			return path.StartsWith (texturePath);
		}
		if (type == typeof (GameObject) && path.EndsWith (".fbx")) {
			correctPath = fbxPath;
			return path.StartsWith (fbxPath);
		}
		// donot care other type
		return true;
	}

	static bool CheckPathRule (string fbxPath, GameObject go) {
		string assetPath = AssetDatabase.GetAssetPath (go);
		foreach (string path in AssetDatabase.GetDependencies (new string[] {assetPath})) {
			string correctPath = "";
			if (! CheckHeroModelPathRule (fbxPath, path, go.name, out correctPath)) {
				EditorUtility.DisplayDialog (
					"Invalid Resource Path (" + go.name + "):",
					string.Format ("Move [{0}] to [{1}]", path, correctPath),
					"Ok"
				);
				return false;
			}
		}
		return true;
	}

	static void ExportPrefabToPkg (string fbxPath, GameObject prefab, string dstDir, string extension=".unitypackage") {
		string prefabPath = AssetDatabase.GetAssetPath (prefab);
		if (! CheckPathRule (fbxPath, prefab)) {
			return;
		}
		string pkgName = Path.Combine (dstDir, prefab.name + extension);
        List<string> dependencies =new List<string>(AssetDatabase.GetDependencies(new string[] { prefabPath }));
        for (int i = dependencies.Count-1; i > 0; i--)
        {
            if (dependencies[i].Contains(".cs"))
            {
                dependencies.RemoveAt(i);
            }
        }
		AssetDatabase.ExportPackage (
            dependencies.ToArray(),
			pkgName
		);
		//AssetDatabase.Refresh ();
		// open file explorer that point to the exported package, easy to do some svn operations
		OpenInFileBrowser.Open (pkgName);
	}

	static void ShowErrorDialog (string errMsg) {
		EditorUtility.DisplayDialog ("Invalid Operation", errMsg, "Ask Charlie For Help");
	}


    public static readonly string effectProjectSvnPath = "004_Sgame_Tools/EffectProject/";
    public static readonly string effectPkgSvnPath = "008_Sgame_Res/GameRes/Effect";
    public static readonly string effectPrefabPath = "Assets/Resources/effects/";
    public static readonly string sceneEffectPrefabPath = "Assets/_Art/Levels/SceneEffect/";

    public static Dictionary<string, string> prefabIDToNameMapping = new Dictionary<string, string>();

    [MenuItem("Assets/Xgame - Package Effect")]
    static void PackageEffect()
    {
        if (Selection.gameObjects.Length <= 0)
        {
            return;
        }
        prefabIDToNameMapping.Clear();
        string pkgPath = RootSvnPath + effectPkgSvnPath;
        ExportPrefabToPkg(CopyToDstPath(Selection.gameObjects, effectPrefabPath), pkgPath);
    }

    [MenuItem("Assets/Xgame - Package SceneEffect")]

    static void PackageSceneEffect()
    {
        if (Selection.gameObjects.Length <= 0)
        {
            return;
        }
        prefabIDToNameMapping.Clear();
        string pkgPath = RootSvnPath + effectPkgSvnPath;
        ExportPrefabToPkg(Selection.gameObjects, pkgPath);
    }

    static string GetBasename(string assetPath, bool withSuffix = true)
    {
        string filename = assetPath.Substring(assetPath.LastIndexOf('/') + 1);
        return withSuffix ? filename : filename.Substring(0, filename.LastIndexOf('.'));
    }

    static void CopyAsset(string originPath, string newPath)
    {
        if (originPath == newPath)
        {
            return;
        }
        if (File.Exists(newPath))
        {
            AssetDatabase.DeleteAsset(newPath);
            AssetDatabase.Refresh();
        }
        if (AssetDatabase.CopyAsset(originPath, newPath))
        {
            AssetDatabase.Refresh();
        }
    }

    static string GetPrefabOriginName(string path)
    {
        string basename = GetBasename(path);
        if (prefabIDToNameMapping.ContainsKey(basename))
        {
            return prefabIDToNameMapping[basename];
        }
        return basename;
    }

    static List<string> CopyToDstPath(GameObject[] prefabs, string dstDir,bool isCopyToResource = true)
    {
        List<string> copyEffectList = new List<string>();
        foreach (GameObject go in prefabs)
        {
            string assetPath = AssetDatabase.GetAssetPath(go);
            string effectID = GetBasename(assetPath);
            if (string.IsNullOrEmpty(effectID))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Prefab Name",
                    "Should Start With Numbers, Like: 10001_example.prefab",
                    "Ok"
                );
                continue;
            }
            CheckAndCreatePath(dstDir);
            string newPath = Path.Combine(dstDir, effectID);
            CopyAsset(assetPath, newPath);
            copyEffectList.Add(newPath);
        }
        return copyEffectList;
    }
    static void ExportPrefabToPkg(GameObject[] gameObjects, string dstDir, string extension = ".unitypackage")
    {
        List<string> paths = new List<string>();
        foreach (var go in gameObjects)
        {
            paths.Add(AssetDatabase.GetAssetPath(go));
        }
        ExportPrefabToPkg(paths, dstDir, extension);
    }
    static void ExportPrefabToPkg(List<string> prefabList, string dstDir, string extension = ".unitypackage")
    {
        foreach (string path in prefabList)
        {
            string pkgName = Path.Combine(dstDir, GetBasename(GetPrefabOriginName(path), false) + extension);
            List<string> dependencies = new List<string>(AssetDatabase.GetDependencies(new string[] { path }));
            for (int i = 0; i < dependencies.Count; )
            {
                if (dependencies[i].Contains("NGUI"))
                {
                    dependencies.RemoveAt(i);
                    continue;
                }
                i++;
            }
            AssetDatabase.ExportPackage(dependencies.ToArray(), pkgName);
            AssetDatabase.Refresh();
            // open file explorer that point to the exported package, easy to do some svn operations
            OpenInFileBrowser.Open(pkgName);
        }
    }
}
