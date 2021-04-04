using System;
using System.Collections.Generic;
using Geomancer;
using Geomancer.Model;
using UnityEngine;

namespace Domino {
  public class NetworkTilePresenter {
    private readonly ILoader loader;
    private readonly IClock clock;
    private readonly ITimer timer;
    private readonly TileShapeMeshCache tileShapeMeshCache;
    private readonly float unityElevationStepHeight;
    private readonly int shapeIndex;

    private readonly TileView tileView;

    private static TileDescription TranslateInitialTile(
        float elevationStepHeight, Location location, Pattern pattern, InitialTile initialTile) {
      // var (groundMesh, outlinesMesh) = tileShapeMeshCache.Get(shapeIndex, unityElevationStepHeight, .025f);
      // var location = initialTile.location;
      var patternTile = pattern.patternTiles[location.indexInGroup];
      float rotateDegrees = patternTile.rotateRadianards / 1000f * 180f / (float) Math.PI;
      return new TileDescription(
          elevationStepHeight,
          rotateDegrees,
          initialTile.elevation,
          initialTile.topColor,
          initialTile.sideColor,
          TranslateMaybeInitialSymbol(RenderPriority.OVERLAY, initialTile.maybeOverlaySymbolDescription),
          TranslateMaybeInitialSymbol(RenderPriority.FEATURE, initialTile.maybeFeatureSymbolDescription),
          TranslateInitialSymbolMap(RenderPriority.ITEM, initialTile.itemSymbolDescriptionByItemId));
    }

    private static ExtrudedSymbolDescription TranslateMaybeInitialSymbol(
        RenderPriority renderPriority,
        InitialSymbol initialSymbol) {
      if (initialSymbol == null) {
        return null;
      }
      return new ExtrudedSymbolDescription(
          renderPriority,
          new SymbolDescription(
              initialSymbol.symbolId,
              initialSymbol.frontColor,
              initialSymbol.rotationDegrees,
              initialSymbol.sizePercent / 100f,
              initialSymbol.outlined ? OutlineMode.WithOutline : OutlineMode.NoOutline,
              initialSymbol.outlineColor),
          initialSymbol.depth > 0,
          initialSymbol.sidesColor);
    }

    private static List<(ulong, ExtrudedSymbolDescription)> TranslateInitialSymbolMap(
        RenderPriority renderPriority, List<(ulong, InitialSymbol)> idToSymbol) {
      var result = new List<(ulong, ExtrudedSymbolDescription)>();
      foreach (var (id, initialSymbol) in idToSymbol) {
        result.Add((id, TranslateMaybeInitialSymbol(renderPriority, initialSymbol)));
      }
      return result;
    }
    
    private static Vector3 CalculatePosition(float elevationStepHeight, Pattern pattern, Location location, int elevation) {
      var positionVec2 = pattern.GetTileCenter(location);
      var positionVec3 = new Vec3(positionVec2.x, positionVec2.y, 0);
      var unityPos = positionVec3.ToUnity();
      unityPos.y += elevation * elevationStepHeight;
      return unityPos;
    }

    public NetworkTilePresenter(Pattern pattern, InitialTile initialTile) {
      var (groundMesh, outlinesMesh) = tileShapeMeshCache.Get(shapeIndex, unityElevationStepHeight, .025f);
      var location = initialTile.location;
      
      var position = CalculatePosition(unityElevationStepHeight, pattern, location, 1);
      // var patternTile = pattern.patternTiles[location.indexInGroup];
      // float rotateDegrees = patternTile.rotateRadianards / 1000f * 180f / (float) Math.PI;
      var tileDescription = TranslateInitialTile(unityElevationStepHeight, location, pattern, initialTile);
      tileView = TileView.Create(loader, groundMesh, outlinesMesh, clock, timer, tileDescription);
      // tileView.gameObject.AddComponent<TerrainTilePresenterTile>().Init(this);
      tileView.gameObject.transform.localPosition = position;
    }
  }
}
