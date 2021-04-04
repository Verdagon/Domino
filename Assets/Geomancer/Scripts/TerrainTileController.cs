using System;
using System.Collections.Generic;
using AthPlayer;
using Geomancer.Model;
using UnityEngine;
using Domino;

namespace Geomancer {
  public class TerrainTilePresenter {
    private GameToDominoConnection domino;
    private MemberToViewMapper vivimap;
    public readonly Location location;
    private TerrainTile terrainTile;
    private Geomancer.Model.Terrain terrain;

    ulong tileViewId;
    ulong unitViewId;

    private ulong nextMemberId = 1;
    // (member ID, member string)
    private List<(ulong, string)> members = new List<(ulong, string)>();
    
    // (member ID, value)
    private List<(ulong, IVector4Animation)> membersFrontColors = new List<(ulong, IVector4Animation)>();
    private List<(ulong, IVector4Animation)> membersSideColors = new List<(ulong, IVector4Animation)>();
    private List<(ulong, InitialSymbol)> membersFeatures = new List<(ulong, InitialSymbol)>();
    private List<(ulong, InitialSymbol)> membersOverlays = new List<(ulong, InitialSymbol)>();
    private List<(ulong, InitialSymbol)> membersItems = new List<(ulong, InitialSymbol)>();
    private List<(ulong, DominoShape)> membersDominoShapes = new List<(ulong, DominoShape)>();
    private List<(ulong, IVector4Animation)> membersDominoColors = new List<(ulong, IVector4Animation)>();
    private List<(ulong, InitialSymbol)> membersUnitFaces = new List<(ulong, InitialSymbol)>();
    private List<(ulong, InitialSymbol)> membersDetails = new List<(ulong, InitialSymbol)>();

    private bool highlighted;
    private bool selected;

    public TerrainTilePresenter(
        GameToDominoConnection domino,
        MemberToViewMapper vivimap,
        Geomancer.Model.Terrain terrain,
        Location location,
        TerrainTile terrainTile) {
      this.domino = domino;
      this.vivimap = vivimap;
      this.location = location;
      this.terrainTile = terrainTile;
      this.terrain = terrain;

      var eternalMemberId = nextMemberId++;
      membersFrontColors.Add((eternalMemberId, Vector4Animation.Color(.4f, .4f, 0, 1)));
      membersSideColors.Add((eternalMemberId, Vector4Animation.Color(.4f, .4f, 0, 1)));

      foreach (var member in terrainTile.members) {
        OnAddMember(member);
      }

      // var patternTile = terrain.pattern.patternTiles[location.indexInGroup];
      var pattern = terrain.pattern;

      var initialTileDescription =
          new InitialTile(
              location,
              terrainTile.elevation,
              CalculateTintedFrontColor(membersFrontColors[membersFrontColors.Count - 1].Item2, selected, highlighted),
              membersSideColors[membersSideColors.Count - 1].Item2,
              CalculateMaybeOverlay(membersOverlays),
              CalculateMaybeFeature(membersFeatures),
              membersItems);
      
      var position = CalculatePosition(terrain.elevationStepHeight, terrain.pattern, location, terrainTile.elevation);
      
      //   var tile = terrain.tiles[location];
      //   int lowestNeighborElevation = tile.elevation;
      //   foreach (var neighborLoc in pattern.GetAdjacentLocations(tile.location, false)) {
      //     if (terrain.TileExists(neighborLoc)) {
      //       lowestNeighborElevation = Math.Min(lowestNeighborElevation, terrain.tiles[neighborLoc].elevation);
      //     } else {
      //       lowestNeighborElevation = 0;
      //     }
      //   }
      //   int depth = Math.Max(1, tile.elevation - lowestNeighborElevation);
      //
      //   var patternTile = pattern.patternTiles[tile.location.indexInGroup];
      //
      //   var highlighted = false;
      //   var frontColor = highlighted ? Vector4Animation.Color(.1f, .1f, .1f) : Vector4Animation.Color(0f, 0, 0f);
      //   var sideColor = highlighted ? Vector4Animation.Color(.1f, .1f, .1f) : Vector4Animation.Color(0f, 0, 0f);
      //
      var patternTileIndex = location.indexInGroup;
      var shapeIndex = pattern.patternTiles[patternTileIndex].shapeIndex;
      //   var radianards = pattern.patternTiles[patternTileIndex].rotateRadianards;
      //   var radians = radianards * 0.001f;
      //   var degrees = (float)(radians * 180f / Math.PI);
      //   var rotation = Quaternion.AngleAxis(-degrees, Vector3.up);
      var unityElevationStepHeight = terrain.elevationStepHeight * ModelExtensions.ModelToUnityMultiplier;


      tileViewId = domino.CreateTile(initialTileDescription);
          
    }

