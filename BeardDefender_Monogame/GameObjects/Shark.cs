using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Drawing;
using Color = Microsoft.Xna.Framework.Color;
using BeardDefender_Monogame.GameLevels;
using System.Collections.Generic;
using BeardDefender_Monogame.GameObjects.Powerups;
using Microsoft.Xna.Framework.Input;
using System;

namespace BeardDefender_Monogame.GameObjects
{
    internal class Shark : Enemy
    {
        private float speed = 150f;
        private Vector2 position;
        private Vector2 velocity;
        private Texture2D texture; // Den textur som visas
        private Texture2D[] textureLeft; // Innehåller texturer för rörelse till vänster.
        private Texture2D[] textureRight; // Innehåller texturer för rörelse till höger.
        private bool sharkIsLeft = false;
        int currentFrameIndex; // Variabel som väljer vilken frame man tar ur arrayen.
        float frameDuration = 0.2f; // Hastighet på animationen.
        float frameTimer = 0f;
        private bool drawShark;
        private bool sharkIsGoingUp = true;

        private bool isOnGround;

        private const float MoveAcceleration = 1000.0f; // Minskad för långsammare acceleration
        private const float MaxMoveSpeed = 200.0f; // Minskad för lägre maxhastighet
        private const float GroundDragFactor = 0.58f;
        private const float AirDragFactor = 0.65f;
        private float maxJumpTime = 0.25f; // Justera för att påverka hur länge spelaren kan påverka hoppet uppåt
        private const float JumpLaunchVelocity = -1000.0f; // Högre värde för högre hopp
        private const float GravityAcceleration = 1500.0f; // Öka för snabbare fall, minska för långsammare
        private const float MaxFallSpeed = 550.0f; // Justera max fallhastighet
        private const float JumpControlPower = 0.14f; // Justera för att påverka spelarens kontroll under hoppet

        float jumpTime;
        bool isJumping;

        public Shark(Vector2 position)
        {
            this.position = position;

            // Array MÅSTE intieras innan man försöker lägga in textures i den.
            textureLeft = new Texture2D[2];
            textureRight = new Texture2D[2];
            drawShark = true;
        }

        // Laddar in bilder till Shark objektet.
        public void LoadContent (ContentManager Content)
        {
            this.textureLeft[0] = Content.Load<Texture2D>("wackShark1_left");
            this.TextureLeft[1] = Content.Load<Texture2D>("wackShark2_left");
            this.TextureRight[0] = Content.Load<Texture2D>("wackShark1_right");
            this.TextureRight[1] = Content.Load<Texture2D>("wackShark2_right");
            this.Texture = this.TextureLeft[0];
        }

        public void Update(KeyboardState keyBoardstate, GameTime gameTime, List<Ground> groundList)
        {
            GetInput(keyBoardstate);
            ApplyPhysics(gameTime);
            HandleCollisions(groundList);
        }

        //Metod som kontrollerar om det finns kollision med ground/marken
        private void HandleCollisions(List<Ground> groundList)
        {
            isOnGround = false;
            foreach (var ground in groundList)
            {
                if (position.Y + texture.Height > ground.Position.Y
                    &&
                    position.X < ground.Position.X + ground.Position.Width &&
                    position.X + texture.Width > ground.Position.X &&
                    position.Y < ground.Position.Y + ground.Position.Height)
                {
                    float groundBottom = ground.Position.Y + ground.Texture.Height;
                    float sharkBottom = position.Y + texture.Height;
                    if (sharkBottom <= groundBottom + velocity.Y)
                    {
                        isOnGround = true;
                        velocity.Y = 0;
                        velocity.X = 0;
                        position.Y = groundBottom - (texture.Height / 4);
                    }
                }
            }
        }

        //Metod som lägger fysik till spellarens rörelse
        private void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (isJumping)
            {
                // Om spelaren fortfarande håller i hoppknappen men inte har hoppat för länge, fortsätt att ge hoppkraft uppåt
                if (jumpTime > 0.0f)
                {
                    velocity.Y += JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                jumpTime += elapsed;
            }

            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
            velocity.X *= (isOnGround ? GroundDragFactor : AirDragFactor);

            //Update positionen X och Y...
            position.Y += velocity.Y * elapsed;
            position.X += velocity.X * elapsed;
            //...och lagra den till nya positionen
            position = new Vector2((float)Math.Round(position.X), (float)Math.Round(position.Y));

            // Återställ hoppet när spelaren landar på marken
            if (position.Y > 720)
            {
                position.Y = 720;
                isOnGround = true;
                isJumping = false;
                velocity.Y = 0;
            }
        }

        //Metod som registrerar tanget input för spelarens rörelse
        private void GetInput(KeyboardState keyboardState)
        {
            isJumping = sharkIsGoingUp;
            
            velocity.X = 0;

            //If-satsen som påverkar jump
            if (sharkIsGoingUp
                && isOnGround)
            {
                isOnGround = false;
                isJumping = true;
                sharkIsGoingUp = true;
                jumpTime = 0.0f;
            }
            else if (sharkIsGoingUp)
            {
                isJumping = false;
            }

        }

