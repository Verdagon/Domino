using System.Collections;
using System.Collections.Generic;

using AthPlayer;
using Domino;
using UnityEngine;
using UnityEngine.UI;

namespace Geomancer {
  public class LookPanelView {
    GameToDominoConnection domino;
    private int screenGW;
    ulong visibleOverlayPanelView;
    // In AthPlayer, the status view is below, so our LookPanelView is at 2 Y.
    // In Editor, our LookPanelView is at the bottom, so has 0 Y.
    int panelGYInScreen;
    // In AthPlayer, the status view has padding on top of it, so our LookPanelView needs 0 bottom padding.
    // In Editor, the status view is at the bottom of the screen, so needs 1 bottom padding.
    int bottomPadding;

    public LookPanelView(GameToDominoConnection domino, int screenGW, int panelGYInScreen, int bottomPadding) {
      this.domino = domino;
      this.screenGW = screenGW;
      this.panelGYInScreen = panelGYInScreen;
      this.bottomPadding = bottomPadding;
    }

    public void ShowMessage(string message) {
      SetStuff(true, message, "", new List<KeyValuePair<SymbolDescription, string>>());
    }
    public void ClearMessage() {
      if (visibleOverlayPanelView != 0) {
        domino.ScheduleClose(visibleOverlayPanelView, 0);
        visibleOverlayPanelView = 0;
      }
    }
    public void SetStuff(
        bool visible,
        string message,
        string status,
        List<KeyValuePair<SymbolDescription, string>> symbolsAndLabels) {
      ClearMessage();
      if (!visible) {
        return;
      }

      int topPadding = 1;
      int contentYStart = this.bottomPadding;
      // 1 line of bottom padding, 1 line of text, 1 padding between, 1 line of text
      int panelGH = bottomPadding + 1 + 1 + 1 + topPadding;


      int panelGXInScreen = 0;
      visibleOverlayPanelView =
        domino.MakePanel(panelGXInScreen, panelGYInScreen, screenGW, panelGH);
      domino.AddRectangle(
        visibleOverlayPanelView,
        0,
        -1,
        0,
        1 + screenGW + 1,
        panelGH,
        1,
        new UnityEngine.Color(0, 0, 0, .85f), new UnityEngine.Color(0, 0, 0, 0));

      int buttonsWidth = 3;

      if (status.Length == 0 && symbolsAndLabels.Count == 0) {
        var lines = LineWrapper.Wrap(message, screenGW - 2 - buttonsWidth);
        for (int i = 0; i < lines.Length; i++) {
          domino.AddString(visibleOverlayPanelView, 0, 1, contentYStart + 2 - i, screenGW - 2, new UnityEngine.Color(1, 1, 1, 1), Fonts.PROSE_OVERLAY_FONT, lines[i]);
        }
      } else {
        domino.AddString(visibleOverlayPanelView, 0, 1, contentYStart + 2, screenGW - 2, new UnityEngine.Color(1, 1, 1, 1), Fonts.PROSE_OVERLAY_FONT, message);
        domino.AddString(visibleOverlayPanelView, 0, screenGW - buttonsWidth - 1 - status.Length, contentYStart + 2, screenGW - 20 - 2, new UnityEngine.Color(1, 1, 1, 1), Fonts.PROSE_OVERLAY_FONT, status);
        domino.SetFadeIn(visibleOverlayPanelView, new OverlayPanelView.FadeIn(0, 100));
        domino.SetFadeOut(visibleOverlayPanelView, new OverlayPanelView.FadeOut(-200, 0));

        int x = 0;
        foreach (var symbolAndLabel in symbolsAndLabels) {
          x += 1; // Left margin

          var symbol = symbolAndLabel.Key;
          var label = symbolAndLabel.Value;
          //visibleOverlayPanelView.AddSymbol(0, x, contentYStart, 1f, 1, symbol.frontColor.Get(long.MaxValue), symbol.symbolId, false);
          x += 2; // Symbol takes up a lot of space

          domino.AddString(visibleOverlayPanelView, 0, x, contentYStart, 20, new UnityEngine.Color(1, 1, 1, 1), Fonts.PROSE_OVERLAY_FONT, label);
          x += label.Length;

          x += 1; // Right margin
        }
      }
    }
  }
}
