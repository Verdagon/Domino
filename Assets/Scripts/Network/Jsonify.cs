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
      json.Add("shape_index", obj.shapeIndex);
      json.Add("rotate_radianards", obj.rotateRadianards);
      json.Add("translate", obj.translate.ToJson());
      json.Add("side_index_to_side_adjacencies", obj.sideIndexToSideAdjacencies.ToJson());
      json.Add("corner_index_to_corner_adjacencies", obj.cornerIndexToCornerAdjacencies.ToJson());
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
      json.Add("group_relative_x", obj.groupRelativeX);
      json.Add("group_relative_y", obj.groupRelativeY);
      json.Add("tile_index", obj.tileIndex);
      json.Add("side_index", obj.sideIndex);
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
      json.Add("group_relative_x", obj.groupRelativeX);
      json.Add("group_relative_y", obj.groupRelativeY);
      json.Add("tile_index", obj.tileIndex);
      json.Add("corner_index", obj.cornerIndex);
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
      json.Add("group_x", obj.groupX);
      json.Add("group_y", obj.groupY);
      json.Add("index_in_group", obj.indexInGroup);
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
      json.Add("font_name", obj.fontName);
      json.Add("unicode", obj.unicode);
      return json;
    }
    public static JSONNode ToJson(this InitialSymbolGlyph obj) {
      if (obj == null) {
        return JSONNull.CreateOrGet();
      }
      var json = new JSONObject();
      json.Add("symbol_id", obj.symbolId.ToJson());
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
      json.Add("depth_percent", obj.depthPercent);
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
      json.Add("rotation_degrees", obj.rotationDegrees);
      json.Add("size_percent", obj.sizePercent);
      return json;
    }
    public static JSONObject ToJson(this InitialTile obj) {
      var json = new JSONObject();
      json.Add("location", obj.location.ToJson());
      json.Add("elevation", obj.elevation);
      json.Add("top_color", obj.topColor.ToJson());
      json.Add("side_solor", obj.sideColor.ToJson());
      json.Add("maybe_overlay_symbol", obj.maybeOverlaySymbol.ToJson());
      json.Add("maybe_feature_symbol", obj.maybeFeatureSymbol.ToJson());
      json.Add("item_id_to_symbol", obj.itemIdToSymbol.ToJson());
      return json;
    }

    public static JSONObject ToJson(this SetupGameMessage obj) {
      var json = new JSONObject();
      json.Add("command", "SetupGame");
      json.Add("lookAt", obj.cameraPosition.ToJson());
      json.Add("look_at_offset_to_camera", obj.lookatOffsetToCamera.ToJson());
      json.Add("elevation_step_height", new JSONNumber(obj.elevationStepHeight));
      json.Add("pattern", obj.pattern.ToJson());
      return json;
    }

    public static JSONObject ToJson(this MakePanelMessage obj) {
      var json = new JSONObject();
      json.Add("command", "MakePanel");
      json.Add("id", obj.id);
      json.Add("panel_grid_x_in_screen", obj.panelGXInScreen);
      json.Add("panel_grid_y_in_screen", obj.panelGYInScreen);
      json.Add("panel_grid_width", obj.panelGW);
      json.Add("panel_grid_height", obj.panelGH);
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