using Domino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geomancer.Model;
using SimpleJSON;

namespace Geomancer {
  public static class JsonHarvester {
    public delegate T Parser<T>(JSONNode node);
    
    public static InitialUnit ParseInitialUnit(JSONObject obj) {
      // public readonly Location location;
      // public readonly InitialSymbol dominoSymbolDescription;
      // public readonly InitialSymbol faceSymbolDescription;
      // public readonly List<(ulong, InitialSymbol)> detailSymbolDescriptionById;
      // public readonly float hpRatio;
      // public readonly float mpRatio;
      
      var location = ExpectMemberLocation(obj, "location");
      var domino = ExpectMemberSymbol(obj, "domino");
      var face = ExpectMemberSymbol(obj, "face");
      var idToDetailSymbol = ParseIdToSymbolMap(ExpectMemberArray(obj, "idToDetailSymbol"));
      var hpPercent = GetMaybeMemberInteger(obj, "hpPercent", out var hp) ? hp : 100;
      var mpPercent = GetMaybeMemberInteger(obj, "mpPercent", out var mp) ? hp : 100;
      return new InitialUnit(
          location, domino, face, idToDetailSymbol, hpPercent, mpPercent);
    }

    public static InitialSymbol ParseInitialSymbol(JSONObject obj) {
      var glyph = ExpectSymbolGlyph(ExpectMemberObject(obj, "glyph"));
      var maybeSides = GetMaybeMemberObject(obj, "sides", out var sides) ? ExpectSymbolSides(sides) : null;
      var maybeOutline = GetMaybeMemberObject(obj, "outline", out var outline) ? ExpectSymbolOutline(outline) : null;
      int rotationDegrees =
          GetMaybeMemberInteger(obj, "rotationDegrees", out var newRotationDegrees) 
              ? newRotationDegrees : 0;
      int sizePercent =
          GetMaybeMemberInteger(obj, "sizePercent", out var newSizePercent)
              ? newSizePercent : 100;
      return new InitialSymbol(
          glyph, maybeOutline, maybeSides, rotationDegrees, sizePercent);
    }

    public static SymbolId parseSymbolId(JSONObject obj) {
      var fontName = ExpectMemberString(obj, "fontName");
      var chaar = ExpectMemberInteger(obj, "unicode");
      return new SymbolId(fontName, chaar);
    }

    public static bool GetMaybeColor(JSONNode node, out IVec4iAnimation result) {
      if (node == null) {
        result = null;
        return false;
      }
      result = ParseColorAnim(node);
      return true;
    }

    // public static IVec4iAnimation ParseColorAnim(JSONNode node) {
    //   if (node is JSONArray arr) {
    //     if (arr.Count != 3 && arr.Count != 4) {
    //       throw new Exception($"Color array had {arr.Count} elements, expected 3 or 4.");
    //     }
    //     int red = ExpectInteger(arr[0], "Color array element 0 not an integer!");
    //     int green = ExpectInteger(arr[1], "Color array element 0 not an integer!");
    //     int blue = ExpectInteger(arr[2], "Color array element 0 not an integer!");
    //     int alpha = arr.Count == 4 ? ExpectInteger(arr[2], "Color array element 0 not an integer!") : 255;
    //     return new ConstantVec4iAnimation(new Vec4i(red, green, blue, alpha));
    //   } else if (node is JSONObject obj) {
    //     int red = ExpectMemberInteger(obj, "red");
    //     int green = ExpectMemberInteger(obj, "green");
    //     int blue = ExpectMemberInteger(obj, "blue");
    //     int alpha = GetMaybeMemberInteger(obj, "alpha", out var r) ? r : 255;
    //     return new ConstantVec4iAnimation(new Vec4i(red, green, blue, alpha));
    //   } else {
    //     throw new Exception("Expected an array for a color!");
    //   }
    // }
    //
    public static Vec3 ParseVec3(JSONNode node) {
      if (node is JSONArray arr) {
        if (arr.Count != 3) {
          throw new Exception($"Vec3 array had {arr.Count} elements, expected 3.");
        }
        int x = ExpectInteger(arr[0], "Color array element 0 not an integer!");
        int y = ExpectInteger(arr[1], "Color array element 0 not an integer!");
        int z = ExpectInteger(arr[2], "Color array element 0 not an integer!");
        return new Vec3(x, y, z);
      } else if (node is JSONObject obj) {
        int x = ExpectMemberInteger(obj, "x");
        int y = ExpectMemberInteger(obj, "y");
        int z = ExpectMemberInteger(obj, "z");
        return new Vec3(x, y, z);
      } else {
        throw new Exception("Expected an array for a Vec3, but got: " + node.GetType().Name);
      }
    }

