using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    private Mesh mesh;
    private MeshCollider meshCollider;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    public int xSize = 20;
    public int zSize = 20;
    public float tileSize = 1f;

    [Header("Perlin Noise Settings")]
    public float noiseScale = 2f;
    public float mountainHeight = 12f;
    public float noiseOffsetX = 0f;
    public float noiseOffsetZ = 0f;

    [Header("Valley Settings")]
    public float valleyRadius = 0.3f;
    public float ridgeSharpness = 2.5f;
    public float valleyFalloff = 5f;
    public float noiseContrast = 2.5f;

    [Header("Pillar Settings")]
    public float radius = 0.5f;
    public Vector2 sampleRegionSize = new Vector2(0, 0);
    public int rejectionSamples = 30;
    private List<Vector2> pillarPositions;
    public GameObject pillarPrefab;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        meshCollider = GetComponent<MeshCollider>();

        noiseOffsetX = Random.Range(0f, 999f);
        noiseOffsetZ = Random.Range(0f, 999f);

        sampleRegionSize = new Vector2(200 ,200);

        CreateShape();
        UpdateMesh();
        GeneratePillars();

        NavMeshSurface[] navMeshSurfaces = GetComponents<NavMeshSurface>();
        navMeshSurfaces[0].BuildNavMesh();
        navMeshSurfaces[1].BuildNavMesh();
    }


    // Generate a heightmap-based mesh with a central valley and surrounding mountains using Perlin noise.
    void CreateShape()
    {   // Create vertices
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float height = GetHeight(x, z);
                vertices[i] = new Vector3(x * tileSize, height, z * tileSize);
                i++;
            }
        }

        // Create triangles
        triangles = new int[xSize * zSize * 6];
        for (int z = 0, vert = 0, tris = 0; z < zSize; z++, vert++)
        {
            for (int x = 0; x < xSize; x++, vert++, tris += 6)
            {
                triangles[tris + 0] = vert;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;
            }
        }

        // Create UVs
        uvs = new Vector2[vertices.Length];
        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);
                i++;
            }
        }
    }

    // Calculate height based on distance from arena center and Perlin noise
    float GetHeight(int x, int z)
    {
        // Calculate distance from center of the arena
        float xNorm = ((float)x / xSize - 0.5f) * 2f;
        float zNorm = ((float)z / zSize - 0.5f) * 2f;
        float distFromCentre = Mathf.Max(Mathf.Abs(xNorm), Mathf.Abs(zNorm));

        // Valley mesh variables 
        float valleyMask = Mathf.InverseLerp(valleyRadius, 1f, distFromCentre);
        valleyMask = Mathf.Pow(Mathf.Max(valleyMask, 0f), valleyFalloff);

        // Perlin noise samples
        float sampleX = (x + noiseOffsetX) / xSize * noiseScale;
        float sampleZ = (z + noiseOffsetZ) / zSize * noiseScale;

        // Perlin noise settings
        float noise = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float maxAmplitude = 0f;

        // Generate noise used to create mountains around the central valley
        for (int o = 0; o < 4; o++)
        {
            float raw = Mathf.PerlinNoise(sampleX * frequency, sampleZ * frequency);

            raw = 1f - Mathf.Abs(raw * 2f - 1f);
            raw = Mathf.Pow(Mathf.Max(raw, 0f), ridgeSharpness); // clamp before pow

            noise += raw * amplitude;
            maxAmplitude += amplitude;
            amplitude *= 0.45f;
            frequency *= 2.1f;
        }

        // Normalize noise
        noise /= maxAmplitude;
        noise = Mathf.Pow(Mathf.Max(noise, 0f), noiseContrast); // clamp before pow

        // Combine noise and valley mask to create the heightmap for the mesh vertices
        float height = noise * valleyMask * mountainHeight;
        
        // Final safety net — never let NaN reach the mesh
        return float.IsNaN(height) ? 0f : height;
    }


    // Procedurally place pillars using Poisson Disc Sampling
    void GeneratePillars()
    {
        // Generate pillar positions using Poisson Disc Sampling 
        pillarPositions = PoissonDiscSampler.GeneratePoints(radius, sampleRegionSize, rejectionSamples);
        // Calculate the mesh's world position to place pillars
        Vector3 meshOrigin = transform.position;
        Vector3 valleyOrigin = meshOrigin + new Vector3((xSize * tileSize - sampleRegionSize.x - pillarPrefab.transform.lossyScale.x) / 2f, 0f, 
                                                        (zSize * tileSize - sampleRegionSize.y - pillarPrefab.transform.lossyScale.z) / 2f);

        // Place pillars at the generated positions
        foreach (Vector2 point in pillarPositions)
        {
            Vector3 raycastOrigin = valleyOrigin + new Vector3(point.x, 200f, point.y);

            // Designate an area in the center of the arena where pillars won't spawn.
            // This ensures pillas won't spawn at the players' starting positions
            if (raycastOrigin.x >= -2f && raycastOrigin.x <= 2f && raycastOrigin.z >= -2f && raycastOrigin.z <= 2f)
            {
                continue;
            }

            // Raycast down to find the terrain height to place pillar at the correct position
            if (Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, 400f))
            {
                Vector3 pillarPos = new Vector3(hit.point.x, pillarPrefab.transform.localScale.y / 2f, hit.point.z);
                Instantiate(pillarPrefab, pillarPos, Quaternion.identity, transform);
            }
        }
    }
    

    // Update the mesh with the generated vertices, triangles, and UVs, and update the mesh collider to match the new mesh
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    // Credit: Sebastian Lague's Poisson Disc Sampling implementation - https://www.youtube.com/watch?v=7WcmyxyFO7o
}