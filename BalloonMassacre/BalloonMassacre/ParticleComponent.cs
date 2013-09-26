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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BalloonMassacre
{
    /// <summary>
    /// Klasse som lager nye eksplosjoner, og kaller på update og draw til ParticleExplotion
    /// Denne klassen er basert på klassen ParticleDemo som du har i dokumentet til ParticleEffects
    /// </summary>
    class ParticleComponent : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Variabler
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Camera camera;

        //Randomness
        public Random rnd = new Random();

        //Explosion stuff
        private ContentManager content;
        List<ParticleExplotion> explosions = new List<ParticleExplotion>();
        ParticleExplosionSettings particleExplosionSettings = new ParticleExplosionSettings();
        ParticleSettings particleSettings = new ParticleSettings();
        Effect explosionEffect;
        Texture2D explosionTexture;
        #endregion

        /// <summary>
        /// Konstruktøren
        /// </summary>
        public ParticleComponent(Game game, ContentManager Content, Camera camera) : base(game)
        {
            graphics = (GraphicsDeviceManager)Game.Services.GetService
            (typeof(IGraphicsDeviceManager));
            this.camera = camera;
            this.content = Content;
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            explosionTexture = content.Load<Texture2D>(@"Textures/ParticleExplotion/ParticleColors");
            explosionEffect = content.Load<Effect>(@"Effects/effect");
            explosionEffect.Parameters["xTexture"].SetValue(explosionTexture);
        }
        public override void Update(GameTime gameTime)
        {
            UpdateExplosions(gameTime);
            base.Update(gameTime);
        }
        /// <summary>
        /// Oppdaterer alle eksplosjonene
        /// </summary>
        protected void UpdateExplosions(GameTime gameTime)
        {
            for (int i = 0; i < explosions.Count; ++i) // Looper gjennom eksplosjonene
            {
                explosions[i].Update(gameTime); // Oppdaterer eksplosjonen

                if (explosions[i].IsDead) // Hvis eksplosjonen er ferdig, fjernes den
                {
                    explosions.RemoveAt(i);
                    --i;
                }
            }
        }
        /// <summary>
        /// Metoden lager en ny eksplosjon på koordinatene den tar som parameter.
        /// Eksplosjonen legges inn i listen med de andre eksplosjonene
        /// </summary>
        public void addExplotion(Vector3 impactCoord)
        {
            explosions.Add(new ParticleExplotion(GraphicsDevice,
            impactCoord, (rnd.Next(particleExplosionSettings.minLife, particleExplosionSettings.maxLife)), (rnd.Next(particleExplosionSettings.minRoundTime, particleExplosionSettings.maxRoundTime)),
            (rnd.Next(particleExplosionSettings.minParticlesPerRound,particleExplosionSettings.maxParticlesPerRound)),(rnd.Next(particleExplosionSettings.minParticles,particleExplosionSettings.maxParticles)),
            new Vector2(explosionTexture.Width,explosionTexture.Height),particleSettings));
        }
        /// <summary>
        /// Metoden kalles på draw metoden til eksplosjonen
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            foreach (ParticleExplotion pe in explosions) // Går gjennom alle eksplosjonene
            {
                pe.Draw(explosionEffect, camera, explosionTexture); // Tegner eksplosjonen
            }
            base.Draw(gameTime);
        }
    }
}
