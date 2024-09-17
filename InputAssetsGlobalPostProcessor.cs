using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Core.Hybrid.Hybrid.CodeGeneration;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DotsInput
{
    public class InputAssetsGlobalPostProcessor : AssetPostprocessor
    {
        private static readonly Regex re = new Regex(@"[^\w\d_]");

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for (int i = 0; i < importedAssets.Length; i++)
            {
                var assetName = importedAssets[i];
                if (Path.GetExtension(assetName) != ".inputactions") continue;
                if(assetName.Contains("ignore")) continue;
                Debug.Log(assetName);
                UpdateEnums(assetName);
            }
        }

        private static string FixName(string name)
        {
            return re.Replace(name, "");
        }

        private static void UpdateEnums(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
            var primitiveKeys = new List<(int, string)>();
            var axisKeys = new List<(int, string)>();
            using var ie = asset.GetEnumerator();
            int pi = 0;
            int ai = 0;
            while (ie.MoveNext())
            {
                var c = ie.Current;
                if (DotsInputMapper.TryGetPrimitiveDotsInputType(c, out _))
                {
                    primitiveKeys.Add((pi, $"{c.actionMap.name} {c.name}"));
                    pi++;
                }

                if (DotsInputMapper.TryGetAxisDotsInputType(c, out _))
                {
                    axisKeys.Add((ai, $"{c.actionMap.name} {c.name}"));
                    ai++;
                }
            }

            var writer = new CodeWriter();
            writer.WriteEnum($"{FixName(asset.name)}Primitive", primitiveKeys);
            writer.WriteEnum($"{FixName(asset.name)}Axis", axisKeys);
            CodeWriter.WriteToFile(writer, $"{asset.name}_generated_enums");
            EditorApplication.delayCall += AssetDatabase.Refresh;
        }
    }
}