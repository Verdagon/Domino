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
        int newPanelId = (int) makePanel.id;
        var newPanel =
            overlayPaneler.MakePanel(
                makePanel.id, makePanel.panelGXInScreen, makePanel.panelGYInScreen, makePanel.panelGW, makePanel.panelGH);
        idToPanel.Add(newPanelId, newPanel);
        viewIdToPanelId.Add(makePanel.id, newPanelId);
      } else if (message is RemoveViewMessage removeView) {
        var panelId = viewIdToPanelId[removeView.viewId];
        var panel = idToPanel[panelId];
        panel.Remove(removeView.viewId);
        viewIdToPanelId.Remove(removeView.viewId);
      } else if (message is ScheduleCloseMessage scheduleClose) {
        var panelId = viewIdToPanelId[scheduleClose.viewId];
        var panel = idToPanel[panelId];
        panel.ScheduleClose(scheduleClose.startMsFromNow);
        idToPanel.Remove(panelId);
        viewIdToPanelId.Remove(scheduleClose.viewId);
      } else if (message is AddRectangleMessage addRectangle) {
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
            addRectangle.color,
            addRectangle.color);
        viewIdToPanelId.Add(addRectangle.newViewId, panelId);
      } else if (message is AddStringMessage addString) {
        var panelId = viewIdToPanelId[addString.parentViewId];
        var panel = idToPanel[panelId];
        panel.AddString(
            addString.newViewsIds,
            addString.parentViewId,
            addString.x,
            addString.y,
            addString.maxWide,
            addString.color,
            addString.fontName,
            addString.str);
        foreach (var newViewId in addString.newViewsIds) {
          viewIdToPanelId.Add(newViewId, panelId);
        }
      } else if (message is AddSymbolMessage addSymbol) {
        var panelId = viewIdToPanelId[addSymbol.parentViewId];
        var panel = idToPanel[panelId];
        panel.AddSymbol(
            addSymbol.newViewId,
            addSymbol.parentViewId,
            addSymbol.x,
            addSymbol.y,
            addSymbol.size,
            addSymbol.z,
            addSymbol.color,
            addSymbol.symbol);
        viewIdToPanelId.Add(addSymbol.newViewId, panelId);
      } else {
        Asserts.Assert(false);
      }
    }
  }
}