        /// <summary>
        ///  Method that updates the shark texture and behavior
        /// </summary>
        /// <param name="spriteBatch"></param>
        // Denna metod räknar ut vilken frame man är på, och sköter hajens movement mellan vissa punkter.
        // Returnerar en int som representerar frame index.
        //public override int Update(
        //    GraphicsDeviceManager _graphics,
        //    GameTime gameTime,
        //    Player player,
        //    Game1 game,
        //    string filePath,
        //    List<PowerUp> powerUpList,
        //    List<Shark> sharkList,
        //    Hedgehog hedgehog,
        //    HealthCounter healthCounter)
        //{
        //    // Uppdatera frameTimer
        //    frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        //    if (frameTimer >= frameDuration)
        //    {
        //        // Nästa frame
        //        currentFrameIndex = (currentFrameIndex + 1) % textureLeft.Length;

        //        // Reset timer.
        //        frameTimer = 0f;
        //    }


        //    ///       Test for sharks upwardsmotion
        //    ////position.Y -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        //    //if (position.Y >= texture.Height && sharkIsGoingUp)
        //    //{
        //    //    position.Y -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        //    //    if (position.Y == texture.Height)
        //    //    {
        //    //        sharkIsGoingUp = false;
        //    //    }
        //    //}

        //    //if (position.Y <= texture.Height && !sharkIsGoingUp)
        //    //{
        //    //    position.Y += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

        //    //    if (position.Y + texture.Height == _graphics.PreferredBackBufferHeight)
        //    //    {
        //    //        sharkIsGoingUp = true;
        //    //    }
        //    //}

        //    //                 Originalkod                      ///
        //    // Kollar om shark inte är till vänster, och på rätt position av skärmen. När shark har kommit
        //    // hela vägen till vänster ändras sharkIsLeft till true och då blir nästa if-condition uppfyllt.
        //    if (!sharkIsLeft && position.X <= _graphics.PreferredBackBufferWidth - texture.Width / 2)
        //    {
        //        // Flyttar shark
        //        position.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        //        // Kontrollerar om shark har kommit till vänstra brytpunkten.
        //        if (position.X <= texture.Width / 2 + 5)
        //        {
        //            // Sätter shark position så den inte åker utanför skärmen, samt sätter sharkIsLeft till true
        //            // vilket gör så att den yttre if-satsen blir falsk, och nästa if-sats blir sann.
        //            position.X = texture.Width / 2;
        //            sharkIsLeft = true;
        //        }
        //    }
        //    if (sharkIsLeft && position.X >= texture.Width / 2)
        //    {
        //        position.X += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

        //        if (position.X >= _graphics.PreferredBackBufferWidth - texture.Width / 2 - 5)
        //        {
        //            position.X = _graphics.PreferredBackBufferWidth - texture.Width / 2;
        //            sharkIsLeft = false;
        //        }
        //    }
        //    RectangleF sharkPos = new RectangleF(this.Position.X, this.Position.Y, this.Texture.Height, this.Texture.Width);

        //    if (player.Position.IntersectsWith(sharkPos))
        //    {
        //        player.HP--;
        //        drawShark = false;
        //        sharkPos = new RectangleF(100, 100, 100, 100);
        //        this.position.X = 0;
        //        this.position.Y = 0;
        //    }
        //    if (player.HP < 1)
        //    {
        //        GameLevel.ResetGame(game, gameTime, filePath, player, healthCounter, powerUpList, sharkList, hedgehog);
        //    }
        //    return currentFrameIndex;
        //}
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!this.SharkIsLeft)
            {
                spriteBatch.Draw(
                    this.TextureLeft[CurrentFrameIndex],
                    this.Position,
                    null,
                    Color.White,
                    0f,
                    new Vector2(
                        this.Texture.Width / 2,
                        this.Texture.Height / 2),
                    Vector2.One,
                    SpriteEffects.None,
                    0f);
            }
            else
            {
                spriteBatch.Draw(
                    this.TextureRight[CurrentFrameIndex],
                    this.Position,
                    null,
                    Color.White,
                    0f,
                    new Vector2(
                        this.Texture.Width / 2,
                        this.Texture.Height / 2),
                    Vector2.One,
                    SpriteEffects.None,
                    0f);
            }
        }
        
        // Get/Set
        public bool DrawShark
        {
            get { return drawShark; }
            set { drawShark = value; }
        }
        public int CurrentFrameIndex
        {
            get { return currentFrameIndex; }
            set { currentFrameIndex = value; }
        }
        public bool SharkIsLeft
        {
            get { return sharkIsLeft; }
            set {  sharkIsLeft = value; }
        }
        public Texture2D[] TextureLeft
        {
            get { return textureLeft; }
            set { textureLeft = value; }
        }
        public Texture2D[] TextureRight
        {
            get { return textureRight; }
            set { textureRight = value; }
        }
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }
        public Vector2 Position
        {
            get { return position; }
            set { position.X = value.X;
                  position.Y = value.Y; }
        }
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        public float MaxJumpTime
        {
            get { return maxJumpTime; }
            set { maxJumpTime = value; }
        }
    }

}
