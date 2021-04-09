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
      void visitUnitDominoColor(DominoColorDescriptionForIDescription domino);
      void visitUnitFace(FaceDescriptionForIDescription symbol);
      void visitUnitDetail(DetailDescriptionForIDescription symbol);
      void visitTileItem(ItemDescriptionForIDescription symbol);
    }

    public interface IDescription {
      void visit(IDescriptionVisitor visitor);
    }
    public class TopColorDescriptionForIDescription : IDescription {
      public readonly IVector4Animation color;
      public TopColorDescriptionForIDescription(IVector4Animation color) { this.color = color; }
      public void visit(IDescriptionVisitor visitor) { visitor.visitTileTopColor(this); }
    }
    public class SideColorDescriptionForIDescription : IDescription {
      public readonly IVector4Animation color;
      public SideColorDescriptionForIDescription(IVector4Animation color) { this.color = color; }
      public void visit(IDescriptionVisitor visitor) { visitor.visitTileSideColor(this); }
    }
    // public class OutlineColorDescriptionForIDescription : IDescription {
    //   public readonly IVector4Animation color;
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
      public readonly DominoShape shape;
      public DominoShapeDescriptionForIDescription(DominoShape shape) { this.shape = shape; }
      public void visit(IDescriptionVisitor visitor) { visitor.visitUnitDominoShape(this); }
    }
    public class DominoColorDescriptionForIDescription : IDescription {
      public readonly IVector4Animation color;
      public DominoColorDescriptionForIDescription(IVector4Animation color) { this.color = color; }
      public void visit(IDescriptionVisitor visitor) { visitor.visitUnitDominoColor(this); }
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
      var jsonStr = @"
        {
          grass: {
            surfaceColor: [0, 75, 0],
            wallColor: [0, 50, 0],
          },
          dirt: {
            surfaceColor: [100, 32, 0],
            wallColor: [64, 25, 0],
          },
          mud: {
            surfaceColor: [50, 16, 0],
            wallColor: [32, 12, 0],
          },
          rocks: {
            overlay: {
              symbol: {font: ""AthSymbols"", char: 102},
              faceColor: [128, 128, 128, 50],
              outlined: true
            }
          },
          healthPotion: {
            item: {
              symbol: {font: ""AthSymbols"", char: 43},
              faceColor: [200, 0, 200, 380]
            }
          },
          manaPotion: {
            item: {
              symbol: {font: ""AthSymbols"", char: 44},
              faceColor: [200, 0, 200, 380]
            }
          },
          cave: {
            feature: {
              symbol: {font: ""AthSymbols"", char: 112},
              faceColor: [200, 0, 200, 380],
              outlined: true,
              outlineColor: [255, 255, 255]
              extruded: true,
              sideColor: [255, 255, 255]
            }
          },
          fire: {
            feature: {
              symbol: {font: ""AthSymbols"", char: 114},
              faceColor: [200, 100, 0, 380]
            }
          },
          magma: {
            overlay: {
              symbol: {font: ""AthSymbols"", char: 114},
              faceColor: [255, 100, 0, 380],
              sideColor: [50, 0, 0, 380]
            }
          },
          avelisk: {
            unit: {
              dominoColor: [255, 255, 0, 255],
              face: {
                symbol: {font: ""AthSymbols"", char: 120},
                faceColor: [255, 255, 255, 380],
                outlined: true,
                depth: 5
              }
            }
          },
          zeddy: {
            detail: {
              symbol: {font: ""AthSymbols"", char: 117},
              outlined: true,
              depth: 5
            }
          }
        }
        ";
      var entries = new Dictionary<string, List<IDescription>>();
      var rootNode = SimpleJSON.JSONObject.Parse(jsonStr);
      var rootObj = rootNode as JSONObject;
      if (rootObj == null) {
        throw new Exception("Couldn't load json root object!");
      }
      foreach (var memberName in rootObj.Keys) {
        var memberObj = ExpectMemberObject(rootObj, memberName);
        var memberEntries = new List<IDescription>();
        if (entries.ContainsKey(memberName)) {
          memberEntries = entries[memberName];
        } else {
          entries.Add(memberName, memberEntries);
        }
        if (memberObj.HasKey("surfaceColor")) {
          memberEntries.Add(new TopColorDescriptionForIDescription(parseColor(memberObj["surfaceColor"])));
          memberObj.Remove("surfaceColor");
        }
        if (memberObj.HasKey("wallColor")) {
          memberEntries.Add(new SideColorDescriptionForIDescription(parseColor(memberObj["wallColor"])));
          memberObj.Remove("wallColor");
        }
        if (GetMaybeMemberObject(memberObj, "overlay", out var overlayObj)) {
          memberEntries.Add(new OverlayDescriptionForIDescription(parseInitialSymbol(overlayObj)));
          memberObj.Remove("overlay");
        }
        if (GetMaybeMemberObject(memberObj, "feature", out var featureObj)) {
          memberEntries.Add(new FeatureDescriptionForIDescription(parseInitialSymbol(featureObj)));
          memberObj.Remove("feature");
        }
        if (GetMaybeMemberObject(memberObj, "item", out var itemObj)) {
          memberEntries.Add(new ItemDescriptionForIDescription(parseInitialSymbol(itemObj)));
          memberObj.Remove("item");
        }
        if (GetMaybeMemberObject(memberObj, "detail", out var detailObj)) {
          memberEntries.Add(new DetailDescriptionForIDescription(parseInitialSymbol(detailObj)));
          memberObj.Remove("detail");
        }
        if (GetMaybeMemberObject(memberObj, "unit", out var unitObj)) {
          var faceSymbol = parseInitialSymbol(ExpectMemberObject(unitObj, "face"));
          memberEntries.Add(new FaceDescriptionForIDescription(faceSymbol));

          if (GetMaybeMemberColor(unitObj, "dominoColor", out var dominoColor)) {
            memberEntries.Add(new DominoColorDescriptionForIDescription(dominoColor));
          }
          
          memberObj.Remove("unit");
        }
        foreach (var unknownKey in memberObj.Keys) {
          throw new Exception("Unknown key: " + unknownKey);
        }
      }
      return new MemberToViewMapper(entries);
    }

    private static InitialSymbol parseInitialSymbol(JSONObject obj) {
      var symbolId = parseSymbolId(ExpectMemberObject(obj, "symbol"));
      int rotationDegrees =
          GetMaybeMemberInteger(obj, "rotationDegrees", out var newRotationDegrees) 
              ? newRotationDegrees : 0;
      int sizePercent =
          GetMaybeMemberInteger(obj, "sizePercent", out var newSizePercent)
              ? newSizePercent : 100;
      IVector4Animation faceColor =
          GetMaybeMemberColor(obj, "faceColor", out var newFrontColor)
              ? newFrontColor : Vector4Animation.WHITE;
      bool outlined = GetMaybeMemberBoolean(obj, "outlined", out var newOutlined) ? newOutlined : false;
      IVector4Animation outlineColor =
          GetMaybeMemberColor(obj, "outlineColor", out var newOutlineColor)
              ? newOutlineColor : Vector4Animation.BLACK;
      IVector4Animation sideColor =
          GetMaybeMemberColor(obj, "sideColor", out var newSideColor)
              ? newSideColor : faceColor;
      int depth = GetMaybeMemberInteger(obj, "depth", out var newDepth) ? newDepth : 0;
      return new InitialSymbol(
          symbolId, rotationDegrees, sizePercent, faceColor, outlined, outlineColor, depth, sideColor);
    }

    private static SymbolId parseSymbolId(JSONObject obj) {
      var fontName = ExpectMemberString(obj, "font");
      var chaar = ExpectMemberInteger(obj, "char");
      return new SymbolId(fontName, chaar);
    }

    private static bool GetMaybeColor(JSONNode node, out IVector4Animation result) {
      if (node == null) {
        result = null;
        return false;
      }
      result = parseColor(node);
      return true;
    }

    private static IVector4Animation parseColor(JSONNode obj) {
      if (obj is JSONArray arr) {
        if (arr.Count != 3 && arr.Count != 4) {
          throw new Exception($"Color array had {arr.Count} elements, expected 3 or 4.");
        }
        int red = ExpectInteger(arr[0], "Color array element 0 not an integer!");
        int green = ExpectInteger(arr[1], "Color array element 0 not an integer!");
        int blue = ExpectInteger(arr[2], "Color array element 0 not an integer!");
        int alpha = arr.Count == 4 ? ExpectInteger(arr[2], "Color array element 0 not an integer!") : 255;
        return new ConstantVector4Animation(new Vector4(red / 255.0f, green / 255.0f, blue / 255.0f, alpha / 255.0f));
      } else {
        throw new Exception("Expected an array for a color!");
      }
    }
    
    private static JSONObject ExpectObject(JSONNode node, string message) {
      if (node == null) {
        throw new Exception(message);
      } else if (node is JSONObject obj) {
        return obj;
      } else {
        throw new Exception(message);
      }
    }

    private static int ExpectInteger(JSONNode node, string message) {
      if (node == null) {
        throw new Exception(message);
      } else if (node is JSONNumber num) {
        if (num.AsDouble > 0 && num.AsDouble < 1) {
          throw new Exception(message);
        }
        return num.AsInt;
      } else {
        throw new Exception(message);
      }
    }

    private static string ExpectString(JSONNode node, string message) {
      if (node == null) {
        throw new Exception("Object null!");
      } else if (node is JSONString str) {
        return str.Value;
      } else {
        throw new Exception(message);
      }
    }

    private static bool GetMaybeObject(JSONNode node, string message, out JSONObject result) {
      if (node == null) {
        result = null;
        return false;
      } else if (node is JSONObject obj) {
        result = obj;
        return true;
      } else {
        throw new Exception(message);
      }
    }
    
    private static bool GetMaybeInteger(JSONNode node, string message, out int result) {
      if (node is JSONNumber num) {
        if (num.AsDouble >= 0 && num.AsDouble < 1) {
          throw new Exception($"Expected integer, but got {num.AsDouble}");
        }
        result = num.AsInt;
        return true;
      } else {
        throw new Exception(message);
      }
    }

    private static bool GetMaybeBoolean(JSONNode node, string message, out bool result) {
      if (node is JSONBool b) {
        result = b.AsBool;
        return true;
      } else {
        throw new Exception(message);
      }
    }

    private static JSONObject ExpectMemberObject(JSONNode node, string memberName) {
      return ExpectObject(node[memberName], $"Member '{memberName}' should be an object but isn't!");
    }
    private static string ExpectMemberString(JSONObject obj, string memberName) {
      return ExpectString(obj[memberName], $"Member '{memberName}' should be a string but isn't!");
    }
    private static int ExpectMemberInteger(JSONObject obj, string memberName) {
      return ExpectInteger(obj[memberName], $"Member '{memberName}' should be an integer but isn't!");
    }
    private static bool GetMaybeMemberObject(JSONObject obj, string memberName, out JSONObject result) {
      if (!obj.HasKey(memberName)) {
        result = null;
        return false;
      }
      return GetMaybeObject(obj[memberName], $"Member '{memberName}' should be an object but isn't!", out result);
    }
    private static bool GetMaybeMemberInteger(JSONObject obj, string memberName, out int result) {
      if (!obj.HasKey(memberName)) {
        result = 0;
        return false;
      }
      return GetMaybeInteger(obj[memberName], $"Member '{memberName}' should be an integer but isn't!", out result);
    }
    private static bool GetMaybeMemberBoolean(JSONObject obj, string memberName, out bool result) {
      if (!obj.HasKey(memberName)) {
        result = false;
        return false;
      }
      return GetMaybeBoolean(obj[memberName], $"Member '{memberName}' should be a boolean but isn't!", out result);
    }
    private static bool GetMaybeMemberColor(JSONObject obj, string memberName, out IVector4Animation result) {
      if (!obj.HasKey(memberName)) {
        result = null;
        return false;
      }
      return GetMaybeColor(obj[memberName], out result);
    }
  }
}
