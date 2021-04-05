using System.Collections;
using System.Collections.Generic;
using AthPlayer;
using Domino;
using UnityEngine;
using UnityEngine.UI;

namespace Geomancer {
  public class ListView {
    public class Entry {
      public SymbolId symbol;
      public string text;

      public Entry(SymbolId symbol, string text) {
        this.symbol = symbol;
        this.text = text;
      }
    }

    private GameToDominoConnection domino;
    private Panel panel;
    private readonly int viewGW, viewGH;

    //IClock cinematicTimer;
    //OverlayPaneler overlayPaneler;
    // OverlayPanelView view;

    public ListView(
        GameToDominoConnection domino,
        ulong viewId,
        int x,
        int y,
        int viewGW,
        int viewGH) {//OverlayPanelView view) {
      this.domino = domino;
      this.viewGW = viewGW;
      this.viewGH = viewGH;
      this.panel = domino.MakePanel(x, y, viewGW, viewGH);
      //this.cinematicTimer = cinematicTimer;
      //this.overlayPaneler = overlayPaneler;
    }

    public void ShowEntries(List<Entry> entries) {
      panel.Clear();

      if (entries.Count > 0) {
        panel.AddBackground(new UnityEngine.Color(0, 0, 0, .9f), new UnityEngine.Color(0, 0, 0, 0));

        for (int i = 0; i < entries.Count; i++) {
          // view.AddSymbol(0, 1, view.symbolsHigh - (i * 2 + 2), 2.0f, 0, new UnityEngine.Color(1, 1, 1), entries[i].symbol);
          panel.AddString(5, viewGH - (i * 2 + 2 - 0.5f), viewGW - 3, new UnityEngine.Color(1, 1, 1), Fonts.PROSE_OVERLAY_FONT, entries[i].text);
        }
      }
    }
  }
}
