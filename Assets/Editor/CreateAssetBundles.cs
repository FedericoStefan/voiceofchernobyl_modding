using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System;

public class CreateAssetBundles : MonoBehaviour
{
    [MenuItem("Assets/Create Asset Bundle")]
    public static void BuildAssetBundle()
    {
        string Output = Application.dataPath + "/../AssetBundles_OUTPUT";
        try
        {
            var importer = AssetImporter.GetAtPath("Assets/AssetsBundlesWanted");
            importer.assetBundleName = "basic_mod_bundle";

            BuildPipeline.BuildAssetBundles(Output, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }
}