    public static Vec2 ParseVec2(JSONNode node) {
      if (node is JSONArray arr) {
        if (arr.Count != 2) {
          throw new Exception($"Vec2 array had {arr.Count} elements, expected 2.");
        }
        int x = ExpectInteger(arr[0], "Color array element 0 not an integer!");
        int y = ExpectInteger(arr[1], "Color array element 0 not an integer!");
        return new Vec2(x, y);
      } else if (node is JSONObject obj) {
        int x = ExpectMemberInteger(obj, "x");
        int y = ExpectMemberInteger(obj, "y");
        return new Vec2(x, y);
      } else {
        throw new Exception("Expected an array for a Vec2, but got: " + node.GetType().Name);
      }
    }

    public static InitialSymbolSides ExpectSymbolSides(JSONObject obj) {
      var depthPercent = ExpectMemberInteger(obj, "depthPercent");
      var color = ExpectMemberColorAnim(obj, "color");
      return new InitialSymbolSides(depthPercent, color);
    }

    public static InitialSymbolGlyph ExpectSymbolGlyph(JSONObject obj) {
      var symbolId = parseSymbolId(ExpectMemberObject(obj, "symbolId"));
      var color = ExpectMemberColorAnim(obj, "color");
      return new InitialSymbolGlyph(symbolId, color);
    }

    public static InitialSymbolOutline ExpectSymbolOutline(JSONObject obj) {
      OutlineMode mode = OutlineMode.NoOutline;
      var modeStr = ExpectMemberString(obj, "type");
      switch (modeStr) {
        case "centered":
          mode = OutlineMode.CenteredOutline;
          break;
        case "outer":
          mode = OutlineMode.OuterOutline;
          break;
        default:
          throw new Exception("Outline type can only be 'centered' or 'outer', was: " + modeStr);
      }
      var color = ExpectMemberColorAnim(obj, "color");
      return new InitialSymbolOutline(mode, color);
    }

    public static Location ExpectLocation(JSONNode node) {
      if (node is JSONObject obj) {
        int groupX = ExpectMemberInteger(obj, "groupX");
        int groupY = ExpectMemberInteger(obj, "groupY");
        int indexInGroup = ExpectMemberInteger(obj, "indexInGroup");
        return new Location(groupX, groupY, indexInGroup);
      } else {
        throw new Exception("Expected an array for a color!");
      }
    }

    public static JSONObject ExpectObject(JSONNode node, string message) {
      if (node == null) {
        throw new Exception(message);
      } else if (node is JSONObject obj) {
        return obj;
      } else {
        throw new Exception(message);
      }
    }

