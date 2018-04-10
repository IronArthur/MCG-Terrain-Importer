using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TerrainLoader : MonoBehaviour
{

    Dictionary<int, Dictionary<string, object>> TileDict;

    static float HeightDiff = 64f;



    float[,] heightMap = new float[129, 129];

    Color[,][] alphaMap = new Color[512, 512][];

    string MapName;

    // Use this for initialization
    void Start()
    {
        this.TileDict = CSVReader.Read("Tiles");

        //m0405 AA_Elev m0101 MX0101

        this.MapName = Path.GetFileNameWithoutExtension("m0101.elv");

        using (BinaryReader reader = new BinaryReader(new FileStream("m0101.elv", FileMode.Open)))
        {
            reader.BaseStream.Position += 4;
            int firstOffset = reader.ReadInt32();
            reader.BaseStream.Position = firstOffset;

            for (int y = 0; y < 6; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    for (int yy = 0; yy < 20; yy++)
                    {
                        for (int xx = 0; xx < 20; xx++)
                        {
                            var height = (reader.ReadByte() - 1);

                            reader.BaseStream.Position += 3;
                            var tileID = reader.ReadInt16();
                            reader.BaseStream.Position += 2;

                            var entry = TileDict[tileID];

                            var realX = x * 20 + xx;
                            var realY = y * 20 + yy;

                            heightMap[realX, realY] = (height + (int)entry["vert0"]) / HeightDiff;

                            heightMap[realX, realY + 1] = (height + (int)entry["vert3"]) / HeightDiff;
                            heightMap[realX + 1, realY + 1] = (height + (int)entry["vert2"]) / HeightDiff;
                            heightMap[realX + 1, realY] = (height + (int)entry["vert1"]) / HeightDiff;


                            var color = new Color(0, 0, 0, 0);
                            var colorb = new Color(0, 0, 0, 0);

                            GetColorFromTerrainTileName((string)entry["text1"],ref color,ref colorb);
                            GetColorFromTerrainTileName((string)entry["text2"],ref color,ref colorb);
                            var colorArr= new Color[] { color, colorb }; ;

                            for (int i = 0; i < 4; i++)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    alphaMap[realY * 4 + i, realX * 4 + j] = colorArr;
                                }

                            }

                            //var pos = (y * 2400 + x * 20) + (yy * 120 + xx);
                        }
                    }
                }
            }
        }


        this.CreateTerrain(heightMap, alphaMap, null);

    }

    // Update is called once per frame
    void Update()
    {

    }




    private void CreateTerrain(float[,] heightMapData, Color[,][] alphaMap, GameObject parent)
    {
        TerrainData _TerrainData = Resources.Load<TerrainData>("DefaultTerrain");//new TerrainData();

        // _TerrainData.size = new Vector3(256, 512, 256);//
        _TerrainData.heightmapResolution = 129;
        // _TerrainData.baseMapResolution = 1024;
        // _TerrainData.SetDetailResolution(0, 8);

        _TerrainData.SetHeights(0, 0, heightMapData);

        /*
                SplatPrototype[] terrainTexture = new SplatPrototype[this.detailTextures.Count];

                for (int i = 0; i < this.detailTextures.Count; i++)
                {
                    terrainTexture[i] = new SplatPrototype();
                    terrainTexture[i].texture = MATexture.Load(this.detailTextures[i], "WRAP", "WRAP", true);
                }

                var ColorMapTexture = MATexture.Load(this.colorMap, "WRAP", "WRAP", true);

                if (!ColorMapTexture)
                    Debug.LogError("not");


                _TerrainData.splatPrototypes = terrainTexture; */

        Debug.Log(_TerrainData.alphamapWidth + " " + _TerrainData.alphamapHeight + " " + _TerrainData.alphamapLayers);

        var mapAlpha = new float[_TerrainData.alphamapWidth, _TerrainData.alphamapHeight, _TerrainData.alphamapLayers];

        for (int y = 0; y < _TerrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < _TerrainData.alphamapWidth; x++)
            {

                var color = alphaMap[x, y];

                if (color==null)
                {
                   // Debug.Log(x + " " + y);
                    continue;
                }

               // Debug.Log(color[0]);
                for (int z = 0; z < _TerrainData.alphamapLayers; z++)
                {
                    switch (z)
                    {
                        case 0:
                            mapAlpha[y, x, z] = color[0].r;
                            break;
                        case 1:
                            mapAlpha[y, x, z] = color[0].g;
                            break;
                        case 2:
                            mapAlpha[y, x, z] = color[0].b;
                            break;
                        case 3:
                            mapAlpha[y, x, z] = color[0].a;
                            break;
                        case 4:
                            mapAlpha[y, x, z] = color[1].r;
                            break;
                        case 5:
                            mapAlpha[y, x, z] = color[1].g;
                            break;
                        case 6:
                            mapAlpha[y, x, z] = color[1].b;
                            break;
                        case 7:
                            mapAlpha[y, x, z] = color[1].a;
                            break;
                    }

                }

            }

        }

        _TerrainData.SetAlphamaps(0, 0, mapAlpha);
        /* */
        _TerrainData.name = Path.GetFileNameWithoutExtension("Map-" + this.MapName);

        // UnityEditor.AssetDatabase.CreateAsset(_TerrainData, "Assets/" + _TerrainData.name + ".asset");


        GameObject terrainObject = Terrain.CreateTerrainGameObject(_TerrainData);
        terrainObject.name = "Map-" + this.MapName;

        if (parent)
        {
            terrainObject.transform.parent = parent.transform;
        }
    }



    private void GetColorFromTerrainTileName(string textName,ref Color color,ref Color colorb)
    {
        switch (textName)
        {
            case "grass":
                color.r += 0.5f;
                break;
            case "dirt":
                color.g += 0.5f;
                break;
            case "moun":
                color.b += 0.5f;
                break;
            case "cliff":
                color.a += 0.5f;
                break;
            case "ruff":
                colorb.r += 0.5f;
                break;
            case "water":
              //  colorb.g += 0.5f;
                break;
            case "conc":
                colorb.b += 0.5f;
                break;
        }

    }
}
