// 단순 유니티 패키지 제작용 스크립트


using UnityEngine;
using System.Collections;
using UnityEditor;
 
public static class ExportPackage {
 

    [MenuItem("Export/태그 및 레이어와 함께 내보내기")]
    public static void export()
    {
        string[] projectContent = new string[] {"Assets/Script", "Assets/Etc/HDRI_Sky" ,"Assets/Etc/HDRPDefaultResources" ,"Assets/Scenes/Field.unity", "ProjectSettings/TagManager.asset", "ProjectSettings/InputManager.asset", "ProjectSettings/ProjectSettings.asset"};
        AssetDatabase.ExportPackage(projectContent, ".배포용/ExportMyProject.unitypackage",ExportPackageOptions.Interactive | ExportPackageOptions.Recurse |ExportPackageOptions.IncludeDependencies);
        Debug.Log("Project Exported");
    }
 
}