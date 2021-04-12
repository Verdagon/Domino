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
    public static Color ColorToUnity(this Vec4i vec4) {
      return new Color(
          vec4.x / 255f, vec4.y / 255f, vec4.z / 255f, vec4.w / 255f);
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
