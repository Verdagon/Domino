using System.Collections.Generic;
using Geomancer;
using Geomancer.Model;
using UnityEngine;

namespace Domino {

  public class GameToDominoConnection {
    public DominoToGameConnection otherSide;
    public EditorServer server;

    private List<IDominoMessage> messages = new List<IDominoMessage>();
    private ulong nextId = 1;

    public GameToDominoConnection(DominoToGameConnection otherSide) {
      this.otherSide = otherSide;
      server = new EditorServer(this);
    }

    public ulong MakePanel(
        int panelGXInScreen,
        int panelGYInScreen,
        int panelGW,
        int panelGH) {
      ulong id = nextId++;
      messages.Add(new MakePanelMessage(id, panelGXInScreen, panelGYInScreen, panelGW, panelGH));
      return id;
    }

    public ulong CreateTile(InitialTile initialTile) {
      ulong id = nextId++;
      messages.Add(new CreateTileMessage(id, initialTile));
      return id;
    }

    public ulong CreateUnit(InitialUnit initialUnit) {
      ulong id = nextId++;
      messages.Add(new CreateUnitMessage(id, initialUnit));
      return id;
    }

    public void SetupGame(Vec3 cameraPosition, float elevationStepHeight) {
      messages.Add(new SetupGameMessage(cameraPosition, elevationStepHeight));
    }

    public void ScheduleClose(ulong viewId, ulong startMsFromNow) {
      messages.Add(new ScheduleCloseMessage(viewId, startMsFromNow));
    }

    public void Remove(ulong viewId, int id) {
      messages.Add(new RemoveMessage(viewId, id));
    }

    public void SetOpacity(ulong viewId, int id, float ratio) {
      messages.Add(new SetOpacityMessage(viewId, id, ratio));
    }

    public void SetFadeOut(ulong id, OverlayPanelView.FadeOut fadeOut) {
      messages.Add(new SetFadeOutMessage(id, fadeOut));
    }

    public void SetFadeIn(ulong id, OverlayPanelView.FadeIn fadeIn) {
      messages.Add(new SetFadeInMessage(id, fadeIn));
    }

    public List<ulong> AddString(
        ulong parentViewId,
        int parentId,
        float x,
        float y,
        int maxWide,
        Color color,
        string fontName,
        string str) {
      List<ulong> newViewIds = new List<ulong>();
      foreach (var c in str) {
        newViewIds.Add(nextId++);
      }
      messages.Add(new AddStringMessage(newViewIds, parentViewId, parentId, x, y, maxWide, color, fontName, str));
      return newViewIds;
    }

    public ulong AddBackground(ulong parentViewId, Color color, Color borderColor) {
      ulong newViewId = nextId++;
      messages.Add(new AddBackgroundMessage(newViewId, parentViewId, color, borderColor));
      return newViewId;
    }

    public ulong AddButton(
        ulong parentViewId,
        int parentId,
        float x,
        float y,
        float width,
        float height,
        int z,
        Color color,
        Color borderColor,
        Color pressedColor,
        ulong onClicked,
        ulong onMouseIn,
        ulong onMouseOut) {
      ulong newViewId = nextId++;
      messages.Add(
          new AddButtonMessage(
              newViewId, parentViewId, parentId, x, y, width, height, z, color, borderColor, pressedColor, onClicked,
              onMouseIn, onMouseOut));
      return newViewId;
    }

    public ulong AddFullscreenRect(ulong parentViewId, Color color) {
      ulong newViewId = nextId++;
      messages.Add(new AddFullscreenRectMessage(newViewId, parentViewId, color));
      return newViewId;
    }

    public ulong AddRectangle(
        ulong parentViewId,
        int parentId,
        float x,
        float y,
        float width,
        float height,
        int z,
        Color color,
        Color borderColor) {
      ulong newViewId = nextId++;
      messages.Add(
          new AddRectangleMessage(newViewId, parentViewId, parentId, x, y, width, height, z, color, borderColor));
      return newViewId;
    }

    public ulong AddSymbol(
        ulong parentViewId,
        int parentId,
        float x,
        float y,
        float size,
        int z,
        Color color,
        SymbolId symbol,
        bool centered) {
      ulong newViewId = nextId++;
      messages.Add(new AddSymbolMessage(newViewId, parentViewId, parentId, x, y, size, z, color, symbol, centered));
      return newViewId;
    }
    
    
    public void ShowPrism(ulong tileViewId, InitialSymbol prismDescription, InitialSymbol prismOverlayDescription) {
      messages.Add(new ShowPrismMessage(tileViewId, prismDescription, prismOverlayDescription));
    }
    public void FadeInThenOut(ulong tileViewId, long inDurationMs, long outDurationMs) {
      messages.Add(new FadeInThenOutMessage(tileViewId, inDurationMs, outDurationMs));
    }
    public void ShowRune(ulong tileViewId, InitialSymbol runeSymbolDescription) {
      messages.Add(new ShowRuneMessage(tileViewId, runeSymbolDescription));
    }
    public void SetOverlay(ulong tileViewId, InitialSymbol maybeOverlay) {
      messages.Add(new SetOverlayMessage(tileViewId, maybeOverlay));
    }
    public void SetFeature(ulong tileViewId, InitialSymbol maybeFeature) {
      messages.Add(new SetFeatureMessage(tileViewId, maybeFeature));
    }
    public void SetSidesColor(ulong tileViewId, IVector4Animation sideColor) {
      messages.Add(new SetSidesColorMessage(tileViewId, sideColor));
    }
    public void SetFrontColor(ulong tileViewId, IVector4Animation frontColor) {
      messages.Add(new SetFrontColorMessage(tileViewId, frontColor));
    }
    public void SetElevation(ulong tileViewId, int elevation) {
      messages.Add(new SetElevationMessage(tileViewId, elevation));
    }
    public void RemoveItem(ulong tileViewId, ulong id) {
      messages.Add(new RemoveItemMessage(tileViewId, id));
    }
    public void ClearItems(ulong tileViewId) {
      messages.Add(new ClearItemsMessage(tileViewId));
    }
    public void AddItem(ulong tileViewId, ulong itemId, InitialSymbol symbolDescription) {
      messages.Add(new AddItemMessage(tileViewId, itemId, symbolDescription));
    }
    public void DestroyTile(ulong tileViewId) {
      messages.Add(new DestroyTileMessage(tileViewId));
    }

    public void DestroyUnit(ulong unitViewId) {
      messages.Add(new DestroyUnitMessage(unitViewId));
    }

    public List<IDominoMessage> TakeMessages() {
      var copy = new List<IDominoMessage>(messages);
      messages.Clear();
      return copy;
    }
  }

  public class DominoToGameConnection {
    public GameToDominoConnection otherSide;

    public DominoToGameConnection() {
      this.otherSide = new GameToDominoConnection(this);
    }

    public List<IDominoMessage> Start(int screenGW, int screenGH) {
      otherSide.server.Start(screenGW, screenGH);
      return otherSide.TakeMessages();
    }

    public void KeyDown(int c, bool leftShiftDown, bool rightShiftDown, bool ctrlDown, bool leftAltDown, bool rightAltDown) {
      otherSide.server.KeyDown(c, leftShiftDown, rightShiftDown, ctrlDown, leftAltDown, rightAltDown);
    }
  }
}
