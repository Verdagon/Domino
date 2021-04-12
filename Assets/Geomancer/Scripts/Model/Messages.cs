using System.Collections.Generic;
using Geomancer.Model;

namespace Domino {
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

  public interface IDominoMessage {
  }

  public class SetupGameMessage : IDominoMessage {
    public readonly Vec3 cameraPosition;
    public readonly Vec3 lookatOffsetToCamera;
    public readonly int elevationStepHeight;
    public readonly Pattern pattern;

    public SetupGameMessage(
        Vec3 cameraPosition,
        Vec3 lookatOffsetToCamera,
        int elevationStepHeight,
        Pattern pattern) {
      this.cameraPosition = cameraPosition;
      this.lookatOffsetToCamera = lookatOffsetToCamera;
      this.elevationStepHeight = elevationStepHeight;
      this.pattern = pattern;
    }
  }

  public class CreateTileMessage : IDominoMessage {
    public readonly ulong newTileId;
    public readonly InitialTile initialTile;

    public CreateTileMessage(ulong newTileId, InitialTile initialTile) {
      this.newTileId = newTileId;
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

  public class ScheduleCloseMessage : IDominoMessage {
    public readonly ulong viewId;
    public readonly long startMsFromNow;

    public ScheduleCloseMessage(ulong viewId, long startMsFromNow) {
      this.viewId = viewId;
      this.startMsFromNow = startMsFromNow;
    }
  }

  public class RemoveViewMessage : IDominoMessage {
    // public readonly ulong panelId;
    public readonly ulong viewId;

    public RemoveViewMessage(ulong viewId) {
      // this.panelId = panelId;
      this.viewId = viewId;
    }
  }

  public class SetOpacityMessage : IDominoMessage {
    public readonly ulong viewId;
    public readonly int id;
    public readonly int ratio;

    public SetOpacityMessage(ulong viewId, int id, int ratio) {
      this.viewId = viewId;
      this.id = id;
      this.ratio = ratio;
    }
  }

  public class SetFadeOutMessage : IDominoMessage {
    public readonly ulong id;
    public readonly FadeOut fadeOut;

    public SetFadeOutMessage(ulong id, FadeOut fadeOut) {
      this.id = id;
      this.fadeOut = fadeOut;
    }
  }

  public class SetFadeInMessage : IDominoMessage {
    public readonly ulong id;
    public readonly FadeIn fadeIn;

    public SetFadeInMessage(ulong id, FadeIn fadeIn) {
      this.id = id;
      this.fadeIn = fadeIn;
    }
  }

  public class AddButtonMessage : IDominoMessage {
    public readonly ulong newViewId;
    public readonly ulong parentViewId;
    public readonly int x;
    public readonly int y;
    public readonly int width;
    public readonly int height;
    public readonly int z;
    public readonly Vec4i color;
    public readonly Vec4i borderColor;
    public readonly Vec4i pressedColor;
    public readonly ulong onClicked;
    public readonly ulong onMouseIn;
    public readonly ulong onMouseOut;

    public AddButtonMessage(
        ulong newViewId,
        // int panelId,
        ulong parentViewId,
        int x,
        int y,
        int width,
        int height,
        int z,
        Vec4i color,
        Vec4i borderColor,
        Vec4i pressedColor,
        ulong onClicked,
        ulong onMouseIn,
        ulong onMouseOut) {
      this.newViewId = newViewId;
      // this.panelId = panelId;
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

  public class AddRectangleMessage : IDominoMessage {
    public readonly ulong newViewId;
    public readonly ulong parentViewId;
    public readonly int x;
    public readonly int y;
    public readonly int width;
    public readonly int height;
    public readonly int z;
    public readonly Vec4i color;
    public readonly Vec4i borderColor;

    public AddRectangleMessage(
        ulong newViewId,
        ulong parentViewId,
        int x,
        int y,
        int width,
        int height,
        int z,
        Vec4i color,
        Vec4i borderColor) {
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

  public class AddSymbolMessage : IDominoMessage {
    public readonly ulong newViewId;
    public readonly ulong parentViewId;
    public readonly int x;
    public readonly int y;
    public readonly int size;
    public readonly int z;
    public readonly Vec4i color;
    public readonly SymbolId symbolId;
    public readonly bool centered;

    public AddSymbolMessage(
        ulong newViewId,
        ulong parentViewId,
        int x,
        int y,
        int size,
        int z,
        Vec4i color,
        SymbolId symbolId,
        bool centered) {
      this.newViewId = newViewId;
      this.parentViewId = parentViewId;
      this.x = x;
      this.y = y;
      this.size = size;
      this.z = z;
      this.color = color;
      this.symbolId = symbolId;
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
    public readonly ulong tileId;
    public readonly InitialSymbol symbol;
  
    public ShowRuneMessage(ulong tileId, InitialSymbol symbol) {
      this.tileId = tileId;
      this.symbol = symbol; 
    }
  }
  public class SetOverlayMessage : IDominoMessage {
    public readonly ulong tileId;
    public readonly InitialSymbol symbol;
  
    public SetOverlayMessage(ulong tileId, InitialSymbol symbol) {
      this.tileId = tileId;
      this.symbol = symbol; 
    }
  }
  public class SetFeatureMessage : IDominoMessage {
    public readonly ulong tileId;
    public readonly InitialSymbol symbol;
  
    public SetFeatureMessage(ulong tileId, InitialSymbol symbol) {
      this.tileId = tileId;
      this.symbol = symbol; 
    }
  }
  public class SetCliffColorMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public readonly IVec4iAnimation sideColor;
  
    public SetCliffColorMessage(ulong tileViewId, IVec4iAnimation sideColor) {
      this.tileViewId = tileViewId;
      this.sideColor = sideColor; 
    }
  }
  public class SetSurfaceColorMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public readonly IVec4iAnimation frontColor;
  
    public SetSurfaceColorMessage(ulong tileViewId, IVec4iAnimation frontColor) {
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
    public readonly ulong itemId;
    public readonly InitialSymbol symbolDescription;
  
    public AddItemMessage(ulong tileViewId, ulong itemId, InitialSymbol symbolDescription) {
      this.tileViewId = tileViewId;
      this.itemId = itemId;
      this.symbolDescription = symbolDescription;
    }
  }
  
  public class AddDetailMessage : IDominoMessage {
    public readonly ulong unitViewId;
    public readonly ulong detailId;
    public readonly InitialSymbol symbolDescription;
  
    public AddDetailMessage(ulong unitViewId, ulong detailId, InitialSymbol symbolDescription) {
      this.unitViewId = unitViewId;
      this.detailId = detailId;
      this.symbolDescription = symbolDescription;
    }
  }
  
  public class RemoveDetailMessage : IDominoMessage {
    public readonly ulong unitViewId;
    public readonly ulong detailId;
  
    public RemoveDetailMessage(ulong unitViewId, ulong detailId) {
      this.unitViewId = unitViewId;
      this.detailId = detailId;
    }
  }
  
  public class DestroyTileMessage : IDominoMessage {
    public readonly ulong tileViewId;
    public DestroyTileMessage(ulong tileViewId) {
      this.tileViewId = tileViewId;
    }
  }
  public class DestroyUnitMessage : IDominoMessage {
    public readonly ulong unitId;
    public DestroyUnitMessage(ulong unitId) {
      this.unitId = unitId;
    }
  }
}