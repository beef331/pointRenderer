using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PointRenderer
{
    public class PointRenderer : MonoBehaviour
    {
        public Scanner scanner;
        void OnRenderObject()
        {
            scanner.Material.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Points, scanner.PointCount);
        }
    }
}