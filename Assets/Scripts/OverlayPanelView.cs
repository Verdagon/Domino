﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Domino {
  public class OverlayFont {
    public readonly string font;
    public readonly float fontSizeMultiplier;

    public OverlayFont(
        string font,
        float fontSizeMultiplier) {
      this.font = font;
      this.fontSizeMultiplier = fontSizeMultiplier;
    }
  }

  public class OverlayPanelView : MonoBehaviour {
    private class OverlayObject {
      public readonly ulong id;
      public readonly GameObject gameObject;
      public readonly HashSet<ulong> childIds;
      public GameObject[] borderRectGameObjects = new GameObject[0];
      public FadeIn fadeIn; // Null if no fade in
      public FadeOut fadeOut; // Null if no fade out
      public Color color;
      public Color buttonPressedColor; // Or a=0 if not button
      public Color borderColor; // Or a=0 if not button

      public OverlayObject(ulong id, GameObject gameObject) {
        this.id = id;
        this.gameObject = gameObject;
        this.childIds = new HashSet<ulong>();
      }
    }

    public class FadeIn {
      public readonly long fadeInStartTimeMs;
      public readonly long fadeInEndTimeMs;

      public FadeIn(long fadeInStartTimeMs, long fadeInEndTimeMs) {
        this.fadeInStartTimeMs = fadeInStartTimeMs;
        this.fadeInEndTimeMs = fadeInEndTimeMs;

        Asserts.Assert(fadeInStartTimeMs >= 0);
        Asserts.Assert(fadeInEndTimeMs >= 0);
      }
    }
    public class FadeOut {
      public readonly long fadeOutStartTimeMs;
      public readonly long fadeOutEndTimeMs;
      public FadeOut(
          long fadeOutStartTimeMs,
          long fadeOutEndTimeMs) {
        this.fadeOutStartTimeMs = fadeOutStartTimeMs;
        this.fadeOutEndTimeMs = fadeOutEndTimeMs;

        // These times are relative to when the overlay is destroyed.
        Asserts.Assert(fadeOutStartTimeMs <= 0);
        Asserts.Assert(fadeOutEndTimeMs <= 0);
      }
    }

    public delegate void OnClicked();

    ILoader loader;
    IClock cinematicTimer;

    private ulong rootId;
    private long openTimeMs;
    private long startFadeOutTimeAfterOpenMs;
    private long finishFadeOutTimeAfterOpenMs;
    private OnClicked startFadeOutCallback;
    private OnClicked finishFadeOutCallback;

    public float panelUnityXInParent { get; private set; }
    public float panelUnityYInParent { get; private set; }
    public float symbolWidth { get; private set; }
    public float symbolHeight { get; private set; }
    public int symbolsWide { get; private set; }
    public int symbolsHigh { get; private set; }

    private Dictionary<ulong, OverlayObject> overlayObjects;
    private HashSet<ulong> fadingObjectIds;
    private Dictionary<ulong, ulong> parentIdByChildId;

    public static OverlayPanelView Create(
        ulong rootId,
        ILoader loader,
        GameObject parent,
        IClock cinematicTimer,
        float panelUnityXInParent,
        float panelUnityYInParent,
        int symbolsWide,
        int symbolsHigh,
        float symbolWidth,
        float symbolHeight) {
      var obj = loader.NewEmptyUIObject();
      var spv = obj.AddComponent<OverlayPanelView>();
      spv.Init(
          rootId,
          loader,
          cinematicTimer,
          parent,
          panelUnityXInParent,
          panelUnityYInParent,
          symbolsWide,
          symbolsHigh,
          symbolWidth,
          symbolHeight);
      //500, -500, -0, 500, 1000, -1000, -500);
      return spv;
    }

    public void Init(
        ulong rootId,
        ILoader loader,
        IClock cinematicTimer,
        GameObject parent,
        float panelUnityXInParent,
        float panelUnityYInParent,
        int symbolsWide,
        int symbolsHigh,
        float symbolWidth,
        float symbolHeight) {
      this.rootId = rootId;
      this.loader = loader;
      this.panelUnityXInParent = panelUnityXInParent;
      this.panelUnityYInParent = panelUnityYInParent;
      this.symbolWidth = symbolWidth;
      this.symbolHeight = symbolHeight;
      this.symbolsWide = symbolsWide;
      this.symbolsHigh = symbolsHigh;
      parentIdByChildId = new Dictionary<ulong, ulong>();
      overlayObjects = new Dictionary<ulong, OverlayObject>();
      overlayObjects.Add(rootId, new OverlayObject(rootId, gameObject));
      fadingObjectIds = new HashSet<ulong>();

      this.cinematicTimer = cinematicTimer;
      this.openTimeMs = cinematicTimer.GetTimeMs();
      this.startFadeOutTimeAfterOpenMs = long.MaxValue;
      this.finishFadeOutTimeAfterOpenMs = long.MaxValue;

      var panelRectTransform = gameObject.GetComponent<RectTransform>();
      panelRectTransform.SetParent(parent.transform, false);
      panelRectTransform.SetAsFirstSibling(); // Move to back
      panelRectTransform.anchorMin = new Vector2(0, 0);
      panelRectTransform.anchorMax = new Vector2(0, 0);
      panelRectTransform.localScale = new Vector3(1, 1, 1);
      panelRectTransform.pivot = new Vector2(0, 0);
      panelRectTransform.anchoredPosition = new Vector2(panelUnityXInParent, panelUnityYInParent);
      panelRectTransform.sizeDelta = new Vector2(symbolsWide * symbolWidth, symbolsHigh * symbolHeight);
    }

    public void SetFadeIn(ulong id, FadeIn fadeIn) {
      Asserts.Assert(overlayObjects.ContainsKey(id));
      var overlayObject = overlayObjects[id];
      overlayObject.fadeIn = fadeIn;
      fadingObjectIds.Add(id);
      SetOpacity(overlayObject, 0);
    }

    public void SetFadeOut(ulong id, FadeOut fadeOut) {
      Asserts.Assert(overlayObjects.ContainsKey(id));
      var overlayObject = overlayObjects[id];
      overlayObject.fadeOut = fadeOut;
      fadingObjectIds.Add(id);
      UpdateOpacity(overlayObject);
    }

    public void AddSymbol(
        ulong newViewId,
        ulong parentId,
        float x,
        float y,
        float size,
        int z,
        Color color,
        SymbolId symbol) {
      AddSymbol(newViewId, parentId, x, y, size, z, color, symbol, false);
    }
    
    public void AddSymbol(
        ulong newViewId,
        ulong parentId,
        float x,
        float y,
        float size,
        int z,
        Color color,
        SymbolId symbol,
        bool centered) {
      var unityX = x * symbolWidth + (centered ? symbolWidth / 2 : 0);
      var unityY = y * symbolHeight + (centered ? symbolHeight / 2 : 0);
      var textGameObject = loader.NewEmptyUIObject();
      textGameObject.transform.SetParent(gameObject.transform, false);
      textGameObject.transform.localScale = new Vector3(1, 1, 1);
      textGameObject.transform.localPosition = new Vector3(0, 0, 0);
      var rectTransform = textGameObject.GetComponent<RectTransform>();
      rectTransform.localPosition = new Vector3(0, 0, 0);
      // .5s here, and * 2 on font size, so unity renders it with more resolution.
      rectTransform.localScale = new Vector3(.5f, .5f, 1);
      rectTransform.pivot = centered ? new Vector2(0.5f, 0.5f) : new Vector2(0, 0);
      rectTransform.anchorMin = new Vector2(0, 0);
      rectTransform.anchorMax = new Vector2(0, 0);
      rectTransform.anchoredPosition = new Vector2(unityX, unityY);
      var textView = textGameObject.AddComponent<Text>();
      textView.raycastTarget = false;
      textView.font = loader.LoadFont(symbol.fontName);
      textView.alignment = centered ? TextAnchor.MiddleCenter : TextAnchor.LowerLeft;
      // * 2 and then we scale by .5 so that unity renders it with more resolution.
      float widthToHeightRatio = symbolWidth / symbolHeight;
      const int FONT_SIZE_MULTIPLIER = 2;
      textView.fontSize = (int)(symbolHeight * size * FONT_SIZE_MULTIPLIER * widthToHeightRatio * 2);
      textView.color = color;
      textView.resizeTextForBestFit = false;
      //textView.
      rectTransform.position = new Vector3(rectTransform.position.x, rectTransform.position.y, z);

      textView.text = char.ConvertFromUtf32(symbol.unicode);

      var overlayObject = new OverlayObject(newViewId, textGameObject);
      overlayObject.color = color;
      overlayObjects.Add(newViewId, overlayObject);
      parentIdByChildId.Add(newViewId, parentId);
      overlayObjects[parentId].childIds.Add(newViewId);
    }

    public void AddRectangle(
        ulong newViewId,
        ulong parentId,
        float x,
        float y,
        float width,
        float height,
        int z,
        Color color,
        Color borderColor) {
      var unityX = x * symbolWidth;
      var unityY = y * symbolHeight;
      var unityWidth = width * symbolWidth;
      var unityHeight = height * symbolHeight;
      AddRectangleUnityCoords(newViewId, parentId, unityX, unityY, unityWidth, unityHeight, z, color, borderColor);
    }

    private void AddRectangleUnityCoords(
        ulong newViewId,
        ulong parentId,
        float unityX,
        float unityY,
        float unityWidth,
        float unityHeight,
        int z,
        Color color,
        Color borderColor) { 

      float borderSize = symbolWidth / 4;

      GameObject[] borderRectGameObjects = new GameObject[0];
      if (borderColor.a > 0) {
        borderRectGameObjects = new GameObject[4];
        for (int i = 0; i < 4; i++) {
          float borderX;
          float borderY;
          float borderWidth;
          float borderHeight;
          if (i == 0) {
            borderX = unityX - borderSize;
            borderY = unityY - borderSize;
            borderWidth = borderSize;
            borderHeight = unityHeight + borderSize * 2;
          } else if (i == 1) {
            borderX = unityX - borderSize;
            borderY = unityY - borderSize;
            borderWidth = unityWidth + borderSize * 2;
            borderHeight = borderSize;
          } else if (i == 2) {
            borderX = unityX + unityWidth;
            borderY = unityY - borderSize;
            borderWidth = borderSize;
            borderHeight = unityHeight + borderSize * 2;
          } else {
            borderX = unityX - borderSize;
            borderY = unityY + unityHeight;
            borderWidth = unityWidth + borderSize * 2;
            borderHeight = borderSize;
          }

          var borderRectGameObject = loader.NewEmptyUIObject();
          borderRectGameObject.transform.SetParent(gameObject.transform, false);
          var borderRectTransform = borderRectGameObject.GetComponent<RectTransform>();
          borderRectTransform.pivot = new Vector2(0, 0);
          borderRectTransform.localScale = new Vector3(1, 1, 1);
          borderRectTransform.anchorMin = new Vector2(0, 0);
          borderRectTransform.anchorMax = new Vector2(0, 0);
          borderRectTransform.anchoredPosition = new Vector2(borderX, borderY);
          borderRectTransform.position = new Vector3(borderRectTransform.position.x, borderRectTransform.position.y, z);
          borderRectTransform.sizeDelta = new Vector2(borderWidth, borderHeight);
          var borderRectImage = borderRectGameObject.AddComponent<Image>();
          borderRectImage.color = borderColor;
          borderRectGameObjects[i] = borderRectGameObject;
        }
      }

      var rectGameObject = loader.NewEmptyUIObject();
      rectGameObject.transform.SetParent(gameObject.transform, false);
      var rectTransform = rectGameObject.GetComponent<RectTransform>();
      rectTransform.pivot = new Vector2(0, 0);
      rectTransform.localScale = new Vector3(1, 1, 1);
      rectTransform.anchorMin = new Vector2(0, 0);
      rectTransform.anchorMax = new Vector2(0, 0);
      rectTransform.anchoredPosition = new Vector2(unityX, unityY);
      rectTransform.position = new Vector3(rectTransform.position.x, rectTransform.position.y, z);
      rectTransform.sizeDelta = new Vector2(unityWidth, unityHeight);
      var image = rectGameObject.AddComponent<Image>();
      image.color = color;

      var overlayObject = new OverlayObject(newViewId, rectGameObject);
      overlayObject.color = color;
      overlayObject.borderRectGameObjects = borderRectGameObjects;
      overlayObject.borderColor = borderColor;
      overlayObjects.Add(newViewId, overlayObject);
      parentIdByChildId.Add(newViewId, parentId);
      overlayObjects[parentId].childIds.Add(newViewId);

      Asserts.Assert(overlayObjects.ContainsKey(newViewId));
    }

    public void AddFullscreenRect(ulong newViewId, Color color) {
      var parent = gameObject.transform.parent.gameObject;
      var parentRectTransform = parent.GetComponent<RectTransform>();
      var parentWidth = parentRectTransform.rect.width;
      var parentHeight = parentRectTransform.rect.height;

      var panelRectTransform = gameObject.GetComponent<RectTransform>();

      AddRectangleUnityCoords(
        newViewId,
        rootId,
        -panelRectTransform.anchoredPosition.x,
        -panelRectTransform.anchoredPosition.y,
        parentWidth,
        parentHeight,
        1,
        color,
        new Color(0, 0, 0, 0));
    }

    public void AddButton(
        ulong newViewId,
        ulong parentId,
        float x,
        float y,
        float width,
        float height,
        int z,
        Color color,
        Color borderColor,
        Color pressedColor,
        OnClicked onClicked,
        OnClicked onMouseIn,
        OnClicked onMouseOut) {
      AddRectangle(newViewId, parentId, x, y, width, height, z, color, borderColor);
      var overlayObject = overlayObjects[newViewId];

      var button = overlayObject.gameObject.AddComponent<Button>();
      button.onClick.AddListener(() => onClicked());
      var colors = new ColorBlock();
      colors.colorMultiplier = 1;
      colors.normalColor = color;
      colors.pressedColor = pressedColor;
      colors.highlightedColor = color;
      colors.selectedColor = color;
      colors.disabledColor = color;
      button.colors = colors;
      var uiClickListener = overlayObject.gameObject.AddComponent<UIClickListener>();
      uiClickListener.MouseEnter += () => onMouseIn();
      uiClickListener.MouseExit += () => onMouseOut();

      overlayObject.buttonPressedColor = pressedColor;
    }

    public void AddBackground(ulong newiewId, Color color, Color borderColor) {
      AddRectangle(newiewId, rootId, 0, 0, symbolsWide, symbolsHigh, 1, color, borderColor);
    }

    //public ulong AddBackgroundAndBorder(Color backgroundColor, Color borderColor) {
    //  int borderId = AddBackground(borderColor);
    //  AddRectangle(borderId, .5f, .5f, symbolsWide - 1, symbolsHigh - 1, 1, backgroundColor);
    //  return borderId;
    //}

    public void AddString(List<ulong> ids, ulong parentId, float x, float y, int maxWide, Color color, string fontName, string str) {
      for (int i = 0; i < str.Length; i++) {
        AddSymbol(ids[i], parentId, x + i, y, 1f, 1, color, new SymbolId(fontName, char.ConvertToUtf32(str[i].ToString(), 0)));
      }
    }

    public void Update() {
      if (loader == null) {
        // We aren't initialized yet
        return;
      }
      Asserts.Assert(fadingObjectIds != null);
      foreach (var id in new HashSet<ulong>(fadingObjectIds))
        UpdateOpacity(overlayObjects[id]);

      var timeSinceOpenMs = cinematicTimer.GetTimeMs() - openTimeMs;
      if (timeSinceOpenMs >= startFadeOutTimeAfterOpenMs) {
        // If we're closing, and we have a startFadeOutCallback, call it.
        if (startFadeOutCallback != null) {
          var cb = startFadeOutCallback;
          startFadeOutCallback = null;
          cb();
        }

        if (fadingObjectIds.Count == 0) {
          if (finishFadeOutCallback != null) {
            var cb = finishFadeOutCallback;
            finishFadeOutCallback = null;
            cb();
          }

          Destroy(gameObject);
        }
      }
    }

    private void UpdateOpacity(OverlayObject overlayObject) {
      var timeSinceOpenMs = cinematicTimer.GetTimeMs() - openTimeMs;

      if (overlayObject.fadeIn != null) {
        var fadeIn = overlayObject.fadeIn;
        if (timeSinceOpenMs < fadeIn.fadeInStartTimeMs) {
          // Do nothing, they should already be transparent, from SetFadeIn.
        } else if (timeSinceOpenMs < fadeIn.fadeInEndTimeMs) {
          var ratio = (float)(timeSinceOpenMs - fadeIn.fadeInStartTimeMs) / (fadeIn.fadeInEndTimeMs - fadeIn.fadeInStartTimeMs);
          SetOpacity(overlayObject, ratio);
        } else {
          // Not expensive because we remove it afterwards and never see it again.
          SetOpacity(overlayObject, 1);
          overlayObject.fadeIn = null;
        }
      }

      if (timeSinceOpenMs >= startFadeOutTimeAfterOpenMs) {
        if (overlayObject.fadeOut != null) {
          var fadeOut = overlayObject.fadeOut;
          // Remember, fadeOut.fadeOutStart/EndTimeS are negative.
          var fadeOutStartTimeMs = finishFadeOutTimeAfterOpenMs + fadeOut.fadeOutStartTimeMs;
          var fadeOutEndTimeMs = finishFadeOutTimeAfterOpenMs + fadeOut.fadeOutEndTimeMs;

          if (timeSinceOpenMs < fadeOutStartTimeMs) {
            // Do nothing, they should already be opaque.
          } else if (timeSinceOpenMs < fadeOutEndTimeMs) {
            var ratio = 1 - (float)(timeSinceOpenMs - fadeOutStartTimeMs) / (fadeOutEndTimeMs - fadeOutStartTimeMs);
            SetOpacity(overlayObject, ratio);
          } else {
            // Not expensive because we remove it afterwards and never see it again.
            SetOpacity(overlayObject, 0);
            overlayObject.fadeOut = null;
          }
        }
      }

      if (overlayObject.fadeIn == null && overlayObject.fadeOut == null) {
        fadingObjectIds.Remove(overlayObject.id);
      }
    }

    public void SetOpacity(ulong id, float ratio) {
      Asserts.Assert(overlayObjects.ContainsKey(id));
      SetOpacity(overlayObjects[id], ratio);
    }

    private void SetOpacity(OverlayObject overlayObject, float ratio) {
      var overlayObjectGameObject = overlayObject.gameObject;

      var text = overlayObjectGameObject.GetComponent<Text>();
      if (text != null) {
        var fadedTextColor = overlayObject.color;
        fadedTextColor.a *= ratio;
        text.color = fadedTextColor;
      }

      var image = overlayObjectGameObject.GetComponent<Image>();
      if (image != null) {
        var fadedBackgroundColor = overlayObject.color;
        fadedBackgroundColor.a *= ratio;
        image.color = fadedBackgroundColor;
      }

      foreach (var borderRectGameObject in overlayObject.borderRectGameObjects) {
        var borderImage = borderRectGameObject.GetComponent<Image>();
        if (borderImage != null) {
          var fadedBackgroundColor = overlayObject.borderColor;
          fadedBackgroundColor.a *= ratio;
          borderImage.color = fadedBackgroundColor;
        }
      }

      var button = overlayObjectGameObject.GetComponent<UnityEngine.UI.Button>();
      if (button != null) {
        var color = overlayObject.color;
        color.a = ratio;

        var buttonColors = button.colors;
        buttonColors.normalColor = color;
        buttonColors.disabledColor = color;
        buttonColors.highlightedColor = color;
        button.colors = buttonColors;
      }

      foreach (var childId in overlayObject.childIds) {
        SetOpacity(overlayObjects[childId], ratio);
      }
    }

    public void Remove(ulong id) {
      if (id == rootId) {
        while (overlayObjects.Count > 1) {
          ulong first = 0;
          foreach (var entry in overlayObjects) {
            if (entry.Key != 0) {
              first = entry.Key;
              break;
            }
          }
          Asserts.Assert(first != 0);
          Remove(first);
        }
      } else {
        Asserts.Assert(parentIdByChildId.ContainsKey(id));
        Asserts.Assert(overlayObjects.ContainsKey(id));
        var overlayObject = overlayObjects[id];

        foreach (var childId in overlayObject.childIds) {
          Remove(childId);
        }

        Asserts.Assert(overlayObjects[id].childIds.Count == 0);

        Destroy(overlayObject.gameObject);
        foreach (var borderRectGameObject in overlayObject.borderRectGameObjects) {
          Destroy(borderRectGameObject);
        }
        overlayObjects.Remove(id);

        ulong parentId = parentIdByChildId[id];
        parentIdByChildId.Remove(id);

        overlayObjects[parentId].childIds.Remove(id);
      }
    }

    //public void OnButtonClick(ulong id) {
    //  Clicked?.Invoke(id);
    //}

    //public void Cancel() {
    //  gameObject.SetActive(false);
    //}

    public void SetOnStartHideCallback(OnClicked startFadeOutCallback) {
      this.startFadeOutCallback = startFadeOutCallback;
    }

    public void SetOnFinishHideCallback(OnClicked finishFadeOutCallback) {
      this.finishFadeOutCallback = finishFadeOutCallback;
    }

    public void ScheduleClose(long startMsFromNow) {
      var nowMs = cinematicTimer.GetTimeMs();
      var timeSinceOpenMs = nowMs - openTimeMs;
      startFadeOutTimeAfterOpenMs = timeSinceOpenMs + startMsFromNow;

      long earliestFadeOutStart = 0;
      foreach (var id in fadingObjectIds) {
        earliestFadeOutStart = Math.Min(earliestFadeOutStart, overlayObjects[id].fadeOut.fadeOutStartTimeMs);
      }
      Asserts.Assert(earliestFadeOutStart <= 0); // fade outs are phrased as negative numbers where 0 is final close time.
      long allFadeOutsDuration = -earliestFadeOutStart;
      finishFadeOutTimeAfterOpenMs = startFadeOutTimeAfterOpenMs + allFadeOutsDuration;
    }
  }
}
