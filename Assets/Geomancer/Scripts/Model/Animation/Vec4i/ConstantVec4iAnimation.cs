using System.Collections;
using System.Collections.Generic;
using Geomancer.Model;


namespace Domino {
  public class ConstantVec4iAnimation : IVec4iAnimation {
    public static readonly ConstantVec4iAnimation white = new ConstantVec4iAnimation(Vec4i.white);
    public static readonly ConstantVec4iAnimation cyan = new ConstantVec4iAnimation(Vec4i.cyan);
    public static readonly ConstantVec4iAnimation red = new ConstantVec4iAnimation(Vec4i.red);
    public static readonly ConstantVec4iAnimation black = new ConstantVec4iAnimation(Vec4i.black);
    public static readonly ConstantVec4iAnimation blue = new ConstantVec4iAnimation(Vec4i.blue);
    
    public readonly Vec4i vec;

    public ConstantVec4iAnimation(Vec4i vec) {
      this.vec = vec;
    }
    public static ConstantVec4iAnimation All(int n) {
      return new ConstantVec4iAnimation(Vec4i.All(n));
    }
  }
}