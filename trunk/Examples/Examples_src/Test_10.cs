﻿#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

// path testi
// ladataan kaupunki, camerapath joka liitetään kameraan, sit parit muut pathit jotka liitetään autoihin.

// skyboxin tilalla ladataan puolipallo johon texturointi. enemmän polyja mutta vain 1 texture.

// auton reitti on vähän miten sattuu joten siihen pieni korjaus:
//  xz tasossa tein sen reitin ja jaksanu alkaa säätää y arvoja blenderissä joten etsitään 
// oikea y xz kohdalta.

using System;
using System.Drawing;
using CSat;
using CSLoader;
using OpenTK;
using OpenTK.Math;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Platform;

namespace CSatExamples
{

    class Game10 : GameWindow
    {
        Skydome skydome = new Skydome();

        Camera cam = new Camera();
        ITextPrinter printer = new TextPrinter();
        TextureFont font = new TextureFont(new Font(FontFamily.GenericSerif, 24.0f));
        public Game10(int width, int height) : base(width, height, GraphicsMode.Default, "City") { }

        Group world = new Group("world"); // tänne lisäillään kaikki kamat
        Object3D city = new Object3D();
        Object3D car = new Object3D();
        MD5Model model = new MD5Model();
        MD5Model.MD5Animation[] anim = new MD5Model.MD5Animation[5];

        Object3D carPath = new Object3D(), cameraPath = new Object3D();

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(0, 0, 0, 0);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.CullFace);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            Mouse.ButtonDown += MouseButtonDown;
            Mouse.ButtonUp += MouseButtonUp;
            Util.Set3DMode();

            skydome.Load("sky/space.jpg", 1f);
            world.Add(skydome); // skydome aina ekana koska se on kaikkien takana

            car.Load("car.obj");
            world.Add(car);

            const float SC = 100;
            city.Load("city.obj", SC, SC, SC);
            world.Add(city);

            // lataa reitit
            cameraPath.Load("camerapath.obj", SC, SC, SC); // sama skaalaus ku cityssä
            carPath.Load("carpath.obj", SC, SC, SC);

            cam.FollowPath(ref cameraPath, true, true);
            car.FollowPath(ref carPath, true, true);

            cam.MakeCurve(3); // tehdään reitistä spline
        }

        bool[] mouseButtons = new bool[5];
        void MouseButtonDown(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = true; }
        void MouseButtonUp(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = false; }

        public override void OnUnload(EventArgs e)
        {
            font.Dispose();
            Util.ClearArrays(); // poistaa kaikki materiaalit ja texturet
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            Util.Resize(e.Width, e.Height, 1.0, 1000);
        }

        public override void OnUpdateFrame(UpdateFrameEventArgs e)
        {
            if (Keyboard[Key.Escape])
                Exit();

            car.UpdatePath((float)e.Time * 0.1f);
            cam.UpdatePath((float)e.Time * 5);


            // laske autolle Y (reitti ei seuraa maaston korkeutta oikein niin lasketaan se sitten tässä).
            car.Position.Y = 1000;
            Vector3 tmpV = car.Position;
            tmpV.Y = -10000;
            if (Intersection.CheckIntersection(ref car.Position, ref tmpV, ref city))
            {
                car.Position.Y = Intersection.intersection.Y;
            }


        }

        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            Settings.NumOfObjects = 0;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            Frustum.CalculateFrustum();
            world.Render();

            Texture.ActiveUnit(0);
            printer.Begin();
            if (MainClass.UseFonts) printer.Draw("objs: " + Settings.NumOfObjects, font);
            printer.End();
            GL.MatrixMode(MatrixMode.Modelview);

            SwapBuffers();
        }

    }
}
