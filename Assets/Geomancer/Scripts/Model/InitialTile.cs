using System.Collections.Generic;
using Geomancer.Model;

namespace Domino {
  public class InitialTile {
    // public readonly float elevationStepHeight;
    // public readonly float tileRotationDegrees;
    // public readonly int depth; // basically elevation
    public readonly Location location;
    public readonly int elevation;
    public readonly IVec4iAnimation topColor;
    public readonly IVec4iAnimation sideColor;
    public readonly InitialSymbol maybeOverlaySymbol;
    public readonly InitialSymbol maybeFeatureSymbol;
    public readonly List<(ulong, InitialSymbol)> itemIdToSymbol;

    public InitialTile(
        Location location,
        int elevation,
        IVec4iAnimation topColor,
        IVec4iAnimation sideColor,
        InitialSymbol maybeOverlaySymbol,
        InitialSymbol maybeFeatureSymbol,
        List<(ulong, InitialSymbol)> itemIdToSymbol) {
      this.location = location;
      this.elevation = elevation;
      this.topColor = topColor;
      this.sideColor = sideColor;
      this.maybeOverlaySymbol = maybeOverlaySymbol;
      this.maybeFeatureSymbol = maybeFeatureSymbol;
      this.itemIdToSymbol = itemIdToSymbol;
    }
  }
}