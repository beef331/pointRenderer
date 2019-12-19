using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Scanner : MonoBehaviour
{
    [SerializeField]
    private int raysPerFrame = 30;
    [SerializeField]
    private Gradient gradient;
    [SerializeField]
    private float maxDist = 50;
    public static Material material;
    [SerializeField]
    private float size = .01f;
    [SerializeField]
    private Vector3 lighDir;
    [SerializeField]
    private Texture2D mask;

    private bool lighting = false;

    private Vector3 lastPos;
    private Quaternion lastRot;

    private VecThree[] points;
    private int currentIndex = 0;
    private ComputeBuffer buffer;

    public static int pointCount = 10000;

    private RaycastHit[] rayHit = new RaycastHit[1];
    private PointRenderer pointRenderer;
    
    private Camera cam;
    

    void OnEnable()
    {
        points = new VecThree[pointCount];
        material = new Material(Shader.Find("Unlit/Scanner"));
        buffer = new ComputeBuffer(points.Length, 24);
        Texture2D tex = new Texture2D(100, 1, TextureFormat.RGBA32, 0, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int i = 0; i < 100; i++)
        {
            tex.SetPixel(i, 0, gradient.Evaluate((float)(i) / 100));
        }
        tex.Apply();
        material.SetTexture("DIST_GRADIENT", tex);
        material.SetBuffer("POINTS", buffer);
        cam = GetComponent<Camera>();
        if(!pointRenderer){
            GameObject newGO = new GameObject("Point Renderer");
            pointRenderer = newGO.AddComponent<PointRenderer>();
        }
    }

    void Update()
    {
        if (buffer == null) return;
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        transform.position += (transform.forward * input.y + transform.right * input.x) * 10f * Time.deltaTime;

        float yRot = 0;


        if (Input.GetKey(KeyCode.Q)) yRot -= 1;
        if (Input.GetKey(KeyCode.E)) yRot += 1;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            lighting = !lighting;
            if (lighting)
            {
                material.EnableKeyword("LIGHTING_ON");
            }
            else
            {
                material.DisableKeyword("LIGHTING_ON");
            }
        }


        transform.Rotate(yRot * 180 * transform.up * Time.deltaTime);

        Ray ray;
        Vector2 point;
        for (int i = 0; i < raysPerFrame; i++)
        {
            point = new Vector2(Random.value, Random.value);
            ray = cam.ViewportPointToRay(point);
            if (Physics.RaycastNonAlloc(ray, rayHit, Mathf.Infinity, 0xFFFFFF, QueryTriggerInteraction.Ignore) == 1)
            {
                if (currentIndex >= points.Length)
                {
                    currentIndex = 0;
                }
                points[currentIndex++] = rayHit[0];
                Debug.DrawLine(transform.position, rayHit[0].point);
            }
        }
        buffer.SetData(points);

        material.SetFloat("MAX_DIST", maxDist);
        material.SetFloat("_Size", size);
        material.SetBuffer("POINTS", buffer);
        material.SetVector("_LightDir", lighDir);
        material.SetVector("UpDir", transform.up);
        material.SetVector("RightDir", transform.right);
        material.SetTexture("MASK", mask);
        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    public struct VecThree
    {
        public float x, y, z, xNorm, yNorm, zNorm;
        public VecThree(float x, float y, float z, float xNorm, float yNorm, float zNorm)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.xNorm = xNorm;
            this.yNorm = yNorm;
            this.zNorm = zNorm;

        }

        public static implicit operator VecThree(RaycastHit i)
        {
            return new VecThree(i.point.x, i.point.y, i.point.z, i.normal.x, i.normal.y, i.normal.z);
        }

    }


}
