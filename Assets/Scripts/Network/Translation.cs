using Domino;
using Geomancer.Model;
using UnityEngine;

namespace Domino {
  public static class Translation {
    public static ExtrudedSymbolDescription TranslateMaybeInitialSymbol(
        RenderPriority renderPriority,
        InitialSymbol initialSymbol) {
      if (initialSymbol == null) {
        return null;
      }
      return new ExtrudedSymbolDescription(
          renderPriority,
          new SymbolDescription(
              initialSymbol.glyph.symbolId,
              new DivideVector4Animation(Translate(initialSymbol.glyph.color), ConstantVector4Animation.All(255)),
              initialSymbol.rotationDegrees,
              initialSymbol.sizePercent / 100f,
              initialSymbol.outline?.mode ?? OutlineMode.NoOutline,
              new DivideVector4Animation(
                Translate(initialSymbol.outline != null ? initialSymbol.outline.color : ConstantVec4iAnimation.black),
                ConstantVector4Animation.All(255))),
          initialSymbol.sides != null ? initialSymbol.sides.depthPercent / 100f : 0,
          new DivideVector4Animation(
              Translate(initialSymbol.sides != null ? initialSymbol.sides.color : ConstantVec4iAnimation.black),
              ConstantVector4Animation.All(255)));
    }

    public static Vector4 Translate(Vec4i vec) {
      return new Vector4(vec.x, vec.y, vec.z, vec.w);
    }

    public static IVector4Animation Translate(IVec4iAnimation anim) {
      if (anim is ConstantVec4iAnimation constant) {
        return new ConstantVector4Animation(Translate(constant.vec));
      } else if (anim is AddVec4iAnimation add) {
        return new AddVector4Animation(Translate(add.left), Translate(add.right));
      } else if (anim is MultiplyVec4iAnimation multiply) {
        return new MultiplyVector4Animation(Translate(multiply.left), Translate(multiply.right));
      } else if (anim is DivideVec4iAnimation divide) {
        return new DivideVector4Animation(Translate(divide.left), Translate(divide.right));
      } else {
        Asserts.Assert(false);
        return null;
      }
    }
  }
}