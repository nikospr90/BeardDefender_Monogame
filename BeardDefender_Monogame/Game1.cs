﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BeardDefender_Monogame.GameObjects;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing;
using Color = Microsoft.Xna.Framework.Color;
using BeardDefender_Monogame.GameObjects.Powerups;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using System;
using System.IO;

namespace BeardDefender_Monogame
{
    enum Scenes
    {
        MAIN_MENU,
        DEATH,
        WIN,
        LEVEL_ONE,
        LEVEL_TWO,
        HIGHSCORE
    };
    enum MenuOption
    {
        PLAY,
        SCORE,
        EXIT
    };
    public class Game1 : Game
    {
        private Scenes activeScenes;
        private MenuOption currentMenuOption = MenuOption.PLAY;
        private Scenes lastPlayedLevel = Scenes.LEVEL_ONE;

        private double levelTimer = 0;
        private const double LevelTimeLimit = 20;

        //private Texture2D texture;

        // Important shit
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        const int MapWidth = 1320;
        const int MapHeight = 720;
        private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private static string filePath = Path.Combine(DesktopPath, "Game HighScore.txt");

        bool startGameSelected = true; // Starta spelet är förvalt
        bool exitGameSelected = false;
        bool highscoreSelected = false;

        private bool previousUpPressed = false;
        private bool previousDownPressed = false;
        private bool previousEnterPressed = false;

        // Powerups
        Heart heart;
        JumpBoost jumpBoost;

        // MainMenu object
        MainMenu mainmenu;
        SpriteFont buttonFont;

        //Highscore object
        Highscore highscore;

        //Winnerscene object
        WinnerScene winnerScene;

        //Deathscene object
        DeathScene deathScene;

        //Background object
        Background background;
        Background background2;

        // Unit objects
        Shark shark;
        Hedgehog hedgehog;
        Crabman crabman;

        //Player object
        Player player;

        // Obstacles/Ground
        Ground groundLower;
        Ground groundLower2;
        Ground groundLower3;
        Ground groundLower4;
        Ground groundLower5;

        List<Ground> groundList;

        //Score grejer
        double score = 0;
        Texture2D ScoreBox;
        Vector2 ScoreBoxPosition;
        SpriteFont ScoreFont;

        //Username input
        TextBox TextBox;
        public string username = "";


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
            deathScene = new DeathScene();
            highscore = new Highscore();
            winnerScene = new WinnerScene();
            background = new Background(0);
            background2 = new Background(1320);
            shark = new(new Vector2(100, 100));
            crabman = new Crabman();

            // Powerups
            heart = new Heart(new Rectangle(900, 600, 60, 60));
            jumpBoost = new JumpBoost(new Rectangle(700, 600, 60, 60));

            ScoreBoxPosition = new Vector2(0, 15);

            player = new Player(new RectangleF(600, 400, 25, 36));

            // Obstacle/Ground. Kunde inte använda texturens Height/Width värden här,
            // 80 representerar Height, width är 640. Får klura ut hur man skulle kunna göra annars.
            groundLower = new(new RectangleF(

                0,
                _graphics.PreferredBackBufferHeight - 80,
                _graphics.PreferredBackBufferWidth / 2,
                80));

            groundLower2 = new(new RectangleF(

                groundLower.Position.Right - 20,
                _graphics.PreferredBackBufferHeight - 80,
                640 + 20,
                80));
            groundLower3 = new(new RectangleF(
                groundLower2.Position.Right - 20,
                _graphics.PreferredBackBufferHeight - 80,
                groundLower2.Position.Width,
                groundLower2.Position.Height
                ));
            groundLower4 = new(new RectangleF(
                groundLower3.Position.Right - 20,
                _graphics.PreferredBackBufferHeight - 80,
                groundLower3.Position.Width,
                groundLower3.Position.Height
                ));
            groundLower5 = new(new RectangleF(
                groundLower4.Position.Right - 20,
                _graphics.PreferredBackBufferHeight - 80,
                groundLower4.Position.Width,
                groundLower4.Position.Height
                ));

            groundList = new()
            {
                groundLower,
                groundLower2,
                groundLower3,
                groundLower4,
                groundLower5,
            };
            hedgehog = new Hedgehog(new Vector2(400, groundLower.Position.Y - 50), Content.Load<Texture2D>("Hedgehog_Right"), 0.03f);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            buttonFont = Content.Load<SpriteFont>("Font");

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //Laddar texturer för MainMenu.
            mainmenu.LoadContent(Content);

