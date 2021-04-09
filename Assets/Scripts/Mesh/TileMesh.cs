using System;
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
        if (shapeIndex >= pattern.cornersByShapeIndex.Count) {
          throw new Exception("Shape index " + shapeIndex + " doesn't exist!");
        }
        
        for (int i = 0; i < pattern.cornersByShapeIndex[shapeIndex].Count; i++) {
          // Reverse; the patterns are right handed but unity is left handed
          // so this reversal should make it clockwise instead of counterclockwise
          var cornerVec2 = pattern.cornersByShapeIndex[shapeIndex][pattern.cornersByShapeIndex[shapeIndex].Count - 1 - i];
          topCorners.Add(new Vec3(cornerVec2.x, cornerVec2.y, 0).ToUnity());
        }
        
        var tileMeshBuilder = new MeshBuilder();
        ExtrudeMesh.AddExtrudedPolygon(tileMeshBuilder, topCorners, .3f);
        var tileMesh = tileMeshBuilder.Build();
        
        var outlinesMeshBuilder = new MeshBuilder();
        OutlineMesh.BuildFrontFaceOutlines(outlinesMeshBuilder, Vector3.up, topCorners, outlineThickness);
        OutlineMesh.BuildBackFaceOutlines(outlinesMeshBuilder, Vector3.up, topCorners, tileHeight, outlineThickness);
        OutlineMesh.BuildColumnOutlines(outlinesMeshBuilder, Vector3.up, topCorners, tileHeight, outlineThickness);
        var outlinesMesh = outlinesMeshBuilder.Build();
        
        shapeIndexToMesh[shapeIndex] = (tileMesh, outlinesMesh);
      }
      return shapeIndexToMesh[shapeIndex];
    }
  }
}