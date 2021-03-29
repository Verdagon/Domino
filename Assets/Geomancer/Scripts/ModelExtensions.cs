using System;
using UnityEngine;
using Geomancer.Model;

namespace Geomancer {
  public static class ModelExtensions {
    public const float ModelToUnityMultiplier = .001f;
    
    public static Vec3 ToVec3(this Vec2 vec2) {
      return new Vec3(vec2.x, vec2.y, 0);
    }
    public static Vector3 ToUnity(this Vec3 vec3) {
      return new Vector3(vec3.x * ModelToUnityMultiplier, vec3.z * ModelToUnityMultiplier, vec3.y * ModelToUnityMultiplier);
    }
    public static Vector3[] ToUnity(this Vec3[] vec3s) {
      var unityVecs = new Vector3[vec3s.Length];
      for (int i = 0; i < vec3s.Length; i++) {
        unityVecs[i] = vec3s[i].ToUnity();
      }
      return unityVecs;
    }
  }
}