            //Laddar texturer för Background.
            background.LoadContent(Content);
            background2.LoadContent(Content);

            //Laddar texturer för deathscene
            deathScene.LoadContent(Content);

            //Laddar texturer för scorebox
            ScoreBox = Content.Load<Texture2D>("ScoreBox");
            ScoreFont = Content.Load<SpriteFont>("ScoreFont");

            //Laddar texture för username textbox
            TextBox = new TextBox(ScoreFont);

            //laddar texturer för Highscore
            highscore.LoadContent(Content);

            //Laddar texturer för Winnerscene
            winnerScene.LoadContent(Content);

            // Laddar texturer och animationer för Player.
            player.LoadContent(Content);

            //Texturer för crabman
            crabman.LoadContent(Content);

            // Texturer för shark
            shark.LoadContent(Content);
            // Ground
            foreach (Ground ground in groundList)
            {
                ground.LoadContent(Content);
            }
            // Powerupzzz
            heart.LoadContent(Content);
            jumpBoost.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            switch (activeScenes)
            {
                case Scenes.MAIN_MENU:

                    // Loopa genom menyvalen
                    if (keyboardState.IsKeyDown(Keys.Down) && !previousDownPressed)
                    {
                        currentMenuOption = (MenuOption)(((int)currentMenuOption + 1) % 3);
                    }
                    else if (keyboardState.IsKeyDown(Keys.Up) && !previousUpPressed)
                    {
                        currentMenuOption = (MenuOption)(((int)currentMenuOption + 2) % 3);
                    }

                    if (keyboardState.IsKeyDown(Keys.Enter) && !previousEnterPressed)
                    {
                        switch (currentMenuOption)
                        {
                            case MenuOption.PLAY:
                                
                                activeScenes = lastPlayedLevel;
                                break;
                            case MenuOption.SCORE:
                                //activeScenes = Scenes.HIGHSCORE;
                                activeScenes = Scenes.DEATH;

                                break;
                            case MenuOption.EXIT:
                                Exit();
                                break;
                        }
                    }

                    previousUpPressed = keyboardState.IsKeyDown(Keys.Up);
                    previousDownPressed = keyboardState.IsKeyDown(Keys.Down);
                    previousEnterPressed = keyboardState.IsKeyDown(Keys.Enter);
                    break;

                case Scenes.LEVEL_ONE:

                    // Kontrollera om spelaren försöker gå tillbaka till huvudmenyn
                    if (keyboardState.IsKeyDown(Keys.Escape))
                    {
                        lastPlayedLevel = activeScenes;
                        activeScenes = Scenes.MAIN_MENU;
                    }
                    else
                    {
                        levelTimer += gameTime.ElapsedGameTime.TotalSeconds;

                        if (player.position.X == crabman.PositionX)
                        {
                            activeScenes = Scenes.DEATH;
                            TextBox.Update();
                            File.AppendAllText(filePath, $"\nScore: {((int)Math.Ceiling(score)).ToString()}");
                        }

                        if (levelTimer >= LevelTimeLimit)
                        {
                            activeScenes = Scenes.LEVEL_TWO;
                            levelTimer = 0;
                        }

                        // Kontrollera spelarens rörelse för att uppdatera bakgrunden
                        bool isPlayerMoving = keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.Right);
                        int playerSpeed = 5;

                        background.Update(gameTime, GraphicsDevice.Viewport.Width, isPlayerMoving, playerSpeed);
                        background2.Update(gameTime, GraphicsDevice.Viewport.Width, isPlayerMoving, playerSpeed);

                        // Uppdatera spelarposition, fiender, osv.
                        player.position.Y = groundLower.Position.Y - (player.Texture.Height / 4);
                        foreach (Ground ground in groundList)
                        {
                            ground.Update(gameTime, GraphicsDevice.Viewport.Width);
                        }
                        shark.CurrentFrameIndex = shark.Update(_graphics, gameTime);
                        hedgehog.Update(gameTime, new Vector2(player.position.X, player.position.Y));
                        player.IsFacingRight = player.MovePlayer(keyboardState, hedgehog, groundList);

                        //returnerar rätt frame index som används i Update.
                        crabman.CurrentFrameIndex = crabman.Update(_graphics, gameTime);

                        // Shark movement, returnerar rätt frame index som används i Update.
                        shark.CurrentFrameIndex = shark.Update(_graphics, gameTime);

                        //Updaterar score i sammaband med spelets timer
                        score += (double)gameTime.ElapsedGameTime.TotalSeconds;

                        // Player movement, sätter players variabel IsFacingRight till returvärdet av
                        // metoden, som håller koll på vilket håll spelaren är riktad åt.
                        player.IsFacingRight =
                            player.MovePlayer(
                                keyboardState,
                                hedgehog,
                                groundList);

                        player.CurrentAnimation.Update(gameTime);
                        crabman.CurrentFrameIndex = crabman.Update(_graphics, gameTime);

                        // Powerups
                        heart.Update(gameTime, player);
                        jumpBoost.Update(gameTime, player);
                    }
                    break;

