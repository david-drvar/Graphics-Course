﻿// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Quadrics;
using SharpGL;
using SharpGL.SceneGraph.Cameras;
using System.Windows.Threading;
using System.Drawing;
using System.Drawing.Imaging;

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

        public DispatcherTimer timer1;
        public DispatcherTimer timer2;
        public DispatcherTimer timer3;




        public double skaliranjeLopte { get; set; }
        public double rotiranjeLopte { get; set; }

        public float maxLopta = 250;
        public float trenutnaLopta = 0;
        public bool nagore = true;

        public bool isKickBallAnimationRunning = false;
        public float korakZ = 10f;
        public float korakY = 10f;
        public float korakX = 0f;


        public double rotacijaInkrement = 0;

        public String izborBoje = "blue";


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

        private enum TextureObjects { Trava = 0, Plastika };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;

        private uint[] m_textures;

        private string[] m_textureFiles = { "..//..//teksture//trava.jpg", "..//..//teksture//plastika.jpg" };


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
        /// 


        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
            this.skaliranjeLopte = 0.6f;
            this.rotiranjeLopte = 0f;
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
            //gl.Color(1f, 0f, 0f);
            // Model sencenja na flat (konstantno)

            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Enable(OpenGL.GL_NORMALIZE);

            timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(10);
            timer1.Tick += new EventHandler(UpdateAnimationBallRotation);
            timer1.Start();

            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(10);
            timer2.Tick += new EventHandler(UpdateAnimationBouncing);
            timer2.Start();

            timer3 = new DispatcherTimer();
            timer3.Interval = TimeSpan.FromMilliseconds(10);
            timer3.Tick += new EventHandler(UpdateAnimationKickBall);
            timer3.Start();

            // Podesavanje inicijalnih parametara kamere
            //lookAtCam = new LookAtCamera();
            //lookAtCam.Position = new Vertex(0f, 0f, 0f);
            //lookAtCam.Target = new Vertex(0f, 0f, -10f);
            //lookAtCam.UpVector = new Vertex(0f, 1f, 0f);
            //right = new Vertex(1f, 0f, 0f);
            //direction = new Vertex(0f, 0f, -1f);
            //lookAtCam.Target = lookAtCam.Position + direction;
            //lookAtCam.Project(gl);

            // teksture
            m_textures = new uint[2];
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

            // Ucitaj slike i kreiraj teksture
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);      // Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);      // Linear Filtering

                image.UnlockBits(imageData);
                image.Dispose();
            }
            //

            gl.ShadeModel(OpenGL.GL_FLAT);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);


            m_scene.LoadScene();
            m_scene.Initialize();



        }

        private void UpdateAnimationBallRotation(object sender, EventArgs e)
        {
            if (isKickBallAnimationRunning == false)
            {
                rotiranjeLopte += rotacijaInkrement;
            }
                
        }

        private void UpdateAnimationBouncing(object sender, EventArgs e)
        {
            if (isKickBallAnimationRunning == false)
            {
                if (nagore == true && trenutnaLopta + 10 <= maxLopta)
                    trenutnaLopta += 10;
                else if (nagore == true && trenutnaLopta == maxLopta)
                {
                    nagore = false;
                    trenutnaLopta -= 10;
                }
                else if (nagore == false && trenutnaLopta - 10 >= 0)
                {
                    nagore = false;
                    trenutnaLopta -= 10;
                }
                else if (trenutnaLopta == 0)
                {
                    nagore = true;
                    trenutnaLopta += 10;
                }
            }

        }

        private void UpdateAnimationKickBall(object sender, EventArgs e)
        {
            if (isKickBallAnimationRunning)
            {
                korakZ += 10;
                korakY += 3;
                korakX -= 1;

            }
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Viewport(0, 0, m_width, m_height);
            gl.LoadIdentity();

            gl.Enable(OpenGL.GL_TEXTURE_2D);

            gl.LookAt(0, 0, 10, 0, 0, 0, 0, 1, 0);


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
            
            //osvetljenje kod gola
            gl.PushMatrix();
            gl.Translate(300.0f, 20.0f, -950.0f);
            gl.Rotate(-90, 1, 0, 0);
            cylinder.CreateInContext(gl);
            //cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            float[] ambijentalnaKomponenta2 = { 0.2f, 0.2f, 0.2f, 1.0f };
            float[] difuznaKomponenta2 = { 1f, 1f, 1f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, ambijentalnaKomponenta2);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, difuznaKomponenta2);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);
            gl.Enable(OpenGL.GL_LIGHT0);
            float[] pozicija = { 300.0f, 0.0f, 0.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pozicija);
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_NORMALIZE);
            gl.PopMatrix();


            gl.PushMatrix();
            //gl.Translate(0.0f, 40.0f, 0.0f);
            //gl.Rotate(90, 1, 0, 0);
            cylinder.CreateInContext(gl);
            //cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            float[] ambijentalnaKomponenta = {1f, 1f, 1f, 1.0f };
            float[] difuznaKomponenta =  { 0.0f, 1.0f, 1.0f, 1.0f };
            if (izborBoje.Equals("blue"))
                difuznaKomponenta = new float[] { 0.0f, 1.0f, 1.0f, 1.0f };
            else if (izborBoje.Equals("red"))
                difuznaKomponenta = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };
            else if (izborBoje.Equals("yellow"))
                difuznaKomponenta = new float[] { 1.0f, 1.0f, 0.0f, 1.0f };
            else if (izborBoje.Equals("purple"))
                difuznaKomponenta = new float[] { 1.0f, 0.0f, 1.0f, 1.0f };
            else if (izborBoje.Equals("white"))
                difuznaKomponenta = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            else if (izborBoje.Equals("green"))
                difuznaKomponenta = new float[] { 0.0f, 1.0f, 0.0f, 1.0f };

            float[] smer = { 0.0f, 1.0f, 0.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, ambijentalnaKomponenta);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, difuznaKomponenta);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, difuznaKomponenta);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, smer);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 35.0f);
            float[] pozicijaReflektora = { 0.0f, 40.0f, 0.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, pozicijaReflektora);
            gl.Enable(OpenGL.GL_LIGHT1);
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.PopMatrix();

            
            gl.PushMatrix();
            
            if (isKickBallAnimationRunning)
                gl.Translate(korakX, korakY, -korakZ - 100);
            if (korakZ >= 900)
            {
                isKickBallAnimationRunning = false;
                korakZ = 0;
                korakY = 0;
                korakX = 0;
            }
            gl.Rotate(0, (float)rotiranjeLopte, 0);
            gl.Translate(0, trenutnaLopta, 0);
            gl.Scale(skaliranjeLopte, skaliranjeLopte, skaliranjeLopte);
            gl.Translate(0f, 50f, 0f);
            m_scene.Draw();
            gl.PopMatrix();
            gl.Translate(0.0f, 90f, -0);

            //gl.Color(0f, 0f, 0f);

            //podloga
            //gl.PushMatrix();

            gl.MatrixMode(OpenGL.GL_TEXTURE);
            //gl.Scale(2f, 2f, 2f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[0]);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            //
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0f, 1f, 0f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex4f(450f, -100f, 500, 1);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex4f(450f, -100f, -1200, 1);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex4f(-450f, -100f, -1200, 1);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex4f(-500f, -100f, 500, 1);

            gl.End();
            gl.Translate(0.0f, -90f, -0);
            //gl.PopMatrix();

            //desni stativ
            gl.PushMatrix();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastika]);
            gl.Translate(0f, 400f, -950f);
            gl.Translate(-200f, -10f, 0);
            gl.Rotate(90f, 0f, 0f);

            cylinder.CreateInContext(gl);

            cylinder.TextureCoords = true;
            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            gl.PopMatrix();

            //gornji desni
            gl.PushMatrix();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastika]);

            gl.Translate(0f, 400f, -950f);
            gl.Translate(-200f, -10f, 0);
            gl.Translate(0f, 0f, -100f);

            upper.CreateInContext(gl);

            upper.TextureCoords = true;
            upper.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            //gornji levi
            gl.PushMatrix();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastika]);

            gl.Translate(0f, 400f, -950f);
            gl.Translate(200f, -10f, 0f);
            gl.Translate(0f, 0f, -100f);

            upper.TextureCoords = true;
            upper.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            gl.PopMatrix();

            //donji desni

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastika]);

            gl.PushMatrix();
            gl.Translate(0f, 400f, -950f);
            gl.Translate(-200f, -10f, 0);
            gl.Translate(0f, -388f, -200f);

            down.CreateInContext(gl);

            down.TextureCoords = true;
            down.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            gl.PopMatrix();

            //donji levi
            gl.PushMatrix();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastika]);

            gl.Translate(0f, 400f, -950f);
            gl.Translate(200f, -10f, 0f);
            gl.Translate(0f, -388f, -200f);

            down.TextureCoords = true;
            down.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            gl.PopMatrix();


            //levi stativ
            gl.PushMatrix();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastika]);

            gl.Translate(0f, 400f, -950f);
            gl.Translate(200f, -10f, 0f);
            gl.Rotate(90f, 0f, 0f);

            cylinder.TextureCoords = true;
            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            gl.PopMatrix();

            //gornji stativ
            gl.PushMatrix();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastika]);

            gl.Translate(0f, 400f, -950f);
            gl.Translate(200f, -10f, 0f);
            gl.Rotate(90f, -90f, 0f);

            cylinder.TextureCoords = true;
            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            gl.PopMatrix();

            //donji stativ
            gl.PushMatrix();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastika]);

            gl.Translate(0f, 400f, -950f);
            gl.Translate(200f, -10f, 0f);
            gl.Translate(0, -388f, -200f);
            gl.Rotate(90f, -90f, 0f);

            cylinder.TextureCoords = true;
            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            gl.PopMatrix();

            //ukoso leva
            gl.PushMatrix();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastika]);

            gl.Translate(0f, 400f, -950f);
            gl.Translate(200f, -10f, 0f);
            gl.Translate(0f, -388f, -200f);
            gl.Rotate(-75f, 0f, 0f);

            cylinder.TextureCoords = true;
            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            gl.PopMatrix();

            //ukoso desna
            gl.PushMatrix();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastika]);


            gl.Translate(0f, 400f, -950f);
            gl.Translate(-200f, -10f, 0);
            gl.Translate(0f, -388f, -200f);
            gl.Rotate(-75f, 0f, 0f);

            cylinder.TextureCoords = true;
            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

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