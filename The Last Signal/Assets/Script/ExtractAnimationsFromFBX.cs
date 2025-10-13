using UnityEngine;
using UnityEditor;
using System.IO;

public class ExtractAnimationsFromFBX
{
    [MenuItem("Tools/Extract Animations from Selected FBX")]
    public static void ExtractAnimations()
    {
        if (Selection.activeObject == null)
        {
            Debug.LogWarning("No FBX selected.");
            return;
        }

        string fbxPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);

        string fbxName = Path.GetFileNameWithoutExtension(fbxPath);

        // Carpeta fija "Animations" en la misma ubicación del FBX
        string parentFolder = Path.GetDirectoryName(fbxPath);
        string folderPath = Path.Combine(parentFolder, "Animations");

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder(parentFolder, "Animations");
        }

        int count = 0;
        foreach (var asset in assets)
        {
            if (asset is AnimationClip clip)
            {
                if (clip.name.Contains("__preview__")) continue;

                AnimationClip newClip = new AnimationClip();
                EditorUtility.CopySerialized(clip, newClip);

                // Evitar colisión de nombres → FBXName_AnimName.anim
                string clipFileName = $"{fbxName}_{clip.name}.anim";
                string clipPath = Path.Combine(folderPath, clipFileName).Replace("\\", "/");

                AssetDatabase.CreateAsset(newClip, clipPath);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ Extracted {count} animation clips to {folderPath}");
    }
}