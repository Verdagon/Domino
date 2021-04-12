using System.Collections.Generic;
using Domino;
using Geomancer.Model;
using SimpleJSON;

namespace GeomancerServer {
  public static class Jsonify {
    
    public static JSONObject ToJson(this Vec3 vec) {
      var obj = new JSONObject();
      obj.Add("x", vec.x);
      obj.Add("y", vec.y);
      obj.Add("z", vec.z);
      return obj;
    }

    public static JSONObject ToJson(this Vec2 vec) {
      var obj = new JSONObject();
      obj.Add("x", vec.x);
      obj.Add("y", vec.y);
      return obj;
    }
    public static JSONObject ToJson(this PatternTile obj) { 
      var json = new JSONObject();
      json.Add("shapeIndex", obj.shapeIndex);
      json.Add("rotateRadianards", obj.rotateRadianards);
      json.Add("translate", obj.translate.ToJson());
      json.Add("sideIndexToSideAdjacencies", obj.sideIndexToSideAdjacencies.ToJson());
      json.Add("cornerIndexToCornerAdjacencies", obj.cornerIndexToCornerAdjacencies.ToJson());
      return json;
    }
    public static JSONArray ToJson(this PatternSideAdjacencyImmList obj) {
      var json = new JSONArray();
      foreach (var el in obj) {
        json.Add(el.ToJson());
      }
      return json;
    }
    public static JSONObject ToJson(this PatternSideAdjacency obj) {
      var json = new JSONObject();
      json.Add("groupRelativeX", obj.groupRelativeX);
      json.Add("groupRelativeY", obj.groupRelativeY);
      json.Add("tileIndex", obj.tileIndex);
      json.Add("sideIndex", obj.sideIndex);
      return json;
    }
    public static JSONArray ToJson(this PatternCornerAdjacencyImmListImmList obj) {
      var json = new JSONArray();
      foreach (var el in obj) {
        json.Add(el.ToJson());
      }
      return json;
    }
    public static JSONArray ToJson(this PatternCornerAdjacencyImmList obj) {
      var json = new JSONArray();
      foreach (var el in obj) {
        json.Add(el.ToJson());
      }
      return json;
    }
    public static JSONObject ToJson(this PatternCornerAdjacency obj) {
      var json = new JSONObject();
      json.Add("groupRelativeX", obj.groupRelativeX);
      json.Add("groupRelativeY", obj.groupRelativeY);
      json.Add("tileIndex", obj.tileIndex);
      json.Add("cornerIndex", obj.cornerIndex);
      return json;
    }



    public static JSONObject ToJson(this Pattern obj) {
      var json = new JSONObject();
      json.Add("name", obj.name);
      json.Add("xOffset", obj.xOffset.ToJson());
      json.Add("yOffset", obj.yOffset.ToJson());
      json.Add("shapeIndexToCorners", obj.shapeIndexToCorners.ToJson());
      json.Add("patternTiles", obj.patternTiles.ToJson());
      return json;
    }
    
    public static JSONArray ToJson(this PatternTileImmList obj) {
      var json = new JSONArray();
      foreach (var el in obj) {
        json.Add(el.ToJson());
      }
      return json;
    }
    public static JSONArray ToJson(this Vec2ImmListImmList obj) {
      var json = new JSONArray();
      foreach (var el in obj) {
        json.Add(el.ToJson());
      }
      return json;
    }
    public static JSONArray ToJson(this Vec2ImmList obj) {
      var json = new JSONArray();
      foreach (var el in obj) {
        json.Add(el.ToJson());
      }
      return json;
    }
    public static JSONObject ToJson(this (ulong, InitialSymbol) obj) {
      var json = new JSONObject();
      json.Add("key", obj.Item1);
      json.Add("value", obj.Item2.ToJson());
      return json;
    }
    public static JSONArray ToJson(this List<(ulong, InitialSymbol)> obj) {
      var json = new JSONArray();
      foreach (var el in obj) {
        json.Add(el.ToJson());
      }
      return json;
    }
    
