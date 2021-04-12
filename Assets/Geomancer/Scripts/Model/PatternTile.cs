using System;
using System.Collections;

using System.Collections.Generic;

namespace Geomancer.Model {
public class PatternTile : IComparable<PatternTile> {
  public static readonly string NAME = "PatternTile";
  public class EqualityComparer : IEqualityComparer<PatternTile> {
    public bool Equals(PatternTile a, PatternTile b) {
      return a.Equals(b);
    }
    public int GetHashCode(PatternTile a) {
      return a.GetDeterministicHashCode();
    }
  }
  public class Comparer : IComparer<PatternTile> {
    public int Compare(PatternTile a, PatternTile b) {
      return a.CompareTo(b);
    }
  }
  private readonly int hashCode;
         public readonly int shapeIndex;
  public readonly int rotateRadianards; // radianard means radians times 1000
  public readonly Vec2 translate;
  public readonly PatternSideAdjacencyImmList sideIndexToSideAdjacencies;
  public readonly PatternCornerAdjacencyImmListImmList cornerIndexToCornerAdjacencies;
  public PatternTile(
      int shapeIndex,
      int rotateRadianards,
      Vec2 translate,
      PatternSideAdjacencyImmList sideIndexToSideAdjacencies,
      PatternCornerAdjacencyImmListImmList cornerIndexToCornerAdjacencies) {
    this.shapeIndex = shapeIndex;
    this.rotateRadianards = rotateRadianards;
    this.translate = translate;
    this.sideIndexToSideAdjacencies = sideIndexToSideAdjacencies;
    this.cornerIndexToCornerAdjacencies = cornerIndexToCornerAdjacencies;
    int hash = 0;
    hash = hash * 37 + shapeIndex;
    hash = hash * 37 + rotateRadianards;
    hash = hash * 37 + translate.GetDeterministicHashCode();
    hash = hash * 37 + sideIndexToSideAdjacencies.GetDeterministicHashCode();
    hash = hash * 37 + cornerIndexToCornerAdjacencies.GetDeterministicHashCode();
    this.hashCode = hash;

  }
  public static bool operator==(PatternTile a, PatternTile b) {
    if (object.ReferenceEquals(a, null))
      return object.ReferenceEquals(b, null);
    return a.Equals(b);
  }
  public static bool operator!=(PatternTile a, PatternTile b) {
    if (object.ReferenceEquals(a, null))
      return !object.ReferenceEquals(b, null);
    return !a.Equals(b);
  }
  public override bool Equals(object obj) {
    if (obj == null) {
      return false;
    }
    if (!(obj is PatternTile)) {
      return false;
    }
    var that = obj as PatternTile;
    return true
               && shapeIndex.Equals(that.shapeIndex)
        && rotateRadianards.Equals(that.rotateRadianards)
        && translate.Equals(that.translate)
        && sideIndexToSideAdjacencies.Equals(that.sideIndexToSideAdjacencies)
        && cornerIndexToCornerAdjacencies.Equals(that.cornerIndexToCornerAdjacencies)
        ;
  }
  public override int GetHashCode() {
    return GetDeterministicHashCode();
  }
  public int GetDeterministicHashCode() { return hashCode; }
  public int CompareTo(PatternTile that) {
    if (shapeIndex != that.shapeIndex) {
      return shapeIndex.CompareTo(that.shapeIndex);
    }
    if (rotateRadianards != that.rotateRadianards) {
      return rotateRadianards.CompareTo(that.rotateRadianards);
    }
    if (translate != that.translate) {
      return translate.CompareTo(that.translate);
    }
    if (sideIndexToSideAdjacencies != that.sideIndexToSideAdjacencies) {
      return sideIndexToSideAdjacencies.CompareTo(that.sideIndexToSideAdjacencies);
    }
    if (cornerIndexToCornerAdjacencies != that.cornerIndexToCornerAdjacencies) {
      return cornerIndexToCornerAdjacencies.CompareTo(that.cornerIndexToCornerAdjacencies);
    }
    return 0;
  }
  public override string ToString() { return DStr(); }
  public string DStr() {
    return "PatternTile(" +
        shapeIndex + ", " +
        rotateRadianards + ", " +
        translate + ", " +
        sideIndexToSideAdjacencies + ", " +
        cornerIndexToCornerAdjacencies
        + ")";

    }
    public static PatternTile Parse(ParseSource source) {
      source.Expect(NAME);
      source.Expect("(");
      var shapeIndex = source.ParseInt();
      source.Expect(",");
      var rotateRadianards = source.ParseInt();
      source.Expect(",");
      var translate = Vec2.Parse(source);
      source.Expect(",");
      var sideIndexToSideAdjacencies = PatternSideAdjacencyImmList.Parse(source);
      source.Expect(",");
      var cornerIndexToCornerAdjacencies = PatternCornerAdjacencyImmListImmList.Parse(source);
      source.Expect(")");
      return new PatternTile(shapeIndex, rotateRadianards, translate, sideIndexToSideAdjacencies, cornerIndexToCornerAdjacencies);
  }
}
       
}
