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

namespace BalloonMassacre
{
    /// <summary>
    /// Blimp klassen, lagrer variabler som trengs for å kunne bevege seg og sjekke kollisjon.
    /// </summary>
    public class Blimp
    {
        #region Variabler
        private Vector3 pos; //blimpen sin posisjon
        private int id; //en id, holder styr på hvilken blimp som er hvilken.
        private Color color; //farge til blimpen
        private const float speed = 0.5f; //farten
        private bool isTurning = false; //Sjekker om blimpen snur
        private float rotation; //rotasjons variabel
        private float currentRotation; //nåtids rotasjon
        private BoundingSphere blimpSphere; //Stor kule som brukes til å sjekke kollisjon mellom blimpene
        private BoundingSphere blimpFrontSphere; //det ligger 2 boundingspheres som sjekker kollisjon med fly og skudd
        private BoundingSphere blimpBackSphere;
        private Matrix world; //world matrisa til blimpen
        #endregion

        #region Properties
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public Vector3 Pos
        {
            get { return pos; }
            set { pos = value; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public float Speed
        {
            get { return speed; }
        }

        public bool IsTurning
        {
            get { return isTurning; }
            set { isTurning = value; }
        }

        public BoundingSphere BlimpSphere
        {
            get { return blimpSphere; }
            set { blimpSphere = value; }
        }

        public BoundingSphere BlimpFrontSphere
        {
            get { return blimpFrontSphere; }
            set { blimpFrontSphere = value; }
        }

        public BoundingSphere BlimpBackSphere
        {
            get { return blimpBackSphere; }
            set { blimpBackSphere = value; }
        }

        public float CurrentRotation
        {
            get { return currentRotation; }
            set { currentRotation = value; }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Matrix World
        {
            get { return world; }
            set { world = value; }
        }
        #endregion

        /// <summary>
        /// Kontruktør
        /// </summary>
        /// <param name="pos">posisjon</param>
        /// <param name="color">farge</param>
        /// <param name="rotation">rotasjon</param>
        public Blimp(Vector3 pos, Color color, float rotation)
        {
            this.pos = pos;
            this.color = color;
            this.rotation = rotation;
        }
    }
}