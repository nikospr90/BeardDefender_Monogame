﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BeardDefender_Monogame.GameObjects;
using System.Collections.Generic;
using System.Drawing;
using Color = Microsoft.Xna.Framework.Color;
using BeardDefender_Monogame.GameObjects.Powerups;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using System;
using System.IO;
using BeardDefender_Monogame.GameLevels;


namespace BeardDefender_Monogame
{
    public enum Scenes
    {
        MAIN_MENU,
        DEATH,
        WIN,
        LEVEL_ONE,
        LEVEL_TWO,
        HIGHSCORE
    };
    public class Game1 : Game
    {
        private Scenes activeScenes;
        private Scenes lastPlayedLevel = Scenes.LEVEL_ONE;

        private double levelTimer = 0;
        private const double LevelTimeLimit = 20;

        // Important shit
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public const int MapWidth = 1320;
        public const int MapHeight = 720;
        private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private static string filePath = Path.Combine(DesktopPath, "Game HighScore.txt");
        private int startX = 5;

        // Till scenebyte för WIN och DEATH scene
        private double menuSceneChangeTimeDelay = 0;

        //Musik
        Song backgroundMusic;

        // Powerups
        Heart Heart;
        JumpBoost JumpBoost;
        GemScore GemScore;
        List<PowerUp> powerUpList;

        // MainMenu object
        MainMenu mainmenu;

        //Highscore object
        Highscore highscore;

        //Winnerscene object
        WinnerScene winnerScene;

        //Deathscene object
        DeathScene deathScene;
        SpriteFont deathtext;

        //Background object
        Background background;
        Background background2;
        List<Background> backgroundList;

        // Unit objects
        Shark shark;
        Hedgehog hedgehog;
        Crabman crabman;
        //Player object
        Player Player;

        // Obstacles/Ground
        Ground groundLower;
        Ground groundLower2;
        Ground groundLower3;
        Ground groundLower4;
        Ground groundLower5;
        List<Ground> groundList;
        //Score grejer
        public static double score = 0;
        public static Texture2D ScoreBox;
        public static Vector2 ScoreBoxPosition;
        public static SpriteFont ScoreFont;
        // Ruta för HP
        HealthCounter HealthCounter;
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
            shark = new(new Vector2(100, 100));
            crabman = new Crabman();
            background = new Background(startX, GraphicsDevice.Viewport.Width);
            background2 = new Background(startX, GraphicsDevice.Viewport.Width);
            backgroundList = new()
            {
                background,
                background2
            };

