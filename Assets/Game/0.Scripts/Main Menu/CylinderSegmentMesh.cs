using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CylinderSegmentMesh : MonoBehaviour
{
    [Header("Cylinder Settings")]
    public float radius = 3f;
    public float height = 2f;
    public int segments = 64;
    public float arcAngle = 60f;

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        int vertCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uv = new Vector2[vertCount];
        int[] triangles = new int[segments * 6];

        float angleStep = Mathf.Deg2Rad * arcAngle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = -arcAngle * 0.5f * Mathf.Deg2Rad + angleStep * i;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;

            vertices[i] = new Vector3(x, -height / 2f, z);
            vertices[i + segments + 1] = new Vector3(x, height / 2f, z);

            uv[i] = new Vector2((float)i / segments, 0f);
            uv[i + segments + 1] = new Vector2((float)i / segments, 1f);
        }

        int triIndex = 0;
        for (int i = 0; i < segments; i++)
        {
            int a = i;
            int b = i + 1;
            int c = i + segments + 1;
            int d = i + segments + 2;

            triangles[triIndex++] = a;
            triangles[triIndex++] = c;
            triangles[triIndex++] = b;

            triangles[triIndex++] = b;
            triangles[triIndex++] = c;
            triangles[triIndex++] = d;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
