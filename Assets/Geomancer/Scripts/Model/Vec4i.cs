using System;
using System.Collections;

using System.Collections.Generic;

namespace Geomancer.Model {
public struct Vec4i : IComparable<Vec4i> {
  public static readonly Vec4i white = new Vec4i(255, 255, 255, 255);
  public static readonly Vec4i cyan = new Vec4i(0, 255, 255, 255);
  public static readonly Vec4i red = new Vec4i(255, 0, 0, 255);
  public static readonly Vec4i black = new Vec4i(0, 0, 0, 255);
  public static readonly Vec4i blue = new Vec4i(0, 0, 255, 255);

  public static readonly string NAME = "Vec4i";
  public class EqualityComparer : IEqualityComparer<Vec4i> {
    public bool Equals(Vec4i a, Vec4i b) {
      return a.Equals(b);
    }
    public int GetHashCode(Vec4i a) {
      return a.GetDeterministicHashCode();
    }
  }
  public class Comparer : IComparer<Vec4i> {
    public int Compare(Vec4i a, Vec4i b) {
      return a.CompareTo(b);
    }
  }
  private readonly int hashCode;
         public readonly long x;
  public readonly long y;
  public readonly long z;
  public readonly long w;
  public Vec4i(
      long x,
      long y,
      long z,
      long w) {
    this.x = x;
    this.y = y;
    this.z = z;
    this.w = w;
    long hash = 0;
    hash = hash * 37 + x;
    hash = hash * 37 + y;
    hash = hash * 37 + z;
    hash = hash * 37 + w;
    this.hashCode = (int)hash;

  }

  public static Vec4i All(int n) {
    return new Vec4i(n, n, n, n);
  }
  public static bool operator==(Vec4i a, Vec4i b) {
    return a.Equals(b);
  }
  public static bool operator!=(Vec4i a, Vec4i b) {
    return !a.Equals(b);
  }
  public static Vec4i operator*(Vec4i a, int n) {
    return new Vec4i(a.x * n, a.y * n, a.z * n, a.w * n);
  }
  public static Vec4i operator/(Vec4i a, int n) {
    return new Vec4i(a.x / n, a.y / n, a.z / n, a.w / n);
  }
  public static Vec4i operator+(Vec4i a, Vec4i b) {
    return new Vec4i(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
  }
  public override int GetHashCode() {
    return GetDeterministicHashCode();
  }
  public int GetDeterministicHashCode() { return hashCode; }
  public int CompareTo(Vec4i that) {
    if (x != that.x) {
      return x.CompareTo(that.x);
    }
    if (y != that.y) {
      return y.CompareTo(that.y);
    }
    if (z != that.z) {
      return z.CompareTo(that.z);
    }
    if (w != that.w) {
      return w.CompareTo(that.w);
    }
    return 0;
  }
  public override string ToString() { return DStr(); }
  public string DStr() {
    return "Vec4i(" +
        x + ", " +
        y + ", " +
        z + ", " +
        w
        + ")";

    }
    public static Vec4i Parse(ParseSource source) {
      source.Expect(NAME);
      source.Expect("(");
      var x = source.ParseInt();
      source.Expect(",");
      var y = source.ParseInt();
      source.Expect(",");
      var z = source.ParseInt();
      source.Expect(",");
      var w = source.ParseInt();
      source.Expect(")");
      return new Vec4i(x, y, z, w);
  }
}
       
}
