using System.Collections.Generic;
using Geomancer;
using Geomancer.Model;
using UnityEngine;

namespace Domino {
  public class TileShapeMeshCache {
    private Pattern pattern;
    private (Mesh, Mesh)[] shapeIndexToMesh;
  
    public TileShapeMeshCache(Pattern pattern) {
      this.pattern = pattern;
      this.shapeIndexToMesh = new (Mesh, Mesh)[pattern.patternTiles.Count];
      for (int i = 0; i < pattern.patternTiles.Count; i++) {
        this.shapeIndexToMesh[i] = (null, null);
      }
    }
    
    public (Mesh, Mesh) Get(int shapeIndex, float tileHeight, float outlineThickness) {
      if (shapeIndexToMesh[shapeIndex].Item1 == null) {
        var topCorners = new List<Vector3>();
        for (int i = 0; i < pattern.cornersByShapeIndex[shapeIndex].Count; i++) {
          // Reverse; the patterns are right handed but unity is left handed
          // so this reversal should make it clockwise instead of counterclockwise
          var cornerVec2 = pattern.cornersByShapeIndex[shapeIndex][pattern.cornersByShapeIndex[shapeIndex].Count - 1 - i];
          topCorners.Add(new Vec3(cornerVec2.x, cornerVec2.y, 0).ToUnity());
        }
        var tileMesh = TileMeshBuilder.BuildTile(topCorners, .3f);
        var outlinesMesh = TileMeshBuilder.BuildOutlines(topCorners, tileHeight, outlineThickness);
        shapeIndexToMesh[shapeIndex] = (tileMesh, outlinesMesh);
      }
      return shapeIndexToMesh[shapeIndex];
    }
  }
  
  class TileMeshBuilder {
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<int> indices = new List<int>();
  
    private TileMeshBuilder() {
    }
  
    private static (List<Vector3>, List<Vector3>) GetLowCorners(List<Vector3> topCorners, float tileHeight) {
      var lowCorners = new List<Vector3>();
      for (int i = 0; i < topCorners.Count; i++) {
        var topCorner = topCorners[i];
        lowCorners.Add(new Vector3(topCorner.x, topCorner.y - tileHeight, topCorner.z));
      }
      
      // So they can be clockwise, because left handed
      var lowCornersReversed = new List<Vector3>(lowCorners);
      lowCornersReversed.Reverse();
  
      return (lowCorners, lowCornersReversed);
    }
    
    public static Mesh BuildOutlines(List<Vector3> topCorners, float tileHeight, float outlineThickness) {
      var (lowCorners, lowCornersReversed) = GetLowCorners(topCorners, tileHeight);
      var outlinesMesh = new TileMeshBuilder();
      for (int i = 0; i < topCorners.Count; i++) {
        outlinesMesh.AddFacePrism(
            topCorners[(i + topCorners.Count - 1) % topCorners.Count],
            topCorners[i],
            topCorners[(i + 1) % topCorners.Count],
            topCorners[(i + 2) % topCorners.Count],
            outlineThickness);
      }
      for (int i = 0; i < lowCornersReversed.Count; i++) {
        outlinesMesh.AddFacePrism(
            lowCornersReversed[(i + lowCornersReversed.Count - 1) % lowCornersReversed.Count],
            lowCornersReversed[i],
            lowCornersReversed[(i + 1) % lowCornersReversed.Count],
            lowCornersReversed[(i + 2) % lowCornersReversed.Count],
            outlineThickness);
      }
      for (int i = 0; i < topCorners.Count; i++) {
        outlinesMesh.AddColumnPrism(
            topCorners[(i + topCorners.Count - 1) % topCorners.Count],
            topCorners[i],
            topCorners[(i + 1) % topCorners.Count],
            lowCorners[i],
            outlineThickness);
      }
      return outlinesMesh.Build();
    }
  