                case Scenes.LEVEL_TWO:

                    // Kontrollera om spelaren försöker gå tillbaka till huvudmenyn
                    if (keyboardState.IsKeyDown(Keys.Escape))
                    {
                        File.AppendAllText(filePath, $"\nScore: {((int)Math.Ceiling(score)).ToString()}");
                        lastPlayedLevel = activeScenes;
                        activeScenes = Scenes.MAIN_MENU;
                    }
                    else
                    {
                        levelTimer += gameTime.ElapsedGameTime.TotalSeconds;

                        if (player.position.X == crabman.PositionX)
                        {
                            activeScenes = Scenes.DEATH;
                            TextBox.Update();
                            File.AppendAllText(filePath, $"\nScore: {((int)Math.Ceiling(score)).ToString()}");
                        }

                        if (levelTimer >= LevelTimeLimit)
                        {
                            activeScenes = Scenes.WIN;
                            TextBox.Update();
                            File.AppendAllText(filePath, $"\n{username}: {((int)Math.Ceiling(score)).ToString()}");
                            levelTimer = 0;
                        }

                        // Kontrollera spelarens rörelse för att uppdatera bakgrunden
                        bool isPlayerMoving = keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.Right);
                        int playerSpeed = 5;

                        background.Update(gameTime, GraphicsDevice.Viewport.Width, isPlayerMoving, playerSpeed);
                        background2.Update(gameTime, GraphicsDevice.Viewport.Width, isPlayerMoving, playerSpeed);

                        // Uppdatera spelarposition, fiender, osv.
                        player.position.Y = groundLower.Position.Y - (player.Texture.Height / 4);
                        foreach (Ground ground in groundList)
                        {
                            ground.Update(gameTime, GraphicsDevice.Viewport.Width);
                        }
                        shark.CurrentFrameIndex = shark.Update(_graphics, gameTime);

                        //hedgehog.Update(gameTime, new Vector2(player.position.X, player.position.Y));

                        player.IsFacingRight = player.MovePlayer(keyboardState, hedgehog, groundList);


                        //returnerar rätt frame index som används i Update.
                        crabman.CurrentFrameIndex = crabman.Update(_graphics, gameTime);

                        // Shark movement, returnerar rätt frame index som används i Update.
                        shark.CurrentFrameIndex = shark.Update(_graphics, gameTime);

                        // Hedgehog movement.
                        //hedgehog.Update(gameTime, new Vector2(player.position.X, player.position.Y));

                        //Updaterar score i sammaband med spelets timer
                        score += (double)gameTime.ElapsedGameTime.TotalSeconds * 2;

                        // Player movement, sätter players variabel IsFacingRight till returvärdet av
                        // metoden, som håller koll på vilket håll spelaren är riktad åt.
                        player.IsFacingRight =
                            player.MovePlayer(
                                keyboardState,
                                hedgehog,
                                groundList);

                        player.CurrentAnimation.Update(gameTime);
                        crabman.CurrentFrameIndex = crabman.Update(_graphics, gameTime);

                        // Powerups
                        heart.Update(gameTime, player);
                        jumpBoost.Update(gameTime, player);
                    }
                    break;

                case Scenes.HIGHSCORE:
                    if (keyboardState.IsKeyDown(Keys.Escape))
                    {
                        activeScenes = Scenes.MAIN_MENU;
                    }
                    break;

                case Scenes.DEATH:
                    if (keyboardState.IsKeyDown(Keys.Escape))
                    {
                        activeScenes = Scenes.MAIN_MENU;
                    }
                    break;