    // private InitialTile MakeInitialTile() {
    //   
    // }

    private static Vector3 CalculatePosition(int elevationStepHeight, Pattern pattern, Location location, int elevation) {
      var positionVec2 = pattern.GetTileCenter(location);
      var positionVec3 = new Vec3(positionVec2.x, positionVec2.y, elevation * elevationStepHeight);
      return positionVec3.ToUnity();
    }

    public void SetHighlighted(bool highlighted) {
      this.highlighted = highlighted;
      RefreshFrontColor();
    }
    public void SetSelected(bool selected) {
      this.selected = selected;
      RefreshFrontColor();
    }

    private void RefreshFrontColor() {
      domino.SetFrontColor(
          tileViewId, 
          CalculateTintedFrontColor(
              membersFrontColors[membersFrontColors.Count - 1].Item2, selected, highlighted));
    }

    private void RefreshSideColor() {
      domino.SetSidesColor(tileViewId, membersSideColors[membersSideColors.Count - 1].Item2);
    }
    
    private void RefreshFeature() {
      domino.SetFeature(tileViewId, membersFeatures.Count == 0 ? null : membersFeatures[membersFeatures.Count - 1].Item2);
    }
    
    private void RefreshUnit() {
      if (this.unitViewId != 0) {
        domino.DestroyUnit(this.unitViewId);
        this.unitViewId = 0;
      }
      if (this.unitViewId == 0 && membersUnitFaces.Count > 0) {
        var defaultColor = Vector4Animation.Color(.4f, .4f, 0, 1);
        var defaultShape = DominoShape.TALL_DOMINO;
        
        var position = CalculatePosition(terrain.elevationStepHeight, terrain.pattern, location, terrainTile.elevation);
        var initialUnit =
            new InitialUnit(
                membersDominoShapes.Count > 0 ? membersDominoShapes[membersDominoShapes.Count - 1].Item2 : defaultShape,
                membersDominoColors.Count > 0 ? membersDominoColors[membersDominoColors.Count - 1].Item2 : defaultColor,
                membersUnitFaces[membersUnitFaces.Count - 1].Item2,
                membersDetails,
                1,
                1);
        this.unitViewId = domino.CreateUnit(initialUnit);
      }
    }

    private void RefreshDomino() {
      // TODO: replace this with a call to unitView.SetDomino
      RefreshUnit();
    }

    private void RefreshUnitFace() {
      // TODO: replace this with a call to unitView.SetFace
      RefreshUnit();
    }

    private void RefreshOverlay() {
      domino.SetOverlay(
          tileViewId, membersOverlays.Count == 0 ? null : membersOverlays[membersOverlays.Count - 1].Item2);
    }
    
    private void RefreshItems() {
      domino.ClearItems(tileViewId);
      foreach (var x in membersItems) {
        domino.AddItem(tileViewId, x.Item1, x.Item2);
      }
    }

    private void RefreshDetails() {
      // unitView.ClearDetails();
      // foreach (var x in membersDetails) {
      //   unitView.AddItem(x.Item1, x.Item2);
      // }
      // TODO put the above in
      RefreshUnit();
    }

