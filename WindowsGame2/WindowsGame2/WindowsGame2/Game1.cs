/*Lee Southerst
 * ICS4U
 * Period 4
 * Mr. Trink
 * XNA Tile Based Project
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace WindowsGame2
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        SoundEffect korobeiniki;
        static int heldShapeID = 1;
        static Texture2D blockImages;
        static Queue<int> blockQueue = new Queue<int>();
        static double timeSinceRotation = 0;
        static double timeSinceHardDrop = 0;
        static double timeSinceHold = 0;
        static double timeSincePause = 0;
        static bool isRunning = true;
        double timeElapsed = 0;
        static int activeAnchorX;
        static int activeAnchorY;
        static bool usedHold = false;
        static List<Vector2> activeBlocks = new List<Vector2>();
        static Random randy = new Random();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static Square[,] board = new Square[10, 24];
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            //sets the tick time to be once every 16 seconds, since Tetris does not need fast Update functions
            base.TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / 16.0);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Square.blocks = Content.Load<Texture2D>("blocks");
            blockImages = Content.Load<Texture2D>("blockImages");
            for(int i = 0; i < 10; i++)
            {
                for(int j = 0; j < 24; j++)
                {
                    board[i, j] = new Square(i,j);
                }
            }
            //plays Tetris song; highly essential
            korobeiniki = Content.Load<SoundEffect>("korobeiniki");
            SoundEffectInstance instance = korobeiniki.CreateInstance();
            instance.IsLooped = true;
            korobeiniki.Play();
            blockQueue.Enqueue(randy.Next(1,7));
            blockQueue.Enqueue(randy.Next(1, 7));
            blockQueue.Enqueue(randy.Next(1, 7));
            CreateShape();
        }

        protected override void UnloadContent()
        {
        }
        protected override void Update(GameTime gameTime)
        {
            timeSinceRotation += gameTime.ElapsedGameTime.TotalMilliseconds;
            timeElapsed += gameTime.ElapsedGameTime.TotalMilliseconds;
            timeSinceHardDrop += gameTime.ElapsedGameTime.TotalMilliseconds;
            timeSinceHold += gameTime.ElapsedGameTime.TotalMilliseconds;
            timeSincePause += gameTime.ElapsedGameTime.TotalMilliseconds;
            if(Keyboard.GetState().IsKeyDown(Keys.P) && timeSincePause > 1000)
            {
                timeSincePause = 0;
                //allows pausing
                if (isRunning)
                    isRunning = false;
                else
                    isRunning = true;
            }
            if (isRunning)
            {
                KeyboardState state = Keyboard.GetState();
                if (state.IsKeyDown(Keys.Left))
                {
                    if (Translate(-1, 0))
                        activeAnchorX--;
                }
                if (state.IsKeyDown(Keys.Right))
                {
                    if (Translate(1, 0))
                        activeAnchorX++;
                }
                if (state.IsKeyDown(Keys.Down))
                {
                    if (Translate(0, 1))
                        activeAnchorY++;
                }
                if (state.IsKeyDown(Keys.A))
                {
                    Rotate(0);
                }
                if (state.IsKeyDown(Keys.D))
                {
                    Rotate(1);
                }
                if(state.IsKeyDown(Keys.H) && timeSinceHold > 1000 && !usedHold)
                {
                    usedHold = true;
                    timeSinceHold = 0;
                    HoldShape();
                }
                if (state.IsKeyDown(Keys.Space) && timeSinceHardDrop > 1000)
                {
                    timeSinceHardDrop = 0;
                    for(int i = 0; i < 24; i++)
                    {
                        if (!Translate(0, 1))
                            break;
                    }
                }
                
                if (timeElapsed > 350)
                {
                    timeElapsed = 0;
                    if (!Translate(0, 1))
                    {
                        activeBlocks.Clear();
                        for (int i = 0; i < 24; i++)
                        {
                            bool rowComplete = true;
                            for (int j = 0; j < 10; j++)
                            {
                                if (!board[j, i].hasBlock)
                                {
                                    rowComplete = false;
                                }
                            }
                            if (rowComplete)
                            {
                                //if row is complete, move all rows above it downwards
                                for (int k = i - 1; k >= 0; k--)
                                    for (int l = 0; l < 10; l++)
                                    {
                                        board[l, k + 1] = board[l, k];
                                        board[l, k + 1].y++;
                                    }
                                for (int l = 0; l < 10; l++)
                                    board[l, 0] = new Square(l, 0);
                            }
                        }
                        //if the block is at the top of the board and can't move down, game over
                        if (activeAnchorY == -1)
                            this.Exit();
                        //gameOver
                        else
                        {
                            usedHold = false;
                            CreateShape();
                        }
                    }
                    else
                        activeAnchorY++;
                }
            }
            // Allows the game to exit
            
            // TODO: Add your update logic here

            base.Update(gameTime);
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            for (int i = 0; i < 10; i++)
            {
                for(int j = 0; j < 24; j++)
                {
                    //draw the game
                    spriteBatch.Draw(Square.blocks, new Rectangle(i * 15,j * 15,15, 15), new Rectangle(board[i,j].colourValue * 15, 0,15,15), Color.White);
                }
            }
            int index = 1;
            foreach(int i in blockQueue)
            {
                //draw the tetris queue
                spriteBatch.Draw(blockImages, new Rectangle(200, 100 * index - 70, 55, 80), new Rectangle(40 * (i-1), 0, 40, 67), Color.White);
                index++;
            }
            spriteBatch.Draw(blockImages, new Rectangle(400, 30, 55, 80), new Rectangle(40 * heldShapeID - 40, -5, 40, 75), Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }
        private void Rotate(int direction)
        {
            //if the block is at the side of the board, move it towards the middle; avoids any index errors
            if(activeAnchorX < 0)
            {
                Translate(1, 0);
                activeAnchorX++;
            }
            if(activeAnchorX > 6)
            {
                Translate(-1, 0);
                activeAnchorX--;
            }
            if (activeAnchorY >= 0 && activeAnchorY < 21 && timeSinceRotation > 250)
            {
                timeSinceRotation = 0;
                List<Square> nonActive = new List<Square>();
                Square[,] rotate = new Square[4, 4];
                //take all blocks that are currently active and put them into a rotations array. Take non active blocks and put them into a list
                for (int i = activeAnchorX; i < activeAnchorX + 4; i++)
                {
                    for (int j = activeAnchorY; j < activeAnchorY + 4; j++)
                    {
                        if (board[i, j].hasBlock && !activeBlocks.Contains(new Vector2(i, j)))
                        {
                            nonActive.Add(board[i, j]);
                            rotate[i - activeAnchorX, j - activeAnchorY].Update(0);
                        }
                        else
                            rotate[i - activeAnchorX, j - activeAnchorY] = board[i, j];
                    }
                }
                var rotateNew = new Square[4, 4];
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        rotateNew[i, j] = new Square(i + activeAnchorX, j + activeAnchorY);

                if (direction == 0)//counterclockwise
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            rotateNew[i, j] = rotate[j, 4 - i - 1]; //transpose array, reverse every column
                            rotateNew[i, j].x = activeAnchorX + i;
                            rotateNew[i, j].y = activeAnchorY + j;
                        }
                    }
                }
                if (direction == 1)//clockwise
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            rotateNew[i, j] = rotate[4 - j - 1, i]; //transpose array, reverse every row
                            rotateNew[i, j].x = activeAnchorX + i;
                            rotateNew[i, j].y = activeAnchorY + j;
                        }
                    }
                }
                bool isValid = true;
                //check if any block will prevent rotation
                foreach (Square s in nonActive)
                {
                    if (rotateNew[s.x - activeAnchorX, s.y - activeAnchorY].hasBlock)
                    {
                        isValid = false;
                        break;
                    }
                }
                //if rotation is valid
                if (isValid)
                {
                    activeBlocks.Clear();
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            //place the rotated blocks
                            if (rotateNew[i, j].hasBlock)
                            {
                                board[activeAnchorX + i, activeAnchorY + j] = rotateNew[i, j];
                                activeBlocks.Add(new Vector2(activeAnchorX + i, activeAnchorY + j));
                            }
                            else if (rotate[i, j].hasBlock)
                            {
                                board[activeAnchorX + i, activeAnchorY + j] = new Square(activeAnchorX + i, activeAnchorY + j);
                            }
                        }
                    }
                }
                //place any blocks that were not active back into the board
                foreach (Square s in nonActive)
                {
                    board[s.x, s.y] = s;
                }
            }
        }
        private bool Translate(int xMod, int YMod)
        {
            List<Square> tempListSquare = new List<Square>();
            //add the squares to translate into a list, and then clear the active blocks from the board
            foreach(Vector2 v in activeBlocks)
            {
                tempListSquare.Add(board[(int)v.X,(int)v.Y]);
                board[(int)v.X, (int)v.Y].Update(0);
            }
            bool isValid = true;
            //check if the translation is valid
            foreach(Square s in tempListSquare)
            {
                if (!(s.x + xMod >= 0 && s.x + xMod < 10 && s.y + xMod >= 0 && s.y + YMod < 24))
                {
                    isValid = false;
                    break;
                }
                if (board[s.x + xMod, s.y + YMod].hasBlock == true)
                {
                    isValid = false;
                    break;
                }
            }
            //if it is valid, place the blocks
            if(isValid)
            {
                activeBlocks.Clear();
                foreach(Square s in tempListSquare)
                {
                    board[s.x + xMod, s.y + YMod].Update(s.colourValue);
                    activeBlocks.Add(new Vector2(s.x + xMod, s.y + YMod));
                }
            }
            else
            {
                foreach(Square s in tempListSquare)
                {
                    board[s.x, s.y].Update(s.colourValue);
                }
            }
            return isValid;
        }
        private void CreateShape()
        {
            blockQueue.Enqueue(randy.Next(1, 8));
            int shapeID = blockQueue.Dequeue();
            if (shapeID == 1)//straight block
            {

                board[3, 0].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[5, 0].Update(shapeID);
                board[6, 0].Update(shapeID);
                activeBlocks.Add(new Vector2(3, 0));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(5, 0));
                activeBlocks.Add(new Vector2(6, 0));
                
            }
            if (shapeID == 2)//square block
            {
                board[4, 1].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[5, 0].Update(shapeID);
                board[5, 1].Update(shapeID);
                activeBlocks.Add(new Vector2(4, 1));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(5, 0));
                activeBlocks.Add(new Vector2(5, 1));
            }
            if (shapeID == 3)//L shape
            {
                board[3, 0].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[5, 0].Update(shapeID);
                board[3, 1].Update(shapeID);
                activeBlocks.Add(new Vector2(3, 0));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(5, 0));
                activeBlocks.Add(new Vector2(3, 1));
            }
            if (shapeID == 4)//Reverse L
            {
                board[3, 0].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[5, 0].Update(shapeID);
                board[5, 1].Update(shapeID);
                activeBlocks.Add(new Vector2(3, 0));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(5, 0));
                activeBlocks.Add(new Vector2(5, 1));
            }
            if (shapeID == 5)//Leftward Z
            {
                board[4, 1].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[5, 0].Update(shapeID);
                board[3, 1].Update(shapeID);
                activeBlocks.Add(new Vector2(4, 1));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(5, 0));
                activeBlocks.Add(new Vector2(3, 1));
            }
            if (shapeID == 6)//Rightward Z
            {
                board[3, 0].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[4, 1].Update(shapeID);
                board[5, 1].Update(shapeID);
                activeBlocks.Add(new Vector2(3, 0));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(4, 1));
                activeBlocks.Add(new Vector2(5, 1));
            }
            if(shapeID == 7)//T Shape
            {
                board[3, 0].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[5, 0].Update(shapeID);
                board[4, 1].Update(shapeID);
                activeBlocks.Add(new Vector2(3, 0));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(5, 0));
                activeBlocks.Add(new Vector2(4, 1));
            }
            activeAnchorX = 3;
            activeAnchorY = -1;
        }
        private void HoldShape()
        {
            int shapeID;
            shapeID = heldShapeID;
            foreach(Vector2 v in activeBlocks)
            {
                heldShapeID = board[(int)v.X, (int)v.Y].colourValue;
                board[(int)v.X, (int)v.Y].Update(0);
            }
            activeBlocks.Clear();
            if (shapeID == 1)//straight block
            {

                board[3, 0].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[5, 0].Update(shapeID);
                board[6, 0].Update(shapeID);
                activeBlocks.Add(new Vector2(3, 0));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(5, 0));
                activeBlocks.Add(new Vector2(6, 0));

            }
            if (shapeID == 2)//square block
            {
                board[4, 1].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[5, 0].Update(shapeID);
                board[5, 1].Update(shapeID);
                activeBlocks.Add(new Vector2(4, 1));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(5, 0));
                activeBlocks.Add(new Vector2(5, 1));
            }
            if (shapeID == 3)//L shape
            {
                board[3, 0].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[5, 0].Update(shapeID);
                board[3, 1].Update(shapeID);
                activeBlocks.Add(new Vector2(3, 0));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(5, 0));
                activeBlocks.Add(new Vector2(3, 1));
            }
            if (shapeID == 4)//Reverse L
            {
                board[3, 0].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[5, 0].Update(shapeID);
                board[5, 1].Update(shapeID);
                activeBlocks.Add(new Vector2(3, 0));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(5, 0));
                activeBlocks.Add(new Vector2(5, 1));
            }
            if (shapeID == 5)//Leftward Z
            {
                board[4, 1].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[5, 0].Update(shapeID);
                board[3, 1].Update(shapeID);
                activeBlocks.Add(new Vector2(4, 1));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(5, 0));
                activeBlocks.Add(new Vector2(3, 1));
            }
            if (shapeID == 6)//Rightward Z
            {
                board[3, 0].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[4, 1].Update(shapeID);
                board[5, 1].Update(shapeID);
                activeBlocks.Add(new Vector2(3, 0));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(4, 1));
                activeBlocks.Add(new Vector2(5, 1));
            }
            if (shapeID == 7)//T Shape
            {
                board[3, 0].Update(shapeID);
                board[4, 0].Update(shapeID);
                board[5, 0].Update(shapeID);
                board[4, 1].Update(shapeID);
                activeBlocks.Add(new Vector2(3, 0));
                activeBlocks.Add(new Vector2(4, 0));
                activeBlocks.Add(new Vector2(5, 0));
                activeBlocks.Add(new Vector2(4, 1));
            }

            activeAnchorX = 3;
            activeAnchorY = -1;
        }
    }
}
