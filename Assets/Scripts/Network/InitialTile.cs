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
    //
    // public override bool Equals(object obj) {
    //   if (!(obj is InitialTile))
    //     return false;
    //   InitialTile that = obj as InitialTile;
    //   // if (elevationStepHeight != that.elevationStepHeight)
    //   //   return false;
    //   // if (tileRotationDegrees != that.tileRotationDegrees)
    //   //   return false;
    //   // if (depth != that.depth)
    //   //   return false;
    //   if (location != that.location)
    //     return false;
    //   if (elevation != that.elevation)
    //     return false;
    //   if (!topColor.Equals(that.topColor))
    //     return false;
    //   if (!sideColor.Equals(that.sideColor))
    //     return false;
    //   if ((maybeOverlaySymbolDescription != null) != (that.maybeOverlaySymbolDescription != null))
    //     return false;
    //   if (maybeOverlaySymbolDescription != null && !maybeOverlaySymbolDescription.Equals(that.maybeOverlaySymbolDescription))
    //     return false;
    //   if ((maybeFeatureSymbolDescription != null) != (that.maybeFeatureSymbolDescription != null))
    //     return false;
    //   if (maybeFeatureSymbolDescription != null && !maybeFeatureSymbolDescription.Equals(that.maybeFeatureSymbolDescription))
    //     return false;
    //   if (itemSymbolDescriptionByItemId.Count != that.itemSymbolDescriptionByItemId.Count)
    //     return false;
    //   for (int i = 0; i < itemSymbolDescriptionByItemId.Count; i++) {
    //     if (itemSymbolDescriptionByItemId[i].Item1 != that.itemSymbolDescriptionByItemId[i].Item1)
    //       return false;
    //     if (!itemSymbolDescriptionByItemId[i].Item2.Equals(that.itemSymbolDescriptionByItemId[i].Item2))
    //       return false;
    //   }
    //   return true;
    // }
    // public override int GetHashCode() {
    //   int hashCode = 0;
    //   hashCode += 27 * location.GetHashCode();
    //   hashCode += 31 * elevation.GetHashCode();
    //   // hashCode += 27 * elevationStepHeight.GetHashCode();
    //   // hashCode += 31 * tileRotationDegrees.GetHashCode();
    //   // hashCode += 37 * depth.GetHashCode();
    //   hashCode += 41 * topColor.GetHashCode();
    //   hashCode += 43 * sideColor.GetHashCode();
    //   if (maybeOverlaySymbolDescription != null)
    //     hashCode += 47 * maybeOverlaySymbolDescription.GetHashCode();
    //   if (maybeFeatureSymbolDescription != null)
    //     hashCode += 53 * maybeFeatureSymbolDescription.GetHashCode();
    //   hashCode += 67 * itemSymbolDescriptionByItemId.Count;
    //   foreach (var entry in itemSymbolDescriptionByItemId) {
    //     hashCode += 87 * entry.Item1.GetHashCode() + 93 * entry.Item2.GetHashCode();
    //   }
    //   return hashCode;
    // }
  }
}