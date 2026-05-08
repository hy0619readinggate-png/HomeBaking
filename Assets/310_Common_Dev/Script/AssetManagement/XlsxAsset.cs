using UnityEngine;

namespace DoDoEng
{
    public class XlsxAsset : ScriptableObject
    {
        // Properties
        public byte[] Bytes => bytes;

        // Methods
        public static XlsxAsset Create(byte[] bytes)
        {
            var asset = CreateInstance<XlsxAsset>();
            asset.bytes = bytes;
            return asset;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private byte[] bytes;
    }
}