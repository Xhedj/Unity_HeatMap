using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    Vector3[] modifiedVertices;
    int[] triangles;
    [SerializeField] private int xSize = 2; //maybe public?
    [SerializeField] private int zSize = 2; //maybe public?
    [SerializeField] private int Volume_per_Sensor = 2;
    [SerializeField] private GameObject ErrorWindow;
    Color[] colours;
    public Gradient gradient;
    private int xDimension;
    private int zDimension;
    void Start()
    {
        //Compute effective dimension of the grid. Take in account the vertices for the volume
        xDimension = (Volume_per_Sensor * (xSize - 1) + xSize);
        zDimension = (Volume_per_Sensor * (zSize - 1) + zSize);

        vertices = new Vector3[ (xDimension) * (zDimension) ];
        modifiedVertices = new Vector3[(xDimension) * (zDimension)];

        mesh = new Mesh();
        colours = new Color[(xDimension) * (zDimension)];

        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
    }


    void CreateShape()
    {
        for (int i = 0, z = 0; z < zDimension; z++)
        {
            for (int x = 0; x < xDimension; x++)
            {
                vertices[i] = new Vector3(x, 0, z);
                i++;
            }
        }
        modifiedVertices = vertices;
        triangles = new int[xDimension * zDimension * 6];
        int vert = 0;
        int tris = 0;
        Debug.Log(triangles.Length);
        for (int z = 0; z < zDimension-1; z++)
        {
            for (int x = 0; x < xDimension - 1; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xDimension;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xDimension;
                triangles[tris + 5] = vert + xDimension + 1;

                vert++;
                tris += 6;
            }
            vert++;
        }


    }

    void OnConnectionEvent(bool success)
    {
        //Debug.Log(success ? "Connected" : "Disconnected");
        if (success)
        {
            ErrorWindow.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {

            ErrorWindow.SetActive(true);
            Time.timeScale = 0;
        }
            
    }

    void OnMessageArrived(string msg)
    {
        
        string[] values = msg.Split(' ');
        Debug.Log($"Length of message: {values.Length}");
        //for (int i = 0; i < values.Length; i++)
        //{
        //    Debug.Log($"Sensor {i}, value: {values[i]}");
        //}
        float[] parsed_values = new float[xSize * zSize];
        for (int i = 0; i < values.Length; i++)
        {
            parsed_values[i] = float.Parse(values[i], CultureInfo.InvariantCulture.NumberFormat)/511;
        }

        //Assign the value of each sensor to the appropriate vertex
        for (int z = 0, x = 0, step = 0; z < zDimension; z+= (Volume_per_Sensor + 1))
        {
            for (int n = 0; n < xSize; n++)
            {
                modifiedVertices[x] = new Vector3(vertices[x].x, parsed_values[n+step], vertices[x].z);
                colours[x] = gradient.Evaluate(parsed_values[n+step]);
                x += Volume_per_Sensor + 1;
            }
            x -= (Volume_per_Sensor + 1);
            x += (xDimension*Volume_per_Sensor) +1 ;
            step += xSize;
        }

        //Interpolate inbetween vertices
        int temp_x = 0;
        
        for(int z=0, i = 1; z < zDimension; z++)
        {
            for(; temp_x < xDimension*i; temp_x++)
            {
                if (z == ((Volume_per_Sensor + 1) * ((int)z / (Volume_per_Sensor + 1))))
                {
                    if (temp_x != ((Volume_per_Sensor + 1) * ((int)temp_x / (Volume_per_Sensor + 1))))
                    {
                        modifiedVertices[temp_x] = new Vector3(vertices[temp_x].x, Mathf.Lerp(vertices[temp_x - 1].y, vertices[temp_x + 1].y, 0.5f), vertices[temp_x].z);
                        colours[temp_x] = gradient.Evaluate(Mathf.Lerp(vertices[temp_x - 1].y, vertices[temp_x + 1].y, 0.5f));
                    }

                }
                else
                {
                    
                    modifiedVertices[temp_x] = new Vector3(vertices[temp_x].x, Mathf.Lerp(vertices[temp_x - zDimension].y, vertices[temp_x + zDimension].y, 0.2f), vertices[temp_x].z);
                    colours[temp_x] = gradient.Evaluate(Mathf.Lerp(vertices[temp_x - zDimension].y, vertices[temp_x + zDimension].y, 0.2f));
                }
            }
            i++;
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        RecalculateMeshes();
    }

    private void OnDrawGizmos()
    {
        if (vertices != null)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Gizmos.DrawSphere(vertices[i], .1f);
            }
        }

    }
    void RecalculateMeshes()
    {
        mesh.colors = colours;
        mesh.vertices = modifiedVertices;
        mesh.RecalculateNormals();
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }





}
