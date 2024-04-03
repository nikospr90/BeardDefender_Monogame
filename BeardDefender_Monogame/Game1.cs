﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BeardDefender_Monogame.GameObjects;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BeardDefender_Monogame
{
    enum Scenes
    {
        MAIN_MENU,
        GAME
    };
    public class Game1 : Game
    {
        private Scenes activeScenes;
        //private Texture2D texture;

        // Important shit
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        const int MapWidth = 1320;
        const int MapHeight = 720;

        bool startGameSelected = true; // Starta spelet är förvalt
        bool exitGameSelected = false;


        // MainMenu object
        MainMenu mainmenu;
        SpriteFont buttonFont;

        //Background object
        Background background;

        // Unit objects
        Shark shark;
        Hedgehog hedgehog;

        //Player object
        Player player;

        // Obstacles/Ground
        Ground groundLower;
        Ground groundLower2;
        List<Ground> lowerGroundList;

        Ground groundUpper;
        Ground groundUpper2;
        List<Ground> upperGroundList;

        public Game1()
        {
            activeScenes = Scenes.MAIN_MENU;

            _graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Grafikinställningar.
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferHeight = MapHeight;
            _graphics.PreferredBackBufferWidth = MapWidth;
            _graphics.ApplyChanges();

            // Unit objects
            mainmenu = new MainMenu();
            background = new Background();
            shark = new(new Vector2(100, 100));
            hedgehog = new Hedgehog(new Vector2(100, 100), Content.Load<Texture2D>("Hedgehog_Right"), 0.03f);


            player = new Player(new Rectangle(100, 400, 25, 36));

            // Obstacle/Ground. Kunde inte använda texturens Height/Width värden här,
            // 80 representerar Height, width är 640. Får klura ut hur man skulle kunna göra annars.
            groundLower = new(new Rectangle(
                0,
                _graphics.PreferredBackBufferHeight - 80,
                _graphics.PreferredBackBufferWidth / 2,
                80));
            groundLower2 = new(new Rectangle(
                groundLower.Position.Right,
                _graphics.PreferredBackBufferHeight - 80,
                640 + 20,
                80));

            groundUpper = new(new Rectangle(
                _graphics.PreferredBackBufferWidth / 4,
                _graphics.PreferredBackBufferHeight - 80 * 2 + 50,
                _graphics.PreferredBackBufferWidth - 640,
                80));
            groundUpper2 = new(new Rectangle(
                _graphics.PreferredBackBufferWidth / 3,
                _graphics.PreferredBackBufferHeight - 80 * 3 + 50,
                _graphics.PreferredBackBufferWidth - 640,
                80
                ));

            upperGroundList = new()
            {
                groundUpper,
                groundUpper2
            };
            lowerGroundList = new()
            {
                groundLower,
                groundLower2
            };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //texture = Content.Load<Texture2D>("BeardDefender_MainMenu");

           
       
         _spriteBatch = new SpriteBatch(GraphicsDevice);
          buttonFont = Content.Load<SpriteFont>("Font"); 
       

        _spriteBatch = new SpriteBatch(GraphicsDevice);

            //Laddar texturer för MainMenu.
            mainmenu.LoadContent(Content);

            

            //Laddar texturer för Background.
            background.LoadContent(Content);

            // Laddar texturer och animationer för Player.
            player.LoadContent(Content);

            // Texturer för shark
            shark.LoadContent(Content);

            // Ground
            foreach (var item in upperGroundList)
            {
                item.LoadContent(Content);
            }
            foreach (var item in lowerGroundList)
            {
                item.LoadContent(Content);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            switch (activeScenes)
            {



                case Scenes.MAIN_MENU:

                    //if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    //{
                    //    activeScenes = Scenes.GAME;
                    //}
                    //if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    //{
                    //    Exit();
                    //}

                    KeyboardState state = Keyboard.GetState();
                    if (state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.Down))
                    {
                        startGameSelected = !startGameSelected;
                        exitGameSelected = !exitGameSelected;
                    }

                    if (startGameSelected == true && state.IsKeyDown(Keys.Enter))
                    {
                        activeScenes = Scenes.GAME;

                    }

                    if (exitGameSelected == true && state.IsKeyDown(Keys.Enter))
                    {
                        Exit();

                    }


                    break;

                case Scenes.GAME:
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    {
                        activeScenes = Scenes.MAIN_MENU;
                    }
                    //if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    //{
                    //    Exit();
                    //}
                    break;

            }

            KeyboardState keyboardState = Keyboard.GetState();
            //if (keyboardState.IsKeyDown(Keys.Escape))
            //{
            //    Exit();
            //}

            // Player pos Y för att stå på marken.
            player.position.Y = groundLower.Position.Y - (player.Texture.Height / 4);

            // Shark movement, returnerar rätt frame index som används i Update.
            shark.CurrentFrameIndex = shark.Update(_graphics, gameTime);
            // Hedgehog movement.
            hedgehog.Update(gameTime, new Vector2(player.position.X, player.position.Y));

            // Player movement, sätter players variabel IsFacingRight till returvärdet av
            // metoden, som håller koll på vilket håll spelaren är riktad åt.
            player.IsFacingRight =
                player.MovePlayer(
                    keyboardState,
                    upperGroundList,
                    lowerGroundList);

            player.CurrentAnimation.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Beige);

            _spriteBatch.Begin();

           


            switch (activeScenes)
            {
                case Scenes.MAIN_MENU:

                    //_spriteBatch.Draw(texture, new Rectangle(0, 0, MapWidth, MapHeight), Color.White);
                    mainmenu.DrawMainMenu(_spriteBatch, MapWidth, MapHeight);

                    // Ritar "Starta spelet"-val
                    _spriteBatch.DrawString(buttonFont, "Starta spelet", new Vector2(100, 100), startGameSelected ? Color.Yellow : Color.White);

                    // Ritar "Avsluta spelet"-val
                    _spriteBatch.DrawString(buttonFont, "Avsluta spelet", new Vector2(100, 140), exitGameSelected ? Color.Yellow : Color.White);


                    break;

                case Scenes.GAME:

                    background.DrawBackground(_spriteBatch, MapWidth, MapHeight);

                    player.DrawPlayer(_spriteBatch);

                    // SHAAAARKs draw metod sköter animationer beroende på åt vilket håll hajen rör sig.
                    shark.Draw(_spriteBatch);
                    hedgehog.Draw(_spriteBatch);

                    //Ground
                    foreach (var item in upperGroundList)
                    {
                        item.Draw(_spriteBatch);
                    }
                    foreach (var item in lowerGroundList)
                    {
                        item.Draw(_spriteBatch);
                    }
                    break;
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}