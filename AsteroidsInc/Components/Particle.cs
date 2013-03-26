using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace AsteroidsInc.Components
{
    public class Particle : GameObject //derives from GameObject, since a particle is essentially a sprite
    {
        public int TTL { get; set; } //time to live, set by particle emitter
        public event EventHandler ParticleExpired; //particle expired event

        public Particle(
            Texture2D texture,
            Vector2 worldLocation,
            Vector2 velocity,
            Color color,
            int framesToLive,
            float rotation = 0f,
            float scale = 1f,
            float depth = 1f,
            SpriteEffects effects = SpriteEffects.None,
            int collisionRadius = 0,
            int xPadding = 0,
            int yPadding = 0)
            : base(texture, worldLocation, velocity, color, false, rotation, scale, depth, effects,
            0, 1, 1, collisionRadius, xPadding, yPadding)
        {
            TTL = framesToLive; //most of the actual work is handled by GameObject
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(TTL >= 1) //If not expired, draw...
                base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            TTL--;

            base.Update(gameTime); //Update GameObject base

            if (TTL <= 0 && ParticleExpired != null) //If expired, notify
                this.ParticleExpired(this, new EventArgs());
        }
    }
}
