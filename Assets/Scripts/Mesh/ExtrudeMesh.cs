using System;
using System.Collections.Generic;
using Geomancer;
using Geomancer.Model;
using UnityEngine;

namespace Domino {
  static class ExtrudeMesh {
    public static void AddExtrudedPolygon(MeshBuilder builder, List<Vector3> topCorners, float tileHeight) {
      var (lowCorners, lowCornersReversed) = GetLowCorners(topCorners, tileHeight);
      builder.AddPolygon(topCorners, new Vector3(0, 1, 0));
      builder.AddPolygon(lowCornersReversed, new Vector3(0, -1, 0));
      // Add sides
      for (int i = 0; i < topCorners.Count; i++) {
        var a = topCorners[i];
        var b = lowCorners[i];
        var c = lowCorners[(i + 1) % topCorners.Count];
        var d = topCorners[(i + 1) % topCorners.Count];
        var normal = Vector3.Cross(b - a, c - a);
        builder.AddQuad(a, b, c, d, normal);
      }
    }

  
    public static (List<Vector3>, List<Vector3>) GetLowCorners(List<Vector3> topCorners, float tileHeight) {
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
  }
}