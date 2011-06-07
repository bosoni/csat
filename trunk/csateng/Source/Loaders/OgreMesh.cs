#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using System.Collections.Generic;
using System.Xml;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public class OgreMesh : Model
    {
        #region ogremesh loader
        public OgreMesh() { }
        public OgreMesh(string name, string fileName)
        {
            LoadMesh(name, fileName);
        }

        public static OgreMesh Load(string name, string fileName)
        {
            OgreMesh mesh = new OgreMesh(name, fileName);
            return mesh;
        }

        void LoadMesh(string name, string fileName)
        {
            Name = name;
            XmlDocument XMLDoc = null;
            XmlElement XMLRoot;

            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(Settings.ModelDir + fileName))
                {
                    // tiedosto muistiin
                    string data = file.ReadToEnd();

                    // Open the .scene File
                    XMLDoc = new XmlDocument();
                    XMLDoc.LoadXml(data);
                }
            }
            catch (Exception e)
            {
                Util.Error(e.ToString());
            }

            // Validate the File
            XMLRoot = XMLDoc.DocumentElement;
            if (XMLRoot.Name != "mesh")
            {
                Util.Error("Error [" + fileName + "]: Invalid .mesh File. Missing <mesh>");
            }

            bool isPath = false; // jos meshi on pathi
            if (name.StartsWith("Path_")) isPath = true;

            // Process the mesh
            processMesh(XMLRoot, isPath);

            if (isPath == false)
            {
                Vbo = new VBO();
                Vbo.DataToVBO(VertexBuffer, IndexBuffer, VBO.VertexMode.UV1);

                Boundings = new BoundingSphere();
                Boundings.CreateBoundingVolume(this);

                // lataa shader
                string shader = Material.ShaderName;
                if (shader != "")
                {
                    Shader = GLSLShader.Load(shader, null);
                }
            }
            else
            {
                Path path = new Path();
                path.AddPath(name, VertexBuffer);
            }
        }

        void processMesh(XmlElement XMLRoot, bool path)
        {
            XmlElement pElement = (XmlElement)XMLRoot.SelectSingleNode("submeshes");
            if (pElement != null) processSubmeshes(pElement, path);
        }

        void processSubmeshes(XmlElement XMLNode, bool path)
        {
            XmlElement pElement = (XmlElement)XMLNode.SelectSingleNode("submesh");
            while (pElement != null)
            {
                processSubmesh(pElement, null, path);
                XmlNode nextNode = pElement.NextSibling;
                pElement = nextNode as XmlElement;
                while (pElement == null && nextNode != null)
                {
                    nextNode = nextNode.NextSibling;
                    pElement = nextNode as XmlElement;
                }
            }
        }

        void processSubmesh(XmlElement XMLNode, SceneNode pParent, bool path)
        {
            XmlElement pElement;
            if (path == false)
            {
                if (MaterialName == "")
                {
                    MaterialName = XML.GetAttrib(XMLNode, "material");
                    Material = MaterialInfo.GetMaterial(MaterialName);
                }

                pElement = (XmlElement)XMLNode.SelectSingleNode("faces");
                if (pElement != null)
                {
                    processFaces(pElement);
                }
            }

            pElement = (XmlElement)XMLNode.SelectSingleNode("geometry");
            if (pElement != null)
            {
                processGeometry(pElement, path);
            }
        }

        void processFaces(XmlElement XMLNode)
        {
            int numOfFaces = (int)XML.GetAttribReal(XMLNode, "count");
            IndexBuffer = new ushort[numOfFaces * 3];
            XmlElement pElement = (XmlElement)XMLNode.SelectSingleNode("face");
            for (int q = 0; q < numOfFaces; q++)
            {
                Vector3 f = XML.ParseFace(pElement);
                IndexBuffer[q * 3] = (ushort)f.X;
                IndexBuffer[q * 3 + 1] = (ushort)f.Y;
                IndexBuffer[q * 3 + 2] = (ushort)f.Z;
                pElement = (XmlElement)pElement.NextSibling;
            }
        }

        void processGeometry(XmlElement XMLNode, bool path)
        {
            int numOfVerts = (int)XML.GetAttribReal(XMLNode, "vertexcount");
            VertexBuffer = new Vertex[numOfVerts];
            XmlElement pElement = (XmlElement)XMLNode.SelectSingleNode("vertexbuffer");
            pElement = (XmlElement)pElement.SelectSingleNode("vertex");
            for (int q = 0; q < numOfVerts; q++)
            {
                processVertex(pElement, q);
                pElement = (XmlElement)pElement.NextSibling;
            }
        }
        void processVertex(XmlElement XMLNode, int vert)
        {
            XmlElement pElement = (XmlElement)XMLNode.SelectSingleNode("position");
            if (pElement != null) VertexBuffer[vert].Position = XML.ParseVector3(pElement);

            pElement = (XmlElement)XMLNode.SelectSingleNode("normal");
            if (pElement != null) VertexBuffer[vert].Normal = XML.ParseVector3(pElement);

            pElement = (XmlElement)XMLNode.SelectSingleNode("texcoord");
            if (pElement != null)
            {
                Vector2 uv = XML.ParseUV(pElement);
                VertexBuffer[vert].UV.X = uv.X;
                VertexBuffer[vert].UV.Y = uv.Y;
            }

        }
        #endregion

        public override void Dispose()
        {
            if (Name != "")
            {
                if (Shader != null) Shader.Dispose();
                if (Material != null) Material.Dispose();
                if (Vbo != null) Vbo.Dispose();
                Log.WriteLine("Disposed: " + Name + " (mesh)", true);
                Name = "";
            }
        }

        protected override void RenderModel()
        {
            GL.LoadMatrix(ref Matrix);
            RenderMesh();
        }

        public override void Render()
        {
            base.Render(); // renderoi objektin ja kaikki siihen liitetyt objektit
        }

        
        public void RenderMesh()
        {
            if (Vbo == null) return;

            if (DoubleSided) GL.Disable(EnableCap.CullFace);
            if (ShadowMapping.ShadowPass && CastShadow)
            {
                Vbo.Render();
            }
            else
            {
                Material.SetMaterial();
                if (Shader != null) Shader.UseProgram();
                else GLSLShader.UseProgram(0);

                GL.MatrixMode(MatrixMode.Texture);
                GL.ActiveTexture(TextureUnit.Texture0 + Settings.SHADOW_TEXUNIT);
                GL.PushMatrix();
                if (WorldMatrix != null) GL.MultMatrix(ref WorldMatrix);
                GL.MatrixMode(MatrixMode.Modelview);

                Vbo.Render();

                GL.MatrixMode(MatrixMode.Texture);
                GL.ActiveTexture(TextureUnit.Texture0 + Settings.SHADOW_TEXUNIT);
                GL.PopMatrix();
                GL.MatrixMode(MatrixMode.Modelview);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            if (DoubleSided) GL.Enable(EnableCap.CullFace);
        }
    }
}