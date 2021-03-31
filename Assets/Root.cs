using System;
using System.Collections;
using System.Collections.Generic;
using Domino;
using Geomancer;
using Geomancer.Model;
using UnityEngine;
using Virtence.VText;

public interface ILoader {
  Material white { get; }
  Material black { get; }
  Material glowWhite { get; }
  GameObject NewEmptyGameObject();
  GameObject NewEmptyGameObject(Vector3 position, Quaternion rotation);
  GameObject NewQuad();
}

public class Root : MonoBehaviour, ILoader {
  private Camera camera;
  private Canvas canvas;

  public Material white { get; private set; }
  public Material black { get; private set; }
  public Material glowWhite { get; private set; }
  public GameObject NewEmptyGameObject() {
    return Instantiate(Resources.Load("EmptyGameObject")) as GameObject;  
  }
  public GameObject NewEmptyGameObject(Vector3 position, Quaternion rotation) {
    return Instantiate(Resources.Load("EmptyGameObject"), position, rotation) as GameObject;  
  }
  public GameObject NewQuad() {
    return Instantiate(Resources.Load("Quad")) as GameObject;  
  }
  
  void Start() {
    camera = GetComponentInChildren<Camera>();
    canvas = GetComponentInChildren<Canvas>();

    white = Instantiate(Resources.Load("White")) as Material;
    white.color = Color.white;
    white.enableInstancing = true;

    glowWhite = Instantiate(Resources.Load("White")) as Material;
    glowWhite.color = Color.white;
    glowWhite.enableInstancing = true;
    glowWhite.SetColor("_EmissionColor", new Vector4(1,1,1,0) * 1.5f);

    black = Instantiate(Resources.Load("White")) as Material;
    black.color = Color.black;
    white.enableInstancing = true;
    
    var clock = new SlowableTimerClock(1.0f);
    
    GameObject glyph = Instantiate(Resources.Load("VText")) as GameObject;
    VText vtext = glyph.GetComponent<VText>();
    vtext.SetText("b");
    vtext.MeshParameter.FontName = "Cascadia";
    vtext.Rebuild();


    var tileObjects = new List<GameObject>();
    var outlineObjects = new List<GameObject>();

    var rand = new Rand(1337);

    var pattern = PentagonPattern9.makePentagon9Pattern();
    var terrain = new Geomancer.Model.Terrain(pattern, 300, new SortedDictionary<Location, TerrainTile>());
    for (int groupX = -15; groupX < 20; groupX++) {
      for (int groupY = -15; groupY < 20; groupY++) {
        for (int tileIndex = 0; tileIndex < pattern.patternTiles.Count; tileIndex++) {
          int elevation = rand.Next() % 5 + 1;
          var loc = new Location(groupX, groupY, tileIndex);
          terrain.tiles.Add(loc, new TerrainTile(loc, elevation, new List<string>()));
        }
      }
    }

    var tileShapeMeshCache = new TileShapeMeshCache(pattern);

    var locs = new List<Location>(terrain.tiles.Keys);
    foreach (var location in locs) {
      var tile = terrain.tiles[location];
      int lowestNeighborElevation = tile.elevation;
      foreach (var neighborLoc in pattern.GetAdjacentLocations(tile.location, false)) {
        if (terrain.TileExists(neighborLoc)) {
          lowestNeighborElevation = Math.Min(lowestNeighborElevation, terrain.tiles[neighborLoc].elevation);
        } else {
          lowestNeighborElevation = 0;
        }
      }
      int depth = Math.Max(1, tile.elevation - lowestNeighborElevation);

      var patternTile = pattern.patternTiles[tile.location.indexInGroup];

      for (int i = 0; i < depth; i++) {

        var highlighted = false;
        var frontColor = highlighted ? Vector4Animation.Color(.1f, .1f, .1f) : Vector4Animation.Color(0f, 0, 0f);
        var sideColor = highlighted ? Vector4Animation.Color(.1f, .1f, .1f) : Vector4Animation.Color(0f, 0, 0f);

        var patternTileIndex = tile.location.indexInGroup;
        var shapeIndex = pattern.patternTiles[patternTileIndex].shapeIndex;
        var radianards = pattern.patternTiles[patternTileIndex].rotateRadianards;
        var radians = radianards * 0.001f;
        var degrees = (float)(radians * 180f / Math.PI);
        var rotation = Quaternion.AngleAxis(-degrees, Vector3.up);
        var unityElevationStepHeight = terrain.elevationStepHeight * ModelExtensions.ModelToUnityMultiplier;
        var (groundMesh, outlinesMesh) = tileShapeMeshCache.Get(shapeIndex, unityElevationStepHeight, .025f);

        var position = pattern.GetTileCenter(location).ToVec3().ToUnity();
        position.y += unityElevationStepHeight * tile.elevation;
        
        var tileView =
            TileView.Create(
                this,
                groundMesh,
                outlinesMesh,
                // tile.location.indexInGroup,
                // tileShapeMeshCache,
                // translateUPos,
                clock,
                clock,
                new TileDescription(
                    unityElevationStepHeight,
                    patternTile.rotateRadianards / 1000f * 180f / (float)Math.PI,
                    depth,
                    frontColor,
                    sideColor,
                    null,
                    null,
                    new List<(ulong, ExtrudedSymbolDescription)>()));
        tileView.gameObject.transform.localPosition = position;
        tileView.gameObject.transform.localRotation = rotation;
        tileObjects.AddRange(tileView.groundGameObjects);
        outlineObjects.AddRange(tileView.outlineGameObjects);
      }
    }

    // foreach (var groundGameObject in tileObjects) {
    //   groundGameObject.isStatic = true;
    // }
    // foreach (var outlineGameObject in outlineObjects) {
    //   outlineGameObject.isStatic = true;
    // }
    // StaticBatchingUtility.Combine(tileObjects.ToArray(), Instantiate(Resources.Load("EmptyGameObject")) as GameObject);
    // StaticBatchingUtility.Combine(outlineObjects.ToArray(), Instantiate(Resources.Load("EmptyGameObject")) as GameObject);
  }

  void Update() {
    
  }
}
