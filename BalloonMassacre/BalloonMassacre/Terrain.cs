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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
namespace BalloonMassacre
{
    /// <summary>
    /// Game component som lager terrenget, (Ett bilde som blir skalert opp 200 ganger!).
    /// Gjort på denne måten fordi vi skal kunne sjekke om flyet sjekker krasjer i det.
    /// Med flere vertices vil det ikke være mulig! Logikken bak det å sette opp terrenget er basert på
    /// riemer og forelesninger i faget.
    /// </summary>
    public class Terrain : Microsoft.Xna.Framework.DrawableGameComponent
    {

        #region Variabler
        private VertexPositionNormalTexture[] terrainVertices; //Verticene til terrenget
        private float terrainScale = 200.0f; //Hvor my terrenget skal scale
        private int terrainWidth = 0, terrainHeight = 0; //Lengden og høyden til bitmappet
        private float[,] heightData; //Høyde data
        private int[] indices; //Indicene til terrenget
        private Vector2 edgeX, edgeZ; //Kanter, brukes for å lage en virtuell barierer for blimpene
        private GraphicsDeviceManager manager; //Graphics device manager
        private GraphicsDevice device; //Graphics device
        private Texture2D terrain; //Bitmapet som brukes som høydekart
        private Texture2D snow; //Snø teksturen
        private Effect effect; //Effekt objektet
        private Camera camera; //Kameraet
        private ContentManager content; //Content manager
        private int verticesLength = 0, indicesLength = 0; //lengden på arrayene
        private VertexBuffer terrainVertexBuffer; //Vertexbuffer
        private IndexBuffer terrainIndexBuffer; //indexbuffer
        #endregion
        #region Properties
        public Vector2 EdgeZ
        {
            get { return edgeZ; }
            set { edgeZ = value; }
        }

        public VertexPositionNormalTexture[] TerrainVertices
        {
            get { return terrainVertices; }
            set { terrainVertices = value; }
        }

        public Vector2 EdgeX
        {
            get { return edgeX; }
            set { edgeX = value; }
        }
        private Matrix worldMatrix;

        public Matrix WorldMatrix
        {
            get { return worldMatrix; }
            set { worldMatrix = value; }
        }
        #endregion

        /// <summary>
        /// Kontruktør som instansierer terrenget, kameraet og Contentmanager
        /// </summary>
        /// <param name="game">Spillet</param>
        /// <param name="content">Content manager</param>
        /// <param name="camera">Kameraet</param>
        public Terrain(Game game, ContentManager content, Camera camera)
            : base(game)
        {
            manager = (GraphicsDeviceManager)Game.Services.GetService
            (typeof(IGraphicsDeviceManager));
            this.camera = camera;
            this.content = content;
            terrain = content.Load<Texture2D>("Terrain/map");
        }

