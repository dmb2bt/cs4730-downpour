using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Downpour
{
    public class AudioManager
    {
        ContentManager content;


        SoundEffect menuSong;
        SoundEffectInstance menu;
        SoundEffect levelSong;
        SoundEffectInstance level;
        SoundEffect powerupSound;
        SoundEffectInstance powerup;
        SoundEffect fireSound;
        SoundEffectInstance fire;
        SoundEffect firePieceSound;
        SoundEffectInstance firepiece;
        SoundEffect deathSound;
        SoundEffectInstance death;
        SoundEffect footstepSound;
        SoundEffectInstance footstep;
        SoundEffect rainSound;
        SoundEffectInstance rain;
        SoundEffect invulnerabilitySound;
        SoundEffectInstance invulnerability;
        SoundEffect thunderSound;
        SoundEffectInstance thunder;

        public AudioManager(ContentManager manager)
        {
            content = manager;
            LoadContent();
        }

        public void LoadContent()
        {
            menuSong = content.Load<SoundEffect>("Sound/mainmenu.wav");
            menu = menuSong.CreateInstance();
            levelSong = content.Load<SoundEffect>("Sound/level.wav");
            level = levelSong.CreateInstance();
            powerupSound = content.Load<SoundEffect>("Sound/burp");
            powerup = powerupSound.CreateInstance();
            fireSound = content.Load<SoundEffect>("Sound/fire");
            fire = fireSound.CreateInstance();
            firePieceSound = content.Load<SoundEffect>("Sound/fire_piece");
            firepiece = firePieceSound.CreateInstance();
            deathSound = content.Load<SoundEffect>("Sound/death");
            death = deathSound.CreateInstance();
            footstepSound = content.Load<SoundEffect>("Sound/footstep");
            rainSound = content.Load<SoundEffect>("Sound/rain");
            rain = rainSound.CreateInstance();
            invulnerabilitySound = content.Load<SoundEffect>("Sound/invulnerability");
            invulnerability = invulnerabilitySound.CreateInstance();
            thunderSound = content.Load<SoundEffect>("Sound/thunder");
            thunder = thunderSound.CreateInstance();
        }

        public void playMenuSong()
        {
            menu.IsLooped = true;
            menu.Volume = 0.50f;
            menu.Play();
        }

        public void stopMenuSong()
        {
            menu.Stop();
        }

        public void playLevelSong()
        {
            level.IsLooped = true;
            level.Volume = 0.25f;
            level.Play();
        }

        public void stopLevelSong()
        {
            level.Stop();
        }

        public void playRainSound()
        {
            rain.IsLooped = true;
            rain.Volume = 0.75f;
            rain.Play();
        }

        public void stopRainSound()
        {
            rain.Stop();
        }

        public void incrementRainVolume()
        {
            rain.Volume += 0.25f;
        }

        public void decrementRainVolume()
        {
            rain.Volume -= 0.25f;
        }

        public void playFireSound()
        {
            fire.Volume = 1.0f;
            fire.Play();
        }

        public void stopFireSound()
        {
            fire.Stop();
        }

        public void playFirePieceSound()
        {
            firepiece.Volume = 0.5f;
            firepiece.Play();
        }

        public void playPowerupSound()
        {
            powerup.Volume = 10.0f;
            powerup.Pitch = 0.5f;
            powerup.Play();
        }

        public void playDeathSound()
        {
            death.Volume = 0.75f;
            death.Play();
        }

        public void playFootstepSound()
        {
            footstep = footstepSound.CreateInstance();
            footstep.Volume = 0.5f;
            footstep.Play();
        }

        public void playInvulnerabilitySound()
        {
            invulnerability.Volume = 0.75f;
            invulnerability.Play();
        }
        public void stopInvulnerabilitySound()
        {
            invulnerability.Stop();
        }

        public void playThunderSound()
        {
            thunder.Volume = 0.5f;
            thunder.Play();
        }
    }
}
