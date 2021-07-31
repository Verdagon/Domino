using System;
using System.Collections.Generic;
using Geomancer;
using Geomancer.Model;
using SimpleJSON;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Domino {
  public static class CommandParser {
    public static PatternSideAdjacency ParsePatternSideAdjacency(JSONNode node) {
      var obj = JsonHarvester.ExpectObject(node, "PatternTile must be an object!");

      return new PatternSideAdjacency(
          JsonHarvester.ExpectMemberInteger(obj, "group_relative_x"),
          JsonHarvester.ExpectMemberInteger(obj, "group_relative_y"),
          JsonHarvester.ExpectMemberInteger(obj, "tile_index"),
          JsonHarvester.ExpectMemberInteger(obj, "side_index"));
    }
    
    public static PatternCornerAdjacency ParsePatternCornerAdjacency(JSONNode node) {
      var obj = JsonHarvester.ExpectObject(node, "PatternTile must be an object!");

      return new PatternCornerAdjacency(
          JsonHarvester.ExpectMemberInteger(obj, "group_relative_x"),
          JsonHarvester.ExpectMemberInteger(obj, "group_relative_y"),
          JsonHarvester.ExpectMemberInteger(obj, "tile_index"),
          JsonHarvester.ExpectMemberInteger(obj, "corner_index"));
    }

    public static PatternTile ParsePatternTile(JSONNode node) {
      var obj = JsonHarvester.ExpectObject(node, "PatternTile must be an object!");

      var shapeIndex = JsonHarvester.ExpectMemberInteger(obj, "shape_index");
      var rotateRadianards = JsonHarvester.ExpectMemberInteger(obj, "rotate_radianards");
      var translate = JsonHarvester.ExpectMemberVec2(obj, "translate");
      var sideIndexToSideAdjacencies =
          new PatternSideAdjacencyImmList(
              JsonHarvester.ExpectMemberArray(obj, "side_index_to_side_adjacencies", ParsePatternSideAdjacency));
      var cornerIndexToCornerAdjacencies =
          new PatternCornerAdjacencyImmListImmList(
              JsonHarvester.ExpectMemberArray(
                  obj, "corner_index_to_corner_adjacencies",
                  cornerAdjacenciesNode => new PatternCornerAdjacencyImmList(
                      JsonHarvester.ExpectArray(
                          cornerAdjacenciesNode, "side_index_to_side_adjacencies should contain arrays!",
                          ParsePatternCornerAdjacency))));

      return new PatternTile(
          shapeIndex, rotateRadianards, translate, sideIndexToSideAdjacencies, cornerIndexToCornerAdjacencies);
    }

    public static Pattern ParsePattern(JSONObject pattern) {
      var name = JsonHarvester.ExpectMemberString(pattern, "name");
      var shapeIndexToCorners =
          new Vec2ImmListImmList(
              JsonHarvester.ExpectMemberArray(
                  pattern, "shape_index_to_corners", (innerListNode) => {
                    var innerList =
                        JsonHarvester.ExpectArray(
                            innerListNode, "shape_index_to_corners should contain arrays!",
                            JsonHarvester.ParseVec2);
                    return new Vec2ImmList(innerList);
                  }));
      var patternTiles =
          new PatternTileImmList(
              JsonHarvester.ExpectMemberArray(
                  pattern, "pattern_tiles", ParsePatternTile));
      var xOffset = JsonHarvester.ExpectMemberVec2(pattern, "x_offset");
      var yOffset = JsonHarvester.ExpectMemberVec2(pattern, "y_offset");
      return new Pattern(name, shapeIndexToCorners, patternTiles, xOffset, yOffset);
    }

    public static IDominoMessage ParseCommand(JSONObject command) {
      if (!command.HasKey("command_type")) {
        Debug.LogError("Command object didn't have key 'command'!");
        return null;
      }
      var typeNode = command["command_type"];
      if (typeNode is JSONString typeStr) {
        switch (typeStr.Value) {
          case "SetupGame":
            return new SetupGameMessage(
                JsonHarvester.ExpectMemberVec3(command, "look_at"), 
                JsonHarvester.ExpectMemberVec3(command, "look_at_offset_to_camera"),
                JsonHarvester.ExpectMemberInteger(command, "elevation_step_height"),
                ParsePattern(JsonHarvester.ExpectMemberObject(command, "pattern")));
          case "CreateTile":
            return new CreateTileMessage(
                JsonHarvester.ExpectMemberULong(command, "tile_id"),
                JsonHarvester.ParseInitialTile(JsonHarvester.ExpectMemberObject(command, "initial_tile")));
          case "CreateUnit":
            return new CreateUnitMessage(
                JsonHarvester.ExpectMemberULong(command, "unit_id"),
                JsonHarvester.ParseInitialUnit(JsonHarvester.ExpectMemberObject(command, "initial_unit")));
          case "MakePanel":
            var id = JsonHarvester.ExpectMemberULong(command, "id");
            var panelGXInScreen = JsonHarvester.ExpectMemberInteger(command, "panel_grid_x_in_screen");
            var panelGYInScreen = JsonHarvester.ExpectMemberInteger(command, "panel_grid_y_in_screen");
            var panelGW = JsonHarvester.ExpectMemberInteger(command, "panel_grid_width");
            var panelGH = JsonHarvester.ExpectMemberInteger(command, "panel_grid_height");
            return new MakePanelMessage(id, panelGXInScreen, panelGYInScreen, panelGW, panelGH);
          case "ScheduleClose":
            return new ScheduleCloseMessage(
                JsonHarvester.ExpectMemberULong(command, "view_id"),
                JsonHarvester.ExpectMemberLong(command, "start_ms_from_now"));
          case "RemoveView":
            return new RemoveViewMessage(JsonHarvester.ExpectMemberULong(command, "view_id"));
          case "SetOpacity":
            Asserts.Assert(false, "impl message!");
            break;
          case "SetFadeOut":
            var fadeOut = JsonHarvester.ExpectMemberObject(command, "fade_out");
            return new SetFadeOutMessage(
                JsonHarvester.ExpectMemberULong(command, "id"),
                new FadeOut(
                    JsonHarvester.ExpectMemberLong(fadeOut, "fade_out_start_time_ms"),
                    JsonHarvester.ExpectMemberLong(fadeOut, "fade_out_end_time_ms")));
          case "SetFadeIn":
            var fadeIn = JsonHarvester.ExpectMemberObject(command, "fade_in");
            return new SetFadeInMessage(
                JsonHarvester.ExpectMemberULong(command, "id"),
                new FadeIn(
                    JsonHarvester.ExpectMemberLong(fadeIn, "fade_in_start_time_ms"),
                    JsonHarvester.ExpectMemberLong(fadeIn, "fade_in_end_time_ms")));
          case "AddButton":
            Asserts.Assert(false, "impl message!");
            break;
          case "AddRectangle":
            return new AddRectangleMessage(
                JsonHarvester.ExpectMemberULong(command, "new_view_id"),
                JsonHarvester.ExpectMemberULong(command, "parent_view_id"),
                JsonHarvester.ExpectMemberInteger(command, "x"),
                JsonHarvester.ExpectMemberInteger(command, "y"),
                JsonHarvester.ExpectMemberInteger(command, "width"),
                JsonHarvester.ExpectMemberInteger(command, "height"),
                JsonHarvester.ExpectMemberInteger(command, "z"),
                JsonHarvester.ExpectMemberColor(command, "color"),
                JsonHarvester.ExpectMemberColor(command, "border_color"));
          case "AddSymbol":
            return new AddSymbolMessage(
                JsonHarvester.ExpectMemberULong(command, "new_view_id"),
                JsonHarvester.ExpectMemberULong(command, "parent_view_id"),
                JsonHarvester.ExpectMemberInteger(command, "x"),
                JsonHarvester.ExpectMemberInteger(command, "y"),
                JsonHarvester.ExpectMemberInteger(command, "size"),
                JsonHarvester.ExpectMemberInteger(command, "z"),
                JsonHarvester.ExpectMemberColor(command, "color"),
                JsonHarvester.parseSymbolId(JsonHarvester.ExpectMemberObject(command, "symbol_id")),
                JsonHarvester.ExpectMemberBoolean(command, "centered"));
          case "ShowPrism":
            Asserts.Assert(false, "impl message!");
            break;
          case "FadeInThenOut":
            Asserts.Assert(false, "impl message!");
            break;
          case "ShowRune":
            Asserts.Assert(false, "impl message!");
            break;
          case "SetOverlay":
            Asserts.Assert(false, "impl message!");
            break;
          case "SetFeature":
            Asserts.Assert(false, "impl message!");
            break;
          case "SetCliffColor":
            return new SetCliffColorMessage(
                JsonHarvester.ExpectMemberULong(command, "tile_id"),
                JsonHarvester.ExpectMemberColorAnim(command, "color"));
          case "SetSurfaceColor":
            return new SetSurfaceColorMessage(
                JsonHarvester.ExpectMemberULong(command, "tile_id"),
                JsonHarvester.ExpectMemberColorAnim(command, "color"));
          case "SetElevation":
            return new SetElevationMessage(
                JsonHarvester.ExpectMemberULong(command, "tile_id"),
                JsonHarvester.ExpectMemberInteger(command, "elevation"));
          case "RemoveItem":
            Asserts.Assert(false, "impl message!");
            break;
          case "ClearItems":
            Asserts.Assert(false, "impl message!");
            break;
          case "AddItem":
            Asserts.Assert(false, "impl message!");
            break;
          case "AddDetail":
            Asserts.Assert(false, "impl message!");
            break;
          case "RemoveDetail":
            Asserts.Assert(false, "impl message!");
            break;
          case "DestroyTile":
            return new DestroyTileMessage(JsonHarvester.ExpectMemberULong(command, "tile_id"));
          case "DestroyUnit":
            return new DestroyUnitMessage(
                JsonHarvester.ExpectMemberULong(command, "unit_id"));
          default:
            Asserts.Assert(false, "Unknown command type: " + typeStr.Value);
            break;
        }
        Asserts.Assert(false);
      } else {
        Debug.LogError("Command object's 'command' field wasn't a string!");
        Asserts.Assert(false);
      }
      Asserts.Assert(false);
      return null;
    }
  }
}
