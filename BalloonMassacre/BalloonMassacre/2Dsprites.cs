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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BalloonMassacre
{
    /// <summary>
    /// Klassen brukes til å tegne opp alt til HUD'en i spillet
    /// </summary>
    class _2Dsprites : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Variabler
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private SpriteFont spriteFont; // Fonten som brukes til teksten på skjermen utenom i høydemåleren
        private SpriteFont heightFont; // Fonten som brukes til teksten i høydemåleren

        private Camera camera;
        private ContentManager content;

        private Vector2 speedInstrumentPosition = new Vector2(1620, 780); // Posisjonen til speedometeret
        private Vector2 speedInstrumentNeedlePosition = new Vector2(1720, 880); // Posisjonen til nålen i spidometeret

        private Vector2 heightInstrumentPosition = new Vector2(100, 780); // Posisjonen til høydemåleren
        private Vector2 heightInstrumentNeedlePosition = new Vector2(200, 880); // Posisjonen til nålen i høydemåleren

        private Texture2D heightInstrumentTexture; // Teksturen til høydemåleren
        private Texture2D speedInstrumentTexture; // Teksturen til speedometeret
        private Texture2D instrumentNeedleTexture; // Teksturen til nålen

        private float heightRot = 0; // Rotasjonsvariabel til nålen i høydemåleren
        private float speedRot = 0; // Rotasjonsvariabelen til nålen i speedometeret

        private string ferdigText = "";
        private string timer = "";
        private string height = "";
        private string killCount = "";
        #endregion

        #region Get og set metoder
        public float SpeedRot
        {
            get { return speedRot; }
            set { speedRot = value; }
        }

        public float HeightRot
        {
            get { return heightRot; }
            set { heightRot = value; }
        }

        public string FerdigText
        {
            get { return ferdigText; }
            set { ferdigText = value; }
        }
        
        public string Timer
        {
            get { return timer; }
            set { timer = value; }
        }

        public string Height
        {
            get { return height; }
            set { height = value; }
        }
        
        public string KillCount
        {
            get { return killCount; }
            set { killCount = value; }
        }
        #endregion

        /// <summary>
        /// Konstruktøren
        /// </summary>
        public _2Dsprites(Game game, ContentManager content, Camera camera)
            : base(game)
        {
            graphics = (GraphicsDeviceManager)Game.Services.GetService
            (typeof(IGraphicsDeviceManager));
            this.camera = camera;
            this.content = content;
        }
        public override void Initialize()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            base.Initialize();
        }
        protected override void LoadContent()
        {
            speedInstrumentTexture = content.Load<Texture2D>("Textures/HUD/speed");
            heightInstrumentTexture = content.Load<Texture2D>("Textures/HUD/height");
            instrumentNeedleTexture = content.Load<Texture2D>("Textures/HUD/needle");
            spriteFont = content.Load<SpriteFont>("Fonts/Ariel");
            heightFont = content.Load<SpriteFont>("Fonts/height");
        }
        protected override void UnloadContent()
        {
        }
        public override void Update(GameTime gameTime)
        {
            speedRot = 20.0f + speedRot*1.5f; // Endrer speedRot til en verdi som passer i spidometeret
            heightRot = heightRot/ 40.0f; // Endrer heightRot til en verdi som passer i høydemåleren
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(speedInstrumentTexture, speedInstrumentPosition, null, Color.White, 0.0f, new Vector2(0,0),1.0f,SpriteEffects.None, 0.0f); // Tegner Speedometeret
            spriteBatch.Draw(heightInstrumentTexture, heightInstrumentPosition, null, Color.White, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.0f); // Tegner høydemåleren
            spriteBatch.End();

            DrawOverlayText(ferdigText, 800, 540, Color.Black, spriteFont); // Tegner teksten som vises når man avslutter
            DrawOverlayText(timer, 20, 200, Color.Black, spriteFont); // Tegner timerteksten
            DrawOverlayText(height, 160, 900, Color.White, heightFont); // Tegner teksten i høydemåleren
            DrawOverlayText(killCount, 20, 250, Color.Black, spriteFont); // Tegner opp antall kills

            spriteBatch.Begin();
            spriteBatch.Draw(instrumentNeedleTexture, speedInstrumentNeedlePosition, null, Color.White, speedRot, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.0f); // Tegner nålen til speedometeret
            spriteBatch.Draw(instrumentNeedleTexture, heightInstrumentNeedlePosition, null, Color.White, heightRot, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.0f); // Tegner nålen til høydemåleren
            spriteBatch.End();

            base.Draw(gameTime);
        }
        /// <summary>
        /// Metoden tar inn tekst, posisjon, farge og SpriteFont for å skrive tekst ut på skjermen
        /// </summary>
        private void DrawOverlayText(string text, int x, int y, Color t, SpriteFont skrift)
        {
            BlendState blendState = GraphicsDevice.BlendState;
            DepthStencilState depthStencilState = GraphicsDevice.DepthStencilState;
            RasterizerState rasterizerState = GraphicsDevice.RasterizerState;
            SamplerState samplerState = GraphicsDevice.SamplerStates[0];

            //Skriver tekst:
            spriteBatch.Begin();
            //Skriver teksten to ganger, først med svart bakgrunn og deretter med hvitt, en piksel ned og til venstre, slik at teksten blir mer lesbar.
            spriteBatch.DrawString(skrift, text, new Vector2(x, y), Color.White);
            spriteBatch.DrawString(skrift, text, new Vector2(x - 1, y - 1), t);
            spriteBatch.End();

            //Må sette diverse parametre tilbake siden SpriteBatch justerer flere parametre (se Shawn Hargreaves Blog):
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }
    }
}
