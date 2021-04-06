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
    private readonly float elevationStepHeight;
    private readonly Pattern pattern;
    // private readonly int shapeIndex;

    private readonly TileView tileView;

    public Location location => tileView.location;
    public int elevation => tileView.elevation;

    public NetworkTilePresenter(
        ILoader loader,
        IClock clock,
        ITimer timer,
        TileShapeMeshCache tileShapeMeshCache,
        float elevationStepHeight,
        Pattern pattern,
        ulong newTileViewId,
        InitialTile initialTile) {
      this.loader = loader;
      this.clock = clock;
      this.timer = timer;
      this.tileShapeMeshCache = tileShapeMeshCache;
      this.elevationStepHeight = elevationStepHeight;
      this.pattern = pattern;
      var shapeIndex = pattern.patternTiles[initialTile.location.indexInGroup].shapeIndex;
      
      var (groundMesh, outlinesMesh) = tileShapeMeshCache.Get(shapeIndex, elevationStepHeight, .025f);
      var location = initialTile.location;
      
      // var position = CalculatePosition(elevationStepHeight, pattern, location, initialTile.elevation);
      // var patternTile = pattern.patternTiles[location.indexInGroup];
      // float rotateDegrees = patternTile.rotateRadianards / 1000f * 180f / (float) Math.PI;
      var tileDescription = TranslateInitialTile(elevationStepHeight, location, pattern, initialTile);
      tileView =
          TileView.Create(
              newTileViewId,
              initialTile.location,
              pattern,
              loader,
              groundMesh,
              outlinesMesh,
              clock,
              timer,
              initialTile.elevation,
              tileDescription);
      // tileView.gameObject.AddComponent<TerrainTilePresenterTile>().Init(this);
      // tileView.gameObject.transform.localPosition = position;
    }

    public void Destroy() {
      tileView.DestroyTile();
    }

    private static TileDescription TranslateInitialTile(
        float elevationStepHeight, Location location, Pattern pattern, InitialTile initialTile) {
      // var (groundMesh, outlinesMesh) = tileShapeMeshCache.Get(shapeIndex, elevationStepHeight, .025f);
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

    public void SetElevation(int elevation) {
      tileView.SetElevation(elevation);
    }

    public void HandleMessage(IDominoMessage message) {
      if (message is SetSurfaceColorMessage setSurfaceColor) {
        tileView.SetSurfaceColor(setSurfaceColor.frontColor);
      } else if (message is SetCliffColorMessage setCliffColor) {
        tileView.SetCliffColor(setCliffColor.sideColor);
      } else if (message is SetElevationMessage setElevation) {
        Asserts.Assert(false);
      } else {
        Asserts.Assert(false);
      }
    }
  }
}
