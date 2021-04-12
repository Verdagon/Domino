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
          JsonHarvester.ExpectMemberInteger(obj, "groupRelativeX"),
          JsonHarvester.ExpectMemberInteger(obj, "groupRelativeY"),
          JsonHarvester.ExpectMemberInteger(obj, "tileIndex"),
          JsonHarvester.ExpectMemberInteger(obj, "sideIndex"));
    }
    
    public static PatternCornerAdjacency ParsePatternCornerAdjacency(JSONNode node) {
      var obj = JsonHarvester.ExpectObject(node, "PatternTile must be an object!");

      return new PatternCornerAdjacency(
          JsonHarvester.ExpectMemberInteger(obj, "groupRelativeX"),
          JsonHarvester.ExpectMemberInteger(obj, "groupRelativeY"),
          JsonHarvester.ExpectMemberInteger(obj, "tileIndex"),
          JsonHarvester.ExpectMemberInteger(obj, "cornerIndex"));
    }

    public static PatternTile ParsePatternTile(JSONNode node) {
      var obj = JsonHarvester.ExpectObject(node, "PatternTile must be an object!");

      var shapeIndex = JsonHarvester.ExpectMemberInteger(obj, "shapeIndex");
      var rotateRadianards = JsonHarvester.ExpectMemberInteger(obj, "rotateRadianards");
      var translate = JsonHarvester.ExpectMemberVec2(obj, "translate");
      var sideIndexToSideAdjacencies =
          new PatternSideAdjacencyImmList(
              JsonHarvester.ExpectMemberArray(obj, "sideIndexToSideAdjacencies", ParsePatternSideAdjacency));
      var cornerIndexToCornerAdjacencies =
          new PatternCornerAdjacencyImmListImmList(
              JsonHarvester.ExpectMemberArray(
                  obj, "cornerIndexToCornerAdjacencies",
                  cornerAdjacenciesNode => new PatternCornerAdjacencyImmList(
                      JsonHarvester.ExpectArray(
                          cornerAdjacenciesNode, "sideIndexToSideAdjacencies should contain arrays!",
                          ParsePatternCornerAdjacency))));

      return new PatternTile(
          shapeIndex, rotateRadianards, translate, sideIndexToSideAdjacencies, cornerIndexToCornerAdjacencies);
    }

    public static Pattern ParsePattern(JSONObject pattern) {
      var name = JsonHarvester.ExpectMemberString(pattern, "name");
      var shapeIndexToCorners =
          new Vec2ImmListImmList(
              JsonHarvester.ExpectMemberArray(
                  pattern, "shapeIndexToCorners", (innerListNode) => {
                    var innerList =
                        JsonHarvester.ExpectArray(
                            innerListNode, "shapeIndexToCorners should contain arrays!",
                            JsonHarvester.ParseVec2);
                    return new Vec2ImmList(innerList);
                  }));
      var patternTiles =
          new PatternTileImmList(
              JsonHarvester.ExpectMemberArray(
                  pattern, "patternTiles", ParsePatternTile));
      var xOffset = JsonHarvester.ExpectMemberVec2(pattern, "xOffset");
      var yOffset = JsonHarvester.ExpectMemberVec2(pattern, "yOffset");
      return new Pattern(name, shapeIndexToCorners, patternTiles, xOffset, yOffset);
    }

    public static IDominoMessage ParseCommand(JSONObject command) {
      if (!command.HasKey("command")) {
        Debug.LogError("Command object didn't have key 'command'!");
        return null;
      }
      var typeNode = command["command"];
      if (typeNode is JSONString typeStr) {
        switch (typeStr.Value) {
          case "SetupGame":
            return new SetupGameMessage(
                JsonHarvester.ExpectMemberVec3(command, "lookAt"), 
                JsonHarvester.ExpectMemberVec3(command, "lookAtOffsetToCamera"),
                JsonHarvester.ExpectMemberInteger(command, "elevationStepHeight"),
                ParsePattern(JsonHarvester.ExpectMemberObject(command, "pattern")));
          case "CreateTile":
            return new CreateTileMessage(
                JsonHarvester.ExpectMemberULong(command, "tileId"),
                JsonHarvester.ParseInitialTile(JsonHarvester.ExpectMemberObject(command, "initialTile")));
          case "CreateUnit":
            return new CreateUnitMessage(
                JsonHarvester.ExpectMemberULong(command, "unitId"),
                JsonHarvester.ParseInitialUnit(JsonHarvester.ExpectMemberObject(command, "initialUnit")));
          case "MakePanel":
            var id = JsonHarvester.ExpectMemberULong(command, "id");
            var panelGXInScreen = JsonHarvester.ExpectMemberInteger(command, "panelGXInScreen");
            var panelGYInScreen = JsonHarvester.ExpectMemberInteger(command, "panelGYInScreen");
            var panelGW = JsonHarvester.ExpectMemberInteger(command, "panelGW");
            var panelGH = JsonHarvester.ExpectMemberInteger(command, "panelGH");
            return new MakePanelMessage(id, panelGXInScreen, panelGYInScreen, panelGW, panelGH);
          case "ScheduleClose":
            return new ScheduleCloseMessage(
                JsonHarvester.ExpectMemberULong(command, "viewId"),
                JsonHarvester.ExpectMemberLong(command, "startMsFromNow"));
          case "RemoveView":
            return new RemoveViewMessage(JsonHarvester.ExpectMemberULong(command, "viewId"));
          case "SetOpacity":
            Asserts.Assert(false, "impl message!");
            break;
          case "SetFadeOut":
            var fadeOut = JsonHarvester.ExpectMemberObject(command, "fadeOut");
            return new SetFadeOutMessage(
                JsonHarvester.ExpectMemberULong(command, "id"),
                new FadeOut(
                    JsonHarvester.ExpectMemberLong(fadeOut, "fadeOutStartTimeMs"),
                    JsonHarvester.ExpectMemberLong(fadeOut, "fadeOutEndTimeMs")));
          case "SetFadeIn":
            var fadeIn = JsonHarvester.ExpectMemberObject(command, "fadeIn");
            return new SetFadeInMessage(
                JsonHarvester.ExpectMemberULong(command, "id"),
                new FadeIn(
                    JsonHarvester.ExpectMemberLong(fadeIn, "fadeInStartTimeMs"),
                    JsonHarvester.ExpectMemberLong(fadeIn, "fadeInEndTimeMs")));
          case "AddButton":
            Asserts.Assert(false, "impl message!");
            break;
          case "AddRectangle":
            return new AddRectangleMessage(
                JsonHarvester.ExpectMemberULong(command, "newViewId"),
                JsonHarvester.ExpectMemberULong(command, "parentViewId"),
                JsonHarvester.ExpectMemberInteger(command, "x"),
                JsonHarvester.ExpectMemberInteger(command, "y"),
                JsonHarvester.ExpectMemberInteger(command, "width"),
                JsonHarvester.ExpectMemberInteger(command, "height"),
                JsonHarvester.ExpectMemberInteger(command, "z"),
                JsonHarvester.ExpectMemberColor(command, "color"),
                JsonHarvester.ExpectMemberColor(command, "borderColor"));
          case "AddSymbol":
            return new AddSymbolMessage(
                JsonHarvester.ExpectMemberULong(command, "newViewId"),
                JsonHarvester.ExpectMemberULong(command, "parentViewId"),
                JsonHarvester.ExpectMemberInteger(command, "x"),
                JsonHarvester.ExpectMemberInteger(command, "y"),
                JsonHarvester.ExpectMemberInteger(command, "size"),
                JsonHarvester.ExpectMemberInteger(command, "z"),
                JsonHarvester.ExpectMemberColor(command, "color"),
                JsonHarvester.parseSymbolId(JsonHarvester.ExpectMemberObject(command, "symbolId")),
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
                JsonHarvester.ExpectMemberULong(command, "tileId"),
                JsonHarvester.ExpectMemberColorAnim(command, "color"));
          case "SetSurfaceColor":
            return new SetSurfaceColorMessage(
                JsonHarvester.ExpectMemberULong(command, "tileId"),
                JsonHarvester.ExpectMemberColorAnim(command, "color"));
          case "SetElevation":
            return new SetElevationMessage(
                JsonHarvester.ExpectMemberULong(command, "tileId"),
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
            return new DestroyTileMessage(JsonHarvester.ExpectMemberULong(command, "tileId"));
          case "DestroyUnit":
            return new DestroyUnitMessage(
                JsonHarvester.ExpectMemberULong(command, "unitId"));
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
