using System;
using System.Collections.Generic;
using Geomancer;
using Geomancer.Model;
using GeomancerServer;
using SimpleJSON;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Domino {
  public class DominoToGameConnection {
    public delegate void ICommandHandler(IDominoMessage command);
    
    private Requester requester;
    private string serverUrl;
    private ICommandHandler commandHandler;
    public DominoToGameConnection(Requester requester, string serverUrl, ICommandHandler commandHandler) {
      this.requester = requester;
      this.serverUrl = serverUrl;
      this.commandHandler = commandHandler;
    }

    public void Start(int screenGW, int screenGH) {
      var obj = new JSONObject();
      obj.Add("event_type", new JSONString("Start"));
      obj.Add("screen_grid_width", new JSONNumber(screenGW));
      obj.Add("screen_grid_height", new JSONNumber(screenGH));
      requester.Request(serverUrl, obj, (response) => {
        HandleResponse(response);
      });
    }

    public void KeyDown(int c, bool leftShiftDown, bool rightShiftDown, bool ctrlDown, bool leftAltDown, bool rightAltDown) {
      var obj = new JSONObject();
      obj.Add("event_type", new JSONString("KeyDown"));
      obj.Add("unicode", c);
      obj.Add("left_shift_down", leftShiftDown);
      obj.Add("right_shift_down", rightShiftDown);
      obj.Add("ctrl_down", ctrlDown);
      obj.Add("left_alt_down", leftAltDown);
      obj.Add("right_alt_down", rightAltDown);
      requester.Request(serverUrl, obj, (response) => {
        HandleResponse(response);
      });
    }

    public void TriggerEvent(ulong e) {
      Asserts.Assert(false);
      // otherSide.TriggerEvent(e);
    }

    public void SetHoveredLocation(ulong tileViewId, Location location) {
      var obj = new JSONObject();
      obj.Add("request", new JSONString("SetHoveredLocation"));
      obj.Add("tile_id", new JSONNumber(tileViewId));
      obj.Add("location", location != null ? location.ToJson() : (JSONNode)JSONNull.CreateOrGet());
      requester.Request(serverUrl, obj, (response) => {
        HandleResponse(response);
      });
    }
    
    public void LocationMouseDown(ulong tileViewId, Location location) {
      var obj = new JSONObject();
      obj.Add("request", new JSONString("LocationMouseDown"));
      obj.Add("tile_id", new JSONNumber(tileViewId));
      obj.Add("location", location != null ? location.ToJson() : (JSONNode)JSONNull.CreateOrGet());
      requester.Request(serverUrl, obj, (response) => {
        HandleResponse(response);
      });
    }
    
    private void HandleResponse(JSONNode response) {
      if (response == null) {
        Debug.LogError("Failed to start game!");
        return;
      }
      if (!response.HasKey("commands")) {
        Debug.LogError("Response didn't contain commands array!");
        return;
      }
      var commandsNode = response["commands"];
      if (commandsNode is JSONArray commandsArray) {
        foreach (var commandNode in commandsArray) {
          if (commandNode.Value is JSONObject commandObj) {
            var command = CommandParser.ParseCommand(commandObj);
            if (command != null) {
              commandHandler(command);
            }
          } else {
            Debug.LogError("Command wasn't an object!");
            return;
          }
        }
      } else {
        Debug.LogError("Response didn't contain commands array!");
        return;
      }
    }
  }
}
