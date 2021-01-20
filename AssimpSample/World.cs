// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using SharpGL.SceneGraph.Cameras;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        private LookAtCamera lookAtCam;
        private float walkSpeed = 0.1f;
        float mouseSpeed = 0.005f;
        double horizontalAngle = 0f;
        double verticalAngle = 0.0f;

        //Pomocni vektori preko kojih definisemo lookAt funkciju
        private Vertex direction;
        private Vertex right;
        private Vertex up;

        public enum SkaliranjeLopte
        {
            Small,
            Normal,
            Bigger,

        };

        public enum BrzinaRotacije
        {
            Normal,
            Slow,
            Fast
        };

        private Boolean sut = false;

        public Boolean Sut
        {
            get { return sut; }
            set { sut = value; }
        }


        private SkaliranjeLopte skaliranje;
        private BrzinaRotacije rotiranje;

        /// <summary>
        ///	 Ugao rotacije Meseca
        /// </summary>
        private float m_moonRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 1700f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1f, 0f, 0f);
            // Model sencenja na flat (konstantno)

            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Enable(OpenGL.GL_NORMALIZE);

            // Podesavanje inicijalnih parametara kamere
            //lookAtCam = new LookAtCamera();
            //lookAtCam.Position = new Vertex(0f, 0f, 0f);
            //lookAtCam.Target = new Vertex(0f, 0f, -10f);
            //lookAtCam.UpVector = new Vertex(0f, 1f, 0f);
            //right = new Vertex(1f, 0f, 0f);
            //direction = new Vertex(0f, 0f, -1f);
            //lookAtCam.Target = lookAtCam.Position + direction;
            //lookAtCam.Project(gl);

            gl.ShadeModel(OpenGL.GL_FLAT);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            m_scene.LoadScene();
            m_scene.Initialize();
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Viewport(0, 0, m_width, m_height);
            gl.LoadIdentity();

            gl.LookAt(0, 0, 10, 0, 0, 0, 0, 1, 0);


            // #region svetlost
            // gl.Enable(OpenGL.GL_LIGHTING);

            gl.PushMatrix();

            //tackasto
            float[] amb = { 1f, 1f, 1f, 1.0f };
            float[] dif = { 1.0f, 1.0f, 1.0f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, amb);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, dif);


            float[] s = { 0f, 0f, -1f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_DIRECTION, s);

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);

            float[] pos = { 600f, 0.0f, 0.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pos);

            

            gl.PopMatrix();

            gl.Enable(OpenGL.GL_LIGHT0);

            // //reflektor
            // gl.PushMatrix();

            // float[] diff = { 1f, 0.4f, 0.3f, 1f };
            // float[] ambb = { 1f, 0.4f, 0.3f, 1f };

            // gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT,
            //ambb);
            // gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE,
            //  diff);
            // gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, ambb);

            // gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 35.0f);

            // gl.Rotate(-50, 1, 0, 0);


            // gl.PopMatrix();

            // //if (OnTackasto)
            // gl.Enable(OpenGL.GL_LIGHT0);
            // //else
            // //    gl.Disable(OpenGL.GL_LIGHT0);

            // //if (OnReflektor)
            // gl.Enable(OpenGL.GL_LIGHT1);
            // //else
            // //    gl.Disable(OpenGL.GL_LIGHT1);

            // gl.Enable(OpenGL.GL_LIGHTING);
            // gl.Enable(OpenGL.GL_NORMALIZE);

            // gl.PopMatrix();
            // #endregion


            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            Cylinder cylinder = new Cylinder();
            cylinder.NormalGeneration = Normals.Smooth;
            cylinder.BaseRadius = 10;
            cylinder.TopRadius = 10;
            cylinder.Height = 400;

            Cylinder upper = new Cylinder();
            upper.NormalGeneration = Normals.Smooth;
            upper.BaseRadius = 10;
            upper.TopRadius = 10;
            upper.Height = 100;

            Cylinder down = new Cylinder();
            down.NormalGeneration = Normals.Smooth;
            down.BaseRadius = 10;
            down.TopRadius = 10;
            down.Height = 200;

            gl.PushMatrix();
            gl.Scale(0.6f, 0.6f, 0.6f);
            gl.Translate(0f, 50f, 0f);
            m_scene.Draw();
            gl.PopMatrix();
            

            gl.Color(1f, 1f, 1f);

            //desni stativ
            gl.PushMatrix();
            gl.Translate(0f, 400f, -950f);
            gl.Translate(-200f, -10f, 0);
            gl.Rotate(90f, 0f, 0f);
            cylinder.CreateInContext(gl);
            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            //gornji desni
            gl.PushMatrix();
            gl.Translate(0f, 400f, -950f);
            gl.Translate(-200f, -10f, 0);
            gl.Translate(0f, 0f, -100f);
            upper.CreateInContext(gl);
            upper.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            //gornji levi
            gl.PushMatrix();
            gl.Translate(0f, 400f, -950f);
            gl.Translate(200f, -10f, 0f);
            gl.Translate(0f, 0f, -100f);
            upper.CreateInContext(gl);
            upper.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            //donji desni
            gl.PushMatrix();
            gl.Translate(0f, 400f, -950f);
            gl.Translate(-200f, -10f, 0);
            gl.Translate(0f, -388f, -200f);
            down.CreateInContext(gl);
            down.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            //donji levi
            gl.PushMatrix();
            gl.Translate(0f, 400f, -950f);
            gl.Translate(200f, -10f, 0f);
            gl.Translate(0f, -388f, -200f);
            down.CreateInContext(gl);
            down.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();


            //levi stativ
            gl.PushMatrix();
            gl.Translate(0f, 400f, -950f);
            gl.Translate(200f, -10f, 0f);
            gl.Rotate(90f, 0f, 0f);
            cylinder.CreateInContext(gl);
            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            //gornji stativ
            gl.PushMatrix();
            gl.Translate(0f, 400f, -950f);
            gl.Translate(200f, -10f, 0f);
            gl.Rotate(90f, -90f, 0f);
            cylinder.CreateInContext(gl);
            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            //donji stativ
            gl.PushMatrix();
            gl.Translate(0f, 400f, -950f);
            gl.Translate(200f, -10f, 0f);
            gl.Translate(0, -388f, -200f);
            gl.Rotate(90f, -90f, 0f);
            cylinder.CreateInContext(gl);
            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            //ukoso leva
            gl.PushMatrix();
            gl.Translate(0f, 400f, -950f);
            gl.Translate(200f, -10f, 0f);
            gl.Translate(0f, -388f, -200f);
            gl.Rotate(-75f, 0f, 0f);
            cylinder.CreateInContext(gl);
            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            //ukoso desna
            gl.PushMatrix();
            gl.Translate(0f, 400f, -950f);
            gl.Translate(-200f, -10f, 0);
            gl.Translate(0f, -388f, -200f);
            gl.Rotate(-75f, 0f, 0f);
            cylinder.CreateInContext(gl);
            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            //podloga
            gl.PushMatrix();
            gl.Color(0.39f, 0.99f, 0.37f);
            gl.Translate(0.0f, 90f, -0);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Vertex4f(450f, -100f, 500, 1);
            gl.Vertex4f(450f, -100f, -1200, 1);
            gl.Vertex4f(-450f, -100f, -1200, 1);
            gl.Vertex4f(-500f, -100f, 500, 1);
            gl.End();
            gl.PopMatrix();


            //ose
            gl.PushMatrix();
            gl.Begin(OpenGL.GL_LINES);
            // x aix
            gl.Color(1f, 0, 0); //red
            gl.Vertex(-400000000.0f, 0.0f, 0.0f);
            gl.Vertex(400000000.0f, 0.0f, 0.0f);
            //y
            gl.Color(0, 1f, 0); //green
            gl.Vertex(0.0f, -400000000.0f, 0.0f);
            gl.Vertex(0.0f, 400000000.0f, 0.0f);
            //z
            gl.Color(0, 0, 1f); //blue
            gl.Vertex(0.0f, 0.0f, -400000000.0f);
            gl.Vertex(0.0f, 0.0f, 400000000.0f);
            gl.End();
            gl.PopMatrix();


            gl.PushMatrix();
            //gl.Ortho2D(1, 1, 1,1);
            //gl.Ortho2D(0, m_width, 0, m_height);
            //gl.MatrixMode(OpenGL.GL_PROJECTION);
            //gl.Translate(m_width - 300, m_height - 30, 0f);
            gl.Viewport(m_width / 2, m_height / 2, m_width / 2, m_height / 2);
            //gl.DrawText3D("Arial", 25344f, 2331f, 1f, "teapot");
            //gl.Color(1f, 1f, 1f);
            //gl.DrawText3D("Arial", 124f, 13f, 304f, "Proba");

            gl.DrawText(m_width - 300, m_height - 30, 0.45f, 0.45f, 0.45f, "Arial", 10, "Predmet: Racunarska grafika");
            gl.DrawText(m_width - 300, m_height - 27, 0.45f, 0.45f, 0.45f, "Arial", 10, "Predmet: Racunarska grafika");
            gl.DrawText(m_width - 300, m_height - 60, 0.45f, 0.45f, 0.45f, "Arial", 10, "Sk.god: 2020/21");
            gl.DrawText(m_width - 300, m_height - 58, 0.45f, 0.45f, 0.45f, "Arial", 10, "Sk.god: 2020/21");
            gl.DrawText(m_width - 300, m_height - 90, 0.45f, 0.45f, 0.45f, "Arial", 10, "Ime: David");
            gl.DrawText(m_width - 300, m_height - 88, 0.45f, 0.45f, 0.45f, "Arial", 10, "Ime: David");
            gl.DrawText(m_width - 300, m_height - 120, 0.45f, 0.45f, 0.45f, "Arial", 10, "Prezime: Drvar");
            gl.DrawText(m_width - 300, m_height - 118, 0.45f, 0.45f, 0.45f, "Arial", 10, "Prezime: Drvar");
            gl.DrawText(m_width - 300, m_height - 150, 0.45f, 0.45f, 0.45f, "Arial", 10, "Sifra zad: 17.2");
            gl.DrawText(m_width - 300, m_height - 148, 0.45f, 0.45f, 0.45f, "Arial", 10, "Sifra zad: 17.2");
            gl.Color(1f, 1f, 1f);
            gl.Translate(m_width - 300, m_height - 30, 0f);
            gl.Viewport(0, 0, m_width, m_height);
            gl.PopMatrix();



            gl.PopMatrix();
            gl.Flush();
        }


        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(50f, (double)width / height, 0.5f, 25000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
