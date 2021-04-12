using System;
using System.Collections.Generic;
using Geomancer.Model;
using UnityEngine;
using Domino;

namespace Geomancer {
  public class PanelPresenter {
    private DominoToGameConnection server;
    IClock clock;
    ITimer timer;
    ILoader loader;
    private OverlayPaneler overlayPaneler;
    Dictionary<int, OverlayPanelView> idToPanel = new Dictionary<int, OverlayPanelView>();
    Dictionary<ulong, int> viewIdToPanelId = new Dictionary<ulong, int>();

    public PanelPresenter(
        IClock clock,
        ITimer timer,
        ILoader loader,
        OverlayPaneler overlayPaneler,
        DominoToGameConnection server) {
      this.server = server;
      this.clock = clock;
      this.timer = timer;
      this.loader = loader;
      this.overlayPaneler = overlayPaneler;
      this.idToPanel = new Dictionary<int, OverlayPanelView>();
      this.viewIdToPanelId = new Dictionary<ulong, int>();

      // foreach (var locationAndTile in terrain.tiles) {
      //   addTerrainTile(locationAndTile.Key, locationAndTile.Value);
      // }

      // RefreshPhantomTiles();
    }

    // public void AddTile(TerrainTilePresenter presenter) {
    //   idToPanelView.Add(presenter.location, presenter);
    // }

    // public Location GetMaybeMouseHighlightLocation() { return maybeMouseHighlightedLocation; }

    // public void DestroyPanelPresenter() {
    //   foreach (var entry in idToPanelView) {
    //     entry.Value.Destroy();
    //   }
    // }

    public void HandleMessage(IDominoMessage message) {
      if (message is MakePanelMessage makePanel) {
        Debug.Log($"MakePanel id {makePanel.id} gx {makePanel.panelGXInScreen} gy {makePanel.panelGYInScreen} gw {makePanel.panelGW} gh {makePanel.panelGH}");
        int newPanelId = (int) makePanel.id;
        var newPanel =
            overlayPaneler.MakePanel(
                makePanel.id, makePanel.panelGXInScreen, makePanel.panelGYInScreen, makePanel.panelGW, makePanel.panelGH);
        idToPanel.Add(newPanelId, newPanel);
        viewIdToPanelId.Add(makePanel.id, newPanelId);
      } else if (message is RemoveViewMessage removeView) {
        Debug.Log($"RemoveView viewId {removeView.viewId}");
        var panelId = viewIdToPanelId[removeView.viewId];
        var panel = idToPanel[panelId];
        panel.Remove(removeView.viewId);
        viewIdToPanelId.Remove(removeView.viewId);
      } else if (message is ScheduleCloseMessage scheduleClose) {
        Debug.Log($"ScheduleClose viewId {scheduleClose.viewId} startMsFromNow {scheduleClose.startMsFromNow}");
        var panelId = viewIdToPanelId[scheduleClose.viewId];
        var panel = idToPanel[panelId];
        panel.ScheduleClose(scheduleClose.startMsFromNow);
        idToPanel.Remove(panelId);
        viewIdToPanelId.Remove(scheduleClose.viewId);
      } else if (message is AddButtonMessage addButton) {
        Debug.Log(
            $"AddButton newViewId {addButton.newViewId} parentViewId {addButton.parentViewId} x {addButton.x} y {addButton.y} width {addButton.width} height {addButton.height} z {addButton.z} color {addButton.color} borderColor {addButton.borderColor} pressedColor {addButton.pressedColor} onClicked {addButton.onClicked} onMouseIn {addButton.onMouseIn} onMouseOut {addButton.onMouseOut}");
        
        var panelId = viewIdToPanelId[addButton.parentViewId];
        var panel = idToPanel[panelId];
        panel.AddButton(
            addButton.newViewId,
            addButton.parentViewId,
            addButton.x,
            addButton.y,
            addButton.width,
            addButton.height,
            addButton.z,
            addButton.color.ColorToUnity(),
            addButton.borderColor.ColorToUnity(),
            addButton.pressedColor.ColorToUnity(),
            () => server.TriggerEvent(addButton.onClicked),
            () => server.TriggerEvent(addButton.onMouseIn),
            () => server.TriggerEvent(addButton.onMouseOut));
        viewIdToPanelId.Add(addButton.newViewId, panelId);
      } else if (message is AddRectangleMessage addRectangle) {
        Debug.Log(
            $"AddRectangle newViewId {addRectangle.newViewId} parentViewId {addRectangle.parentViewId} x {addRectangle.x} y {addRectangle.y} width {addRectangle.width} height {addRectangle.height} z {addRectangle.z} color {addRectangle.color} borderColor {addRectangle.borderColor}");
    
        var panelId = viewIdToPanelId[addRectangle.parentViewId];
        var panel = idToPanel[panelId];
        panel.AddRectangle(
            addRectangle.newViewId,
            addRectangle.parentViewId,
            addRectangle.x,
            addRectangle.y,
            addRectangle.width,
            addRectangle.height,
            addRectangle.z,
            addRectangle.color.ColorToUnity(),
            addRectangle.color.ColorToUnity());
        viewIdToPanelId.Add(addRectangle.newViewId, panelId);
      // } else if (message is AddStringMessage addString) {
      //   Debug.Log(
      //       $"AddString newViewIds ... parentViewId {addString.parentViewId} x {addString.x} y {addString.y} maxWide {addString.maxWide} color {addString.color} fontName {addString.fontName} str {addString.str}");
      //   
      //   var panelId = viewIdToPanelId[addString.parentViewId];
      //   var panel = idToPanel[panelId];
      //   panel.AddString(
      //       addString.newViewsIds,
      //       addString.parentViewId,
      //       addString.x,
      //       addString.y,
      //       addString.maxWide,
      //       addString.color,
      //       addString.fontName,
      //       addString.str);
      //   foreach (var newViewId in addString.newViewsIds) {
      //     viewIdToPanelId.Add(newViewId, panelId);
      //   }
      } else if (message is AddSymbolMessage addSymbol) {
        Debug.Log(
            $"AddSymbol newViewId {addSymbol.newViewId} parentViewId {addSymbol.parentViewId} x {addSymbol.x} y {addSymbol.y} size {addSymbol.size} z {addSymbol.z} color {addSymbol.color} symbol {addSymbol.symbolId} centered {addSymbol.centered}");
        
        var panelId = viewIdToPanelId[addSymbol.parentViewId];
        var panel = idToPanel[panelId];
        panel.AddSymbol(
            addSymbol.newViewId,
            addSymbol.parentViewId,
            addSymbol.x,
            addSymbol.y,
            addSymbol.size,
            addSymbol.z,
            addSymbol.color.ColorToUnity(),
            addSymbol.symbolId);
        viewIdToPanelId.Add(addSymbol.newViewId, panelId);
      } else if (message is SetFadeInMessage fadeIn) {
        Debug.Log($"SetFadeIn id {fadeIn.id} fadeIn ...");
        var panelId = viewIdToPanelId[fadeIn.id];
        var panel = idToPanel[panelId];
        panel.SetFadeIn(fadeIn.id, fadeIn.fadeIn);
      } else if (message is SetFadeOutMessage fadeOut) {
        Debug.Log($"SetFadeOut id {fadeOut.id} fadeOut ...");
        var panelId = viewIdToPanelId[fadeOut.id];
        var panel = idToPanel[panelId];
        panel.SetFadeOut(fadeOut.id, fadeOut.fadeOut);
      } else {
        Asserts.Assert(false);
      }
    }
  }
}