    public static Mesh BuildTile(List<Vector3> topCorners, float tileHeight) {
      var (lowCorners, lowCornersReversed) = GetLowCorners(topCorners, tileHeight);
      var facesMesh = new TileMeshBuilder();
      facesMesh.AddPolygon(topCorners, new Vector3(0, 1, 0));
      facesMesh.AddPolygon(lowCornersReversed, new Vector3(0, -1, 0));
      // Add sides
      for (int i = 0; i < topCorners.Count; i++) {
        var a = topCorners[i];
        var b = lowCorners[i];
        var c = lowCorners[(i + 1) % topCorners.Count];
        var d = topCorners[(i + 1) % topCorners.Count];
        var normal = Vector3.Cross(b - a, c - a);
        facesMesh.AddQuad(a, b, c, d, normal);
      }
      return facesMesh.Build();
    }
    
    private void AddQuad(
        Vector3 a,
        Vector3 b,
        Vector3 c,
        Vector3 d,
        Vector3 normal) {
      int aIndex = vertices.Count;
      vertices.Add(a);
      normals.Add(normal);
      int bIndex = vertices.Count;
      vertices.Add(b);
      normals.Add(normal);
      int cIndex = vertices.Count;
      vertices.Add(c);
      normals.Add(normal);
      int dIndex = vertices.Count;
      vertices.Add(d);
      normals.Add(normal);
        
      indices.Add(aIndex);
      indices.Add(bIndex);
      indices.Add(cIndex);
        
      indices.Add(aIndex);
      indices.Add(cIndex);
      indices.Add(dIndex);
    }
    
    // There are three line segments in play here: "foot", "calf", and "thigh",
    // between four points: "toe", "heel", "knee", "pelvis".
    //  heel __________ knee   
    //    /   calf   \   
    //  foot /      \ thigh  
    //    /        \ 
    //   toe         pelvis
    // This function adds a prism around the calf.
    private void AddFacePrism(
        Vector3 toe,
        Vector3 heel,
        Vector3 knee,
        Vector3 pelvis,
        float inscribeRadius) {
      var up = new Vector3(0, 1, 0);
      var foot = toe - heel;
      var calf = knee - heel; // NOTE: This is from heel to knee!
      var thigh = pelvis - knee;
      var footOut = Vector3.Cross(up, foot).normalized;
      var calfOut = Vector3.Cross(calf, up).normalized;
      var thighOut = Vector3.Cross(thigh, up).normalized;
  
      var heelOutDir = (footOut + calfOut).normalized;
      var kneeOutDir = (calfOut + thighOut).normalized;
      var heelOutABit = heelOutDir * inscribeRadius / Vector3.Dot(calfOut, heelOutDir);
      var kneeOutABit = kneeOutDir * inscribeRadius / Vector3.Dot(calfOut, kneeOutDir);
  
      var upABit = up * inscribeRadius;
  
      var heelUpIn = heel + upABit - heelOutABit;
      var heelUpOut = heel + upABit + heelOutABit;
      var kneeUpOut = knee + upABit + kneeOutABit;
      var kneeUpIn = knee + upABit - kneeOutABit;
      AddQuad(heelUpIn, heelUpOut, kneeUpOut, kneeUpIn, up);
      
      var heelDownIn = heel - upABit - heelOutABit;
      var heelDownOut = heel - upABit + heelOutABit;
      var kneeDownOut = knee - upABit + kneeOutABit;
      var kneeDownIn = knee - upABit - kneeOutABit;
      AddQuad(kneeDownOut, heelDownOut, heelDownIn, kneeDownIn, -up);
      
      AddQuad(kneeDownOut, kneeUpOut, heelUpOut, heelDownOut, calfOut);
      
      AddQuad(heelUpIn, kneeUpIn, kneeDownIn, heelDownIn, -calfOut);
  
  
      // var heelUpCalfOut = heel + upABit + calfOut * inscribeRadius;
      // var heelDownCalfOut = heel - upABit + calfOut * inscribeRadius;
      // var heelUp = heel + upABit;
      // var heelDown = heel - upABit;
      // AddQuad(heelUp, heelUpCalfOut, heelDownCalfOut, heelDown, calf);
    }
    
