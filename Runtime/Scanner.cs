﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
namespace PointRenderer
{
	public class Scanner : MonoBehaviour
	{
		[SerializeField]
		private int raysPerFrame = 30;
		[SerializeField]
		private Gradient gradient;
		[SerializeField]
		private float maxDist = 50;
		public Material Material { get; private set; }

		[SerializeField]
		private float size = .01f;
		[SerializeField]
		private Texture2D mask;
		[SerializeField]
		private Vector3 lightDir = new Vector3(1, 1, 1);

		private VecThree[] points;
		private int currentIndex = 0;
		private ComputeBuffer buffer;

		public int PointCount { get; private set; } = 10000;
		private PointRenderer pointRenderer;

		private Camera cam;


		void OnEnable()
		{
			points = new VecThree[PointCount];
			Material = new Material(Shader.Find("Hidden/Scanner"));
			buffer = new ComputeBuffer(points.Length, 24);
			Texture2D tex = new Texture2D(100, 1, TextureFormat.RGBA32, 0, true);
			tex.wrapMode = TextureWrapMode.Clamp;
			for (int i = 0; i < 100; i++)
			{
				tex.SetPixel(i, 0, gradient.Evaluate((float)(i) / 100));
			}
			tex.Apply();
			Material.SetTexture("DIST_GRADIENT", tex);
			Material.SetBuffer("POINTS", buffer);
			cam = GetComponent<Camera>();
			if (!pointRenderer)
			{
				GameObject newGO = new GameObject("Point Renderer");
				pointRenderer = newGO.AddComponent<PointRenderer>();
				pointRenderer.scanner = this;
			}
		}

		void Update()
		{
			if (buffer == null) return;
			Ray ray;
			Vector2 point;
			for (int i = 0; i < raysPerFrame; i++)
			{
				point = new Vector2(Random.value, Random.value);
				ray = cam.ViewportPointToRay(point);
				ray.origin = transform.position;
				if (Physics.Raycast(ray, out RaycastHit hit, maxDist, 0xFFFFFF, QueryTriggerInteraction.Ignore))
				{
					if (currentIndex >= points.Length)
					{
						currentIndex = 0;
					}
					points[currentIndex++] = hit;
				}
			}

			buffer.SetData(points.OrderByDescending(x => Vector3.SqrMagnitude(new Vector3(x.x, x.y, x.z) - transform.position)).ToArray());

			Material.SetFloat("MAX_DIST", maxDist);
			Material.SetFloat("_Size", size);
			Material.SetVector("UpDir", transform.up);
			Material.SetVector("RightDir", transform.right);
			Material.SetTexture("MASK", mask);
			Material.SetVector("_LightDir", lightDir);
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
}