    public static int ExpectInteger(JSONNode node, string message) {
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

    public static bool ExpectBoolean(JSONNode node, string message) {
      if (node == null) {
        throw new Exception(message);
      } else if (node is JSONBool num) {
        return num.AsBool;
      } else {
        throw new Exception(message);
      }
    }

    public static string ExpectString(JSONNode node, string message) {
      if (node == null) {
        throw new Exception("Object null!");
      } else if (node is JSONString str) {
        return str.Value;
      } else {
        throw new Exception(message);
      }
    }

    public static bool GetMaybeObject(JSONNode node, string message, out JSONObject result) {
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
    
    public static bool GetMaybeInteger(JSONNode node, string message, out int result) {
      if (node is JSONNumber num) {
        if (num.AsDouble > 0 && num.AsDouble < 1) {
          throw new Exception($"Expected integer, but got {num.AsDouble}");
        }
        result = num.AsInt;
        return true;
      } else {
        throw new Exception(message);
      }
    }

    public static bool GetMaybeBoolean(JSONNode node, string message, out bool result) {
      if (node is JSONBool b) {
        result = b.AsBool;
        return true;
      } else {
        throw new Exception(message);
      }
    }

    public static JSONObject ExpectMemberObject(JSONObject node, string memberName) {
      return ExpectObject(node[memberName], $"Member '{memberName}' should be an object but isn't!");
    }
    
    public static JSONArray ExpectMemberArray(JSONObject node, string memberName) {
      return ExpectArray(ExpectMember(node, memberName), $"Member '{memberName}' should be an array but isn't!");
    }
    
    public static string ExpectMemberString(JSONObject obj, string memberName) {
      return ExpectString(obj[memberName], $"Member '{memberName}' should be a string but isn't!");
    }
    
    public static List<T> ExpectMemberArray<T>(JSONObject obj, string memberName, Parser<T> elementParser) {
      return ExpectArray<T>(ExpectMember(obj, memberName), $"Member '{memberName}' should be an array, but it isn't!", elementParser);
    }

    public static List<T> ExpectArray<T>(JSONNode arrayNode, string errorMessage, Parser<T> elementParser) {
      if (arrayNode is JSONArray array) {
        List<T> result = new List<T>();
        foreach (var element in arrayNode.Children) {
          result.Add(elementParser(element));
        }
        return result;
      } else {
        throw new Exception(errorMessage);
      }
    }

    public static JSONArray ExpectArray(JSONNode arrayNode, string errorMessage) {
      if (arrayNode is JSONArray array) {
        return array;
      } else {
        throw new Exception(errorMessage);
      }
    }

    public static int ExpectMemberInteger(JSONObject obj, string memberName) {
      return ExpectInteger(ExpectMember(obj, memberName), $"Member '{memberName}' should be an integer but isn't!");
    }

    public static bool ExpectMemberBoolean(JSONObject obj, string memberName) {
      return ExpectBoolean(ExpectMember(obj, memberName), $"Member '{memberName}' should be a boolean but isn't!");
    }

    public static Location ExpectMemberLocation(JSONObject obj, string memberName) {
      return ExpectLocation(ExpectMemberObject(obj, memberName));
    }

    public static JSONNode ExpectMember(JSONObject obj, string memberName) {
      if (!obj.HasKey(memberName)) {
        throw new Exception("Expected member '" + memberName + "' but was missing!");
      }
      return obj[memberName];
    }

    public static InitialSymbol ExpectMemberSymbol(JSONObject obj, string memberName) {
      return ParseInitialSymbol(ExpectMemberObject(obj, memberName));
    }
    
    public static List<(ulong, InitialSymbol)> ParseIdToSymbolMap(JSONArray arr) {
      return ExpectArray(
          arr,
          "ID-to-symbol map should be an array!",
          (elementNode) => {
            var elementObj = ExpectObject(elementNode, "ID-to-symbol map should contain objects!");
            return (
                ExpectMemberULong(elementObj, "id"),
                ExpectMemberSymbol(elementObj, "symbol"));
          });
    }

    public static List<(ulong, InitialSymbol)> ExpectMemberIdToSymbolMap(JSONObject obj, string memberName) {
      return ParseIdToSymbolMap(ExpectMemberArray(obj, memberName));
    }

    public static Vec3 ExpectMemberVec3(JSONObject obj, string memberName) {
      return ParseVec3(ExpectMember(obj, memberName));
    }

     public static Vec4i ExpectMemberColor(JSONObject obj, string memberName) {
       return ParseColor(ExpectMember(obj, memberName));
     }


    public static Vec2 ExpectMemberVec2(JSONObject obj, string memberName) {
      return ParseVec2(ExpectMember(obj, memberName));
    }

    public static bool GetMaybeMemberObject(JSONObject obj, string memberName, out JSONObject result) {
      if (!obj.HasKey(memberName)) {
        result = null;
        return false;
      }
      return GetMaybeObject(obj[memberName], $"Member '{memberName}' should be an object but isn't!", out result);
    }
    
    public static bool GetMaybeMemberVec3(JSONObject obj, string memberName, out Vec3 result) {
      if (!obj.HasKey(memberName)) {
        result = new Vec3(0, 0, 0);
        return false;
      }
      result = ParseVec3(obj[memberName]);
      return true;
    }

    public static bool GetMaybeMemberInteger(JSONObject obj, string memberName, out int result) {
      if (!obj.HasKey(memberName)) {
        result = 0;
        return false;
      }
      return GetMaybeInteger(obj[memberName], $"Member '{memberName}' should be an integer but isn't!", out result);
    }
    
    public static bool GetMaybeMemberBoolean(JSONObject obj, string memberName, out bool result) {
      if (!obj.HasKey(memberName)) {
        result = false;
        return false;
      }
      return GetMaybeBoolean(ExpectMember(obj, memberName), $"Member '{memberName}' should be a boolean but isn't!", out result);
    }
    
    public static InitialTile ParseInitialTile(JSONObject obj) {
      var location = ExpectMemberLocation(obj, "location");
      var elevation = ExpectMemberInteger(obj, "elevation");
      var topColor = ParseColorAnim(ExpectMemberObject(obj, "topColor"));
      var sideColor = ParseColorAnim(ExpectMemberObject(obj, "sideColor"));
      var maybeOverlaySymbol = GetMaybeMemberObject(obj, "maybeOverlaySymbol", out var s) ? ParseInitialSymbol(s) : null;
      var maybeFeatureSymbol = GetMaybeMemberObject(obj, "maybeFeatureSymbol", out var f) ? ParseInitialSymbol(f) : null;
      var itemIdToSymbol = new List<(ulong, InitialSymbol)>();
      return new InitialTile(location, elevation, topColor, sideColor, maybeOverlaySymbol, maybeFeatureSymbol, itemIdToSymbol);
    }
    
    public static ulong ExpectULong(JSONNode node, string message) {
      if (node == null) {
        throw new Exception(message);
      } else if (node is JSONNumber num) {
        if (num.AsDouble > 0 && num.AsDouble < 1) {
          throw new Exception(message);
        }
        return num.AsULong;
      } else {
        throw new Exception(message);
      }
    }

    public static long ExpectLong(JSONNode node, string message) {
      if (node == null) {
        throw new Exception(message);
      } else if (node is JSONNumber num) {
        if (num.AsDouble > 0 && num.AsDouble < 1) {
          throw new Exception(message);
        }
        return num.AsLong;
      } else {
        throw new Exception(message);
      }
    }

    public static ulong ExpectMemberULong(JSONObject obj, string memberName) {
      return ExpectULong(ExpectMember(obj, memberName), $"Member '{memberName}' should be an integer but isn't!");
    }

    public static long ExpectMemberLong(JSONObject obj, string memberName) {
      return ExpectLong(ExpectMember(obj, memberName), $"Member '{memberName}' should be a long but isn't!");
    }

    public static bool GetMaybeMemberColor(JSONObject obj, string memberName, out IVec4iAnimation result) {
      if (!obj.HasKey(memberName)) {
        result = null;
        return false;
      }
      return GetMaybeColor(obj[memberName], out result);
    }

    public static bool GetMaybeMemberLocation(JSONObject obj, string memberName, out Location result) {
      if (!obj.HasKey(memberName)) {
        result = null;
        return false;
      }
      if (obj[memberName] is JSONNull) {
        result = null;
        return false;
      }
      result = ExpectMemberLocation(obj, memberName);
      return true;
    }

     public static IVec4iAnimation ParseColorAnim(JSONNode node) {
       if (node is JSONArray arr) {
         return new ConstantVec4iAnimation(ParseColor(node));
       } else if (node is JSONObject obj) {
         if (GetMaybeMemberString(obj, "type", out var type)) {
           switch (type) {
             case "multiply":
               return new MultiplyVec4iAnimation(
                   ExpectMemberColorAnim(obj, "left"),
                   ExpectMemberColorAnim(obj, "right"));
             case "divide":
               return new DivideVec4iAnimation(
                   ExpectMemberColorAnim(obj, "left"),
                   ExpectMemberColorAnim(obj, "right"));
             case "add":
               return new AddVec4iAnimation(
                   ExpectMemberColorAnim(obj, "left"),
                   ExpectMemberColorAnim(obj, "right"));
             case "constant":
               return new ConstantVec4iAnimation(
                   ExpectMemberColor(obj, "val"));
             default:
               throw new Exception("Unknown animation type: " + type);
           }
         } else {
           return new ConstantVec4iAnimation(ParseColor(node));
         }
       } else {
         throw new Exception("Expected an array for a color!");
       }
     }
     
     public static IVec4iAnimation ExpectMemberColorAnim(JSONObject obj, string memberName) {
       return ParseColorAnim(ExpectMember(obj, memberName));
     }

     
     public static Vec4i ParseColor(JSONNode node) {
       if (node is JSONArray arr) {
         if (arr.Count != 3 && arr.Count != 4) {
           throw new Exception($"Color array had {arr.Count} elements, expected 3 or 4.");
         }
         int red = ExpectInteger(arr[0], "Color array element 0 not an integer!");
         int green = ExpectInteger(arr[1], "Color array element 0 not an integer!");
         int blue = ExpectInteger(arr[2], "Color array element 0 not an integer!");
         int alpha = arr.Count == 4 ? ExpectInteger(arr[2], "Color array element 0 not an integer!") : 255;
         return new Vec4i(red, green, blue, alpha);
       } else if (node is JSONObject obj) {
         int red = ExpectMemberInteger(obj, "x");
         int green = ExpectMemberInteger(obj, "y");
         int blue = ExpectMemberInteger(obj, "z");
         int alpha = GetMaybeMemberInteger(obj, "w", out var r) ? r : 255;
         return new Vec4i(red, green, blue, alpha);
       } else {
         throw new Exception("Expected an array for a color!");
       }
     }
     
     
     public static bool GetMaybeMemberString(JSONObject obj, string memberName, out string result) {
       if (!obj.HasKey(memberName)) {
         result = "";
         return false;
       }
       return GetMaybeString(obj[memberName], $"Member '{memberName}' should be an string but isn't!", out result);
     }

     public static bool GetMaybeString(JSONNode node, string message, out string result) {
       if (node is JSONString str) {
         result = str.Value;
         return true;
       } else {
         throw new Exception(message);
       }
     }

  }
}
