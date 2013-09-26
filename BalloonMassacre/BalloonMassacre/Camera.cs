/// Laget av: Jan Tore Karlsen og Håvard Rustad Olsen
/// Tittel: BalloonMassacre
/// Beskrivelse: Dette er ett flyspill der målet er å skyte ned så
/// mange luftskip som mulig på to minutter.
/// Krasjer man i bakken eller i en ballong er spillet over.
/// Man styrer flyet med piltastene, gasser på A, bremser på S og skyter på Space.
///
/// Siste oppdatering: 21.11.2012
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BalloonMassacre
{
    /// <summary>
    /// Kamera klasse
    /// </summary>
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        #region Variabler
        private GraphicsDeviceManager graphics; //Graphics device manager
        private Matrix projection; //projection matrisa
        private Matrix view; //view matrisa
        private Vector3 cameraPosition = new Vector3(60, 80, -80); //kameraets posisjon
        private Vector3 cameraTarget = Vector3.Zero; //kameraets mål
        private Vector3 cameraUpVector = Vector3.Up; //Opp vektor
        private Quaternion cameraRotation = Quaternion.Identity; //Rotasjonen til kameraet
        #endregion

        #region Properties
        public Vector3 CameraPosition
        {
            get { return cameraPosition; }
            set { cameraPosition = value; }
        }

        public Quaternion CameraRotation
        {
            get { return cameraRotation; }
            set { cameraRotation = value; }
        }

        public Vector3 CameraUpVector
        {
            get { return cameraUpVector; }
            set { cameraUpVector = value; }
        }

        public Matrix View
        {
            get { return view; }
            set { view = value; }
        }
        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; }
        }
        #endregion

        /// <summary>
        /// Kontruktør som kun tar inn spillet
        /// </summary>
        /// <param name="game">Spillet</param>
        public Camera(Game game)
            : base(game)
        {
            graphics = (GraphicsDeviceManager)Game.Services.GetService
            (typeof(IGraphicsDeviceManager));
        }

        public override void Initialize()
        {
            // TODO: Add your initialization code here
            base.Initialize();
            this.InitCamera();
        }
        /// <summary>
        /// Initsialiserer kameraet, blir kalt på i Initialize()
        /// </summary>
        private void InitCamera()
        {
            float aspectRatio =
            (float)graphics.GraphicsDevice.Viewport.Width /
            (float)graphics.GraphicsDevice.Viewport.Height;

            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
            aspectRatio, 1.0f, 10000.0f, out projection);

            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget,
            ref cameraUpVector, out view);

            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();
        }
        /// <summary>
        /// Oppdaterer komponenten
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}