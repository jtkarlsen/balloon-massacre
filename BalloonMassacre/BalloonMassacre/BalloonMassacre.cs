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
    /// Hovedklassen for spillet
    /// Klassen kaller de andre komponentene og setter sammen spillet
    /// </summary>
    public class BalloonMassacre : Microsoft.Xna.Framework.Game
    {

        #region Variabler
        enum CollisionType { None, Blimp, Terrain } // Enum for å differensiere kollisjoner
        private GraphicsDeviceManager graphics;
        private GraphicsDevice device;
        private SpriteBatch spriteBatch;

        // Komponentene
        private Terrain terrain;
        private Camera camera;
        private Plane game;
        private Skybox skybox;
        private _2Dsprites sprites;
        private ParticleComponent explo;
        private Blimps blimps;

        private TimeSpan timer = TimeSpan.FromMilliseconds(120000); // Timer som brukes til å avgjøre når spillet er over

        private int killCount = 0; // Variabel som holder antall skutte ballonger
        #endregion

        public BalloonMassacre()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
        }
        /// <summary>
        /// Deklarerer komponentene
        /// </summary>
        protected override void Initialize()
        {
            camera = new Camera(this);
            this.Components.Add(camera);
            
            skybox = new Skybox(this, this.Content, camera);
            this.Components.Add(skybox);
            
            terrain = new Terrain(this, this.Content, camera);
            this.Components.Add(terrain);
            
            blimps = new Blimps(this, this.Content, camera, terrain);
            this.Components.Add(blimps);
            
            game = new Plane(this, this.Content, camera);
            this.Components.Add(game);
            
            explo = new ParticleComponent(this, this.Content, camera);
            this.Components.Add(explo);

            sprites = new _2Dsprites(this, this.Content, camera);
            this.Components.Add(sprites);

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            base.Initialize();
        }
        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;

            base.LoadContent();
        }
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Sjekker etter input, om timeren er gått ut, eller om ting kolliderer
        /// </summary>
        protected override void Update(GameTime gameTime)
        {

            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.Escape)) // Lar spilleren avslutte ved å trykke esc
                this.Exit();

            if (timer > TimeSpan.Zero) // Sjekker om tiden er gått ut, hvis ikke teller den vidre ned og setter tid stringen og killcount stringen til 2Dsprites
            {
                timer -= gameTime.ElapsedGameTime;
                sprites.Timer = timer.ToString();
                sprites.KillCount = killCount.ToString() + " Kills";
            }
            else // Hvis tiden er ute avsluttes flyet, og setter timer og killcountstringen fra 2Dsprites til ingenting, og setter avsluttningsteksten.
            {
                game.Dispose();
                sprites.Timer = "";
                sprites.KillCount = "";
                sprites.FerdigText = "Spillet er over! \nDu fikk "+killCount+" kills";
            }
            sprites.SpeedRot = game.MoveSpeed; // Setter speedometerrotasjonen
            sprites.HeightRot = game.SpitfirePosition.Y; // Setter høydemålerrotasjonen
            string height = Math.Round(game.SpitfirePosition.Y / 2, 0, MidpointRounding.ToEven).ToString(); // Setter høydemålerverdien
            sprites.Height = height + " moh"; // Setter høydemålerverdien
            if (terrainCollision() == CollisionType.Terrain && timer != TimeSpan.Zero) // Sjekker om flyet har kollidert med bakken, hvis den har kollidert, eksploderer det og spillet avsluttes
            {
                timer = TimeSpan.FromMilliseconds(0);
                explo.addExplotion(game.SpitfirePosition);
                game.Explosion.Play();
            }
            checkCollition(); // Sjekker kollisjon
            base.Update(gameTime);
        }
        /// <summary>
        /// Metode som sjekker om fly eller skudd kolliderer med luftskip
        /// </summary>
        private CollisionType checkCollition()
        {
            foreach (Blimp bl in blimps.Blimps1) // Går gjennom blimpene
            {
                foreach (Bullet bu in game.BulletList) // Går gjennom skuddene
                {
                    if (bu.bulletSphere.Contains(bl.BlimpFrontSphere) != ContainmentType.Disjoint || bu.bulletSphere.Contains(bl.BlimpBackSphere) != ContainmentType.Disjoint) // Sjekker om et skudd har kollidert med blimp
                    {
                        blimps.Blimps1.Remove(bl);
                        game.BulletList.Remove(bu);
                        killCount++;
                        explo.addExplotion(bl.Pos);
                        game.Explosion.Play();
                        return CollisionType.Blimp;
                    }
                }
                if (game.SpitfireSpere.Contains(bl.BlimpFrontSphere) == ContainmentType.Intersects || game.SpitfireSpere.Contains(bl.BlimpBackSphere) == ContainmentType.Intersects) // Sjekker om fly har kollidert med blimp
                {
                    timer = TimeSpan.FromMilliseconds(0);
                    blimps.Blimps1.Remove(bl);
                    killCount++;
                    explo.addExplotion(game.SpitfirePosition);
                    game.Explosion.Play();
                    return CollisionType.Blimp;
                }
            }
            return CollisionType.None;
        }
        /// <summary>
        /// Sjekker om flyet kolliderer med terrenget
        /// </summary>
        private CollisionType terrainCollision()
        {
            Vector3 vertPos;
            for (int i = 0; i < terrain.TerrainVertices.Length; i++) // Går gjennom vertexene til terrenget
            {
                vertPos = Vector3.Transform(terrain.TerrainVertices[i].Position, terrain.WorldMatrix); // Vertexpossisjon til terrenget
                float x = (float)Math.Round(game.SpitfirePosition.X, 0, MidpointRounding.ToEven); // Avrunder x koordinaten til flyet
                float z = (float)Math.Round(game.SpitfirePosition.Z, 0, MidpointRounding.ToEven); // Avrunder z koordinaten til flyet
                float terrainX = (float)Math.Round(vertPos.X, 0, MidpointRounding.ToEven); // Avrunder x koordinaten til terrenget
                float terrainZ = (float)Math.Round(vertPos.Z, 0, MidpointRounding.ToEven); // Avrunder z koordinaten til terrenget
                if (x <= terrainX + 200 && x > terrainX - 200 && z <= terrainZ + 200 && z > terrainZ - 200) // Sjekker om flyet er i samme x og z posisjon som terrenget
                {
                    if (game.SpitfirePosition.Y <= vertPos.Y) // Sjekker om flyet er på samme høyde som terrenget
                        return CollisionType.Terrain;
                }
            }
            return CollisionType.None;
        }
        protected override void Draw(GameTime gameTime)
        {
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.Solid;
            device.RasterizerState = rs;

            base.Draw(gameTime);
        }
    }
}
