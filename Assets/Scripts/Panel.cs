using System.Collections.Generic;
using UnityEngine;

namespace Domino {
  // public class Panel {
  //   private GameToDominoConnection domino;
  //   private ulong panelId;
  //   private int symbolsWide;
  //   private int symbolsHigh;
  //
  //   private Dictionary<ulong, View> viewIdToDescendant;
  //
  //   public Panel(
  //       GameToDominoConnection domino,
  //       ulong panelId,
  //       int symbolsWide,
  //       int symbolsHigh) {
  //     this.domino = domino;
  //     this.panelId = panelId;
  //     this.symbolsWide = symbolsWide;
  //     this.symbolsHigh = symbolsHigh;
  //     this.viewIdToDescendant = new Dictionary<ulong, View>();
  //   }
  //   
  //   public View AddBackground(Color color, Color borderColor) {
  //     ulong id = domino.AddRectangle(panelId, 0, 0, symbolsWide, symbolsHigh, 1, color, borderColor);
  //     var view = new View(domino, this, id, symbolsWide, symbolsHigh);
  //     viewIdToDescendant.Add(id, view);
  //     return view;
  //   }
  //
  //   public void AddString(
  //       float x,
  //       float y,
  //       int maxWide,
  //       Color color,
  //       string fontName,
  //       string str) {
  //     domino.AddString(panelId, x, y, maxWide, color, fontName, str);
  //   }
  //
  //   public void Clear() {
  //     foreach (var viewId in new List<ulong>(viewIdToDescendant.Keys)) {
  //       viewIdToDescendant[viewId].Remove();
  //     }
  //   }
  //
  //   public void InnerUnregister(ulong viewId) {
  //     viewIdToDescendant.Remove(viewId);
  //   }
  // }
  //
  // public class View {
  //   private GameToDominoConnection domino;
  //   private Panel panel;
  //   private ulong panelId;
  //   private ulong viewId;
  //   private int symbolsWide;
  //   private int symbolsHigh;
  //
  //   public View(
  //       GameToDominoConnection domino,
  //       Panel panel,
  //       ulong viewId,
  //       int symbolsWide,
  //       int symbolsHigh) {
  //     this.domino = domino;
  //     this.panel = panel;
  //     this.viewId = viewId;
  //     this.symbolsWide = symbolsWide;
  //     this.symbolsHigh = symbolsHigh;
  //   }
  //
  //   public void Remove() {
  //     viewId = 0;
  //     domino.RemoveView(viewId);
  //     panel.InnerUnregister(viewId);
  //   }
  // }
}