using Domino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleJSON;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;

namespace Geomancer {
  public class MemberToViewMapper {
    public interface IDescriptionVisitor {
      void visitTileTopColor(TopColorDescriptionForIDescription color);
      void visitTileSideColor(SideColorDescriptionForIDescription color);
      void visitTileOverlay(OverlayDescriptionForIDescription symbol);
      void visitTileFeature(FeatureDescriptionForIDescription symbol);
      void visitUnitDominoShape(DominoShapeDescriptionForIDescription domino);
      void visitUnitFace(FaceDescriptionForIDescription symbol);
      void visitUnitDetail(DetailDescriptionForIDescription symbol);
      void visitTileItem(ItemDescriptionForIDescription symbol);
    }

    public interface IDescription {
      void visit(IDescriptionVisitor visitor);
    }
    public class TopColorDescriptionForIDescription : IDescription {
      public readonly IVec4iAnimation color;
      public TopColorDescriptionForIDescription(IVec4iAnimation color) { this.color = color; }
      public void visit(IDescriptionVisitor visitor) { visitor.visitTileTopColor(this); }
    }
    public class SideColorDescriptionForIDescription : IDescription {
      public readonly IVec4iAnimation color;
      public SideColorDescriptionForIDescription(IVec4iAnimation color) { this.color = color; }
      public void visit(IDescriptionVisitor visitor) { visitor.visitTileSideColor(this); }
    }
    // public class OutlineColorDescriptionForIDescription : IDescription {
    //   public readonly IVec4iAnimation color;
    //   public OutlineColorDescriptionForIDescription(Vector4Animation color) { this.color = color; }
    // }
    public class OverlayDescriptionForIDescription : IDescription {
      public readonly Domino.InitialSymbol symbol;
      public OverlayDescriptionForIDescription(Domino.InitialSymbol symbol) { this.symbol = symbol; }
      public void visit(IDescriptionVisitor visitor) { visitor.visitTileOverlay(this); }
    }
    public class FeatureDescriptionForIDescription : IDescription {
      public readonly Domino.InitialSymbol symbol;
      public FeatureDescriptionForIDescription(Domino.InitialSymbol symbol) { this.symbol = symbol; }
      public void visit(IDescriptionVisitor visitor) { visitor.visitTileFeature(this); }
    }
    public class FaceDescriptionForIDescription : IDescription {
      public readonly Domino.InitialSymbol symbol;
      public FaceDescriptionForIDescription(Domino.InitialSymbol symbol) { this.symbol = symbol; }
      public void visit(IDescriptionVisitor visitor) { visitor.visitUnitFace(this); }
    }
    public class DominoShapeDescriptionForIDescription : IDescription {
      public readonly InitialSymbol symbol;
      public DominoShapeDescriptionForIDescription(InitialSymbol symbol) { this.symbol = symbol; }
      public void visit(IDescriptionVisitor visitor) { visitor.visitUnitDominoShape(this); }
    }
    public class ItemDescriptionForIDescription : IDescription {
      public readonly Domino.InitialSymbol symbol;
      public ItemDescriptionForIDescription(Domino.InitialSymbol symbol) { this.symbol = symbol; }
      public void visit(IDescriptionVisitor visitor) { visitor.visitTileItem(this); }
    }
    public class DetailDescriptionForIDescription : IDescription {
      public readonly Domino.InitialSymbol symbol;
      public DetailDescriptionForIDescription(Domino.InitialSymbol symbol) { this.symbol = symbol; }
      public void visit(IDescriptionVisitor visitor) { visitor.visitUnitDetail(this); }
    }

    Dictionary<string, List<IDescription>> entries;
    public MemberToViewMapper(Dictionary<string, List<IDescription>> entries) {
      this.entries = entries;
    }

    public List<IDescription> getEntries(string name) {
      if (!entries.ContainsKey(name)) {
        throw new Exception("No entries for member: " + name);
      }
      return entries[name];
    }

    public static MemberToViewMapper LoadMap(string filename) {
      string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, filename);
      string jsonStr = System.IO.File.ReadAllText(filePath);
      var entries = new Dictionary<string, List<IDescription>>();
      var rootNode = SimpleJSON.JSONObject.Parse(jsonStr);
      var rootObj = rootNode as JSONObject;
      if (rootObj == null) {
        throw new Exception("Couldn't load json root object!");
      }
      foreach (var memberName in rootObj.Keys) {
        var memberObj = JsonHarvester.ExpectMemberObject(rootObj, memberName);
        var memberEntries = new List<IDescription>();
        if (entries.ContainsKey(memberName)) {
          memberEntries = entries[memberName];
        } else {
          entries.Add(memberName, memberEntries);
        }
        if (memberObj.HasKey("surface_color")) {
          memberEntries.Add(new TopColorDescriptionForIDescription(JsonHarvester.ParseColorAnim(memberObj["surface_color"])));
          memberObj.Remove("surface_color");
        }
        if (memberObj.HasKey("wall_color")) {
          memberEntries.Add(new SideColorDescriptionForIDescription(JsonHarvester.ParseColorAnim(memberObj["wall_color"])));
          memberObj.Remove("wall_color");
        }
        if (JsonHarvester.GetMaybeMemberObject(memberObj, "overlay", out var overlayObj)) {
          memberEntries.Add(new OverlayDescriptionForIDescription(JsonHarvester.ParseInitialSymbol(overlayObj)));
          memberObj.Remove("overlay");
        }
        if (JsonHarvester.GetMaybeMemberObject(memberObj, "feature", out var featureObj)) {
          memberEntries.Add(new FeatureDescriptionForIDescription(JsonHarvester.ParseInitialSymbol(featureObj)));
          memberObj.Remove("feature");
        }
        if (JsonHarvester.GetMaybeMemberObject(memberObj, "item", out var itemObj)) {
          memberEntries.Add(new ItemDescriptionForIDescription(JsonHarvester.ParseInitialSymbol(itemObj)));
          memberObj.Remove("item");
        }
        if (JsonHarvester.GetMaybeMemberObject(memberObj, "detail", out var detailObj)) {
          memberEntries.Add(new DetailDescriptionForIDescription(JsonHarvester.ParseInitialSymbol(detailObj)));
          memberObj.Remove("detail");
        }
        if (JsonHarvester.GetMaybeMemberObject(memberObj, "unit", out var unitObj)) {
          var faceSymbol = JsonHarvester.ParseInitialSymbol(JsonHarvester.ExpectMemberObject(unitObj, "face"));
          memberEntries.Add(new FaceDescriptionForIDescription(faceSymbol));

          var dominoSymbol = JsonHarvester.ParseInitialSymbol(JsonHarvester.ExpectMemberObject(unitObj, "domino"));
          memberEntries.Add(new DominoShapeDescriptionForIDescription(dominoSymbol));

          memberObj.Remove("unit");
        }
        foreach (var unknownKey in memberObj.Keys) {
          throw new Exception("Unknown key: " + unknownKey);
        }
      }
      return new MemberToViewMapper(entries);
    }
  }
}
