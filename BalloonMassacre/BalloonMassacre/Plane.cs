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
    /// Struct som instansieres til et bullet objekt.
    /// </summary>
    public struct Bullet
    {
        public Vector3 position;
        public Quaternion rotation;
        public BoundingSphere bulletSphere;
    }
    /// <summary>
    /// Klasse for å instansiere, tegne og manøvrere flyet og skuddene
    /// </summary>
    public class Plane : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Variabler
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private GraphicsDevice device;
        private Camera camera;
        private ContentManager content;

        private enum CollisionType { None, Target, Skybox } // Enum for å differensiere kollisjonene

        private SoundEffect tomgang; // Lyd som spilles av på tomgang
        private SoundEffect gass; // Lyd som spilles av når man gir gass
        private SoundEffect shot; // Lyd som spilles når man skyter
        private SoundEffect explosion; // Lyd som spilles når noe eksploderer

        private SoundEffectInstance tomgangInstance; // Instans av lydefekten tomgang
        private SoundEffectInstance gassInstance; // Instans av lydefekten gass

        private Effect effect; 

        private Texture2D bulletTexture; // Teksturen for skuddet

        private Model spitfireModel; // Modellen av flyet
        private Model propellerModel; // Modellen av propellen

        private BoundingSphere spitfireSpere; // Flyet sin boundingSphere

        private Vector3 lightDirection = new Vector3(3, -2, 5); // Lysretning
        private Vector3 spitfirePosition = new Vector3(0f, 3500, 0); // Startposisjon for flyet
        private Vector3 x; // Vector3 som brukes til fysikken bak rotasjon av flyet
        private Vector3 z; // Vector3 som brukes til fysikken bak rotasjon av flyet
        private Vector3 y; // Vector3 som brukes til fysikken bak rotasjon av flyet
        private Vector3 cameraUpDirection; // Vector3 som brukes til vinkling av fly og skudd

        private Quaternion spitfireRotation = Quaternion.Identity; // Quaterninon for rotasjon av flyet
        private Quaternion cameraRotation = Quaternion.Identity; // Quaterninon for rotasjon av kameraet

        private List<Bullet> bulletList = new List<Bullet>(); // Liste over kuler
        
        private float moveSpeed = 1.25f; // Farten til flyet
        private float propellerRotation = 1; // Rotasjonsverdien til propellen
        private double lastBulletTime = 0; // Verdi for å vite når neste skudd kan skytes
        #endregion
        #region Get og set metoder
        public SoundEffect Explosion
        {
            get { return explosion; }
            set { explosion = value; }
        }
        public Vector3 SpitfirePosition
        {
            get { return spitfirePosition; }
            set { spitfirePosition = value; }
        }
        public List<Bullet> BulletList
        {
            get { return bulletList; }
            set { bulletList = value; }
        }
        public BoundingSphere SpitfireSpere
        {
            get { return spitfireSpere; }
            set { spitfireSpere = value; }
        }
        public float MoveSpeed
        {
            get { return moveSpeed; }
            set { moveSpeed = value; }
        }
        #endregion

        /// <summary>
        /// Konstruktør
        /// </summary>
        public Plane(Game game, ContentManager content, Camera camera) : base(game)
        {
            graphics = (GraphicsDeviceManager)Game.Services.GetService
            (typeof(IGraphicsDeviceManager));
            this.camera = camera;
            this.content = content;
        }
        public override void Initialize()
        {
            lightDirection.Normalize();
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            device = graphics.GraphicsDevice;
            base.Initialize();
        }
        protected override void LoadContent()
        {
            effect = content.Load<Effect>("Effects/effect");
            // Modellen til flyet er hentet her: http://sketchup.google.com/3dwarehouse/details?mid=e1dc00e1f6a2cc3c972cda98e8563492&prevstart=0
            // Men det er bearbeidet i Autodesk og sepparert til to forskjellige modeller, propell og flyet.
            spitfireModel = content.Load<Model>("Models/spitfire");
            propellerModel = content.Load<Model>("Models/propeller");
            tomgang = content.Load<SoundEffect>("Sound/tomgang"); // http://www.soundsnap.com/tags/spitfire
            gass = content.Load<SoundEffect>("Sound/gass"); // http://www.soundsnap.com/tags/spitfire
            bulletTexture = content.Load<Texture2D>("Textures/Bullet/bullet"); // Hentet fra riemers tutorial: http://www.riemers.net/eng/Tutorials/XNA/Csharp/series2.php

            shot = content.Load<SoundEffect>("Sound/shot"); // http://www.uic.edu/classes/phyb/phyb401a/sounds/GUNSHOT.WAV
            explosion = content.Load<SoundEffect>("Sound/explosion"); //http://cd.textfiles.com/maxsounds/WAV/EFEITOS/EXPLOSAO.WAV

            tomgangInstance = tomgang.CreateInstance(); // Lager instansen av den innlastede filen til tomgang
            gassInstance = gass.CreateInstance(); // Lager instansen av den innlastede filen til gass
        }
        protected override void UnloadContent()
        {
        }
        public override void Update(GameTime gameTime)
        {
            if (gassInstance.State != SoundState.Playing) // Starter tomganglyden om gasslyden ikke spiller, og stopper den om den gjøre det
                tomgangInstance.Play();
            else
                tomgangInstance.Pause();
            
            ProcessKeyboard(gameTime);
            Falling(moveSpeed);
            Gravity(ref spitfirePosition, spitfireRotation, moveSpeed);
            AirFriction();
            MoveForward(ref spitfirePosition, spitfireRotation, moveSpeed);
            spitfireSpere = new BoundingSphere(spitfirePosition, 0.04f); // Lager ny boundingsphere til flyet
            UpdateCamera();
            UpdateBulletPositions(moveSpeed);
            base.Update(gameTime);
        }
        /// <summary>
        /// Oppdaterer kamera sitt view, projection og posisjon etter posisjonen og retningen til flyet
        /// </summary>
        private void UpdateCamera()
        {
            cameraRotation = Quaternion.Lerp(cameraRotation, spitfireRotation, 0.1f); // Lager litt ettersleng på kameraet
            Vector3 campos = new Vector3(0, 0.1f, 0.6f);
            campos = Vector3.Transform(campos, Matrix.CreateFromQuaternion(cameraRotation)); // Setter kameraposisjon
            campos += spitfirePosition; // Setter kameraposisjon
            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(cameraRotation)); // Setter cameraup variabelen

            camera.View = Matrix.CreateLookAt(campos, spitfirePosition, camup); // Setter kamera sin View matrise
            camera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 0.2f, 500000.0f); // Setter kamera sin projection matrise

            camera.CameraPosition = campos;
            cameraUpDirection = camup;
        }
        /// <summary>
        /// Metoden sjekker om man trykker på knapper, og gjør handlinger ut i fra 
        /// den trykte knappen. Ved piltastene kan man styre flyet, og med mellomromstasten
        /// kan man skyte. A og S er gass og bremse, mens mellomromstasten er for å skyte.
        /// Flyet vil svinge fortere jo større fart det har.
        /// </summary>
        private void ProcessKeyboard(GameTime gameTime)
        {
            float leftRightRot = 0; // Rotasjonsvariabel

            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f; // Hastigheten flyet skal svinge med, øker litt gjennom spillet
            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.Right))
                leftRightRot += turningSpeed * moveSpeed; // Regigerer rotasjonsvariabelen, delvis bassert på flyets hastighet. Større fart = større svingehastighet
            if (keys.IsKeyDown(Keys.Left))
                leftRightRot -= turningSpeed * moveSpeed; // Regigerer rotasjonsvariabelen, delvis bassert på flyets hastighet. Større fart = større svingehastighet

            float upDownRot = 0; // Rotasjonsvariabel
            if (keys.IsKeyDown(Keys.Down))
                upDownRot += (turningSpeed / 2f) * moveSpeed; // Regigerer rotasjonsvariabelen, delvis bassert på flyets hastighet. Større fart = større svingehastighet
            if (keys.IsKeyDown(Keys.Up))
                upDownRot -= (turningSpeed / 2f) * moveSpeed; // Regigerer rotasjonsvariabelen, delvis bassert på flyets hastighet. Større fart = større svingehastighet

            if (keys.IsKeyDown(Keys.A))
            {
                moveSpeed *= 1.005f; // Endrer fartsvariabelen
                propellerRotation += 0.9f; // Endrer propell rotasjonsvariabelen
                if (gassInstance.State != SoundState.Playing) // Spiller av lyden for gass hvis lyden ikke allerede spilles.
                    gassInstance.Play();
            }
            else // Pauser lyden for gass hvis man ikke holder ikke akselerasjonsknappen
                gassInstance.Pause(); 

            if (keys.IsKeyDown(Keys.S)) 
            {
                moveSpeed /= 1.005f; // Slakker ned
            }

            propellerRotation += 0.1f; // Roterer propellen litt uavhengig av om man gir gass
            moveSpeed /= 1.001f; // Mister litt fart om man ikke gir gass
            Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), leftRightRot) * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), upDownRot); // Lager en rotasjonsquaternion av bevegelsene gjort hittil i metoden
            spitfireRotation *= additionalRot; // Endrer flyet sin rotasjon etter den nye rotasjonsquaternionen

            if (keys.IsKeyDown(Keys.Space))
            {
                double currentTime = gameTime.TotalGameTime.TotalMilliseconds; // Variabel med spillets nåværende tid
                if (currentTime - lastBulletTime > 100) // Sjekker om det har gått mer enn 100 millisekunder siden sist skudd ble avfyrt
                {
                    Bullet newBullet = new Bullet(); // Lager et nytt skuddobjekt
                    newBullet.position = spitfirePosition; // Gir objektet flyet sin posisjon
                    newBullet.rotation = spitfireRotation; // Gir objektet flyet sin retning
                    bulletList.Add(newBullet); // Legger objektet i listen over skudd
                    lastBulletTime = currentTime; // Setter tiden skuddet ble skutt
                    shot.Play(); // Spiller av skuddlyd
                }
            }
        }
        /// <summary>
        /// Metoden forflytter flyet i retningen det er vridd, 
        /// basert på hastigheten det har.
        /// </summary>
        private void MoveForward(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), rotationQuat);
            position += addVector * speed;
        }

        #region Fysikk
        /// <summary>
        /// Bremser flyet i forhold til hvor fort det beveger seg, 
        /// for å simulere luftmotstand.
        /// </summary>
        private void AirFriction()
        {
            float motstand = (float)Math.Exp(moveSpeed / 500f);
            moveSpeed /= motstand;
        }
        /// <summary>
        /// Flytter flyet mot bakken basert på hvor stor fart flyet har, 
        /// jo fortere det drar, jo mindre faller det. 
        /// Dette er for å simulere oppdriften vingene gir flyet.
        /// </summary>
        private void Gravity(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            Vector3 addVector = new Vector3(0, -1, 0);
            position += addVector * 9.81f / (speed * speed * speed * speed * 1000.0f);
        }
        /// <summary>
        /// Metoden simulerer tiltingen flyet får mot bakken når farten synker.
        /// Når farten blir lav nok, blir snuten på flyet dratt rett ned, uansett hvilken retning og rotasjon flyet har.
        /// Det vil bli dratt hardere mot bakken jo mindre fart det har.
        /// </summary>
        private void Falling(float speed)
        {
            x = (Matrix.CreateFromQuaternion(spitfireRotation).Forward);
            z = (Matrix.CreateFromQuaternion(spitfireRotation).Up);
            y = (Matrix.CreateFromQuaternion(spitfireRotation).Right);
            Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(-1, 0, 0), z.Y / (speed * speed * speed * speed * 2000.0f)) * Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), y.Y / (speed * speed * speed * speed * 2000.0f));
            spitfireRotation *= additionalRot;
            moveSpeed *= 1 + (x.Y / -160.0f); // Gir flyet en økt akselerasjon om det er vendt med snuten mot bakken
        }
        #endregion

        /// <summary>
        /// Flytter kulene og kulenes boundingSphere sin posisjon.
        /// </summary>
        private void UpdateBulletPositions(float moveSpeed)
        {
            for (int i = 0; i < bulletList.Count; i++) // Traverserer kulene
            {
                Bullet currentBullet = bulletList[i]; // Henter ut en kule
                MoveForward(ref currentBullet.position, currentBullet.rotation, moveSpeed + 15.0f); // Endrer posisjonen
                currentBullet.bulletSphere = new BoundingSphere(currentBullet.position, 0.05f); // Endrer boundingsphere sin posisjon
                bulletList[i] = currentBullet; // Legger kulen tilbake
            }
        }
        public override void Draw(GameTime gameTime)
        {
            DrawSpitfire(); // Tegner flyet
            DrawBullets(); // Tegner kulene
            base.Draw(gameTime);
        }
        /// <summary>
        /// Metode som tegner flyet etter å ha skalert det ned med riktig posisjon og vridning.
        /// </summary>
        private void DrawSpitfire()
        {
            Matrix worldMatrix = Matrix.CreateScale(0.02f, 0.02f, 0.02f) * Matrix.CreateFromQuaternion(spitfireRotation) * Matrix.CreateTranslation(spitfirePosition); // Lager matrix til flyet
            Matrix[] matrixSpitfire = new Matrix[spitfireModel.Bones.Count]; // Matrixarray av bones
            //Denne sørger for at absolutte transformasjonsmatriser for hver
            //ModelMesh legges inn i matrisetabellen:
            spitfireModel.CopyAbsoluteBoneTransformsTo(matrixSpitfire);
            foreach (ModelMesh mesh in spitfireModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = matrixSpitfire[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                    // Lys & action:
                    effect.EnableDefaultLighting();
                    effect.LightingEnabled = true;
                    effect.VertexColorEnabled = false;
                }
                mesh.Draw();
            }
            DrawPropeller(worldMatrix); // Tegner propellen i forhold til flyets posisjon
        }
        /// <summary>
        /// Metoden tegner propellen etter matrixen som blir opprettet i drawSpitfire metoden
        /// </summary>
        private void DrawPropeller(Matrix world)
        {
            Matrix worldMatrix = Matrix.CreateRotationZ(-propellerRotation) * Matrix.CreateTranslation(new Vector3(0.0f,1.0f,-3.5f)) * world; 
            Matrix[] matrixPropeller = new Matrix[propellerModel.Bones.Count];
            //Denne sørger for at absolutte transformasjonsmatriser for hver
            //ModelMesh legges inn i matrisetabellen:
            propellerModel.CopyAbsoluteBoneTransformsTo(matrixPropeller);
            foreach (ModelMesh mesh in propellerModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = matrixPropeller[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                    // Lys & action:
                    effect.EnableDefaultLighting();
                    effect.LightingEnabled = true;
                    effect.VertexColorEnabled = false;
                }
                mesh.Draw();
            }
        }
        /// <summary>
        /// Metode som tegner alle skuddene som eksisterer
        /// </summary>
        private void DrawBullets()
        {
            if (bulletList.Count > 0) // Sjekker om det eksisterer noen skudd
            {
                VertexPositionTexture[] bulletVertices = new VertexPositionTexture[bulletList.Count * 6];
                int i = 0;
                foreach (Bullet currentBullet in bulletList)
                {
                    Vector3 center = currentBullet.position;
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 0));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                }
                effect.CurrentTechnique = effect.Techniques["PointSprites"];
                effect.Parameters["xWorld"].SetValue(Matrix.Identity);
                effect.Parameters["xProjection"].SetValue(camera.Projection);
                effect.Parameters["xView"].SetValue(camera.View);
                effect.Parameters["xCamPos"].SetValue(camera.CameraPosition);
                effect.Parameters["xTexture"].SetValue(bulletTexture);
                effect.Parameters["xCamUp"].SetValue(cameraUpDirection);
                effect.Parameters["xPointSpriteSize"].SetValue(0.5f);
                device.BlendState = BlendState.Additive;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, bulletVertices, 0, bulletList.Count * 2);
                }
                device.BlendState = BlendState.Opaque;
            }
        }
    }
}