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
using Microsoft.Xna.Framework.Graphics;

namespace BalloonMassacre
{
    /// <summary>
    /// En vertex som brukes i Particle, denne koden er fra din forelesning om partikkelefekt
    /// </summary>
    public struct ParticleVertex
    {
        public Vector3 position;
        public Vector2 textureCoordinate;
        public ParticleVertex(Vector3 position, Vector2 textureCoordinate)
        {
            this.position = position;
            this.textureCoordinate = textureCoordinate;
        }
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
             (
                new VertexElement(0, VertexElementFormat.Vector3,VertexElementUsage.Position, 0), new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2,VertexElementUsage.TextureCoordinate, 0)
             );
    }
    /// <summary>
    /// Strukten vil bli instansiert til et partikkelobjekt, denne koden er fra din forelesning om partikkelefekt
    /// </summary>
    struct Particle
    {
        public float size;
        public Vector3 direction;
        public ParticleVertex [] pvs; //ParticleVertexeS (PVS)
        public Particle(Vector3 _position, Vector2 _textureCoordinates, Vector3 _direction, float _size) {
            Vector3 p1,p2,p3;
            this.direction = _direction;
            this.size = _size;
            pvs = new ParticleVertex[3];
            p1 = new Vector3(_position.X, _position.Y, _position.Z);
            p2 = new Vector3(_position.X + size, _position.Y, _position.Z);
            p3 = new Vector3(_position.X, _position.Y + size, _position.Z);
            Vector2 tc1, tc2, tc3;
            tc1 = new Vector2(_textureCoordinates.X, _textureCoordinates.Y);
            tc2 = new Vector2(_textureCoordinates.X + size, _textureCoordinates.Y);
            tc3 = new Vector2(_textureCoordinates.X, _textureCoordinates.Y + size);
            //Oppretter 3 vertekser for denne partikkelen:
            pvs[0].position = p1;
            pvs[0].textureCoordinate = tc1;
            pvs[1].position = p2;
            pvs[1].textureCoordinate = tc2;
            pvs[2].position = p3;
            pvs[2].textureCoordinate = tc3;
        }
        public void UpdatePosition() 
        {
            pvs[0].position += this.direction;
            pvs[1].position += this.direction;
            pvs[2].position += this.direction;
        }
        public void UpdateTextureCoordinate(Vector2 newCoordinates) 
        {
            Vector2 tc1, tc2, tc3;
            tc1 = new Vector2(newCoordinates.X, newCoordinates.Y);
            tc2 = new Vector2(newCoordinates.X + size, newCoordinates.Y);
            tc3 = new Vector2(newCoordinates.X, newCoordinates.Y + size);

            pvs[0].textureCoordinate = tc1;
            pvs[1].textureCoordinate = tc2;
            pvs[2].textureCoordinate = tc3;
        }
    }
    /// <summary>
    /// Klasse som utfører selve partikkeleksplosjonen, denne koden er fra din forelesning om partikkelefekt
    /// </summary>
    class ParticleExplotion
    {
        // Array med Particle-objekter, opprettes i konstr.
        Particle[] particles;
        // Kollisjonens posisjon.
        Vector3 position;
        // Hvor mye “liv” som gjenstår før antall partikler begynner å avta
        int lifeLeft;
        // Hvor mange nye partikler som skal slippes for hver “runde”
        int numParticlesPerRound;
        // Maks antall partikler - totalt.
        int maxParticles;
        // Angir tiden mellom hver runde.
        int roundTime;
        // En teller som angir hvor mye tid som er gått siden starten
        // på pågående runde.
        int timeSinceLastRound = 0;
        // Vertex & graphics-info.

        GraphicsDevice graphicsDevice;
        // Texture
        Vector2 textureSize;
        // Settinger for hver enkelt partikkel – kun størrelse.
        private ParticleSettings particleSettings;
        // Følgende to variabler er indekser inn i partikkeltabellen:
        // Markerer slutten på aktive partikler (se fig. Under)
        int endOfLiveParticlesIndex = 0;
        // Markerer starten på aktive partikler (og slutten på ”døde”)
        int endOfDeadParticlesIndex = 0;

        public bool IsDead // Bool som forteller som eksplosjonen er over
        {
            get { return endOfDeadParticlesIndex == maxParticles; }
        }

        ParticleVertex[] vertexes;

        static Random rnd = new Random();
        // Konstruktør:
        public ParticleExplotion(GraphicsDevice graphicsDevice, Vector3 position, int lifeLeft, int roundTime, int numParticlesPerRound, int maxParticles, Vector2 textureSize, ParticleSettings particleSettings)
        {
            this.position = position;
            this.lifeLeft = lifeLeft;
            this.numParticlesPerRound = numParticlesPerRound;
            this.maxParticles = maxParticles;
            this.roundTime = roundTime;
            this.graphicsDevice = graphicsDevice;
            this.textureSize = textureSize;
            this.particleSettings = particleSettings;
            particles = new Particle[maxParticles];
            InitializeParticles();
        }

        private void InitializeParticles()
        {
            // Loop until max particles
            for (int i = 0; i < maxParticles; ++i)
            {
                Vector2 txtCoord = new Vector2(rnd.Next(0, (int)textureSize.X) / textureSize.X, rnd.Next(0, (int)textureSize.Y) / textureSize.Y);
                Vector3 direction = new Vector3((float)rnd.NextDouble() * 2 - 1, (float)rnd.NextDouble() * 2 - 1, (float)rnd.NextDouble() * 2 - 1);
                direction.Normalize();
                // Multiply by NextDouble to make sure that
                // all particles move at random speeds
                direction *= (float)rnd.NextDouble();
                //Particlesize:
                float size = ((float)rnd.NextDouble() * particleSettings.maxSize)/10;
                particles[i] = new Particle(position, txtCoord, direction, size);
            }
            //Kopierer til vertekstabell:
            vertexes = new ParticleVertex[particles.Length * 3];
            for (int i = 0; i < particles.Length; i++)
            {
                vertexes[i*3] = particles[i].pvs[0];
                vertexes[i*3 + 1] = particles[i].pvs[1];
                vertexes[i*3 + 2] = particles[i].pvs[2];
            }
        }
        public void Update(GameTime gameTime)
        {
            // Decrement life left until it's gone
            if (lifeLeft > 0)
                lifeLeft -= gameTime.ElapsedGameTime.Milliseconds;
            // Time for new round?
            timeSinceLastRound += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastRound > roundTime)
            {
                // New round ‐ add and remove particles
                timeSinceLastRound -= roundTime;
                // Increment end of live particles index each
                // round until end of list is reached
                if (endOfLiveParticlesIndex < maxParticles)
                {
                    endOfLiveParticlesIndex += numParticlesPerRound;
                    if (endOfLiveParticlesIndex > maxParticles)
                        endOfLiveParticlesIndex = maxParticles;
                }
                if (lifeLeft <= 0)
                {
                    // Increment end of dead particles index each
                    // round until end of list is reached
                    if (endOfDeadParticlesIndex < maxParticles)
                    {
                        endOfDeadParticlesIndex += numParticlesPerRound;
                        if (endOfDeadParticlesIndex > maxParticles)
                            endOfDeadParticlesIndex = maxParticles;
                    }
                }
            }
            // Update positions of all live particles
            for (int i = endOfDeadParticlesIndex; i < endOfLiveParticlesIndex; ++i)
            {
                //particles[i].position += particles[i].direction;
                particles[i].UpdatePosition();
                // Assign a random texture coordinate for color
                // to create a flashing effect for each particle
                Vector2 newCoords = new Vector2(rnd.Next(0, (int)textureSize.X) / textureSize.X, rnd.Next(0, (int)textureSize.Y) / textureSize.Y);
                particles[i].UpdateTextureCoordinate(newCoords);
            }
            //Oppdater verteksene:
            for (int j = 0; j < particles.Length; j++)
            {
                vertexes[j*3] = particles[j].pvs[0];
                vertexes[j*3 + 1] = particles[j].pvs[1];
                vertexes[j*3 + 2] = particles[j].pvs[2];
            }
        }

        public void Draw(Effect effect, Camera camera, Texture2D explosionTexture)
        {
            // Only draw if there are live particles
            if (endOfLiveParticlesIndex - endOfDeadParticlesIndex > 0)
            {
                // Set HLSL parameters
                effect.Parameters["xTexture"].SetValue(explosionTexture);
                effect.CurrentTechnique = effect.Techniques["ParticleExplotion"];
                effect.Parameters["WorldViewProjection"].SetValue(camera.View * camera.Projection);
                // Draw particles
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexes, endOfDeadParticlesIndex*3, endOfLiveParticlesIndex - endOfDeadParticlesIndex, ParticleVertex.VertexDeclaration);
                }
            }
        }
    }
}
