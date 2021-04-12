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
          new DivideVector4Animation(Translation.Translate(initialTile.topColor), ConstantVector4Animation.All(255)),
          new DivideVector4Animation(Translation.Translate(initialTile.sideColor), ConstantVector4Animation.All(255)),
          Translation.TranslateMaybeInitialSymbol(RenderPriority.OVERLAY, initialTile.maybeOverlaySymbol),
          Translation.TranslateMaybeInitialSymbol(RenderPriority.FEATURE, initialTile.maybeFeatureSymbol),
          TranslateInitialSymbolMap(RenderPriority.ITEM, initialTile.itemIdToSymbol));
    }

    private static List<(ulong, ExtrudedSymbolDescription)> TranslateInitialSymbolMap(
        RenderPriority renderPriority, List<(ulong, InitialSymbol)> idToSymbol) {
      var result = new List<(ulong, ExtrudedSymbolDescription)>();
      foreach (var (id, initialSymbol) in idToSymbol) {
        result.Add((id, Translation.TranslateMaybeInitialSymbol(renderPriority, initialSymbol)));
      }
      return result;
    }

    public void SetElevation(int elevation) {
      tileView.SetElevation(elevation);
    }

    public void HandleMessage(IDominoMessage message) {
      if (message is SetSurfaceColorMessage setSurfaceColor) {
        tileView.SetSurfaceColor(
            new DivideVector4Animation(
                Translation.Translate(setSurfaceColor.frontColor),
                ConstantVector4Animation.All(255)));
      } else if (message is SetCliffColorMessage setCliffColor) {
        tileView.SetCliffColor(
            new DivideVector4Animation(
                Translation.Translate(setCliffColor.sideColor),
                ConstantVector4Animation.All(255)));
      } else if (message is SetElevationMessage setElevation) {
        Asserts.Assert(false);
      } else if (message is SetOverlayMessage setOverlay) {
        tileView.SetOverlay(Translation.TranslateMaybeInitialSymbol(RenderPriority.OVERLAY, setOverlay.symbol));
      } else if (message is SetFeatureMessage setFeature) {
        tileView.SetFeature(Translation.TranslateMaybeInitialSymbol(RenderPriority.OVERLAY, setFeature.symbol));
      } else if (message is AddItemMessage addItem) {
        tileView.AddItem(addItem.itemId, Translation.TranslateMaybeInitialSymbol(RenderPriority.ITEM, addItem.symbolDescription));
      } else if (message is RemoveItemMessage removeItem) {
        tileView.RemoveItem(removeItem.itemId);
      } else if (message is ClearItemsMessage clearItems) {
        tileView.ClearItems();
      } else {
        Asserts.Assert(false);
      }
    }
  }
}
