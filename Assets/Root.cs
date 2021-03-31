using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domino;
using Geomancer;
using Geomancer.Model;
using UnityEngine;
using Virtence.VText;

public class SimpleFuture<T> {
  public delegate void IOnComplete(T value);
  public event IOnComplete OnComplete;

  public void Resolve(T value) {
    OnComplete.Invoke(value);
  }
}

public interface ILoader {
  Material white { get; }
  Material black { get; }
  Material glowWhite { get; }
  SimpleFuture<Mesh> getMeshMaybeAsync(VTextParameters symbolId);
  GameObject NewEmptyGameObject();
  GameObject NewEmptyGameObject(Vector3 position, Quaternion rotation);
  GameObject NewQuad();
}

public struct VTextParameters {
  public readonly SymbolId symbolId;
  public readonly bool expanded;
  public readonly bool extruded;

  public VTextParameters(
      SymbolId symbolId,
      bool expanded,
      bool extruded) {
    this.symbolId = symbolId;
    this.expanded = expanded;
    this.extruded = extruded;
  }

  public override int GetHashCode() {
    return symbolId.GetHashCode() + (expanded ? 47 : 0) + (extruded ? 31 : 0);
  }
}

public class Root : MonoBehaviour, ILoader {
  private Camera camera;
  private Canvas canvas;

  private Dictionary<VTextParameters, SimpleFuture<Mesh>> vtextParametersToMesh;

  public SimpleFuture<Mesh> getMeshMaybeAsync(VTextParameters parameters) {
    if (vtextParametersToMesh.TryGetValue(parameters, out SimpleFuture<Mesh> value)) {
      return value;
    }
    
    var promise = new SimpleFuture<Mesh>();
    GameObject vtextGameObject = Instantiate(Resources.Load("VText")) as GameObject;
    if (vtextGameObject == null) {
      Debug.LogError("Couldn't instantiate VText!");
      return promise;
    }
    bool finished = false;
    VText vtext = vtextGameObject.GetComponent<VText>();
    vtext.MeshParameter.FontName = parameters.symbolId.fontName + (parameters.expanded ? "Expanded.ttf" : "Simplified.ttf");
    vtext.SetText(char.ConvertFromUtf32(parameters.symbolId.unicode));
    vtext.RenderParameter.Materials = new[] {black, black, black};
    vtext.MeshParameter.Depth = parameters.extruded ? 1 : 0;
    vtext.Rebuild();
    vtext.TextRenderingFinished += (s, a) => {
      if (finished) {
        return;
      }
      finished = true;
      
      var vtextMesh = vtext.GetComponentInChildren<MeshFilter>().sharedMesh;
      
      var mesh = new Mesh();
      mesh.SetVertices(vtextMesh.vertices);
      mesh.SetNormals(vtextMesh.normals);
      mesh.SetTriangles(vtextMesh.GetTriangles(0), 0);
      mesh.RecalculateNormals();
      mesh.RecalculateBounds();
      mesh.RecalculateTangents();
      
      Debug.Log("Finished loading symbol! tris: " + mesh.triangles.Length);
      promise.Resolve(mesh);
      // promise.OnComplete(mesh);
      // Asserts.Assert(didSet);
      // Destroy(vtextGameObject);
    };
    vtextParametersToMesh.Add(parameters, promise);
    
    return promise;
  }

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

    vtextParametersToMesh = new Dictionary<VTextParameters, SimpleFuture<Mesh>>();

    white = Instantiate(Resources.Load("White")) as Material;
    white.color = Color.white;
    white.enableInstancing = true;

    glowWhite = Instantiate(Resources.Load("White")) as Material;
    glowWhite.color = Color.white;
    glowWhite.enableInstancing = true;
    glowWhite.EnableKeyword("_EMISSION");
    glowWhite.SetColor("_EmissionColor", new Vector4(1,1,1,1));

    black = Instantiate(Resources.Load("White")) as Material;
    black.color = Color.black;
    white.enableInstancing = true;
    
    doThings();
  }

  void doThings() {
    
    var clock = new SlowableTimerClock(1.0f);

    var tileObjects = new List<GameObject>();
    var outlineObjects = new List<GameObject>();

    var rand = new Rand(1337);

    var pattern = PentagonPattern9.makePentagon9Pattern();
    var terrain = new Geomancer.Model.Terrain(pattern, 300, new SortedDictionary<Location, TerrainTile>());
    for (int groupX = 0; groupX < 5; groupX++) {
      for (int groupY = 0; groupY < 5; groupY++) {
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
              clock,
              clock,
              new TileDescription(
                  unityElevationStepHeight,
                  patternTile.rotateRadianards / 1000f * 180f / (float)Math.PI,
                  depth,
                  frontColor,
                  sideColor,
                  null,
                  new ExtrudedSymbolDescription(
                      RenderPriority.SYMBOL,
                      new SymbolDescription(
                          new SymbolId("AthSymbols", 0x002B),
                          Vector4Animation.Color(.8f, 0, .8f, 1.5f),
                          0,
                          1,
                          OutlineMode.NoOutline),
                      true,
                      Vector4Animation.BLACK),
                  new List<(ulong, ExtrudedSymbolDescription)>()));
      tileView.gameObject.transform.localPosition = position;
      tileObjects.AddRange(tileView.groundGameObjects);
      outlineObjects.AddRange(tileView.outlineGameObjects);
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
