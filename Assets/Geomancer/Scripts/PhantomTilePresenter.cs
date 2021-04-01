using System;
using System.Collections.Generic;
using AthPlayer;
using Geomancer.Model;
using UnityEngine;
using Domino;

namespace Geomancer {
  public class PhantomTilePresenterTile : MonoBehaviour {
    // PhantomTilePresenter attaches this to the TileView it creates, so that when EditorPresenter
    // raycasts, it can know the PhantomTilePresenter that owns this TileView.
    // This approach is an implementation detail of the Editor, and shouldnt enter Domino.
    public PhantomTilePresenter presenter;

    public void Init(PhantomTilePresenter presenter) {
      this.presenter = presenter;
    }
  }

  public class PhantomTilePresenter {
    //public delegate void OnMouseInEvent();
    //public delegate void OnMouseOutEvent();
    public delegate void OnPhantomTileClickedEvent();

    IClock clock;
    ITimer timer;
  Pattern pattern;
    public readonly Location location;
    ILoader loader;
    private TileShapeMeshCache tileShapeMeshCache;

    private int elevationStepHeight;
    
    Vector3 tileCenter;
    TileView tileView;
    private bool highlighted;

    public PhantomTilePresenter(
        IClock clock,
      ITimer timer,
        Pattern pattern,
        Location location,
        ILoader loader,
        TileShapeMeshCache tileShapeMeshCache,
        int elevationStepHeight) {
      this.clock = clock;
      this.timer = timer;
      this.pattern = pattern;
      this.location = location;
      this.loader = loader;
      this.tileShapeMeshCache = tileShapeMeshCache;
      this.elevationStepHeight = elevationStepHeight;

      var positionVec2 = pattern.GetTileCenter(location);

      tileCenter = new Vec3(positionVec2.x, positionVec2.y, 0).ToUnity();

      ResetViews();
    }

    private (Vector4Animation, Vector4Animation) GetColors(bool highlighted) {
      var frontColor = highlighted ? Vector4Animation.Color(.2f, .2f, .2f) : Vector4Animation.Color(0f, 0, 0f);
      var sideColor = highlighted ? Vector4Animation.Color(.2f, .2f, .2f) : Vector4Animation.Color(0f, 0, 0f);
      return (frontColor, sideColor);
    }
    
    public void SetHighlighted(bool highlighted) {
      var (frontColor, sideColor) = GetColors(highlighted);
      tileView.SetFrontColor(frontColor);
      tileView.SetSidesColor(sideColor);
      // tileView.SetDescription(GetTileDescription(pattern, location, highlighted));
    }

    private void ResetViews() {
      if (tileView != null) {
        tileView.DestroyTile();
        tileView = null;
      }

      var position = CalculatePosition(elevationStepHeight, pattern, location, 1);

      var patternTileIndex = location.indexInGroup;
      var shapeIndex = pattern.patternTiles[patternTileIndex].shapeIndex;
      //   var radianards = pattern.patternTiles[patternTileIndex].rotateRadianards;
      //   var radians = radianards * 0.001f;
      //   var degrees = (float)(radians * 180f / Math.PI);
      //   var rotation = Quaternion.AngleAxis(-degrees, Vector3.up);
      var unityElevationStepHeight = elevationStepHeight * ModelExtensions.ModelToUnityMultiplier;
      var (groundMesh, outlinesMesh) = tileShapeMeshCache.Get(shapeIndex, unityElevationStepHeight, .025f);


      var tileDescription = GetTileDescription(pattern, location, elevationStepHeight, highlighted);
      tileView = TileView.Create(loader, groundMesh, outlinesMesh, clock, timer, tileDescription);
      tileView.gameObject.AddComponent<PhantomTilePresenterTile>().Init(this);
      
      tileView.gameObject.transform.localPosition = position;
    }
    
    private static Vector3 CalculatePosition(int elevationStepHeight, Pattern pattern, Location location, int elevation) {
      var positionVec2 = pattern.GetTileCenter(location);
      var positionVec3 = new Vec3(positionVec2.x, positionVec2.y, elevation * elevationStepHeight);
      return positionVec3.ToUnity();
    }
    
    //
    // private static SymbolId GetTerrainTileShapeSymbol(Pattern pattern, PatternTile patternTile) {
    //   switch (pattern.name) {
    //     case "square":
    //       if (patternTile.shapeIndex == 0) {
    //         return new SymbolId("AthSymbols", 0x006A);
    //       }
    //       break;
    //     case "pentagon9":
    //       if (patternTile.shapeIndex == 0) {
    //         return new SymbolId("AthSymbols", 0x0069);
    //       } else if (patternTile.shapeIndex == 1) {
    //         return new SymbolId("AthSymbols", 0x0068);
    //       }
    //       break;
    //     case "hex":
    //       if (patternTile.shapeIndex == 0) {
    //         return new SymbolId("AthSymbols", 0x0035);
    //       }
    //       break;
    //   }
    //   return new SymbolId("AthSymbols", 0x0065);
    // }
    //
    private static TileDescription GetTileDescription(
        Pattern pattern, Location location, float elevationStepHeight, bool highlighted) {
      var patternTile = pattern.patternTiles[location.indexInGroup];
    
      var frontColor = highlighted ? Vector4Animation.Color(.1f, .1f, .1f) : Vector4Animation.Color(0f, 0, 0f);
      var sideColor = highlighted ? Vector4Animation.Color(.1f, .1f, .1f) : Vector4Animation.Color(0f, 0, 0f);
    
      return
        new TileDescription(
              elevationStepHeight,
              patternTile.rotateRadianards / 1000f * 180f / (float)Math.PI,
              1,
              frontColor,
              sideColor,
              // new ExtrudedSymbolDescription(
              //   RenderPriority.TILE,
              //   new SymbolDescription(
              //       symbolName,
              //       frontColor,
              //       patternTile.rotateRadianards / 1000f * 180f / (float)Math.PI,
              //       1,
              //       OutlineMode.WithOutline,
              //       Vector4Animation.Color(.2f, .2f, .2f)),
              //   false,
              //   sideColor),
              null,
              null,
              new List<(ulong, ExtrudedSymbolDescription)>());
    }

    public void DestroyPhantomTilePresenter() {
      tileView.DestroyTile();
    }

    // public void OnStrMutListEffect(IStrMutListEffect effect) {
    //   effect.visitIStrMutListEffect(this);
    // }
    //
    // public void visitStrMutListCreateEffect(StrMutListCreateEffect effect) { }
    //
    // public void visitStrMutListDeleteEffect(StrMutListDeleteEffect effect) { }
    //
    // public void visitStrMutListAddEffect(StrMutListAddEffect effect) {
    //   ResetViews();
    // }
    //
    // public void visitStrMutListRemoveEffect(StrMutListRemoveEffect effect) {
    //   ResetViews();
    // }
  }
}