    private void OnAddMember(string member) {
      ulong memberId = nextMemberId++;
      members.Add((memberId, member));
      // var visitor = new AttributeAddingVisitor(this, memberId);
      foreach (var thing in vivimap.getEntries(member)) {
        if (thing is MemberToViewMapper.TopColorDescriptionForIDescription topColor) {
          membersFrontColors.Add((memberId, topColor.color));
          if (tileViewId != 0) {
            RefreshFrontColor();
          }
        } else if (thing is MemberToViewMapper.SideColorDescriptionForIDescription sideColor) {
          membersSideColors.Add((memberId, sideColor.color));
          if (tileViewId != 0) {
            RefreshSideColor();
          }
        } else if (thing is MemberToViewMapper.OverlayDescriptionForIDescription overlay) {
          membersOverlays.Add((memberId, overlay.symbol));
          if (tileViewId != 0) {
            RefreshOverlay();
          }
        } else if (thing is MemberToViewMapper.FeatureDescriptionForIDescription feature) {
          membersFeatures.Add((memberId, feature.symbol));
          if (tileViewId != 0) {
            RefreshFeature();
          }
        } else if (thing is MemberToViewMapper.DominoColorDescriptionForIDescription dominoColor) {
          membersDominoColors.Add((memberId, dominoColor.color));
          if (unitViewId != 0) {
            RefreshUnit();
          } else {
            RefreshDomino();
          }
        } else if (thing is MemberToViewMapper.DominoShapeDescriptionForIDescription dominoShape) {
          membersDominoShapes.Add((memberId, dominoShape.shape));
          if (unitViewId != 0) {
            RefreshUnit();
          } else {
            RefreshDomino();
          }
        } else if (thing is MemberToViewMapper.FaceDescriptionForIDescription face) {
          membersUnitFaces.Add((memberId, face.symbol));
          if (unitViewId == 0) {
            RefreshUnit();
          } else {
            RefreshUnitFace();
          }
        } else if (thing is MemberToViewMapper.DetailDescriptionForIDescription detail) {
          membersDetails.Add((memberId, detail.symbol));
          if (unitViewId == 0) {
            RefreshUnit();
          } else {
            RefreshDetails();
          }
        } else if (thing is MemberToViewMapper.ItemDescriptionForIDescription item) {
          membersItems.Add((memberId, item.symbol));
          if (tileViewId != 0) {
            RefreshItems();
          }
        } else {
          Asserts.Assert(false);
        }
      }
    }

    public void OnRemoveMember(int index) {
      var (memberId, member) = members[index];
      members.RemoveAt(index);
      foreach (var thing in vivimap.getEntries(member)) {
        if (thing is MemberToViewMapper.TopColorDescriptionForIDescription topColor) {
          membersFrontColors.RemoveAll(x => x.Item1 == memberId);
          if (tileViewId != 0) {
            RefreshFrontColor();
          }
        } else if (thing is MemberToViewMapper.SideColorDescriptionForIDescription sideColor) {
          membersSideColors.RemoveAll(x => x.Item1 == memberId);
          if (tileViewId != 0) {
            RefreshSideColor();
          }
        } else if (thing is MemberToViewMapper.OverlayDescriptionForIDescription overlay) {
          membersOverlays.RemoveAll(x => x.Item1 == memberId);
          if (tileViewId != 0) {
            RefreshOverlay();
          }
        } else if (thing is MemberToViewMapper.FeatureDescriptionForIDescription feature) {
          membersFeatures.RemoveAll(x => x.Item1 == memberId);
          if (tileViewId != 0) {
            RefreshFeature();
          }
        } else if (thing is MemberToViewMapper.DominoShapeDescriptionForIDescription dominoShape) {
          membersDominoShapes.RemoveAll(x => x.Item1 == memberId);
          if (unitViewId != 0) {
            RefreshDomino();
          }
        } else if (thing is MemberToViewMapper.DominoColorDescriptionForIDescription dominoColor) {
          membersDominoColors.RemoveAll(x => x.Item1 == memberId);
          if (unitViewId != 0) {
            RefreshDomino();
          }
        } else if (thing is MemberToViewMapper.FaceDescriptionForIDescription face) {
          membersUnitFaces.RemoveAll(x => x.Item1 == memberId);
          if (unitViewId != 0) {
            RefreshUnitFace();
          }
        } else if (thing is MemberToViewMapper.DetailDescriptionForIDescription detail) {
          membersDetails.RemoveAll(x => x.Item1 == memberId);
          if (unitViewId != 0) {
            RefreshDetails();
          }
        } else if (thing is MemberToViewMapper.ItemDescriptionForIDescription item) {
          membersItems.RemoveAll(x => x.Item1 == memberId);
          if (tileViewId != 0) {
            RefreshItems();
          }
        } else {
          Asserts.Assert(false);
        }
      }
    }

