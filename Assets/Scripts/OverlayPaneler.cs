﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Domino {
  public class OverlayPaneler {
    GameObject panelRootGameObject;
    ILoader loader;
    IClock cinematicTimer;
    // U = in unity units
    // G = in grid units
    readonly float gridOriginUXInScreen;
    readonly float gridOriginUYInScreen;
    readonly float symbolWidth;
    readonly float symbolHeight;
    public readonly int screenGW;
    public readonly int screenGH;

    public OverlayPaneler(
        GameObject panelRootGameObject,
        ILoader loader,
        IClock cinematicTimer) {
      this.panelRootGameObject = panelRootGameObject;
      this.loader = loader;
      this.cinematicTimer = cinematicTimer;

      var parentRectTransform = panelRootGameObject.GetComponent<RectTransform>();
      var parentWidth = parentRectTransform.rect.width;
      var parentHeight = parentRectTransform.rect.height;

      symbolWidth = 12;
      symbolHeight = 18;

      screenGW = (int)(parentWidth / symbolWidth);
      screenGH = (int)(parentHeight / symbolHeight);

      //Debug.LogError("parent w " + parentWidth + " h " + parentHeight + " gw " + screenGW + " gh " + screenGH);

      float gridUnityWidth = screenGW * symbolWidth;
      float gridUnityHeight = screenGH * symbolHeight;
      gridOriginUXInScreen = (parentWidth - gridUnityWidth) / 2;
      gridOriginUYInScreen = (parentHeight - gridUnityHeight) / 2;
    }
    
    // G = in grid units
    public OverlayPanelView MakePanel(
        ulong rootId,
        int panelGXInScreen,
        int panelGYInScreen,
        int panelGW,
        int panelGH) {
      var spv =
          OverlayPanelView.Create(
              rootId,
              loader,
          panelRootGameObject,
          cinematicTimer,
          panelGXInScreen * symbolWidth + gridOriginUXInScreen,
          panelGYInScreen * symbolHeight + gridOriginUYInScreen,
          panelGW,
          panelGH,
          symbolWidth,
          symbolHeight);
      return spv;
    }
  }
}