            // Powerups
            heart = new Heart(new Rectangle(900, 600, 60, 60));
            jumpBoost = new JumpBoost(new Rectangle(700, 600, 60, 60));
            gemScore = new GemScore(new Rectangle(1200, 540, 60, 60));
            powerUpList = new()
            {
                heart,
                jumpBoost,
                gemScore,
            };
            healthCounter = new(new Vector2(1070, 15));
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
                groundLower2.Position.Height));

            groundLower4 = new(new RectangleF(
                groundLower3.Position.Right - 20,
                _graphics.PreferredBackBufferHeight - 80,
                groundLower3.Position.Width,
                groundLower3.Position.Height));

            groundLower5 = new(new RectangleF(
                groundLower4.Position.Right - 20,
                _graphics.PreferredBackBufferHeight - 80,
                groundLower4.Position.Width,
                groundLower4.Position.Height));

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
            //Laddar musikfilen
            ContentManager content = new ContentManager(this.Services, "Content");
            backgroundMusic = content.Load<Song>("BitGame");
            MediaPlayer.Volume = 0.05f;

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //Laddar texturer för MainMenu.
            mainmenu.LoadContent(Content);

            //Laddar texturer för Background.
            foreach (Background background in backgroundList)
            {
                background.LoadContent(Content);
            }

            //Laddar texturer för deathscene
            deathScene.LoadContent(Content);
            deathtext = Content.Load<SpriteFont>("DeathSceneFont");

            //Laddar texturer för scorebox
            ScoreBox = Content.Load<Texture2D>("ScoreBox");
            ScoreFont = Content.Load<SpriteFont>("ScoreFont");

            //Laddar textur för hp box.
            healthCounter.LoadContent(Content); 

            //laddar texturer för Highscore
            highscore.LoadContent(Content);

            //Laddar texturer för Winnerscene
            winnerScene.LoadContent(Content);

            // Laddar texturer och animationer för Player.
            player = new Player(new RectangleF(600, 400, 25, 36));
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
            foreach(PowerUp powerUp in powerUpList)
            {
                powerUp.LoadContent(Content);
            }
        }
        protected override void Update(GameTime gameTime)
        {
            if ( MediaPlayer.State != MediaState.Playing )
            {
                MediaPlayer.Play(backgroundMusic);
            }

            KeyboardState keyboardState = Keyboard.GetState();

            // Skapa delay så scene inte byts till main menu vid escape-knapptryck när man står i death eller win scene
            menuSceneChangeTimeDelay += gameTime.ElapsedGameTime.TotalSeconds;

            // Tillåt ändring efter en halv sekund
            if (menuSceneChangeTimeDelay >= 0.5) 
            {
                switch (activeScenes)
                {
                    case Scenes.MAIN_MENU:

                        // Uppdaterar bakgrunden på main menu samt loopar igenom menyvalen.
                        mainmenu.Update(gameTime, this, keyboardState);
                        break;

                    case Scenes.LEVEL_ONE:

                        // Kontrollera om spelaren försöker gå tillbaka till huvudmenyn
                        if (keyboardState.IsKeyDown(Keys.Escape))
                        {
                            GameLevel.ShowMainMenu(this);
                        }
                        else
                        {
                            levelTimer += gameTime.ElapsedGameTime.TotalSeconds;

                            if (player.position.X <= crabman.PositionX + 125 && player.position.Y <= crabman.PositionY + 100)
                            {
                                GameLevel.DeathByCrabman(gameTime, this, filePath);
                            }

                            if (levelTimer >= LevelTimeLimit)
                            {
                                LevelOne.ChangeLevel(this, powerUpList);
                            }

                            LevelOne.Update
                                (keyboardState,
                                gameTime,
                                this.GraphicsDevice,
                                _graphics,
                                backgroundList,
                                player,
                                groundList,
                                hedgehog,
                                crabman,
                                shark,
                                powerUpList,
                                healthCounter);
                        }
                        break;

                    case Scenes.LEVEL_TWO:

                        // Kontrollera om spelaren försöker gå tillbaka till huvudmenyn
                        if (keyboardState.IsKeyDown(Keys.Escape))
                        {
                            GameLevel.ShowMainMenu(this);
                        }
                        else
                        {
                            levelTimer += gameTime.ElapsedGameTime.TotalSeconds;
                            if (player.position.X <= crabman.PositionX + 125 && player.position.Y <= crabman.PositionY + 100)
                            {
                                GameLevel.DeathByCrabman(gameTime, this, filePath);
                            }
                            if (levelTimer >= LevelTimeLimit)
                            {
                                File.AppendAllText(filePath, $"\nScore: {((int)Math.Ceiling(score)).ToString()} points");
                                activeScenes = Scenes.WIN;
                                levelTimer = 0;

                                player.HP = 1;

                                healthCounter.Update(gameTime, player);

                                gemScore.Taken = false;
                                heart.Taken = false;
                                jumpBoost.Taken = false;

                                gemScore.Position = new Rectangle(1200, 540, 60, 60);
                                heart.Position = new Rectangle(900, 600, 60, 60);
                                jumpBoost.Position = new Rectangle(700, 600, 60, 60);
                                player.Position = new RectangleF(600, 400, 25, 36);
                            }


                            LevelTwo.Update
                                (keyboardState,
                                gameTime,
                                this.GraphicsDevice,
                                _graphics,
                                backgroundList,
                                player,
                                groundList,
                                hedgehog,
                                crabman,
                                shark,
                                powerUpList,
                                healthCounter);
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
                            activeScenes = Scenes.HIGHSCORE;
                            menuSceneChangeTimeDelay = 0;
                            LastPlayedLevel = Scenes.LEVEL_ONE;
                            Game1.score = 0;
                        }
                        break;

                    case Scenes.WIN:
                        if (keyboardState.IsKeyDown(Keys.Escape))
                        {
                            activeScenes = Scenes.HIGHSCORE;
                            menuSceneChangeTimeDelay = 0;
                            LastPlayedLevel = Scenes.LEVEL_ONE;
                            Game1.score = 0;
                        }
                        break;
                }
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
                    mainmenu.DrawMainMenu(_spriteBatch, MapWidth, MapHeight);
                break;

                case Scenes.LEVEL_ONE:
                    LevelOne.Draw(
                        _spriteBatch,
                        backgroundList,
                        groundList,
                        powerUpList,
                        player,
                        crabman,
                        shark,
                        hedgehog,
                        healthCounter);
                break;

                case Scenes.LEVEL_TWO:
                    LevelTwo.Draw(
                        _spriteBatch,
                        backgroundList,
                        groundList,
                        powerUpList,
                        player,
                        crabman,
                        shark,
                        hedgehog,
                        healthCounter);
                break;

                case Scenes.HIGHSCORE:
                    highscore.DrawBackground(_spriteBatch, MapWidth, MapHeight);
                break;

                case Scenes.WIN:
                    winnerScene.DrawBackground(_spriteBatch, MapWidth, MapHeight, score);
                break;

                case Scenes.DEATH:
                    deathScene.DrawBackground(_spriteBatch, MapWidth, MapHeight, score);
                    _spriteBatch.DrawString(deathtext, "ME CRABMAN!! I EAT YOU!!!", new Vector2(140, 470), Color.Black);
                break;
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        // Get set
        public Scenes ActiveScenes
        {
            get { return this.activeScenes; }
            set { this.activeScenes = value; }
        }
        public Scenes LastPlayedLevel
        {
            get { return this.lastPlayedLevel; }
            set { lastPlayedLevel = value; }
        }
        public double LevelTimer
        {
            get { return this.levelTimer; }
            set { this.levelTimer = value; }
        }
        internal Player player
        {
            get { return this.Player; }
            set { Player = value; }
        }
        internal HealthCounter healthCounter
        {
            get { return this.HealthCounter; }
            set { this.HealthCounter = value; }
        }
        internal Heart heart
        {
            get { return this.Heart; }
            set { Heart = value; }
        }
        internal JumpBoost jumpBoost
        {
            get { return this.JumpBoost; }
            set { JumpBoost = value; }
        }
        internal GemScore gemScore
        {
            get { return this.GemScore; }
            set { GemScore = value; }
        }
    }
}