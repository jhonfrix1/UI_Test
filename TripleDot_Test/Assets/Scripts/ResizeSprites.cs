using UnityEngine;
using UnityEditor;
using System.IO;

public class ResizePOTOverwrite : Editor
{
    [MenuItem("Tools/Resize PNGs to POT (Overwrite + Undo Safe)")]
    static void ResizePNGsToPOT()
    {
        Object[] textures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);

        foreach (Object obj in textures)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (!path.ToLower().EndsWith(".png")) continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            Texture2D original = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Texture2D src = Object.Instantiate(original);

            int origW = src.width;
            int origH = src.height;

            int newW = NextPowerOfTwo(origW);
            int newH = NextPowerOfTwo(origH);

            if (origW == newW && origH == newH)
                continue;

            float scale = Mathf.Min((float)newW / origW, (float)newH / origH);
            int scaledW = Mathf.RoundToInt(origW * scale);
            int scaledH = Mathf.RoundToInt(origH * scale);
            Texture2D scaled = ScaleTexture(src, scaledW, scaledH);

            Texture2D resized = new Texture2D(newW, newH, TextureFormat.RGBA32, false);
            Color32[] clear = new Color32[newW * newH];
            for (int i = 0; i < clear.Length; i++) clear[i] = new Color32(0, 0, 0, 0);
            resized.SetPixels32(clear);

            int offsetX = (newW - scaledW) / 2;
            int offsetY = (newH - scaledH) / 2;

            for (int y = 0; y < scaledH; y++)
            {
                for (int x = 0; x < scaledW; x++)
                {
                    resized.SetPixel(x + offsetX, y + offsetY, scaled.GetPixel(x, y));
                }
            }

            resized.Apply();

            byte[] pngData = resized.EncodeToPNG();
            File.WriteAllBytes(path, pngData);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            Texture2D updatedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Undo.RegisterCompleteObjectUndo(updatedTex, "Resize PNG to POT");
        }

        AssetDatabase.Refresh();
    }

    static int NextPowerOfTwo(int value)
    {
        int power = 1;
        while (power < value)
            power *= 2;
        return power;
    }

    static Texture2D ScaleTexture(Texture2D src, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        RenderTexture.active = rt;
        Graphics.Blit(src, rt);
        Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }
}
