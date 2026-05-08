using System.IO;
using UnityEditor.AssetImporters;

namespace DoDoEng
{
    [ScriptedImporter(1, "xlsx")]
    public class XlsxImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var textAsset = XlsxAsset.Create(bytes);

            ctx.AddObjectToAsset("text", textAsset);
            ctx.SetMainObject(textAsset);
        } 
    }
}