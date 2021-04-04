using System.Collections.Generic;
using Geomancer.Model;

namespace Domino {
  public class InitialTile {
    // public readonly float elevationStepHeight;
    // public readonly float tileRotationDegrees;
    // public readonly int depth; // basically elevation
    public readonly Location location;
    public readonly int elevation;
    public readonly IVector4Animation topColor;
    public readonly IVector4Animation sideColor;
    public readonly InitialSymbol maybeOverlaySymbolDescription;
    public readonly InitialSymbol maybeFeatureSymbolDescription;
    public readonly List<(ulong, InitialSymbol)> itemSymbolDescriptionByItemId;

    public InitialTile(
        Location location,
        int elevation,
        IVector4Animation topColor,
        IVector4Animation sideColor,
        InitialSymbol maybeOverlaySymbolDescription,
        InitialSymbol maybeFeatureSymbolDescription,
        List<(ulong, InitialSymbol)> itemSymbolDescriptionByItemId) {
      this.location = location;
      this.elevation = elevation;
      this.topColor = topColor;
      this.sideColor = sideColor;
      this.maybeOverlaySymbolDescription = maybeOverlaySymbolDescription;
      this.maybeFeatureSymbolDescription = maybeFeatureSymbolDescription;
      this.itemSymbolDescriptionByItemId = itemSymbolDescriptionByItemId;
    }
  }
}