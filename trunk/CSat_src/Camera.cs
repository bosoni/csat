#region --- License ---
/*
Copyright (C) 2008 mjt[matola@sci.fi]

This file is part of CSat - small C# 3D-library

CSat is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
 
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.
 
You should have received a copy of the GNU Lesser General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.


-mjt,  
email: matola@sci.fi
*/
#endregion

using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    public class Camera : ObjectInfo
    {
        public static Camera cam = null;

        public Camera()
        {
            name = "camera";
        }
        public Camera(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// p�ivit� kameran paikka XZ tasolla.
        /// </summary>
        public void UpdateXZ()
        {
            Camera.cam = this;
            GL.LoadIdentity();
            GL.Rotate(-rotation.X, 1.0f, 0, 0);
            GL.Rotate(-rotation.Y, 0, 1.0f, 0);
            GL.Rotate(-rotation.Z, 0, 0, 1.0f);
            GL.Translate(-position.X, -position.Y, -position.Z);
        }

        /// <summary>
        /// p�ivit� kameran paikka (6DOF kamera)
        /// </summary>
        public void Update6DOF()
        {
            Camera.cam = this;

            GL.LoadIdentity();

            Vector3 vp; // view point
            vp = view + position;
            Glu.LookAt(position.X, position.Y, position.Z, vp.X, vp.Y, vp.Z, up.X, up.Y, up.Z);
        }

        /// <summary>
        /// k��nn� kuvakulma pos:iin
        /// </summary>
        /// <param name="pos"></param>
        public void LookAt(Vector3 pos)
        {
            Camera.cam = this;
            GL.LoadIdentity();
            Glu.LookAt(position.X, position.Y, position.Z, pos.X, pos.Y, pos.Z, up.X, up.Y, up.Z);
        }

        /// <summary>
        /// k��nn� kuvakulma xyz:aan
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void LookAt(float x, float y, float z)
        {
            Camera.cam = this;
            GL.LoadIdentity();
            Glu.LookAt(position.X, position.Y, position.Z, x, y, z, up.X, up.Y, up.Z);
        }
    }
}
