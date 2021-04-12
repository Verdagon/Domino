namespace Domino {
  public enum OutlineMode {
    NoOutline = 0,
    OuterOutline = 1,
    CenteredOutline = 2
  }

  public class InitialSymbolGlyph {
    public readonly SymbolId symbolId;
    public readonly IVec4iAnimation color;

    public InitialSymbolGlyph(
        SymbolId symbolId,
        IVec4iAnimation color) {
      this.symbolId = symbolId;
      this.color = color;
    }
  }
  
  public class InitialSymbolOutline {
    public readonly OutlineMode mode;
    public readonly IVec4iAnimation color;

    public InitialSymbolOutline(
        OutlineMode mode,
        IVec4iAnimation color) {
      this.mode = mode;
      this.color = color;
    }
  }

  public class InitialSymbolSides {
    public readonly int depthPercent;
    public readonly IVec4iAnimation color;

    public InitialSymbolSides(
        int depthPercent,
        IVec4iAnimation color) {
      this.depthPercent = depthPercent;
      this.color = color;
    }
  }

  public class InitialSymbol {
    public readonly InitialSymbolGlyph glyph;
    public readonly InitialSymbolOutline outline;
    public readonly InitialSymbolSides sides;
    public readonly int rotationDegrees;
    public readonly int sizePercent;

    public InitialSymbol(
          InitialSymbolGlyph glyph,
          InitialSymbolOutline outline,
          InitialSymbolSides sides,
          int rotationDegrees,
          int sizePercent) {
      this.glyph = glyph;
      this.outline = outline;
      this.sides = sides;
      this.rotationDegrees = rotationDegrees;
      this.sizePercent = sizePercent;
    }
  }
}