namespace Domino {
  public class InitialSymbol {
    public readonly SymbolId symbolId;
    public readonly int rotationDegrees;
    public readonly int sizePercent;
    public readonly IVector4Animation frontColor;
    public readonly bool outlined;
    public readonly IVector4Animation outlineColor;
    public readonly int depthPercent;
    public readonly IVector4Animation sidesColor;

    public InitialSymbol(
        SymbolId symbolId,
        int rotationDegrees,
        int sizePercent,
        IVector4Animation frontColor,
        bool outlined,
        IVector4Animation outlineColor,
        int depthPercent,
        IVector4Animation sidesColor) {
      this.symbolId = symbolId;
      this.frontColor = frontColor;
      this.rotationDegrees = rotationDegrees;
      this.sizePercent = sizePercent;
      this.frontColor = frontColor;
      this.outlined = outlined;
      this.outlineColor = outlineColor;
      this.depthPercent = depthPercent;
      this.sidesColor = sidesColor;

      Asserts.Assert(outlineColor != null);
    }
  }
}