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

    public struct MyVertexPositionNormalColored : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;
        public MyVertexPositionNormalColored(Vector3 position, Color color, Vector3
        normal)
        {
            this.Position = position;
            this.Color = color;
            this.Normal = normal;
        }
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
        new VertexElement(0, VertexElementFormat.Vector3,
        VertexElementUsage.Position, 0),
        new VertexElement(sizeof(float) * 3, VertexElementFormat.Color,
        VertexElementUsage.Color, 0),
        new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector3,
        VertexElementUsage.Normal, 0)
        );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Blimps : Microsoft.Xna.Framework.DrawableGameComponent
    {
        //Holder rede på hvordan type kollisjon som skjer
        enum CollisionType { None, BlimpCollision, BlimpImpact }

        #region Variabler
        private GraphicsDeviceManager graphics; //Graphics device manager
        private SpriteBatch spriteBatch; //spritebatch
        private Effect effect; //Effect object
        private Random rand = new Random(); //random instance
        private ContentManager Content; //Instans av ContentManager
        private Camera camera; //Kameraet
        private Terrain terrain; //Terrenget
        private const int MAXBLIMPS = 50; //maks antall blimper
        //Vertices array til å tegne blimpene.
        private MyVertexPositionNormalColored[] verticesSphere;
        private MyVertexPositionNormalColored[] verticesSides;
        private MyVertexPositionNormalColored[] verticesTop;
        private MyVertexPositionNormalColored[] verticesBottom;
        private MyVertexPositionNormalColored[] tail;

        //Matriser
        private Matrix world;
        private Matrix rotMatrix;

        private Stack<Matrix> matrixStack = new Stack<Matrix>(); //Stack som holder på world matrisene til blimpene
        private List<Blimp> blimps; //Liste med blimps
        #endregion

        #region Properties
        public List<Blimp> Blimps1
        {
            get { return blimps; }
            set { blimps = value; }
        }
        #endregion

        /// <summary>
        /// Kontruktør
        /// </summary>
        /// <param name="game">Spillet</param>
        /// <param name="Content">Content manager</param>
        /// <param name="camera">Kameraet</param>
        /// <param name="terrain">Terrenget</param>
        public Blimps(Game game, ContentManager Content, Camera camera, Terrain terrain)
            : base(game)
        {
            graphics = (GraphicsDeviceManager)Game.Services.GetService
            (typeof(IGraphicsDeviceManager));
            this.camera = camera;
            this.Content = Content;
            this.terrain = terrain;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Laster inn effekt objektet, og lager blimpene.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            effect = Content.Load<Effect>(@"Effects/effect");
            AddBlimps();
            SetUpVertices();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        #region Setter opp blimpen
        /// <summary>
        /// Setter sammen blimp verticene.
        /// Er egentlig en kule som bare er strukket i Z-retning.
        /// Returnerer ballongens vertices array
        /// </summary>
        private MyVertexPositionNormalColored[] InitVertices()
        {
            float c = (float)Math.PI / 180.0f; //Bruker radianer.
            float phir = 0.0f;
            float phir20 = 0.0f;
            float thetar = 0.0f;
            float x = 0.0f, y = 0.0f, z = 0.0f;//X, Y, og Z koordinater
            int i = 0;
            int size = 10 * 38; //størrelsen på arrayet

            verticesSphere = new MyVertexPositionNormalColored[size];

            //Kjører gjennom to løkke og lager sirkelen.
            for (float phi = -100.0f; phi <= 80.0f; phi += 20)
            {
                phir = c * phi; //phi omgjort til radianer
                phir20 = c * (phi + 20); //(phi+20) omgjort til radianer
                //Varierer teta:
                for (float theta = -180.0f; theta <= 180.0f; theta += 20)
                {
                    thetar = c * theta;
                    x = (float)Math.Cos(phir) * (float)Math.Sin(thetar);
                    y = (float)Math.Cos(phir) * (float)Math.Cos(thetar);
                    z = (float)Math.Sin(phir);

                    verticesSphere[i].Position = new Vector3(x, y, z);
                    i++;
                    x = (float)Math.Cos(phir20) * (float)Math.Sin(thetar);
                    y = (float)Math.Cos(phir20) * (float)Math.Cos(thetar);
                    z = (float)Math.Sin(phir20);
                    verticesSphere[i].Position = new Vector3(x, y, z);
                    i++;
                }
            }
            //Instansierer boksen under ballongen i blimpen
            verticesSides = new MyVertexPositionNormalColored[10];
            verticesBottom = new MyVertexPositionNormalColored[4];
            verticesTop = new MyVertexPositionNormalColored[4];

            //Sidene
            verticesSides[0].Position = new Vector3(0.5f, -0.5f, -0.5f);
            verticesSides[1].Position = new Vector3(0.5f, 0.5f, -0.5f);
            verticesSides[2].Position = new Vector3(0.5f, -0.5f, 0.5f);
            verticesSides[3].Position = new Vector3(0.5f, 0.5f, 0.5f);
            verticesSides[4].Position = new Vector3(-0.5f, -0.5f, 0.5f);
            verticesSides[5].Position = new Vector3(-0.5f, 0.5f, 0.5f);
            verticesSides[6].Position = new Vector3(-0.5f, -0.5f, -0.5f);
            verticesSides[7].Position = new Vector3(-0.5f, 0.5f, -0.5f);
            verticesSides[8].Position = new Vector3(0.5f, -0.5f, -0.5f);
            verticesSides[9].Position = new Vector3(0.5f, 0.5f, -0.5f);
            //slutt Sidene

            //Bunnen
            verticesBottom[0].Position = new Vector3(-0.5f, -0.5f, -0.5f);
            verticesBottom[1].Position = new Vector3(0.5f, -0.5f, -0.5f);
            verticesBottom[2].Position = new Vector3(-0.5f, -0.5f, 0.5f);
            verticesBottom[3].Position = new Vector3(0.5f, -0.5f, 0.5f);
            //slutt Bunnen

            //Toppen
            verticesTop[0].Position = new Vector3(-0.5f, 0.5f, -0.5f);
            verticesTop[1].Position = new Vector3(0.5f, 0.5f, -0.5f);
            verticesTop[2].Position = new Vector3(-0.5f, 0.5f, 0.5f);
            verticesTop[3].Position = new Vector3(0.5f, 0.5f, 0.5f);
            //slutt Toppen

            //instansierer halen
            tail = new MyVertexPositionNormalColored[4];

            //Lager halen
            tail[0].Position = new Vector3(-0.5f, -0.5f, -0.5f);
            tail[1].Position = new Vector3(0.5f, -0.5f, -0.5f);
            tail[2].Position = new Vector3(-0.5f, -0.5f, 0.5f);
            tail[3].Position = new Vector3(0.5f, -0.5f, 0.5f);

            //setter farge og normalvektor på boksen og halen
            for (int j = 0; j < 10; j++)
            {
                verticesSides[j].Color = Color.Gray;
                verticesSides[j].Normal = new Vector3(0, 0, 0);
            }
            for (int j = 0; j < 4; j++)
            {
                verticesTop[j].Color = Color.Gray;
                verticesTop[j].Normal = new Vector3(0, 1, 0);
                verticesBottom[j].Color = Color.Gray;
                verticesBottom[j].Normal = new Vector3(0, 0, 0);
                tail[j].Color = Color.Gray;
                tail[j].Normal = new Vector3(0, 1, 0);
            }

            return verticesSphere;
        }

        /// <summary>
        /// Setter opp indicene til blimpen
        /// </summary>
        /// <returns>indices arrayet</returns>
        private int[] InitIndices()
        {
            int[] indices = new int[9 * 37 * 6];
            int counter = 0;

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 37; x++)
                {
                    int lowerLeft = x + y * 38;
                    int lowerRight = (x + 1) + y * 38;
                    int topLeft = x + (y + 1) * 38;
                    int topRight = (x + 1) + (y + 1) * 38;

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }

            return indices;

        }

        /// <summary>
        /// Kalkulerer normalvektorene til blimpen
        /// </summary>
        /// <param name="vertices">Verticene til blimpen</param>
        /// <param name="indices">Indicene til blimpen</param>
        /// <returns>Vertices array etter å ha satt på normalvektor</returns>
        private MyVertexPositionNormalColored[] CalculateNormals(MyVertexPositionNormalColored[] vertices, int[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            /// Går gjennom en tredjedel av indicene og setter sammen to vertexer
            /// og deretter tar kryssprodukt av dem og lager normalen.
            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];
                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);
                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();

            return vertices;
        }

        /// <summary>
        /// Kaller på Vertices arrayet, Indices arrayet,
        /// og normal kalkuleringen. (Er en slags samle metode som blir kalt på i loadContent)
        /// </summary>
        private void SetUpVertices()
        {
            MyVertexPositionNormalColored[] terrainVertices = InitVertices();
            int[] terrainIndices = InitIndices();
            terrainVertices = CalculateNormals(terrainVertices, terrainIndices);
        }
        #endregion

        /// <summary>
        /// Kaller på blimpLogic(Gametime gameTime), se egen metode
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            // TODO: Add your update logic here

            BlimpLogic(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// Legger til nye blimps
        /// </summary>
        private void AddBlimps()
        {
            blimps = new List<Blimp>();
            Blimp b;
            int i = 1;
            while (blimps.Count < MAXBLIMPS)
            {
                int x = rand.Next((int)terrain.EdgeX.X, (int)terrain.EdgeX.Y); //X verdi innenfor en grense
                int z = rand.Next((int)terrain.EdgeZ.X, (int)terrain.EdgeZ.Y); //Z verdi innenfor en grense
                float y = (float)rand.Next(2000, 3000); //Y verdi innenfor en grense
                Color color = new Color(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255)); //Setter tilfeldig farge på blimpen
                float rot = rand.Next(0, (int)(2 * MathHelper.Pi * 100)); //Snur alle forskjellige veier
                rot = rot / 100f;
                b = new Blimp(new Vector3(x, y, z), color, rot);
                b.Id = i;
                b.CurrentRotation = b.Rotation;
                b.BlimpSphere = new BoundingSphere(new Vector3(b.Pos.X, b.Pos.Y, b.Pos.Z), 150f);

                //sjekker om 2 blimper opprettes i samme posisjon.
                if (checkCollision(b) == CollisionType.None)
                    blimps.Add(b);

                i++;
            }

        }

        #region Bevegelse og logikk
        /// <summary>
        /// Metode som får blimpene til å bevege seg fremover
        /// </summary>
        /// <param name="b">en blimp</param>
        private void Move(Blimp b)
        {
            rotMatrix = new Matrix();
            Vector3 addVector;
            rotMatrix = Matrix.Identity * Matrix.CreateRotationY(b.Rotation); //Finner rotasjonen til blimpen
            addVector = Vector3.Transform(new Vector3(0, 0, 1), rotMatrix); //Transformerer en vektor slik at ballongen alltid peker den veien den beveger seg.
            b.Pos += addVector * b.Speed;
        }

        /// <summary>
        /// Brukes når en blimp skal ungå en annen
        /// </summary>
        /// <param name="b">en blimp</param>
        private void Reverese(Blimp b)
        {
            rotMatrix = new Matrix();
            Vector3 addVector;
            rotMatrix = Matrix.Identity * Matrix.CreateRotationY(b.Rotation);
            addVector = Vector3.Transform(new Vector3(0, 0, -1), rotMatrix);
            b.Pos += addVector * b.Speed * 2;
        }

        /// <summary>
        /// Her er det all logikken skjer
        /// Skjekker om to blimper kolliderer eller om de er utenfor en viss grense.
        /// </summary>
        /// <param name="gameTime">tar inn gameTime fra update</param>
        private void BlimpLogic(GameTime gameTime)
        {
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 2500.0f;

            //Går igjennom alle blimpene og skjekker om de er innenfor en grense
            foreach (Blimp b in blimps)
            {
                Move(b);
                if (b.Pos.X <= terrain.EdgeX.X + 200 || b.Pos.X >= terrain.EdgeX.Y - 200 || b.Pos.Z <= terrain.EdgeZ.X + 200 || b.Pos.Z >= terrain.EdgeZ.Y - 200)
                {
                    //Om man blimpen er utenfor Edge, vil han roterer 0 til 180 grader får å komme seg tilbake.
                    if (b.Rotation <= b.CurrentRotation + MathHelper.Pi)
                        b.Rotation += turningSpeed * b.Speed;

                }
                else
                    b.CurrentRotation = b.Rotation; //Setter tilbake rotasjonen

                //Lager BoundingSpherene til blimpen
                b.BlimpSphere = new BoundingSphere(new Vector3(b.Pos.X, b.Pos.Y, b.Pos.Z), 150f);
                b.BlimpBackSphere = new BoundingSphere(new Vector3(b.Pos.X, b.Pos.Y, b.Pos.Z - 200 / 3), 60f);
                b.BlimpFrontSphere = new BoundingSphere(new Vector3(b.Pos.X, b.Pos.Y, b.Pos.Z + 200 / 3), 60f);

                //Skjekker om to blimper er på tur å kollidere
                if (checkCollision(b) == CollisionType.BlimpCollision)
                {
                    //Om dette stemmer snur begge 90 grader
                    if (b.Rotation <= b.CurrentRotation + MathHelper.PiOver4)
                        b.Rotation += turningSpeed * b.Speed;

                }
                //Om de ikke unngår hverandre selv etter å snu, vil de starte å rygge unna.
                if (checkImpact(b) == CollisionType.BlimpImpact)
                {
                    Reverese(b);
                }
                else
                {
                    b.CurrentRotation = b.Rotation;
                }

            }

        }
        #endregion

        #region Kollisjon
        /// <summary>
        /// Ser om 2 blimper er på tur å kollidere
        /// Bruker den ytre BoundingSpheren til blimpen
        /// </summary>
        /// <param name="b">en blimp</param>
        /// <returns>kollisjons type</returns>
        private CollisionType checkCollision(Blimp b)
        {
            foreach (Blimp blimp in blimps)
            {
                if (blimp.Id != b.Id)
                {
                    if (b.BlimpSphere.Contains(blimp.BlimpSphere) != ContainmentType.Disjoint)
                        return CollisionType.BlimpCollision;
                }
            }
            return CollisionType.None;
        }

        /// <summary>
        /// Sjekker om to blimper kolliderer
        /// </summary>
        /// <param name="b">en blimp</param>
        /// <returns>kollisjonstype</returns>
        private CollisionType checkImpact(Blimp b)
        {
            foreach (Blimp blimp in blimps)
            {
                if (blimp.Id != b.Id)
                {
                    if (b.BlimpFrontSphere.Contains(blimp.BlimpFrontSphere) != ContainmentType.Disjoint || b.BlimpFrontSphere.Contains(blimp.BlimpBackSphere) != ContainmentType.Disjoint)
                    {
                        return CollisionType.BlimpImpact;
                    }
                }
            }
            return CollisionType.None;
        }
        #endregion

        #region Tegning
        /// <summary>
        /// Draw metode som kaller på DrawBlimp()
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.FillMode = FillMode.Solid; //Tegner dem solide
            rasterizerState1.CullMode = CullMode.None; //Ingen culling på
            graphics.GraphicsDevice.RasterizerState = rasterizerState1;

            DrawBlimp();
            base.Draw(gameTime);

        }

        /// <summary>
        /// Tegner hele blimpen, ordner world matrisa, og sender informasjon til shaderen
        /// Det brukes en matrise stack slik at boksen og halen ordner seg automatisk etter ballongen
        /// </summary>
        private void DrawBlimp()
        {

            foreach (Blimp b in blimps)
            {
                for (int i = 0; i < verticesSphere.Length; i++)
                    verticesSphere[i].Color = b.Color;

                world = Matrix.Identity * Matrix.CreateScale(50f, 50f, 100f) * Matrix.CreateRotationY(b.Rotation) * Matrix.CreateTranslation(b.Pos);

                matrixStack.Push(world); //Pusher matrisa på stacken
                b.World = world;
                effect.CurrentTechnique = effect.Techniques["Colored"];
                effect.Parameters["xWorld"].SetValue(world);
                effect.Parameters["xView"].SetValue(camera.View);
                effect.Parameters["xProjection"].SetValue(camera.Projection);
                effect.Parameters["xEnableLighting"].SetValue(true);
                effect.Parameters["xAmbient"].SetValue(0.8f);
                effect.Parameters["xLightDirection"].SetValue(new Vector3(-0.5f, 1, 0.5f));

                //Starter tegning - bruker egendefinert effekt:
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, verticesSphere, 0, 378, MyVertexPositionNormalColored.VertexDeclaration);
                }

                DrawBox(); //Tegner boksen
                DrawTail(); //Tegner halen

            }
        }

        /// <summary>
        /// Tegner boksen
        /// </summary>
        private void DrawBox()
        {
            Matrix _world = matrixStack.Peek(); //Henter ut world matrisa til ballongen på stacken

            world = Matrix.Identity * Matrix.CreateScale(0.5f, 0.5f, 0.5f) * Matrix.CreateTranslation(0f, -1.0f, 0f) * _world;

            //Sender inn worldmatrisa
            effect.Parameters["xWorld"].SetValue(world);

            //Starter tegning :
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, verticesSides, 0, 8, MyVertexPositionNormalColored.VertexDeclaration);
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, verticesBottom, 0, 2, MyVertexPositionNormalColored.VertexDeclaration);
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, verticesTop, 0, 2, MyVertexPositionNormalColored.VertexDeclaration);
            }
        }

        /// <summary>
        /// Tegner halen, siden den er laget ved å posisjonere to flate firkanter,
        /// tegnes de hver for seg.
        /// </summary>
        private void DrawTail()
        {
            Matrix _world = matrixStack.Peek(); //Henter ut world matrisa til ballongen på stacken

            //setter opp første world matrise
            world = Matrix.Identity * Matrix.CreateScale(1.8f, 1.8f, 1.0f) * Matrix.CreateTranslation(0f, 0.9f, -0.45f) * _world;
            //Sender inn worldmatrisa
            effect.Parameters["xWorld"].SetValue(world);

            //Starter å tegne første del
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, tail, 0, 2, MyVertexPositionNormalColored.VertexDeclaration);
            }
            //setter opp andre world matrise
            world = Matrix.Identity * Matrix.CreateScale(1.8f, 1.8f, 1.0f) * Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateTranslation(-0.9f, 0f, -0.45f) * _world;
            //Sender inn worldmatrisa
            effect.Parameters["xWorld"].SetValue(world);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, tail, 0, 2, MyVertexPositionNormalColored.VertexDeclaration);
            }
        }
        #endregion
    }
}