    public void AddMember(string member) {
      terrainTile.members.Add(member);
      OnAddMember(member);
    }

    public void RemoveMember(string member) {
      int index = terrainTile.members.IndexOf(member);
      Asserts.Assert(index >= 0);
      terrainTile.members.RemoveAt(index);
      OnRemoveMember(index);
    }

    public void RemoveMemberAt(int index) {
      terrainTile.members.RemoveAt(index);
      OnRemoveMember(index);
    }

    public void SetElevation(int elevation) {
      terrainTile.elevation = elevation;
      domino.SetElevation(tileViewId, elevation);
    }

    //   if (unitView) {
    //     unitView.Destruct();
    //     unitView = null;
    //   }
    //
    //   if (maybeUnitDescription != null) {
    //     
    //     // unitView.SetDescription(maybeUnitDescription);
    //   }
    // }

    // private (TileDescription, UnitDescription) GetDescriptions() {
    //   var defaultUnitDescription =
    //     new UnitDescription(
    //       null,
    //       new DominoDescription(false, Vector4Animation.Color(.5f, 0, .5f)),
    //       new InitialSymbol(
    //         RenderPriority.DOMINO,
    //         new SymbolDescription(
    //           "a", Vector4Animation.Color(0, 1, 0), 45, 1, OutlineMode.WithBackOutline),
    //         true,
    //         Vector4Animation.Color(0, 0, 0)),
    //       new List<(int, InitialSymbol)>(),
    //       1,
    //       1);
    //
    //   var (tileDescription, unitDescription) =
    //     vivimap.Vivify(defaultTileDescription, defaultUnitDescription, members);
    //   return (tileDescription, unitDescription);
    // }

    public void DestroyTerrainTilePresenter() {
      domino.DestroyTile(tileViewId);
    }

    private static InitialSymbol CalculateMaybeOverlay(List<(ulong, InitialSymbol)> membersOverlays) {
      return membersOverlays.Count == 0 ? null : membersOverlays[membersOverlays.Count - 1].Item2;
    }

    private static InitialSymbol CalculateMaybeFeature(List<(ulong, InitialSymbol)> membersFeatures) {
      return membersFeatures.Count == 0 ? null : membersFeatures[membersFeatures.Count - 1].Item2;
    }

    private static IVector4Animation CalculateTintedFrontColor(
        IVector4Animation membersFrontColor, bool selected, bool highlighted) {
      if (selected && highlighted) {
        return
            new MultiplyVector4Animation(
                new AddVector4Animation(
                    new MultiplyVector4Animation(membersFrontColor, 5f),
                    new MultiplyVector4Animation(Vector4Animation.Color(1, 1, 1, 1), 3f)),
                1 / 8f);
      } else if (selected) {
        return
            new MultiplyVector4Animation(
                new AddVector4Animation(
                    new MultiplyVector4Animation(membersFrontColor, 6f),
                    new MultiplyVector4Animation(Vector4Animation.Color(1, 1, 1, 1), 2f)),
                1 / 8f);
      } else if (highlighted) {
        return
            new MultiplyVector4Animation(
                new AddVector4Animation(
                    new MultiplyVector4Animation(membersFrontColor, 7f),
                    new MultiplyVector4Animation(Vector4Animation.Color(1, 1, 1, 1), 1f)),
                1 / 8f);
      } else {
        return membersFrontColor;
      }
    }
  }
}
