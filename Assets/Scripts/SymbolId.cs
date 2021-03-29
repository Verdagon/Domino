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
      Asserts.Assert(fontName.EndsWith(".ttf"));
      this.fontName = fontName;
      this.unicode = unicode;
    }
  }
}
