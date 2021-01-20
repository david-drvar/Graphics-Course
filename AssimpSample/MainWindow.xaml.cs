using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace AssimpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        public static float brzinaRotacije = 1;
        public ObservableCollection<float> BrzinaRotacije
        {
            get;
            set;
        }

        public static double skaliranje = 0.3;
        public ObservableCollection<double> Skaliranje
        {
            get;
            set;
        }

        public ObservableCollection<string> AmbijentalnaKomponenta
        {
            get;
            set;
        }

        #endregion Atributi

        #region Konstruktori

        public MainWindow()
        {
            BrzinaRotacije = new ObservableCollection<float>();
            BrzinaRotacije.Add(2);
            BrzinaRotacije.Add(5);
            BrzinaRotacije.Add(10);
            BrzinaRotacije.Add(15);
            BrzinaRotacije.Add(20);
            BrzinaRotacije.Add(25);
            BrzinaRotacije.Add(30);
            BrzinaRotacije.Add(35);
            BrzinaRotacije.Add(40);
            BrzinaRotacije.Add(45);

            Skaliranje = new ObservableCollection<double>();
            Skaliranje.Add(0.1);
            Skaliranje.Add(0.2);
            Skaliranje.Add(0.3);
            Skaliranje.Add(0.4);
            Skaliranje.Add(0.5);
            Skaliranje.Add(0.6);
            Skaliranje.Add(0.7);
            Skaliranje.Add(0.8);
            Skaliranje.Add(0.9);
            Skaliranje.Add(1);

            // Inicijalizacija komponenti
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.DataContext = this;


            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Ball"), "soccer.3DS", (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key != Key.V && m_world.Sut)
                return;



            switch (e.Key)
            {
                case Key.F4: this.Close(); break;

                case Key.S:
                    if (m_world.RotationY < -90)
                    {
                        break;
                    }
                    else
                    {
                        m_world.RotationY -= 5.0f;
                    }
                    break;
                case Key.F:
                    if (m_world.RotationY > 90)
                    {
                        break;
                    }
                    else
                    {
                        m_world.RotationY += 5.0f;
                    }
                    break;
                case Key.E:
                    if (!(m_world.RotationX > -5))
                    {
                        break;
                    }
                    else
                    {
                        m_world.RotationX -= 5.0f;
                    }
                    break;
                case Key.D:
                    if (!(m_world.RotationX < 80))
                    {
                        break;
                    }
                    else
                    {
                        m_world.RotationX += 5.0f;
                    }
                    break;
                case Key.Add: m_world.SceneDistance -= 100.0f; break;
                case Key.Subtract: m_world.SceneDistance += 100.0f; break;
                case Key.V: m_world.Sut = true; break;
                case Key.F3:
                    OpenFileDialog opfModel = new OpenFileDialog();
                    bool result = (bool)opfModel.ShowDialog();
                    if (result)
                    {

                        try
                        {
                            World newWorld = new World(Directory.GetParent(opfModel.FileName).ToString(), Path.GetFileName(opfModel.FileName), (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
                            m_world.Dispose();
                            m_world = newWorld;
                            m_world.Initialize(openGLControl.OpenGL);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta:\n" + exp.Message, "GRESKA", MessageBoxButton.OK);
                        }
                    }
                    break;
            }
        }
    }
}
