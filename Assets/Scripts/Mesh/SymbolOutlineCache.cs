using System;
using System.Collections.Generic;
using Geomancer;
using Geomancer.Model;
using UnityEngine;
using Virtence.VText;

namespace Domino {
  // public class SymbolOutlineCache {
  //   private VTextGlyphBuilder glyphBuilder;
  //   private Dictionary<int, Mesh> unicodeToMesh;
  //   private float outlineThickness;
  //
  //   public SymbolOutlineCache(VTextGlyphBuilder glyphBuilder, float outlineThickness) {
  //     this.glyphBuilder = glyphBuilder;
  //     this.outlineThickness = outlineThickness;
  //     this.unicodeToMesh = new Dictionary<int, Mesh>();
  //   }
  //   
  //   public Mesh Get(int unicode) {
  //     if (unicodeToMesh.TryGetValue(unicode, out var mesh)) {
  //       return mesh;
  //     }
  //
  //     var contours = glyphBuilder.GetContours((char) unicode);
  //
  //     var outlinesMeshBuilder = new MeshBuilder();
  //     foreach (var contour in contours) {
  //       OutlineMesh.BuildFrontFaceOutlines(outlinesMeshBuilder, contour.VertexList, outlineThickness);
  //     }
  //     var outlinesMesh = outlinesMeshBuilder.Build();
  //     unicodeToMesh.Add(unicode, outlinesMesh);
  //     return outlinesMesh;
  //   }
  // }
}