    // There are three line segments in play here: "foot", "calf", and "thigh",
    // between four points: "toe", "heel", "knee", "pelvis".
    //  heel __________ knee   
    //    /   calf   \   
    //  foot /      \ thigh  
    //    /        \ 
    //   toe         pelvis
    // This function adds a prism around the calf.
    private void AddColumnPrism(
        Vector3 topToe,
        Vector3 topHeel,
        Vector3 topKnee,
        Vector3 bottomHeel,
        float inscribeRadius) {
      var up = new Vector3(0, 1, 0);
      var foot = topToe - topHeel;
      var calf = topKnee - topHeel; // NOTE: This is from heel to knee!
      var footOut = Vector3.Cross(up, foot).normalized;
      var calfOut = Vector3.Cross(calf, up).normalized;
  
      var heelOutDir = (footOut + calfOut).normalized;
      var heelOutABit = heelOutDir * inscribeRadius / Vector3.Dot(calfOut, heelOutDir);
  
      var upABit = up * inscribeRadius;
  
      var topHeelUpOut = topHeel + upABit + heelOutABit;
      var bottomHeelDownOut = bottomHeel - upABit + heelOutABit;
  
  
      var topHeelUpCalfOut = topHeelUpOut + calf.normalized * inscribeRadius * 2;
      var topHeelUpFootOut = topHeelUpOut + foot.normalized * inscribeRadius * 2;
      var bottomHeelDownCalfOut = bottomHeelDownOut + calf.normalized * inscribeRadius * 2;
      var bottomHeelDownFootOut = bottomHeelDownOut + foot.normalized * inscribeRadius * 2;
      AddQuad(topHeelUpOut, topHeelUpFootOut, bottomHeelDownFootOut, bottomHeelDownOut, footOut);
      AddQuad(topHeelUpCalfOut, topHeelUpOut, bottomHeelDownOut, bottomHeelDownCalfOut, calfOut);
  
      var topHeelUpCalfOutAndBackIn = topHeelUpCalfOut - calfOut.normalized * inscribeRadius;
      var bottomHeelDownCalfOutAndBackIn = bottomHeelDownCalfOut - calfOut.normalized * inscribeRadius;
      AddQuad(topHeelUpCalfOutAndBackIn, topHeelUpCalfOut, bottomHeelDownCalfOut, bottomHeelDownCalfOutAndBackIn, calf);
      
      var topHeelUpFootOutAndBackIn = topHeelUpFootOut - footOut.normalized * inscribeRadius;
      var bottomHeelDownFootOutAndBackIn = bottomHeelDownFootOut - footOut.normalized * inscribeRadius;
      AddQuad(topHeelUpFootOut, topHeelUpFootOutAndBackIn, bottomHeelDownFootOutAndBackIn, bottomHeelDownFootOut, foot);
    }
  
    private void AddPolygon(
        List<Vector3> corners,
        Vector3 normal) {
      List<int> addedIndices = new List<int>();
      for (int i = 0; i < corners.Count; i++) {
        addedIndices.Add(vertices.Count);
        vertices.Add(corners[i]);
        normals.Add(normal);
      }
      for (int i = 1; i < corners.Count - 1; i++) {
        indices.Add(addedIndices[0]);
        indices.Add(addedIndices[i]);
        indices.Add(addedIndices[i + 1]);
      }
    }
    
    private Mesh Build() {
      var mesh = new Mesh();
      mesh.SetVertices(vertices.ToArray());
      mesh.SetNormals(normals.ToArray());
      mesh.SetTriangles(indices, 0);
      mesh.RecalculateNormals();
      mesh.RecalculateBounds();
      mesh.RecalculateTangents();
      return mesh;
    }
  }

}