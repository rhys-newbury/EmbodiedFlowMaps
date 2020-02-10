using System;
using UnityEngine;

namespace Obi
{
	public struct ObiPathFrame
    {
        public enum Axis
        {
            X = 0,
            Y = 1,
            Z = 2
        }

		public Vector3 position;

        public Vector3 tangent;
        public Vector3 normal;
        public Vector3 binormal;

		public Vector4 color;
        public float thickness;
       
        public ObiPathFrame(Vector3 position, Vector3 tangent, Vector3 normal, Vector3 binormal, Vector4 color, float thickness){
			this.position = position;
			this.normal = normal;
			this.tangent = tangent;
            this.binormal = binormal;
			this.color = color;
            this.thickness = thickness;
		}

        public void Reset()
        {
            position = Vector3.zero;
            tangent = Vector3.forward;
            normal = Vector3.up;
            binormal = Vector3.right;
            color = Color.white;
            thickness = 0;
        }

		public static ObiPathFrame operator +(ObiPathFrame c1, ObiPathFrame c2) 
	    {
            return new ObiPathFrame(c1.position + c2.position,c1.tangent + c2.tangent,c1.normal + c2.normal,c1.binormal + c2.binormal,c1.color + c2.color, c1.thickness + c2.thickness);
	    }

		public static ObiPathFrame operator *(float f,ObiPathFrame c) 
	    {
            return new ObiPathFrame(c.position * f, c.tangent * f, c.normal * f, c.binormal * f,c.color * f, c.thickness * f);
	    }

        public void SetTwist(float twist)
        {
            Quaternion twistQ = Quaternion.AngleAxis(twist, tangent);
            normal = twistQ * normal;
            binormal = twistQ * binormal;
        }

        public void SetTwistAndTangent(float twist, Vector3 tangent)
        {
            this.tangent = tangent;
            normal = new Vector3(tangent.y, tangent.x, 0).normalized;
            binormal = Vector3.Cross(normal, tangent);

            Quaternion twistQ = Quaternion.AngleAxis(twist, tangent);
            normal = twistQ * normal;
            binormal = twistQ * binormal;
        }

        public void Transport(ObiPathFrame frame, float twist)
        {
            // Calculate delta rotation:
            Quaternion rotQ = Quaternion.FromToRotation(tangent, frame.tangent);
            Quaternion twistQ = Quaternion.AngleAxis(twist, frame.tangent);
            Quaternion finalQ = twistQ * rotQ;

            // Rotate previous frame axes to obtain the new ones:
            normal = finalQ * normal;
            binormal = finalQ * binormal;
            tangent = frame.tangent;
            position = frame.position;
            thickness = frame.thickness;
            color = frame.color;
        }

        public void Transport(Vector3 newPosition, Vector3 newTangent, float twist)
        {
            // Calculate delta rotation:
            Quaternion rotQ = Quaternion.FromToRotation(tangent, newTangent);
            Quaternion twistQ = Quaternion.AngleAxis(twist, newTangent);
            Quaternion finalQ = twistQ * rotQ;

            // Rotate previous frame axes to obtain the new ones:
            normal = finalQ * normal;
            binormal = finalQ * binormal;
            tangent = newTangent;
            position = newPosition;

        }

        // Transport, hinting the normal.
        public void Transport(Vector3 newPosition, Vector3 newTangent, Vector3 newNormal, float twist)
        {
            normal = Quaternion.AngleAxis(twist, newTangent) * newNormal;
            tangent = newTangent;
            binormal = Vector3.Cross(normal, tangent);
            position = newPosition;
        }

        public Matrix4x4 ToMatrix(Axis mainAxis)
        {
            Matrix4x4 basis = new Matrix4x4();

            int xo = ((int)mainAxis) % 3 * 4;
            int yo = ((int)mainAxis + 1) % 3 * 4;
            int zo = ((int)mainAxis + 2) % 3 * 4;

            basis[xo]     = tangent[0];
            basis[xo + 1] = tangent[1];
            basis[xo + 2] = tangent[2];

            basis[yo]     = binormal[0];
            basis[yo + 1] = binormal[1];
            basis[yo + 2] = binormal[2];

            basis[zo]     = normal[0];
            basis[zo + 1] = normal[1];
            basis[zo + 2] = normal[2];

            return basis;
        }

        public void DebugDraw(float size)
        {
            Debug.DrawRay(position, binormal * size, Color.red);
            Debug.DrawRay(position, normal * size, Color.green);
            Debug.DrawRay(position, tangent * size, Color.blue);
        }
	}
}

