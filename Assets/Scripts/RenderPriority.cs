using System;

namespace Domino {
  public enum RenderPriority {
    // Opaque things (any order is fine)
    TILE = 0,
    DOMINO = 1,
    METER = 2,

    // Transparent things (try to keep em ordered back to front)
    OVERLAY = 3,
    ITEM = 3,
    FEATURE = 4,
    SYMBOL = 5,
    RUNE = 6,
  }
}
