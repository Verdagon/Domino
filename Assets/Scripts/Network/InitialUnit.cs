using System.Collections.Generic;
using Geomancer.Model;

namespace Domino {
  public class InitialUnit {
    public readonly Location location;
    public readonly int elevation;
    public readonly DominoShape shape;
    public readonly IVector4Animation color;
    public readonly InitialSymbol faceSymbolDescription;
    public readonly List<(ulong, InitialSymbol)> detailSymbolDescriptionById;
    public readonly float hpRatio;
    public readonly float mpRatio;

    public InitialUnit(
        Location location,
        int elevation,
        DominoShape shape,
        IVector4Animation color,
        InitialSymbol faceSymbolDescription,
        List<(ulong, InitialSymbol)> detailSymbolDescriptionById,
        float hpRatio,
        float mpRatio) {
      this.location = location;
      this.elevation = elevation;
      this.shape = shape;
      this.color = color;
      this.faceSymbolDescription = faceSymbolDescription;
      this.detailSymbolDescriptionById = detailSymbolDescriptionById;
      this.hpRatio = hpRatio;
      this.mpRatio = mpRatio;
    }
  }
}