using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSC
{
public class SpriteAnimation
    {
        private Texture2D spriteSheet;
        private int frameWidth;
        private int frameHeight;
        private int frameStart;
        private int frameEnd;
        private float frameTime;
        private int currentFrameIndex;
        private float timer;

        public SpriteAnimation(Texture2D spriteSheet, int frameWidth, int frameHeight, int frameStart, int frameEnd, float frameTime)
        {
            this.spriteSheet = spriteSheet;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.frameStart = frameStart;
            this.frameEnd = frameEnd;
            this.frameTime = frameTime;
            this.currentFrameIndex = frameStart;
            this.timer = 0f;
        }

        public void Update(float deltaTime)
        {
            timer += deltaTime;

            if (timer > frameTime)
            {
                currentFrameIndex = (currentFrameIndex + 1) % (frameEnd - frameStart + 1) + frameStart;
                timer = 0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, bool isPlayer, float Rotation, Vector2 Origin, bool isEnemy)
        {
            int frameX = currentFrameIndex % (spriteSheet.Width / frameWidth);
            int frameY = currentFrameIndex / (spriteSheet.Width / frameWidth);
            Rectangle sourceRectangle = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            if (isPlayer)
            {
                position.X -= 48;
                position.Y -= 48;
                if (Player.IsFacingLeft)
                {
                    spriteBatch.Draw(spriteSheet, position, sourceRectangle, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.FlipHorizontally, 0f);
                } else
                {
                    spriteBatch.Draw(spriteSheet, position, sourceRectangle, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);
                }
                
            } else if (isEnemy)
            {
                spriteBatch.Draw(spriteSheet, position, sourceRectangle, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);
            } 
            else
            {
                position.X -= 16;
                position.Y -= 16;
                spriteBatch.Draw(spriteSheet, position, sourceRectangle, Color.White, Rotation, Origin, 1.5f, SpriteEffects.None, 0f);
            }
        }
    }
}