                case Scenes.WIN:
                    if (keyboardState.IsKeyDown(Keys.Escape))
                    {
                        activeScenes = Scenes.MAIN_MENU;
                    }
                    break;
            }

            //Flytta in i deathscene när vi har fixat kollision med fiende.
            deathScene.CurrentFrameIndex = deathScene.Update(_graphics, gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Beige);

            _spriteBatch.Begin();

            switch (activeScenes)

            {
                case Scenes.MAIN_MENU:

                    ////_spriteBatch.Draw(texture, new Rectangle(0, 0, MapWidth, MapHeight), Color.White);

                    mainmenu.DrawMainMenu(_spriteBatch, MapWidth, MapHeight);

                    //// Ritar "Starta spelet"-val
                    _spriteBatch.DrawString(buttonFont, "PLAY", new Vector2(616, 370), currentMenuOption == MenuOption.PLAY ? Color.Red : Color.Black);

                    ////Ritar "Highscore"-val
                    _spriteBatch.DrawString(buttonFont, "SCORE", new Vector2(590, 470), currentMenuOption == MenuOption.SCORE ? Color.Red : Color.Black);

                    //// Ritar "Avsluta spelet"-val
                    _spriteBatch.DrawString(buttonFont, "EXIT", new Vector2(618, 575), currentMenuOption == MenuOption.EXIT ? Color.Red : Color.Black);
                    break;

                case Scenes.LEVEL_ONE:

                    background.DrawBackground(_spriteBatch, MapWidth, MapHeight);
                    background2.DrawBackground(_spriteBatch, MapWidth, MapHeight);

                    //ScoreBox Texturer rittas här
                    _spriteBatch.Draw(ScoreBox, ScoreBoxPosition, Color.White);
                    _spriteBatch.DrawString(ScoreFont, "Score : ", new Vector2(28, 30), Color.Black);
                    _spriteBatch.DrawString(ScoreFont, ((int)score).ToString(), new Vector2(138, 30), Color.Black);

                    player.DrawPlayer(_spriteBatch);

                    // SHAAAARKs draw metod sköter animationer beroende på åt vilket håll hajen rör sig.
                    crabman.Draw(_spriteBatch);
                    shark.Draw(_spriteBatch);
                    hedgehog.Draw(_spriteBatch);

                    //Ground
                    foreach (var item in groundList)
                    {
                        item.Draw(_spriteBatch);
                    }

                    if (!heart.Taken)
                    {
                        heart.Draw(_spriteBatch);
                    }

                    if (!jumpBoost.Taken)
                    {
                        jumpBoost.Draw(_spriteBatch);
                    }
                    break;

                case Scenes.LEVEL_TWO:

                    background.DrawBackground(_spriteBatch, MapWidth, MapHeight);
                    background2.DrawBackground(_spriteBatch, MapWidth, MapHeight);

                    //ScoreBox Texturer rittas här
                    _spriteBatch.Draw(ScoreBox, ScoreBoxPosition, Color.White);
                    _spriteBatch.DrawString(ScoreFont, "Score : ", new Vector2(28, 30), Color.Black);
                    _spriteBatch.DrawString(ScoreFont, ((int)score).ToString(), new Vector2(138, 30), Color.Black);

                    player.DrawPlayer(_spriteBatch);

                    // SHAAAARKs draw metod sköter animationer beroende på åt vilket håll hajen rör sig.
                    crabman.Draw(_spriteBatch);
                    shark.Draw(_spriteBatch);
                    //hedgehog.Draw(_spriteBatch);

                    //Ground
                    foreach (var item in groundList)
                    {
                        item.Draw(_spriteBatch);
                    }

                    if (!heart.Taken)
                    {
                        heart.Draw(_spriteBatch);
                    }

                    if (!jumpBoost.Taken)
                    {
                        jumpBoost.Draw(_spriteBatch);
                    }
                    break;

                case Scenes.DEATH:

                    //Testar bara men ska vara highscore scene här sen.
                    //highscore.DrawBackground(_spriteBatch, MapWidth, MapHeight);
                    TextBox.Draw(_spriteBatch, new Vector2(100, 100), Color.Black);
                    deathScene.DrawBackground(_spriteBatch, MapWidth, MapHeight, score);
                    break;

                case Scenes.WIN:

                    //Testar bara men ska vara highscore scene här sen.
                    //highscore.DrawBackground(_spriteBatch, MapWidth, MapHeight);
                    TextBox.Draw(_spriteBatch, new Vector2(100, 100), Color.Black);
                    winnerScene.DrawBackground(_spriteBatch, MapWidth, MapHeight, score);
                    break;
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}