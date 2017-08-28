using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace SplitMesh
{/// <summary>
/// Mesh网格数据类
/// </summary>
    public class MeshInfo
    {
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector2> uvs;
        public List<Vector3> normals;
        public List<Vector4> tangents;
        public Vector3 size, center;
        public MeshInfo()
        {
            vertices = new List<Vector3>();
            triangles = new List<int>();
            uvs = new List<Vector2>();
            normals = new List<Vector3>();
            tangents = new List<Vector4>();
            size = center = Vector3.zero;
        }
        public MeshInfo(Mesh mesh)
        {
            vertices = new List<Vector3>(mesh.vertices);
            triangles = new List<int>(mesh.triangles);
            uvs = new List<Vector2>(mesh.uv);
            normals = new List<Vector3>(mesh.normals);
            tangents = new List<Vector4>(mesh.tangents);
            center = mesh.bounds.center;
            size = mesh.bounds.size;
        }
        public void Add(Mesh mesh)
        {
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                vertices.Add(mesh.vertices[i]);
                uvs.Add(mesh.uv[i]);
                normals.Add(mesh.normals[i]);
                tangents.Add(mesh.tangents[i]);
            }
            int length = triangles.Count;
            for (int i = 0; i < mesh.triangles.Length; i++)
            {
                triangles.Add(mesh.triangles[i] + length);
            }
        }
        public void Add(Vector3 vert, Vector2 uv, Vector3 normal, Vector4 tangent)
        {
            vertices.Add(vert);
            uvs.Add(uv);
            normals.Add(normal);
            tangents.Add(tangent);
        }
        public Mesh GetMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.normals = normals.ToArray();
            mesh.tangents = tangents.ToArray();
            mesh.triangles = triangles.ToArray();
            return mesh;
        }
        //public void MapperSphere(Rect range){}
        public void MapperCube(Rect range)
        {
            if (uvs.Count < vertices.Count)
                uvs = new List<Vector2>(vertices.Count);
            int count = triangles.Count / 3;
            for (int i = 0; i < count; i++)
            {
                int _i0 = triangles[i * 3];
                int _i1 = triangles[i * 3 + 1];
                int _i2 = triangles[i * 3 + 2];

                Vector3 v0 = vertices[_i0] - center + size / 2f;
                Vector3 v1 = vertices[_i1] - center + size / 2f;
                Vector3 v2 = vertices[_i2] - center + size / 2f;
                v0 = new Vector3(v0.x / size.x, v0.y / size.y, v0.z / size.z);
                v1 = new Vector3(v1.x / size.x, v1.y / size.y, v1.z / size.z);
                v2 = new Vector3(v2.x / size.x, v2.y / size.y, v2.z / size.z);

                Vector3 a = v0 - v1;
                Vector3 b = v2 - v1;
                Vector3 dir = Vector3.Cross(a, b);
                float x = Mathf.Abs(Vector3.Dot(dir, Vector3.right));
                float y = Mathf.Abs(Vector3.Dot(dir, Vector3.up));
                float z = Mathf.Abs(Vector3.Dot(dir, Vector3.forward));
                if (x > y && x > z)
                {
                    uvs[_i0] = new Vector2(v0.z, v0.y);
                    uvs[_i1] = new Vector2(v1.z, v1.y);
                    uvs[_i2] = new Vector2(v2.z, v2.y);
                }
                else if (y > x && y > z)
                {
                    uvs[_i0] = new Vector2(v0.x, v0.z);
                    uvs[_i1] = new Vector2(v1.x, v1.z);
                    uvs[_i2] = new Vector2(v2.x, v2.z);
                }
                else if (z > x && z > y)
                {
                    uvs[_i0] = new Vector2(v0.x, v0.y);
                    uvs[_i1] = new Vector2(v1.x, v1.y);
                    uvs[_i2] = new Vector2(v2.x, v2.y);
                }
                uvs[_i0] = new Vector2(range.xMin + (range.xMax - range.xMin) * uvs[_i0].x, range.yMin + (range.yMax - range.yMin) * uvs[_i0].y);
                uvs[_i1] = new Vector2(range.xMin + (range.xMax - range.xMin) * uvs[_i1].x, range.yMin + (range.yMax - range.yMin) * uvs[_i1].y);
                uvs[_i2] = new Vector2(range.xMin + (range.xMax - range.xMin) * uvs[_i2].x, range.yMin + (range.yMax - range.yMin) * uvs[_i2].y);
            }
        }
        public void CombineVertices(float range)
        {
            range *= range;
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = i + 1; j < vertices.Count; j++)
                {
                    bool dis = (vertices[i] - vertices[j]).sqrMagnitude < range;
                    bool uv = (uvs[i] - uvs[j]).sqrMagnitude < range;
                    bool dir = Vector3.Dot(normals[i], normals[j]) > 0.999f;
                    if (dis && uv && dir)
                    {
                        for (int k = 0; k < triangles.Count; k++)
                        {
                            if (triangles[k] == j)
                                triangles[k] = i;
                            if (triangles[k] > j)
                                triangles[k]--;
                        }
                        vertices.RemoveAt(j);
                        normals.RemoveAt(j);
                        tangents.RemoveAt(j);
                        uvs.RemoveAt(j);
                    }
                }
            }
        }
        public void Reverse()
        {
            int count = triangles.Count / 3;
            for (int i = 0; i < count; i++)
            {
                int t = triangles[i * 3 + 2];
                triangles[i * 3 + 2] = triangles[i * 3 + 1];
                triangles[i * 3 + 1] = t;
            }
            count = vertices.Count;
            for (int i = 0; i < count; i++)
            {
                normals[i] *= -1;
                Vector4 tan = tangents[i];
                tan.w = -1;
                tangents[i] = tan;
            }
        }

        public static Vector4 CalculateTangent(Vector3 normal)
        {
            Vector3 tan = Vector3.Cross(normal, Vector3.up);
            if (tan == Vector3.zero)
                tan = Vector3.Cross(normal, Vector3.forward);
            tan = Vector3.Cross(tan, normal);
            return new Vector4(tan.x, tan.y, tan.z, 1.0f);
        }
    }
}