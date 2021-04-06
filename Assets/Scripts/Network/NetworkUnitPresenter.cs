using System;
using System.Collections.Generic;
using Geomancer;
using Geomancer.Model;
using UnityEngine;

namespace Domino {
  public class NetworkUnitPresenter {
    private readonly ILoader loader;
    private readonly IClock clock;
    private readonly ITimer timer;
    // private readonly TileShapeMeshCache tileShapeMeshCache;
    private readonly float elevationStepHeight;
    private readonly Pattern pattern;
    // private readonly int shapeIndex;

    private readonly UnitView unitView;
    private Vector3 lookatOffsetToCamera;
    
    public NetworkUnitPresenter(
        ILoader loader,
        IClock clock,
        ITimer timer,
        // TileShapeMeshCache tileShapeMeshCache,
        float elevationStepHeight,
        Pattern pattern,
        ulong newTileViewId,
        Vector3 lookatOffsetToCamera,
        InitialUnit initialUnit) {
      this.loader = loader;
      this.clock = clock;
      this.timer = timer;
      // this.tileShapeMeshCache = tileShapeMeshCache;
      this.elevationStepHeight = elevationStepHeight;
      this.pattern = pattern;
      this.lookatOffsetToCamera = lookatOffsetToCamera;
      var shapeIndex = pattern.patternTiles[initialUnit.location.indexInGroup].shapeIndex;
      
      // var (groundMesh, outlinesMesh) = tileShapeMeshCache.Get(shapeIndex, elevationStepHeight, .025f);
      var location = initialUnit.location;
      
      var position = CalculatePosition(elevationStepHeight, pattern, location, initialUnit.elevation);
      var unitDescription = TranslateInitialUnit(initialUnit);
      unitView = UnitView.Create(loader, clock, timer, position, unitDescription, lookatOffsetToCamera);
      
              // newTileViewId, initialTile.location, loader, groundMesh, outlinesMesh, clock, timer, unitDescription);
      // tileView.gameObject.AddComponent<TerrainTilePresenterTile>().Init(this);
      unitView.gameObject.transform.localPosition = position;
    }

    public void Destroy() {
      unitView.Destruct();
    }

    private static UnitDescription TranslateInitialUnit(InitialUnit initialUnit) {
      // var (groundMesh, outlinesMesh) = tileShapeMeshCache.Get(shapeIndex, elevationStepHeight, .025f);
      // var location = initialUnit.location;
      // var patternUnit = pattern.patternUnits[location.indexInGroup];
      // float rotateDegrees = patternUnit.rotateRadianards / 1000f * 180f / (float) Math.PI;
      return new UnitDescription(
          new DominoDescription(
              initialUnit.shape == DominoShape.TALL_DOMINO,
              initialUnit.color),
          TranslateMaybeInitialSymbol(RenderPriority.DOMINO, initialUnit.faceSymbolDescription),
          TranslateInitialSymbolMap(RenderPriority.DOMINO, initialUnit.detailSymbolDescriptionById),
          initialUnit.hpRatio,
          initialUnit.mpRatio);
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
    
    public void HandleMessage(IDominoMessage message) {
      // if (message is SetSurfaceColorMessage setSurfaceColor) {
      //   unitView.SetSurfaceColor(setSurfaceColor.frontColor);
      // } else if (message is SetCliffColorMessage setCliffColor) {
      //   unitView.SetCliffColor(setCliffColor.sideColor);
      // } else {
        Asserts.Assert(false);
      // }
    }
    
    public void SetCameraDirection(Vector3 lookatOffsetToCamera) {
      this.lookatOffsetToCamera = lookatOffsetToCamera;
      unitView.SetCameraDirection(lookatOffsetToCamera);
    }
  }
}
