using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domino {
  public struct SymbolId {
    public readonly string fontName;
    public readonly int unicode;

    public SymbolId(string fontName, int unicode) {
      Asserts.Assert(!fontName.EndsWith(".ttf")); // We'll add "Simplified.ttf" or "Expanded.ttf" to it later
      this.fontName = fontName;
      this.unicode = unicode;
    }

    public override int GetHashCode() {
      return fontName.GetHashCode() + unicode * 73;
    }

    public override bool Equals(object obj) {
      return base.Equals(obj);
    }
  }
}
