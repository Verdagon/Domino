using System;
using System.Collections.Generic;
using Geomancer;
using Geomancer.Model;
using UnityEngine;

namespace Domino {
  public class NetworkUnitPresenter {
    public delegate int IGetElevation(Location location);
    
    private readonly ILoader loader;
    private readonly IClock clock;
    private readonly ITimer timer;
    // private readonly TileShapeMeshCache tileShapeMeshCache;
    private readonly float elevationStepHeight;
    private readonly Pattern pattern;
    // private readonly int shapeIndex;

    public Location location => unitView.location;

    private readonly UnitView unitView;
    // private Vector3 lookatOffsetToCamera;
    // private int elevation;

    private IGetElevation getElevation;
    
    public NetworkUnitPresenter(
        ILoader loader,
        IClock clock,
        ITimer timer,
        // TileShapeMeshCache tileShapeMeshCache,
        float elevationStepHeight,
        Pattern pattern,
        ulong newUnitViewId,
        Vector3 lookatOffsetToCamera,
        InitialUnit initialUnit,
        IGetElevation getElevation) {
      this.loader = loader;
      this.clock = clock;
      this.timer = timer;
      // this.tileShapeMeshCache = tileShapeMeshCache;
      this.elevationStepHeight = elevationStepHeight;
      this.pattern = pattern;
      this.getElevation = getElevation;
      // this.lookatOffsetToCamera = lookatOffsetToCamera;
      // this.elevation = elevation;
      // var shapeIndex = pattern.patternTiles[initialUnit.location.indexInGroup].shapeIndex;
      
      // var (groundMesh, outlinesMesh) = tileShapeMeshCache.Get(shapeIndex, elevationStepHeight, .025f);
      // var location = initialUnit.location;

      int elevation = getElevation(initialUnit.location);
      var unitDescription = TranslateInitialUnit(initialUnit);
      unitView =
          UnitView.Create(
              loader, clock, timer, pattern, elevationStepHeight, initialUnit.location, elevation, unitDescription, lookatOffsetToCamera);
      
              // newTileViewId, initialTile.location, loader, groundMesh, outlinesMesh, clock, timer, unitDescription);
      // tileView.gameObject.AddComponent<TerrainTilePresenterTile>().Init(this);
      // unitView.gameObject.transform.localPosition = position;
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
          Translation.TranslateMaybeInitialSymbol(RenderPriority.DOMINO, initialUnit.dominoSymbol),
          Translation.TranslateMaybeInitialSymbol(RenderPriority.DOMINO, initialUnit.faceSymbol),
          TranslateInitialSymbolMap(RenderPriority.DOMINO, initialUnit.idToDetailSymbol),
          initialUnit.hpRatio,
          initialUnit.mpRatio);
    }

    private static List<(ulong, ExtrudedSymbolDescription)> TranslateInitialSymbolMap(
        RenderPriority renderPriority, List<(ulong, InitialSymbol)> idToSymbol) {
      var result = new List<(ulong, ExtrudedSymbolDescription)>();
      foreach (var (id, initialSymbol) in idToSymbol) {
        result.Add((id, Translation.TranslateMaybeInitialSymbol(renderPriority, initialSymbol)));
      }
      return result;
    }
    
    public void RefreshElevation() {
      unitView.TeleportTo(unitView.location, getElevation(unitView.location));
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
      // this.lookatOffsetToCamera = lookatOffsetToCamera;
      unitView.SetCameraDirection(lookatOffsetToCamera);
    }
  }
}
