using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Geomancer.Model;


namespace Domino {
  public class MultiplyVec4iAnimation : IVec4iAnimation {
    public readonly IVec4iAnimation left;
    public readonly IVec4iAnimation right;

    public MultiplyVec4iAnimation(IVec4iAnimation left, IVec4iAnimation right) {
      this.left = left;
      this.right = right;
    }
  }
}