using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ExportTerrain_Bak : EditorWindow
{
    public enum SaveFormat_bak
    {
        Triangles,
        Quads,
    }

    public enum SaveResolution_bak
    {
        Full,
        Half,
        Quarter,
        Eighth,
        Sixteenth,
    }

    SaveFormat_bak saveFormat = SaveFormat_bak.Triangles;
    SaveResolution_bak saveResolution = SaveResolution_bak.Half;
    static TerrainData terrain;
    static Vector3 terrainPos;

    int tCount;
    int counter;
    int totalCount;

    //[MenuItem("Terrain/Export To Obj...")]
    static public void Init()
    {
        terrain = null;
        Terrain terrainObject = Selection.activeObject as Terrain;
        if (!terrainObject)
        {
            terrainObject = Terrain.activeTerrain;
        }
        if (terrainObject)
        {
            terrain = terrainObject.terrainData;
            terrainPos = terrainObject.transform.position;
        }
        EditorWindow.GetWindow(typeof(ExportTerrain)).Show();
    }

    public void OnGUI()
    {
        if (!terrain)
        {
            GUILayout.Label("No terrain found");
            if (GUILayout.Button("Cancel"))
            {
                EditorWindow.GetWindow(typeof(ExportTerrain)).Close();
            }
            return;
        }
        saveFormat = (SaveFormat_bak)EditorGUILayout.EnumPopup("Export Format", (Enum)saveFormat);
        saveResolution = (SaveResolution_bak)EditorGUILayout.EnumPopup("Resolution", (Enum)saveResolution);

        if (GUILayout.Button("Export"))
        {
            Export();
        }
    }

    public void Export()
    {
        string fileName = EditorUtility.SaveFilePanel("Export .obj file", "", "Terrain", "obj");
        int w = terrain.heightmapWidth;
        int h = terrain.heightmapHeight;
        Vector3 meshScale = terrain.size;
        var tRes = Mathf.Pow(2, (int)(saveResolution));
        meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
        Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
        float[,] tData = terrain.GetHeights(0, 0, w, h);

        w = (int)((w - 1) / tRes + 1);
        h = (int)((h - 1) / tRes + 1);
        var tVertices = new Vector3[w * h];
        var tUV = new Vector2[w * h];
        int[] tPolys = null;
        if (saveFormat == SaveFormat_bak.Triangles)
        {
            tPolys = new int[(w - 1) * (h - 1) * 6];
        }
        else
        {
            tPolys = new int[(w - 1) * (h - 1) * 4];
        }

        // Build vertices and UVs
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(x, (int)(tData[(int)(x * tRes), (int)(y * tRes)]), y)) + terrainPos;
                tUV[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
            }
        }

        var index = 0;
        if (saveFormat == SaveFormat_bak.Triangles)
        {
            // Build triangle indices: 3 indices into vertex array for each triangle
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output two triangles
                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = (y * w) + x + 1;

                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    tPolys[index++] = (y * w) + x + 1;
                }
            }
        }
        else
        {
            // Build quad indices: 4 indices into vertex array for each quad
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output one quad
                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    tPolys[index++] = (y * w) + x + 1;
                }
            }
        }

        // Export to .obj
        StreamWriter sw = null;
        try
        {
            sw = new StreamWriter(fileName);
            sw.WriteLine("# Unity terrain OBJ File");

            // Write vertices
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            counter = tCount = 0;
            totalCount = (tVertices.Length * 2 + (saveFormat == SaveFormat_bak.Triangles ? tPolys.Length / 3 : tPolys.Length / 4)) / 1000;
            StringBuilder sb = null;
            for (int i = 0; i < tVertices.Length; i++)
            {
                UpdateProgress();
                sb = new StringBuilder("v ", 20);
                // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
                // Which is important when you're exporting huge terrains.
                sb.Append(tVertices[i].x.ToString()).Append(" ").
                   Append(tVertices[i].y.ToString()).Append(" ").
                   Append(tVertices[i].z.ToString());
                sw.WriteLine(sb);
            }
            // Write UVs
            for (int i = 0; i < tUV.Length; i++)
            {
                UpdateProgress();
                sb = new StringBuilder("vt ", 22);
                sb.Append(tUV[i].x.ToString()).Append(" ").
                   Append(tUV[i].y.ToString());
                sw.WriteLine(sb);
            }
            if (saveFormat == SaveFormat_bak.Triangles)
            {
                // Write triangles
                for (int i = 0; i < tPolys.Length; i += 3)
                {
                    UpdateProgress();
                    sb = new StringBuilder("f ", 43);
                    sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                       Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                       Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);
                    sw.WriteLine(sb);
                }
            }
            else
            {
                // Write quads
                for (int i = 0; i < tPolys.Length; i += 4)
                {
                    UpdateProgress();
                    sb = new StringBuilder("f ", 57);
                    sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                       Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                       Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1).Append(" ").
                       Append(tPolys[i + 3] + 1).Append("/").Append(tPolys[i + 3] + 1);
                    sw.WriteLine(sb);
                }
            }
        }
        catch (Exception err)
        {
            Debug.Log("Error saving file: " + err.Message);
        }
        sw.Close();

        terrain = null;
        EditorUtility.ClearProgressBar();
        EditorWindow.GetWindow(typeof(ExportTerrain)).Close();
    }

    public void UpdateProgress()
    {
        if (counter++ == 1000)
        {
            counter = 0;
            EditorUtility.DisplayProgressBar("Saving...", "", Mathf.InverseLerp(0, totalCount, ++tCount));
        }
    }
}