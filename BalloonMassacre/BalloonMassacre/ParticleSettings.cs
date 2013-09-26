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

namespace BalloonMassacre
{
    class ParticleSettings
    {
        // Partikkelens størrelse
        public int maxSize = 15;
    }
    class ParticleExplosionSettings
    {
        // Livslengde
        public int minLife = 1000;
        public int maxLife = 2000;
        // Partikkler per runde
        public int minParticlesPerRound = 2000;
        public int maxParticlesPerRound = 8000;
        // Rundetid
        public int minRoundTime = 16;
        public int maxRoundTime = 50;
        // Antall partikkler
        public int minParticles = 5000;
        public int maxParticles = 10000;
    }
}
