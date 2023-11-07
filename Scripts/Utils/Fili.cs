using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Fili
{
    static bool initDone;

    static public string ReadAllText(string path)
    {
#if UNITY_ANDROID
        if(!initDone) { BetterStreamingAssets.Initialize(); initDone = true; }
        return BetterStreamingAssets.ReadAllText($"{path}");
#elif UNITY_EDITOR
        Debug.Log($"tree path: ./BTTrees/{path}");
        return File.ReadAllText($"./BTTrees/{path}");
#else
        Debug.Log($"tree path: ./TheGoodDrive_Data/StreamingAssets/BTTrees/{path}");
        return File.ReadAllText($"./TheGoodDrive_Data/StreamingAssets/BTTrees/{path}");
#endif
    }

    static public string ReadAllTextDev(string path)
    {
        return File.ReadAllText($"./Assets/_Developpers/Agents/{path}");
    }
    static public bool FileExists(string path)
    {
#if UNITY_ANDROID
        if(!initDone) { BetterStreamingAssets.Initialize(); initDone = true; }
        return BetterStreamingAssets.FileExists($"/{path}");

#elif UNITY_EDITOR             
        Debug.Log($"FILE EXISTS ${path}  {File.Exists($"./BTTrees/{path}")}");
        return File.Exists($"./BTTrees/{path}");
#else
        return File.Exists($"./TheGoodDrive_Data/StreamingAssets/BTTrees/{path}");
#endif
    }

    static public bool DevFileExists(string path)
    {
        return File.Exists($"./Assets/_Developpers/Agents/{path}");
    }


    static public string[] GetAllFiles(string path)
    {
#if UNITY_ANDROID
        if(!initDone) { BetterStreamingAssets.Initialize(); initDone = true; }
        Debug.Log($"GetAllFiles {path}");
        return BetterStreamingAssets.GetFiles($"/{path}");
#else
        var files = Directory.GetFiles($"./BTTrees/{path}");
        for (int i = 0; i < files.Length; i++)
            files[i] = files[i].Replace("\\", "/").Replace("//", "/").Replace("./BTTrees/", "");
        return files;
#endif
    }

    static public void WriteAllText(string path, string data)
    {
#if UNITY_ANDROID
        if(!initDone) { BetterStreamingAssets.Initialize(); initDone = true; }
        File.WriteAllText($"{Application.persistentDataPath}/{path}", data);
#else
        File.WriteAllText($"./BTTrees/{path}", data);
#endif
    }
}
