using System.Collections.Generic;
using Geomancer.Model;
using UnityEngine;

namespace Domino {
  public interface IDominoMessage {
  }

  public class SetupGameMessage : IDominoMessage {
    public readonly Vec3 cameraPosition;
    public readonly float elevationStepHeight;
    public readonly Pattern pattern;

    public SetupGameMessage(
        Vec3 cameraPosition,
        float elevationStepHeight,
        Pattern pattern) {
      this.cameraPosition = cameraPosition;
      this.elevationStepHeight = elevationStepHeight;
      this.pattern = pattern;
    }
  }

  public class CreateTileMessage : IDominoMessage {
    public readonly ulong id;
    public readonly InitialTile initialTile;

    public CreateTileMessage(ulong id, InitialTile initialTile) {
      this.id = id;
      this.initialTile = initialTile;
    }
  }

  public class CreateUnitMessage : IDominoMessage {
    public readonly ulong id;
    public readonly InitialUnit initialUnit;

    public CreateUnitMessage(ulong id, InitialUnit initialUnit) {
      this.id = id;
      this.initialUnit = initialUnit;
    }
  }

  public class MakePanelMessage : IDominoMessage {
    public readonly ulong id;
    public readonly int panelGXInScreen;
    public readonly int panelGYInScreen;
    public readonly int panelGW;
    public readonly int panelGH;

    public MakePanelMessage(
        ulong id,
        int panelGXInScreen,
        int panelGYInScreen,
        int panelGW,
        int panelGH) {
      this.id = id;
      this.panelGXInScreen = panelGXInScreen;
      this.panelGYInScreen = panelGYInScreen;
      this.panelGW = panelGW;
      this.panelGH = panelGH;
    }
  }

  class ScheduleCloseMessage : IDominoMessage {
    public readonly ulong viewId;
    public readonly long startMsFromNow;

    public ScheduleCloseMessage(ulong viewId, long startMsFromNow) {
      this.viewId = viewId;
      this.startMsFromNow = startMsFromNow;
    }
  }

  class RemoveMessage : IDominoMessage {
    public readonly ulong viewId;
    public readonly int id;

    public RemoveMessage(ulong viewId, int id) {
      this.viewId = viewId;
      this.id = id;
    }
  }

  class SetOpacityMessage : IDominoMessage {
    public readonly ulong viewId;
    public readonly int id;
    public readonly float ratio;

    public SetOpacityMessage(ulong viewId, int id, float ratio) {
      this.viewId = viewId;
      this.id = id;
      this.ratio = ratio;
    }
  }

  class SetFadeOutMessage : IDominoMessage {
    public readonly ulong id;
    public readonly OverlayPanelView.FadeOut fadeOut;

    public SetFadeOutMessage(ulong id, OverlayPanelView.FadeOut fadeOut) {
      this.id = id;
      this.fadeOut = fadeOut;
    }
  }

  class SetFadeInMessage : IDominoMessage {
    public readonly ulong id;
    public readonly OverlayPanelView.FadeIn fadeIn;

    public SetFadeInMessage(ulong id, OverlayPanelView.FadeIn fadeIn) {
      this.id = id;
      this.fadeIn = fadeIn;
    }
  }

  class AddStringMessage : IDominoMessage {
    public readonly List<ulong> newViewsIds;
    public readonly ulong parentViewId;
    public readonly float x;
    public readonly float y;
    public readonly int maxWide;
    public readonly Color color;
    public readonly string fontName;
    public readonly string str;

    public AddStringMessage(
        List<ulong> newViewsIds,
        ulong parentViewId,
        float x,
        float y,
        int maxWide,
        Color color,
        string fontName,
        string str) {
      this.newViewsIds = newViewsIds;
      this.parentViewId = parentViewId;
      this.x = x;
      this.y = y;
      this.maxWide = maxWide;
      this.color = color;
      this.fontName = fontName;
      this.str = str;
    }
  }

  class AddBackgroundMessage : IDominoMessage {
    public readonly ulong newViewId;
    public readonly ulong parentViewId;
    public readonly Color color;
    public readonly Color borderColor;

    public AddBackgroundMessage(ulong newViewId, ulong parentViewId, Color color, Color borderColor) {
      this.newViewId = newViewId;
      this.parentViewId = parentViewId;
      this.color = color;
      this.borderColor = borderColor;
    }
  }

  class AddButtonMessage : IDominoMessage {
    public readonly ulong newViewId;
    public readonly ulong parentViewId;
    public readonly float x;
    public readonly float y;
    public readonly float width;
    public readonly float height;
    public readonly int z;
    public readonly Color color;
    public readonly Color borderColor;
    public readonly Color pressedColor;
    public readonly ulong onClicked;
    public readonly ulong onMouseIn;
    public readonly ulong onMouseOut;

    public AddButtonMessage(
        ulong newViewId,
        ulong parentViewId,
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
      this.newViewId = newViewId;
      this.parentViewId = parentViewId;
      this.x = x;
      this.y = y;
      this.width = width;
      this.height = height;
      this.z = z;
      this.color = color;
      this.borderColor = borderColor;
      this.pressedColor = pressedColor;
      this.onClicked = onClicked;
      this.onMouseIn = onMouseIn;
      this.onMouseOut = onMouseOut;
    }
  }

