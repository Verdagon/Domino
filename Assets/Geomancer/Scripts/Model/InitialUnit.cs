using System.Collections.Generic;
using Geomancer.Model;

namespace Domino {
  public class InitialUnit {
    public readonly Location location;
    public readonly InitialSymbol dominoSymbol;
    public readonly InitialSymbol faceSymbol;
    public readonly List<(ulong, InitialSymbol)> idToDetailSymbol;
    public readonly float hpRatio;
    public readonly float mpRatio;

    public InitialUnit(
        Location location,
        InitialSymbol dominoSymbol,
        InitialSymbol faceSymbol,
        List<(ulong, InitialSymbol)> idToDetailSymbol,
        float hpRatio,
        float mpRatio) {
      this.location = location;
      this.dominoSymbol = dominoSymbol;
      this.faceSymbol = faceSymbol;
      this.idToDetailSymbol = idToDetailSymbol;
      this.hpRatio = hpRatio;
      this.mpRatio = mpRatio;
    }
  }
}
