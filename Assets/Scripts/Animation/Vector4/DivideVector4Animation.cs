using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Domino {
  public class DivideVector4Animation : IVector4Animation {
    IVector4Animation a;
    IVector4Animation b;

    public DivideVector4Animation(IVector4Animation a, IVector4Animation b) {
      this.a = a;
      this.b = b;
    }

    public Vector4 Get(long timeMs) {
      var leftVec = a.Get(timeMs);
      var rightVec = b.Get(timeMs);
      return new Vector4(
          leftVec.x / rightVec.x, leftVec.y / rightVec.y, leftVec.z / rightVec.z, leftVec.w / rightVec.w);
    }

    public IVector4Animation Simplify(long timeMs) {
      a = a.Simplify(timeMs);
      b = b.Simplify(timeMs);
      if (a is IdentityVector4Animation) {
        return b;
      }
      if (b is IdentityVector4Animation) {
        return a;
      }
      if ((a is ConstantVector4Animation) && (b is ConstantVector4Animation)) {
        Vector4 constant = Get(timeMs);
        if (constant.EqualsE(new Vector4(0, 0, 0), .001f)) {
          return new IdentityVector4Animation();
        } else {
          return new ConstantVector4Animation(constant);
        }
      }
      return this;
    }
  }
}