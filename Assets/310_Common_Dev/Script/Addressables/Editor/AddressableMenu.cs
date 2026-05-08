using UnityEditor;
using UnityEngine;

public class AddressableMenu
{
    [MenuItem("DoDoEng/Addressable/ClearCache")]
    public static void CleanCache()
    {
        if (Caching.ClearCache())
        {
            Debug.LogWarning("Successfully cleaned all caches.");
        }
        else
        {
            Debug.LogWarning("Cache was in use.");
        }
    }
}
