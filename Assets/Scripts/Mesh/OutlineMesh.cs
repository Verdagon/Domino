using System;
using System.Collections.Generic;
using Geomancer;
using Geomancer.Model;
using UnityEngine;

namespace Domino {
  static class OutlineMesh {
    private static List<Vector3> WithoutColinearCorners(List<Vector3> corners) {
      var results = new List<Vector3>();
      for (int i = 0; i < corners.Count; i++) {
        var thisCorner = corners[i];
        var prevCorner = corners[(i + corners.Count - 1) % corners.Count];
        var nextCorner = corners[(i + 1) % corners.Count];
        var toPrev = (prevCorner - thisCorner).normalized;
        var toNext = (nextCorner - thisCorner).normalized;
        if (!Vector3.Cross(toPrev, toNext).EqualsE(new Vector3(0, 0, 0), 0.001f)) {
          results.Add(thisCorner);
        }
      }
      return results;
    }
    
    public static void BuildFrontFaceOutlines(
        MeshBuilder builder,
        Vector3 up,
        List<Vector3> uncleanedCorners,
        float outlineThickness) {
      var corners = WithoutColinearCorners(uncleanedCorners);
      for (int i = 0; i < corners.Count; i++) {
        AddFacePrism(
            builder,
            up,
            corners[(i + corners.Count - 1) % corners.Count],
            corners[i],
            corners[(i + 1) % corners.Count],
            corners[(i + 2) % corners.Count],
            outlineThickness);
      }
    }
    
    public static void BuildBackFaceOutlines(
        MeshBuilder builder,
        Vector3 up,
        List<Vector3> uncleanedTopCorners,
        float tileHeight,
        float outlineThickness) {
      var topCorners = WithoutColinearCorners(uncleanedTopCorners);
      var (lowCorners, lowCornersReversed) = ExtrudeMesh.GetLowCorners(topCorners, tileHeight);
      for (int i = 0; i < lowCornersReversed.Count; i++) {
        AddFacePrism(
            builder,
            up,
            lowCornersReversed[(i + lowCornersReversed.Count - 1) % lowCornersReversed.Count],
            lowCornersReversed[i],
            lowCornersReversed[(i + 1) % lowCornersReversed.Count],
            lowCornersReversed[(i + 2) % lowCornersReversed.Count],
            outlineThickness);
      }
    }
    
    public static void BuildColumnOutlines(
        MeshBuilder builder,
        Vector3 up,
        List<Vector3> topCorners,
        float tileHeight,
        float outlineThickness) {
      var (lowCorners, lowCornersReversed) = ExtrudeMesh.GetLowCorners(topCorners, tileHeight);
      for (int i = 0; i < topCorners.Count; i++) {
        AddColumnPrism(
            builder,
            up,
            topCorners[(i + topCorners.Count - 1) % topCorners.Count],
            topCorners[i],
            topCorners[(i + 1) % topCorners.Count],
            lowCorners[i],
            outlineThickness);
      }
    }

    // There are three line segments in play here: "foot", "calf", and "thigh",
    // between four points: "toe", "heel", "knee", "pelvis".
    // . heel _________ knee .....
    // ..... / . calf . \ ........
    // foot / .......... \ thigh .
    // ... / ............ \ ......
    // . toe ........... pelvis ..
    // This function adds a prism around the calf.
    private static void AddFacePrism(
        MeshBuilder builder,
        Vector3 up,
        Vector3 toe,
        Vector3 heel,
        Vector3 knee,
        Vector3 pelvis,
        float inscribeRadius) {
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
      builder.AddQuad(heelUpIn, heelUpOut, kneeUpOut, kneeUpIn, up);
      
      var heelDownIn = heel - upABit - heelOutABit;
      var heelDownOut = heel - upABit + heelOutABit;
      var kneeDownOut = knee - upABit + kneeOutABit;
      var kneeDownIn = knee - upABit - kneeOutABit;
      builder.AddQuad(kneeDownOut, heelDownOut, heelDownIn, kneeDownIn, -up);
      
      builder.AddQuad(kneeDownOut, kneeUpOut, heelUpOut, heelDownOut, calfOut);
      
      builder.AddQuad(heelUpIn, kneeUpIn, kneeDownIn, heelDownIn, -calfOut);
  
  
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
    private static void AddColumnPrism(
        MeshBuilder builder,
        Vector3 up,
        Vector3 topToe,
        Vector3 topHeel,
        Vector3 topKnee,
        Vector3 bottomHeel,
        float inscribeRadius) {
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
      builder.AddQuad(topHeelUpOut, topHeelUpFootOut, bottomHeelDownFootOut, bottomHeelDownOut, footOut);
      builder.AddQuad(topHeelUpCalfOut, topHeelUpOut, bottomHeelDownOut, bottomHeelDownCalfOut, calfOut);
  
      var topHeelUpCalfOutAndBackIn = topHeelUpCalfOut - calfOut.normalized * inscribeRadius;
      var bottomHeelDownCalfOutAndBackIn = bottomHeelDownCalfOut - calfOut.normalized * inscribeRadius;
      builder.AddQuad(topHeelUpCalfOutAndBackIn, topHeelUpCalfOut, bottomHeelDownCalfOut, bottomHeelDownCalfOutAndBackIn, calf);
      
      var topHeelUpFootOutAndBackIn = topHeelUpFootOut - footOut.normalized * inscribeRadius;
      var bottomHeelDownFootOutAndBackIn = bottomHeelDownFootOut - footOut.normalized * inscribeRadius;
      builder.AddQuad(topHeelUpFootOut, topHeelUpFootOutAndBackIn, bottomHeelDownFootOutAndBackIn, bottomHeelDownFootOut, foot);
    }
  }
}