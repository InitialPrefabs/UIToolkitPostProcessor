using System;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace InitialPrefabs.UIToolkit.PostProcessor {

    /// <summary>
    /// Custom importer to read Scriban Templates as a <see cref="TextAsset"/>.
    /// </summary>
    [ScriptedImporter(1, "scriban-cs")]
    internal class ScribanAssetImporter : ScriptedImporter {

        public override void OnImportAsset(AssetImportContext ctx) {
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var txtAsset = new TextAsset(new ReadOnlySpan<byte>(bytes));
            ctx.AddObjectToAsset("ScribanTemplate", txtAsset);
            ctx.SetMainObject(txtAsset);
        }
    }
}