    public static JSONObject ToJson(this Location obj) {
      var json = new JSONObject();
      json.Add("groupX", obj.groupX);
      json.Add("groupY", obj.groupY);
      json.Add("indexInGroup", obj.indexInGroup);
      return json;
    }
    public static JSONObject ToJson(this Vec4i obj) {
      var json = new JSONObject();
      json.Add("x", obj.x);
      json.Add("y", obj.y);
      json.Add("z", obj.z);
      json.Add("w", obj.w);
      return json;
    }
    public static JSONNode ToJson(this IVec4iAnimation obj) {
      if (obj is ConstantVec4iAnimation cvec4i) {
        var json = new JSONArray();
        json.Add(cvec4i.vec.x);
        json.Add(cvec4i.vec.y);
        json.Add(cvec4i.vec.z);
        json.Add(cvec4i.vec.w);
        return json;
      } else {
        Asserts.Assert(false);
        return null;
      }
    }
    public static JSONNode ToJson(this SymbolId obj) {
      var json = new JSONObject();
      json.Add("fontName", obj.fontName);
      json.Add("unicode", obj.unicode);
      return json;
    }
    public static JSONNode ToJson(this InitialSymbolGlyph obj) {
      if (obj == null) {
        return JSONNull.CreateOrGet();
      }
      var json = new JSONObject();
      json.Add("symbolId", obj.symbolId.ToJson());
      json.Add("color", obj.color.ToJson());
      return json;
    }
    public static JSONNode ToJson(this InitialSymbolOutline obj) {
      if (obj == null) {
        return JSONNull.CreateOrGet();
      }
      var json = new JSONObject();
      json.Add("mode", obj.mode.ToString());
      json.Add("color", obj.color.ToJson());
      return json;
    }
    public static JSONNode ToJson(this InitialSymbolSides obj) {
      if (obj == null) {
        return JSONNull.CreateOrGet();
      }
      var json = new JSONObject();
      json.Add("depthPercent", obj.depthPercent);
      json.Add("color", obj.color.ToJson());
      return json;
    }
    public static JSONNode ToJson(this InitialSymbol obj) {
      if (obj == null) {
        return JSONNull.CreateOrGet();
      }
      var json = new JSONObject();
      json.Add("glyph", obj.glyph.ToJson());
      json.Add("outline", obj.outline.ToJson());
      json.Add("sides", obj.sides.ToJson());
      json.Add("rotationDegrees", obj.rotationDegrees);
      json.Add("sizePercent", obj.sizePercent);
      return json;
    }
    public static JSONObject ToJson(this InitialTile obj) {
      var json = new JSONObject();
      json.Add("location", obj.location.ToJson());
      json.Add("elevation", obj.elevation);
      json.Add("topColor", obj.topColor.ToJson());
      json.Add("sideColor", obj.sideColor.ToJson());
      json.Add("maybeOverlaySymbol", obj.maybeOverlaySymbol.ToJson());
      json.Add("maybeFeatureSymbol", obj.maybeFeatureSymbol.ToJson());
      json.Add("itemIdToSymbol", obj.itemIdToSymbol.ToJson());
      return json;
    }

    public static JSONObject ToJson(this SetupGameMessage obj) {
      var json = new JSONObject();
      json.Add("command", "SetupGame");
      json.Add("lookAt", obj.cameraPosition.ToJson());
      json.Add("lookAtOffsetToCamera", obj.lookatOffsetToCamera.ToJson());
      json.Add("elevationStepHeight", new JSONNumber(obj.elevationStepHeight));
      json.Add("pattern", obj.pattern.ToJson());
      return json;
    }

    public static JSONObject ToJson(this MakePanelMessage obj) {
      var json = new JSONObject();
      json.Add("command", "MakePanel");
      json.Add("id", obj.id);
      json.Add("panelGXInScreen", obj.panelGXInScreen);
      json.Add("panelGYInScreen", obj.panelGYInScreen);
      json.Add("panelGW", obj.panelGW);
      json.Add("panelGH", obj.panelGH);
      return json;
    }

    public static JSONObject ToJson(this CreateTileMessage obj) {
      var json = new JSONObject();
      json.Add("command", "CreateTile");
      json.Add("id", obj.newTileId);
      json.Add("initialTile", obj.initialTile.ToJson());
      return json;
    }

    public static JSONObject ToJson(this IDominoMessage command) {
      if (command is SetupGameMessage setupGame) {
        return setupGame.ToJson();
      } else if (command is MakePanelMessage makePanel) {
        return makePanel.ToJson();
      } else if (command is CreateTileMessage createTile) {
        return createTile.ToJson();
      } else {
        Asserts.Assert(false);
        return null;
      }
    }
  }
}