using UnityEngine;
using UnityEditor;

using System.Collections;

public class SModelImporter : AssetPostprocessor {

    string[] whiteList = new string[] { "_Art/Models", "_Art/Levels" };

    bool IsNeedAutoSetting(string path)
    {

        for (int i = 0; i < whiteList.Length; i++)
        {
            if (path.Contains(whiteList[i]))
            {
                return true;
            }
        }
        return false;
    }

	void OnPreprocessModel () {

        if (!IsNeedAutoSetting(assetPath))
        {
			return;
		}
		ModelImporter importer = assetImporter as ModelImporter;
		// animation fbx, no need to import materials
        if (!importer.assetPath.Contains("@"))
        {
            //去掉Tangents,手机材质基本用不到
            importer.importTangents = ModelImporterTangents.None;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
        }

        importer.animationType = ModelImporterAnimationType.Legacy;
        importer.generateAnimations = ModelImporterGenerateAnimations.GenerateAnimations;
        

        
        
	}

	void OnPostprocessModel (GameObject go) {

        if (IsNeedAutoSetting(assetPath))
        {
            ModelImporter importer = assetImporter as ModelImporter;
            // extra animation operations
            Animation anim = go.GetComponent<Animation>();
            //Debug.Log(AssetDatabase.GetAssetPath(go));
            if (anim)
            {
                anim.playAutomatically = false;
                if (anim.GetClipCount() <= 0)
                {
                    // hero's fbx setting
                    importer.importAnimation = false;
                }
                anim.cullingType = AnimationCullingType.AlwaysAnimate;
            }
            //importer.importMaterials = false;
        }
	}
}
