using System.Collections.Generic;
using UnityEngine;

namespace Domino {
  public class MeshBuilder {
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<int> indices = new List<int>();
  
    public MeshBuilder() {
    }
  
    public void AddQuad(
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
    
    public void AddPolygon(
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
    
    public Mesh Build() {
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