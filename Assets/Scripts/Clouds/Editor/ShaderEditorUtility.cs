using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ShaderEditorUtility {

    public static void SetShaderConst (Object shader, string constName, bool enabled) {
        // Read shader contents
        var path = AssetDatabase.GetAssetPath (shader);
        var reader = new StreamReader (path);
        string shaderContents = reader.ReadToEnd ();
        reader.Close ();

        // Set value
        string currentValue = constName + ((enabled) ? " 0" : " 1");
        if (shaderContents.Contains (currentValue)) {
            string desiredValue = constName + ((enabled) ? " 1" : " 0");
            shaderContents = shaderContents.Replace (currentValue, desiredValue);

            // Write to shader
            var writer = new StreamWriter (path);
            writer.Write (shaderContents);
            writer.Close ();

            // Reimport asset
            AssetDatabase.ImportAsset (path);
        }
    }
}