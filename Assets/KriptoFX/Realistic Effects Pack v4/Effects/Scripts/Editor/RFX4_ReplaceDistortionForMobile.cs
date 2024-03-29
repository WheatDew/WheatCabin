
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

public class RFX4_ReplaceDistortionForMobile : IActiveBuildTargetChanged
{
    const string distortionShaderFileName = "UberDistortion";

    public int callbackOrder { get { return 0; } }
    public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
    {
        Debug.Log("Switched build target to " + newTarget);
        if (newTarget == BuildTarget.Android || newTarget == BuildTarget.iOS) ReplaceDistortionShader(true);
        else ReplaceDistortionShader(false);
    }


    void ReplaceDistortionShader(bool isMobile)
    {
        var allShaders = Directory.GetFiles(Application.dataPath, "*.shader", SearchOption.AllDirectories);

        string distortionShaderPath = null;
        string distortionShaderCode = null;
        foreach (var currentShader in allShaders)
        {
            if (currentShader.Contains(distortionShaderFileName))
            {
                distortionShaderPath = currentShader;
                distortionShaderCode = File.ReadAllText(distortionShaderPath);
                if (distortionShaderCode.Contains("KriptoFX")) break;
            }
        }

        if (distortionShaderPath == null) return;

        if (isMobile)
        {
            distortionShaderCode = RemoveGrabPass(distortionShaderCode);
            distortionShaderCode = ChangeTagOrder(distortionShaderCode, "-10");
        }
        else
        {
            distortionShaderCode = AddGrabPass(distortionShaderCode);
            distortionShaderCode = ChangeTagOrder(distortionShaderCode, "+1");
        }

        File.WriteAllText(distortionShaderPath, distortionShaderCode);
    }

    string RemoveGrabPass(string code)
    {
        var grabIdx = code.IndexOf("GrabPass", StringComparison.OrdinalIgnoreCase);
        if (grabIdx <= 0) return code;
        var lastTagIdx = code.IndexOf("}", grabIdx, StringComparison.OrdinalIgnoreCase);
        code = code.Insert(grabIdx, "/*");
        code = code.Insert(lastTagIdx + 3, "*/");
        return code;
    }

    string AddGrabPass(string code)
    {
        var grabIdx = code.IndexOf("GrabPass", StringComparison.OrdinalIgnoreCase);
        var commentIdx = code.IndexOf("/*", grabIdx - 3, StringComparison.OrdinalIgnoreCase);
        if (grabIdx <= 0 || commentIdx == -1 || commentIdx > grabIdx + 10) return code;
        var lastTagIdx = code.IndexOf("}", grabIdx, StringComparison.OrdinalIgnoreCase);

        code = code.Remove(grabIdx - 2, 2);
        code = code.Remove(lastTagIdx - 1, 2);
        return code;
    }

    string ChangeTagOrder(string code, string queueOrder)
    {
        var queueIdx = code.IndexOf("Queue", StringComparison.OrdinalIgnoreCase);
        if (queueIdx == -1) return code;

        var transparentIdx = code.IndexOf("Transparent", queueIdx, StringComparison.OrdinalIgnoreCase);
        var lastTagIdx = code.IndexOf("\"", transparentIdx, StringComparison.OrdinalIgnoreCase);
        code = code.Remove(transparentIdx, lastTagIdx - transparentIdx);
        return code.Insert(transparentIdx, "Transparent" + queueOrder);
    }
}