        public override void Initialize()
        {
            // TODO: Add your initialization code here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent, kaller på metoder som setter sammen kartet,
        /// og instansierer effektobjektet.
        /// </summary>
        protected override void LoadContent()
        {
            device = manager.GraphicsDevice;
            effect = content.Load<Effect>("Effects/effect");
            LoadHeightData(terrain);
            SetUpVertices();
            LoadTextures();
            Edges();
            base.LoadContent();
        }

        /// <summary>
        /// Laster inn tekstur
        /// </summary>
        private void LoadTextures()
        {
            //http://thumbs2.modthesims.info/img/1/4/3/6/5/MTS_tkdjunkie-166622-snowtexture.png
            snow = content.Load<Texture2D>("Textures/Terrain/snowtexture");
        }

        /// <summary>
        /// Setter virtuelle grenseverider
        /// </summary>
        private void Edges()
        {
            edgeX = new Vector2(-1800, 1800);
            edgeZ = new Vector2(-1800, 1800);
        }

        #region Setter opp terrenget
        /// <summary>
        /// Samle metode som kaller på alle metodene som brukes til å sette sammen terrenget.
        /// Metoden blir deretter kalt i LoadContent
        /// </summary>
        private void SetUpVertices()
        {
            terrainVertices = InitTerrainVertices();
            int[] terrainIndices = SetUpIndices();
            terrainVertices = CalculateNormals(terrainVertices, terrainIndices);
            CopyToTerrainBuffers(terrainVertices, terrainIndices);
            verticesLength = GetVertexLength(terrainVertices);
            indicesLength = GetIndicesLength(terrainIndices);
        }

        /// <summary>
        /// Setter opp terrengets verticeser
        /// </summary>
        /// <returns>vertices arrayet</returns>
        private VertexPositionNormalTexture[] InitTerrainVertices()
        {
            VertexPositionNormalTexture[] terrainVertices = new VertexPositionNormalTexture[(terrainWidth * terrainHeight)];
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainHeight; y++)
                {
                    //Setter posisjon i fra (0,0,0) i positiv X og negativ Z, bruker data fra bitmap til å bestemme Y
                    terrainVertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);
                    terrainVertices[x + y * terrainWidth].TextureCoordinate.X = (float)x / 10.0f;
                    terrainVertices[x + y * terrainWidth].TextureCoordinate.Y = (float)y / 10.0f;
                }
            }
            return terrainVertices;
        }

        /// <summary>
        /// Får vertices arrayet sin lengde
        /// </summary>
        /// <param name="vertices">vetices arrayet</param>
        /// <returns>lengden</returns>
        private int GetVertexLength(VertexPositionNormalTexture[] vertices)
        {
            return vertices.Length;
        }

        /// <summary>
        /// Får indices arrayet sin lengde
        /// </summary>
        /// <param name="indices">indices arrayet</param>
        /// <returns>lengden</returns>
        private int GetIndicesLength(int[] indices)
        {
            return indices.Length;
        }

        /// <summary>
        /// Setter opp indicene til terrenget
        /// </summary>
        /// <returns>indices arrayet</returns>
        private int[] SetUpIndices()
        {
            indices = new int[((terrainWidth - 1) * (terrainHeight - 1) * 6)];
            int counter = 0;
            for (int y = 0; y < terrainHeight - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;
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
        /// Kalkulerer normalvektorerene
        /// </summary>
        /// <param name="vertices">vertices arrayet</param>
        /// <param name="indices">indices arrayet</param>
        /// <returns>vertices med kalkulerte normaler</returns>
        private VertexPositionNormalTexture[] CalculateNormals(VertexPositionNormalTexture[] vertices, int[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            ///setter sammen 2 vectorer ved å bruke to vertice posisjoner og kalkulerer normalen
            ///ved å ta kryssproduktet av disse.
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
            //Normaliserer normalene
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();
            return vertices;
        }

        /// <summary>
        /// Kopierer vertices arrayet og indices arrayet til Bufferne på Skjermkortet.
        /// </summary>
        /// <param name="vertices">vertices array</param>
        /// <param name="indices">indices array</param>
        private void CopyToTerrainBuffers(VertexPositionNormalTexture[] vertices, int[] indices)
        {
            terrainVertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
            terrainVertexBuffer.SetData(vertices);
            terrainIndexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            terrainIndexBuffer.SetData(indices);
        }

        /// <summary>
        /// Laster inn bitmapet og kalkulerer høydedata.
        /// </summary>
        /// <param name="heightMap">bitmap</param>
        private void LoadHeightData(Texture2D heightMap)
        {
            terrainWidth = heightMap.Width; //setter bredde lik bitmappet sin bredde
            terrainHeight = heightMap.Height; //setter høyden lik bitmappet sin høyde
            Color[] heightMapColors = new Color[terrainWidth * terrainHeight];
            heightMap.GetData(heightMapColors);
            heightData = new float[terrainWidth, terrainHeight];

            //Går gjennom hele bitmappet og finner høydedata basert på hvor høy rødfarge verdien i RGB er.
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainHeight; y++)
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R / 10.0f;
        }
        #endregion
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        #region Tegning
        /// <summary>
        /// Kaller på DrawTerrain()
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            DrawTerain(-(terrainWidth * terrainScale) / 2, (terrainWidth * terrainScale) / 2); //translaterer terrenget etter hvor stort det er
            base.Draw(gameTime);
        }

        /// <summary>
        /// Tegner opp terrenget
        /// </summary>
        /// <param name="transX">x-translasjon</param>
        /// <param name="transZ">z-translasjon</param>
        private void DrawTerain(float transX, float transZ)
        {
            //Velger technique
            effect.CurrentTechnique = effect.Techniques["Textured"];

            worldMatrix = Matrix.Identity * Matrix.CreateScale(terrainScale) * Matrix.CreateTranslation(transX, -1000, transZ);
            //Sender inn parameter til shaderen
            effect.Parameters["xTexture"].SetValue(snow);
            effect.Parameters["xWorld"].SetValue(worldMatrix);
            effect.Parameters["xView"].SetValue(camera.View);
            effect.Parameters["xProjection"].SetValue(camera.Projection);
            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xAmbient"].SetValue(0.4f);
            effect.Parameters["xLightDirection"].SetValue(new Vector3(-0.5f, -1, -0.5f));
            //sender bufferne til skjermkort
            device.SetVertexBuffer(terrainVertexBuffer);
            device.Indices = terrainIndexBuffer;
            //Starter å tegne
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, verticesLength, 0, (indicesLength / 3));
            }
        }
        #endregion
    }
}