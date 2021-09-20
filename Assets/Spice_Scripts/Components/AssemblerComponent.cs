using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Assembler
{
    public class AssemblerComponent : MonoBehaviour
    {
        [Serializable]
        public class SnapPoint
        {
            public Vector3 point;
            public Vector3 normal;

            public int ID;

            public SnapPoint(Vector3 p, Vector3 n, int i)
            {
                point = p;
                normal = n.normalized;
                ID = i;
            }
        }

        public static AssemblerComponent coreComp;

        [Tooltip("Is this part the center that cannot be destroyed?")]
        public bool core;

        public SnapPoint[] points;

        [HideInInspector] public AssemblerPoint[] pointColliders;
        
        [HideInInspector] public bool connected;
    }
    
    public class PointHitCheck
    {
        public float dist;
        public AssemblerPoint point;
        
        public PointHitCheck(float hitDistance, AssemblerPoint hitPoint)
        {
            dist = hitDistance;
            point = hitPoint;
        }
    }
}
