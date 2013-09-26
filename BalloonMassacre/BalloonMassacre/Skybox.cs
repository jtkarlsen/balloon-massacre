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
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace BalloonMassacre
{
    class Skybox : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region variabler
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private GraphicsDevice device;

        private Effect effect;

        private Texture2D[] skyboxTextures; // Liste med teksturer som brukes i skybox, teksturene er hentet fra http://www.redsorceress.com/skyboxs/iceflats.zip
        private Model skyboxModel; // Modellen som brukes til skybox er hentet fra Riemer's tutorial på http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series2/Skybox.php
        private Camera camera;
        private ContentManager content;
        #endregion

        /// <summary>
        /// Konstruktør
        /// </summary>
        public Skybox(Game game, ContentManager content, Camera camera) : base(game)
        {
            graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
            this.camera = camera;
            this.content = content;
        }
        public override void Initialize()
        {
            base.Initialize();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            device = graphics.GraphicsDevice;

            effect = content.Load<Effect>("Effects/effect");
            skyboxModel = LoadModel("Models/skybox", out skyboxTextures);
        }
        /// <summary>
        /// Metode som laster inn en modell og knytter tekstur til den
        /// </summary>
        private Model LoadModel(string assetName, out Texture2D[] textures)
        {
            Model newModel = content.Load<Model>(assetName);
            textures = new Texture2D[newModel.Meshes.Count];
            int i = 0;
            foreach (ModelMesh mesh in newModel.Meshes)
                foreach (BasicEffect currentEffect in mesh.Effects)
                    textures[i++] = currentEffect.Texture;

            foreach (ModelMesh mesh in newModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();

            return newModel;
        }
        protected override void UnloadContent()
        {
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {   
            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = false;
            device.DepthStencilState = dss;
            
            Matrix[] skyboxTransforms = new Matrix[skyboxModel.Bones.Count];
            skyboxModel.CopyAbsoluteBoneTransformsTo(skyboxTransforms);
            
            int i = 0;
            foreach (ModelMesh mesh in skyboxModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = skyboxTransforms[mesh.ParentBone.Index] * Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(camera.CameraPosition);
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(camera.View);
                    currentEffect.Parameters["xProjection"].SetValue(camera.Projection);
                    currentEffect.Parameters["xTexture"].SetValue(skyboxTextures[i++]);
                }
                mesh.Draw();
            }
            
            dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            device.DepthStencilState = dss;
            
            base.Draw(gameTime);
        }
    }
}