  class AddFullscreenRectMessage : IDominoMessage {
    public readonly ulong newViewId;
    public readonly ulong parentViewId;
    public readonly Color color;

    public AddFullscreenRectMessage(ulong newViewId, ulong parentViewId, Color color) {
      this.newViewId = newViewId;
      this.parentViewId = parentViewId;
      this.color = color;
    }
  }

  class AddRectangleMessage : IDominoMessage {
    public readonly ulong newViewId;
    public readonly ulong parentViewId;
    public readonly float x;
    public readonly float y;
    public readonly float width;
    public readonly float height;
    public readonly int z;
    public readonly Color color;
    public readonly Color borderColor;

    public AddRectangleMessage(
        ulong newViewId,
        ulong parentViewId,
        float x,
        float y,
        float width,
        float height,
        int z,
        Color color,
        Color borderColor) {
      this.newViewId = newViewId;
      this.parentViewId = parentViewId;
      this.x = x;
      this.y = y;
      this.width = width;
      this.height = height;
      this.z = z;
      this.color = color;
      this.borderColor = borderColor;
    }
  }

  class AddSymbolMessage : IDominoMessage {
    public readonly ulong newViewId;
    public readonly ulong parentViewId;
    public readonly float x;
    public readonly float y;
    public readonly float size;
    public readonly int z;
    public readonly Color color;
    public readonly SymbolId symbol;
    public readonly bool centered;

    public AddSymbolMessage(
        ulong newViewId,
        ulong parentViewId,
        float x,
        float y,
        float size,
        int z,
        Color color,
        SymbolId symbol,
        bool centered) {
      this.newViewId = newViewId;
      this.parentViewId = parentViewId;
      this.x = x;
      this.y = y;
      this.size = size;
      this.z = z;
      this.color = color;
      this.symbol = symbol;
      this.centered = centered;
    }
  }
  
  public class ShowPrismMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public readonly InitialSymbol prismDescription;
    public readonly InitialSymbol prismOverlayDescription;
  
    public ShowPrismMessage(ulong tileViewId, InitialSymbol prismDescription, InitialSymbol prismOverlayDescription) {
      this.tileViewId = tileViewId;
      this.prismDescription = prismDescription;
      this.prismOverlayDescription = prismOverlayDescription;
  
    }
  }
  public class FadeInThenOutMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public readonly long inDurationMs;
    public readonly long outDurationMs;
  
    public FadeInThenOutMessage(ulong tileViewId, long inDurationMs, long outDurationMs) {
      this.tileViewId = tileViewId;
      this.inDurationMs = inDurationMs;
      this.outDurationMs = outDurationMs;
    }
  }
  public class ShowRuneMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public readonly InitialSymbol runeSymbolDescription;
  
    public ShowRuneMessage(ulong tileViewId, InitialSymbol runeSymbolDescription) {
      this.tileViewId = tileViewId;
      this.runeSymbolDescription = runeSymbolDescription; 
    }
  }
  public class SetOverlayMessage : IDominoMessage {
    public readonly ulong tileViewId;
    InitialSymbol maybeOverlay;
  
    public SetOverlayMessage(ulong tileViewId, InitialSymbol maybeOverlay) {
      this.tileViewId = tileViewId;
      this.maybeOverlay = maybeOverlay; 
    }
  }
  public class SetFeatureMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public readonly InitialSymbol maybeFeature;
  
    public SetFeatureMessage(ulong tileViewId, InitialSymbol maybeFeature) {
      this.tileViewId = tileViewId;
      this.maybeFeature = maybeFeature; 
    }
  }
  public class SetCliffColorMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public readonly IVector4Animation sideColor;
  
    public SetCliffColorMessage(ulong tileViewId, IVector4Animation sideColor) {
      this.tileViewId = tileViewId;
      this.sideColor = sideColor; 
    }
  }
  public class SetSurfaceColorMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public readonly IVector4Animation frontColor;
  
    public SetSurfaceColorMessage(ulong tileViewId, IVector4Animation frontColor) {
      this.tileViewId = tileViewId;
      this.frontColor = frontColor;
    }
  }
  public class SetElevationMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public readonly int elevation;
  
    public SetElevationMessage(ulong tileViewId, int elevation) {
      this.tileViewId = tileViewId;
      this.elevation = elevation;
    }
  }
  public class RemoveItemMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public readonly ulong itemId;
  
    public RemoveItemMessage(ulong tileViewId, ulong itemId) {
      this.tileViewId = tileViewId;
      this.itemId = itemId;
    }
  }
  public class ClearItemsMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public ClearItemsMessage(ulong tileViewId) {
      this.tileViewId = tileViewId;
    }
  }
  public class AddItemMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public readonly ulong id;
    public readonly InitialSymbol symbolDescription;
  
    public AddItemMessage(ulong tileViewId, ulong id, InitialSymbol symbolDescription) {
      this.tileViewId = tileViewId;
      this.id = id;
      this.symbolDescription = symbolDescription;
  
    }
  }
  public class DestroyTileMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public DestroyTileMessage(ulong tileViewId) {
      this.tileViewId = tileViewId;
    }
  }
  public class DestroyUnitMessage : IDominoMessage {
    public readonly ulong unitViewId;
    public DestroyUnitMessage(ulong unitViewId) {
      this.unitViewId = unitViewId;
    }
  }
}