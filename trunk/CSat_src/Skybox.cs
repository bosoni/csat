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

namespace CSat
{
    public class Skybox
    {
        Object3D skybox = new Object3D();

        public void Dispose()
        {
            skybox.Dispose();
        }

        /// <summary>
        /// lataa skybox.
        /// </summary>
        /// <param name="skyName">skyboxin nimi, eli esim plainsky_  jos tiedostot on plainsky_front.jpg, plainsky_back.jpg jne</param>
        /// <param name="ext">tiedoston p��te, eli jpg, png, dds, ..</param>
        /// <param name="scale"></param>
        public void Load(string skyName, string ext, float scale)
        {
            Object3D.Textured = false; // �l� lataa objektin textureita automaattisesti
            skybox = new Object3D("skybox.obj", scale, scale, scale); // lataa skybox
            Object3D.Textured = true; // seuraava saa ladatakin..

            skybox.BoundingMode(BoundingVolume.None);
            string[] side = { "bottom", "left", "back", "right", "top", "front" };

            for (int q = 0; q < 6; q++)
            {
                string fileName = skyName + side[q] + "." + ext;
                Texture newSkyTex = new Texture();
                Texture.WrapModeS = TextureWrapMode.ClampToEdge;
                Texture.WrapModeT = TextureWrapMode.ClampToEdge;

                newSkyTex = Texture.Load(fileName);

                // etsi sivun materiaali
                string mat = skybox.GetObject(q).materialName;

                Material matInf = Material.GetMaterial(mat);

                if (matInf != null)
                {
                    // korvaa vanhat texturet
                    matInf.diffuseTex = newSkyTex;
                }
            }
            Texture.WrapModeS = TextureWrapMode.Repeat;
            Texture.WrapModeT = TextureWrapMode.Repeat;

        }

        // rendaa taivas
        public void Render(Camera camera)
        {
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Rotate(-camera.rotation.X, 1.0f, 0, 0);
            GL.Rotate(-camera.rotation.Y, 0, 1.0f, 0);
            GL.Rotate(-camera.rotation.Z, 0, 0, 1.0f);

            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);

            skybox.Render();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);

            GL.PopMatrix();
        }

        public void Render()
        {
            Render(Camera.cam);
        }

    }
}