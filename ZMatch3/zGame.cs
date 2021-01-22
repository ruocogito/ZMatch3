using ZMatch3.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;

using OpenTK.Mathematics;

//using System.Numerics;
namespace ZMatch3
{
    class zGame : GameWindow
    {

        // Because we're adding a texture, we modify the vertex array to include texture coordinates.
        // Texture coordinates range from 0.0 to 1.0, with (0.0, 0.0) representing the bottom left, and (1.0, 1.0) representing the top right
        // The new layout is three floats to create a vertex, then two floats to create the coordinates
        /* private readonly float[] _vertices =
         {
             // Position         Texture coordinates
              0.09f -0.85f,  0.1f +0.85f, 0.0f, 1.0f, 1.0f, // top right
              0.09f -0.85f, -0.1f +0.85f, 0.0f, 1.0f, 0.0f, // bottom right
             -0.09f -0.85f, -0.1f +0.85f, 0.0f, 0.0f, 0.0f, // bottom left
             -0.09f -0.85f,  0.1f +0.85f, 0.0f, 0.0f, 1.0f  // top left
         };*/
        //hx = 0.2
        //hy = 0.2
        private readonly float[] _vertices =
        {
            // Position         Texture coordinates
             0.1f -0.661f,  0.1f +0.6287f, 0.0f, 1.0f, 1.0f, // top right
             0.1f -0.661f, -0.1f +0.6287f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.1f -0.661f, -0.1f +0.6287f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.1f -0.661f,  0.1f +0.6287f, 0.0f, 0.0f, 1.0f  // top left
        };
        //x: 0.01:3px?
        //y: 0.01:3px?
        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        // These are the handles to OpenGL objects. A handle is an integer representing where the object lives on the
        // graphics card. Consider them sort of like a pointer; we can't do anything with them directly, but we can
        // send them to OpenGL functions that need them.

        // What these objects are will be explained in OnLoad.
        private int _vertexBufferObject;

        private int _vertexArrayObject;

        // This class is a wrapper around a shader, which helps us manage it.
        // The shader class's code is in the Common project.
        // What shaders are and what they're used for will be explained later in this tutorial.
        private Shader _shader;
        
        // Add a handle for the EBO
        private int _elementBufferObject;
        private Texture[] _texture;
        Texture txtrbomb, v_reaper, h_reaper;
        System.Collections.Generic.Dictionary<string, Texture> txtr_fortext;
        private zElm[,] Elms;
        private float elmdx, elmdy;
        float delta;
        private System.Random rand;
        private (short,short) ichosenelm;
        (ushort i, ushort j, int tickcounter)[] activebombs;
        (ushort i, ushort j, int tickcounter, ushort type)[] activereapers;
        ushort score;
        System.DateTime dtstarted;
        ushort gameMode;
        private ushort ChoiceRandomFromRangeExceptOne(ushort one = ushort.MaxValue, ushort yaone=ushort.MaxValue)
        {
            System.Collections.Generic.List<ushort> lrange = new System.Collections.Generic.List<ushort>();
            for(ushort i=0;i<5;i++)
                if(i!=one && i!=yaone) 
                    lrange.Add(i);
            return lrange[rand.Next(0, lrange.Count)];
        }
        public zGame(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            elmdx = 0.18f;
            elmdy = 0.18f;
            delta = 0.003f;
            //// docs.microsoft.com: Instantiate random number generator using system-supplied value as seed.
            rand = new System.Random();
            ichosenelm = (-1,-1);
            waittomatch = 0;
            test_keydelay = 0;
            activebombs = new (ushort i, ushort j, int tickcounter)[3];
            activereapers = new (ushort i, ushort j, int tickcounter, ushort type)[3];
            score = 0;
            gameMode = 0;
            
        } 
        void DestroyElm(ushort i, ushort j)
        {
            Elms[i, j].IsDestroed = true;
            score++;
        }
        void GenerateField()
        {
            for (ushort i = 0; i < 8; i++)
                for (ushort j = 0; j < 8; j++)
                {
                    if (i > 1 && j < 2 && Elms[i - 1, j].elmtype == Elms[i - 2, j].elmtype) //2+ elms to left, 1 or 0 to up
                        Elms[i, j] = new zElm(ChoiceRandomFromRangeExceptOne(Elms[i - 1, j].elmtype));
                    else if (i < 2 && j > 1 && Elms[i, j - 1].elmtype == Elms[i, j - 2].elmtype) //2+ to up, 1 or 0 to left
                        Elms[i, j] = new zElm(ChoiceRandomFromRangeExceptOne(Elms[i, j - 1].elmtype));
                    else if (i > 1 && j > 1) //2+ to up and 2+ to left
                        if (Elms[i - 1, j].elmtype == Elms[i - 2, j].elmtype &&  //left == left left
                            Elms[i, j - 1].elmtype == Elms[i, j - 2].elmtype)  //up == up up
                            if (Elms[i, j - 1].elmtype == Elms[i - 1, j].elmtype) // up == left 
                                Elms[i, j] = new zElm(ChoiceRandomFromRangeExceptOne(Elms[i, j - 1].elmtype));
                            else
                                Elms[i, j] = new zElm(ChoiceRandomFromRangeExceptOne(Elms[i, j - 1].elmtype, Elms[i - 1, j].elmtype));
                        else if (Elms[i - 1, j].elmtype == Elms[i - 2, j].elmtype)  //left == left left
                            Elms[i, j] = new zElm(ChoiceRandomFromRangeExceptOne(Elms[i - 1, j].elmtype));
                        else if (Elms[i, j - 1].elmtype == Elms[i, j - 2].elmtype)  //up == up up) 
                            Elms[i, j] = new zElm(ChoiceRandomFromRangeExceptOne(Elms[i, j - 1].elmtype));
                        else
                            Elms[i, j] = new zElm(ChoiceRandomFromRangeExceptOne());
                    else
                        Elms[i, j] = new zElm(ChoiceRandomFromRangeExceptOne());
                }

        }
        // Now, we start initializing OpenGL.
        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer(); 
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // The shaders have been modified to include the texture coordinates, check them out after finishing the OnLoad function.
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader_bomb.frag");
            _shader.Use();

           
            // Because there's now 5 floats between the start of the first vertex and the start of the second,
            // we modify this from 3 * sizeof(float) to 5 * sizeof(float).
            // This will now pass the new vertex array to the buffer.
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            
            // Next, we also setup texture coordinates. It works in much the same way.
            // We add an offset of 3, since the first vertex coordinate comes after the first vertex
            // and change the amount of data to 2 because there's only 2 floats for vertex coordinates
            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            _texture = new Texture[]
            {
                Texture.LoadFromFile("Resources/red_six.png"),
                Texture.LoadFromFile("Resources/blue-four.png"),
                Texture.LoadFromFile("Resources/green-triangle.png"),
                Texture.LoadFromFile("Resources/purpure_five.png"),
                Texture.LoadFromFile("Resources/yellow_trapezoid.png"),
                Texture.LoadFromFile("Resources/active_reaper.png")
            };

            Elms = new zElm[8, 8];
           

            /*foreach(var txtr in _texture)
            {
                txtr.Use(TextureUnit.Texture0);
            }*/

            txtrbomb = Texture.LoadFromFile("Resources/bomb.png");
          
            v_reaper = Texture.LoadFromFile("Resources/v_reaper.png");
            h_reaper = Texture.LoadFromFile("Resources/h_reaper.png");
            txtr_fortext = new System.Collections.Generic.Dictionary<string, Texture>();
            foreach(var c in "score0123456789")
                txtr_fortext.Add(""+c, Texture.LoadFromFile("Resources/"+ c + ".png"));
            txtr_fortext.Add(":", Texture.LoadFromFile("Resources/ddot.png"));
            txtr_fortext.Add(" ", Texture.LoadFromFile("Resources/nothing.png")); 
            txtr_fortext.Add("play", Texture.LoadFromFile("Resources/play.png"));
            txtr_fortext.Add("gameover", Texture.LoadFromFile("Resources/gameover.png"));
            txtr_fortext.Add("time", Texture.LoadFromFile("Resources/time.png"));
            txtr_fortext.Add("field", Texture.LoadFromFile("Resources/field.png"));
            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);
            base.OnLoad();
        }
        void DrawControl(float Tx,float Ty, float Sx, float Sy, string texturename)
        {
            var transform = Matrix4.Identity;
            transform = transform * Matrix4.CreateTranslation(Tx, Ty, 0.0f);
            transform = transform * Matrix4.CreateScale(Sx, Sy, 1f);
            txtr_fortext[texturename].Use(TextureUnit.Texture0);
            txtr_fortext[" "].Use(TextureUnit.Texture1);
            _shader.Use();
            _shader.SetMatrix4("transform", transform);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        }
        void DrawText(string text)
        {
            float i = -0.035f;
            
            foreach (var c in text)
            {
                string key = "" + c;
                DrawControlObject((i, 0.25f), key);
                i += 0.08f;
            }
        }
        void DrawControlObject((float, float) MTxy, string drwchar)
        {
            var transform = Matrix4.Identity;
            transform = transform* Matrix4.CreateTranslation(MTxy.Item1, MTxy.Item2, 0.0f);
            txtr_fortext[drwchar].Use(TextureUnit.Texture0);
            txtr_fortext[" "].Use(TextureUnit.Texture1);
            _shader.Use();
            _shader.SetMatrix4("transform", transform);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        }
        private (float, float) CalcMatrixTxyByElmIndexes(int i, int j) => (i * elmdx, -j * elmdy);
        //on game field:
        //------------
        // X1
        //    X2
        // 
        //-----------
        // X2.y < X1.y
        // X2.x > X1.x
        private void DrawElm((float, float) MTxy, ushort ielm, ushort jelm)
        {
            var transform = Matrix4.Identity;
            var rot = Matrix4.Identity; 
            if (Elms[ielm, jelm].rotangle > 0) 
            {
                rot=rot*Matrix4.CreateTranslation(0.661f, -0.6287f, 0.0f);
                rot=rot*Matrix4.CreateRotationZ(Elms[ielm, jelm].rotangle);
                rot=rot*Matrix4.CreateTranslation(-0.661f, +0.6287f, 0.0f);
            }
            transform = transform*Matrix4.CreateTranslation(MTxy.Item1, MTxy.Item2, 0.0f);
            _texture[Elms[ielm, jelm].elmtype].Use(TextureUnit.Texture0);
            //_texture[type].Use(TextureUnit.Texture1);
            if(Elms[ielm, jelm].IsBomb)
                txtrbomb.Use(TextureUnit.Texture1);
            else if(Elms[ielm, jelm].ReaperType==0 || Elms[ielm, jelm].elmtype==5)
                txtr_fortext[" "].Use(TextureUnit.Texture1);
            else if (Elms[ielm, jelm].ReaperType == 1 )
                h_reaper.Use(TextureUnit.Texture1);
            else
                v_reaper.Use(TextureUnit.Texture1);

            _shader.Use();
            _shader.SetMatrix4("transform", rot*transform);


            //_shader.SetVector3("objectColor", new Vector3(1.0f, 1.0f, 1.0f));
           // _shader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        }
        ushort LineMatchI(ushort i, ushort j, short di)
        {
            short k=(short)i;
            while((k>0 && di<0 || k<7 && di>0) && Elms[k,j].elmtype== Elms[k+di,j].elmtype)
                k += di;
            return (ushort) k;
        }
        ushort LineMatchJ(ushort i, ushort j, short dj)
        {
            short k = (short)j;
            while ((k > 0 && dj < 0 || k < 7 && dj > 0) && Elms[i, k].elmtype == Elms[i, k + dj].elmtype)
                k += dj;
            return (ushort)k;
         }
        void MakeGravityJ(ushort i, ushort j)
        {
            for(ushort k=0;k<=7;k++)
            {
                if (Elms[i, k].elmtype == 5 && Elms[i, k].IsDestroed == false)
                    return;
            }
            for (short l = (short)j; l >= 0; l--)
            {
                if (!Elms[i, l].IsDestroed)
                    continue;
                if(Elms[i,l].SpawnBombThere)
                {
                    Elms[i, l].SpawnBombThere = false;
                    Elms[i, l].IsDestroed = false;
                    Elms[i, l].IsBomb = true;
                    continue;
                }
                else if (Elms[i, l].SpawnHReaperThere)
                {
                    Elms[i, l].SpawnHReaperThere = false;
                    Elms[i, l].IsDestroed = false;
                    Elms[i, l].ReaperType = 1;
                    continue;
                }
                else if (Elms[i, l].SpawnVReaperThere)
                {
                    Elms[i, l].SpawnVReaperThere = false;
                    Elms[i, l].IsDestroed = false;
                    Elms[i, l].ReaperType = 2;
                    continue;
                }
                if (Elms[i,l].IsBomb)
                {
                    for(ushort k=0;k<3;k++)
                    {
                        if(activebombs[k].tickcounter == 0)
                        {
                            activebombs[k].tickcounter = 15;
                            activebombs[k].i = i;
                            activebombs[k].j = (ushort)l;
                            Elms[i, l].IsBomb = false;
                            break;
                        }
                    }
                }
                if(Elms[i,l].ReaperType > 0)
                {
                    
                    for (ushort k = 0; k < 3; k++)
                    {
                        if (activereapers[k].tickcounter == 0)
                        {
                            activereapers[k].tickcounter = 2;
                            activereapers[k].i = i;
                            activereapers[k].j = (ushort)l;
                            activereapers[k].type = Elms[i, l].ReaperType;
                            Elms[i, l].ReaperType = 0;
                            break;
                        }
                    }
                }
                short aliveupelm = -1;
                for (short k = l; k >= 0; k--)
                {
                    if (!Elms[i, k].IsDestroed)
                    {
                        aliveupelm = (short)k;
                        break;
                    }
                }
                if(aliveupelm >= 0)
                {
                    StartExchangeAnimation(((short)i, l), ((short)i, aliveupelm),false);

                }
                else //no any alive elm up
                {
                    Elms[i,l] = new zElm(ChoiceRandomFromRangeExceptOne());
                    Elms[i, l].waittospawn = 20;
                  //  int tickcount = (int)(l*elmdy / delta);
                   // if(l>0)
                    //    Elms[i, l].StartAnimation((i, 0), tickcount, -delta, false);
                }
            }
            
        }
        void MakeGravityI(ushort i, ushort j)
        {
            for(ushort k=i; k<=7 && Elms[k,j].IsDestroed;k++)
                MakeGravityJ(k, j);
            
        }
        void DestroyLineI(ushort i1, ushort j, ushort i2)
        {
            for (ushort k = i1; k <= i2; k++)
                DestroyElm(k, j);
                
            MakeGravityI(i1, j);
        }
        void DestroyLineJ(ushort i, ushort j1, ushort j2)
        {
            for (ushort k = j1; k <= j2; k++)
                DestroyElm(i, k);
            MakeGravityJ(i, j2);
        }
       bool CalcMatchThere(ushort i, ushort j)
        {
            ushort left_match = LineMatchI(i, j, -1);
            ushort right_match = LineMatchI(i, j, 1);
            bool matchi = right_match - left_match >= 2;
            
            ushort up_match = LineMatchJ(i, j, 1);
            ushort down_match = LineMatchJ(i, j, -1);
            bool matchj = up_match - down_match >= 2;

            if (matchi || matchj)
            {
                Elms[i, j].isfakeexchanged = false;
                //if (Elms[i, j].isanimated) Elms[Elms[i, j].getorgij.Item1, Elms[i, j].getorgij.Item2].isfakeexchanged = false;
                Elms[i, j].isanimated = false;
            }
            if (matchi && matchj || right_match - left_match >= 4 || up_match - down_match >= 4)
            {
                Elms[i, j].SpawnBombThere = true;
            }
            if (!(matchi && matchj) && (right_match - left_match == 3))
                Elms[i, j].SpawnHReaperThere = true;
            else if (!(matchi && matchj) && (up_match - down_match == 3))
                Elms[i, j].SpawnVReaperThere = true;
            //past spawned reaper in the match
          /*  if (!(matchi && matchj) && matchi)
            {
                for (ushort k = left_match; k <= right_match; k++)
                    if (Elms[k, j].ReaperType > 0)
                    {
                        SpawnActiveReaper(k, j);
                        return true;
                    }
            }
            else if (!(matchi && matchj) && matchj)
            {
                for (ushort k = down_match; k <= up_match; k++)
                    if (Elms[i, k].ReaperType > 0)
                    {
                        SpawnActiveReaper(k, j);
                        return true;
                    }
            }
            if(matchi && matchj)
            {
                bool jreaper = false;
                for (ushort k = down_match; k <= up_match; k++)
                {
                    if (Elms[i, k].ReaperType > 0)
                        jreaper = true;
                }
                bool ireaper = false;
                for (ushort k = left_match; k <= right_match; k++)
                    if (Elms[k, j].ReaperType > 0)
                        ireaper = true;

            }*/




            if (matchi && !(matchi && matchj))
                DestroyLineI(left_match, j, right_match);
            if (matchj && !(matchi && matchj))
                DestroyLineJ(i, down_match, up_match);
            if (matchi && matchj)
            {
                DestroyLineI(left_match, j, right_match);
                if (down_match != j)
                    DestroyLineJ(i, (ushort)(j - 1), down_match);
                if (up_match != j)
                    DestroyLineJ(i, (ushort)(j + 1), up_match);
            }

                return matchi || matchj;
        }
        (short, short) toshortuple((ushort, ushort) ij) => ((short)ij.Item1, (short)ij.Item2);

        void GlobalCalcMatch()
        {
            bool match = false;
            do
            {
                //Calc Match
                for (short j = 7; j >= 0; j--)
                {
                    for (int k = 63; k >= 0; k -= 3)
                    {
                        if (match = CalcMatchThere((ushort)(k % 8), (ushort)j))
                            break;
                    }
                    if (match)
                        break;
                }
            } while (match);
        }
        int waittomatch;
        short SearchNotReaperElmI(ushort i, ushort j, short di)
        {
            for(short k=(short)i;di>0 && k<=7 || di<0 && k>=0;k=(short)(k+di))
            {
                if (Elms[k, j].elmtype < 5 || Elms[k, j].elmtype == 5 && Elms[k, j].IsDestroed == true)
                    return k;
            }
            return -1;
        }
        short SearchNotReaperElmJ(ushort i, ushort j, short dj)
        {
            for (short k = (short)j; dj > 0 && k <= 7 || dj < 0 && k >= 0; k = (short)(k + dj))
            {
                if (Elms[i, k].elmtype < 5 || Elms[i, k].elmtype == 5 && Elms[i, k].IsDestroed == true)
                    return k;
            }
            return -1;
        }
        void GameThink()
        {
            bool animationexist = false;
            for (ushort i = 0; i < 8; i++)
                for (ushort j = 0; j < 8; j++)
                {
                    if (Elms[i, j].IsDestroed) continue;
                    float drot = (i, j) == toushortuple(ichosenelm) ? MathHelper.DegreesToRadians(3f) : 0;
                    if (Elms[i, j].ReaperType > 0 && (i, j) != toushortuple(ichosenelm)) Elms[i, j].rotangle = 0;
                    Elms[i, j].Think(drot);
                    if (Elms[i, j].IsAnmFinished() && Elms[i, j].isfakeexchanged)
                    {
                        bool match = CalcMatchThere(i, j);
                        if (!match)
                        {
                            match = CalcMatchThere(Elms[i, j].getorgij.Item1, Elms[i, j].getorgij.Item2);
                            if (match)
                            {
                                Elms[i, j].isfakeexchanged = false;
                                Elms[i, j].isanimated = false;
                            }
                        }
                        if (!match) //move back
                        {
                            //(ushort, ushort) Elm2 = Elms[i, j].CalcNeighbor(i, j);
                            //Elms[i, j].isfakeexchanged = false;
                            StartExchangeAnimation(toshortuple(Elms[i, j].getorgij), toshortuple((i, j)), false);
                        }
                        //else

                    }
                    else if (Elms[i, j].isanimated && !Elms[i, j].IsAnmFinished())
                    {
                        animationexist = true;
                        //draw in motion
                        DrawElm(Elms[i, j].getTmtrxXY(CalcMatrixTxyByElmIndexes(Elms[i, j].getorgij.Item1, Elms[i, j].getorgij.Item2)), i, j);
                    }
                    else if (Elms[i, j].IsAnmFinished() && Elms[i, j].elmtype == 5 && Elms[i, j].IsDestroed == false)
                    {
                        int tickcount = (int)(elmdx / delta);
                        //reaper think code
                        if (Elms[i, j].ReaperType == 1)
                        {
                            short nextil = SearchNotReaperElmI(i, j, -1);
                            short nextir = SearchNotReaperElmI(i, j, 1);
                            if (Elms[i, j].getorgij.Item1 > i && nextil >= 0)
                            {
                                score++;
                                ExchangeElms(i, j, nextil, j);
                                Elms[i, j].IsDestroed = true;
                                Elms[nextil, j].StartAnimation((i, j), tickcount * (i - nextil), -delta, true);
                            }
                            else if (Elms[i, j].getorgij.Item1 < i && nextir <= 7 && nextir >= 0)
                            {
                                score++;
                                ExchangeElms(i, j, nextir, j);
                                Elms[i, j].IsDestroed = true;
                                Elms[nextir, j].StartAnimation((i, j), tickcount * (nextir - i), delta, true);
                            }
                            else //if ((Elms[i, j].getorgij.Item1 > i && nextil < 0) || (Elms[i, j].getorgij.Item1 < i && nextir > 7))
                            {
                                Elms[i, j].IsDestroed = true;
                                Elms[i, j].isanimated = false;
                                Elms[i, j].ReaperType = 0;
                                // MakeGravityJ(i, j);
                            }

                        }
                        else
                        {
                            short nextjd = SearchNotReaperElmJ(i, j, -1);
                            short nextju = SearchNotReaperElmJ(i, j, 1);
                            if (Elms[i, j].getorgij.Item2 > j && nextjd >= 0)
                            {
                                score++;
                                ExchangeElms(i, j, i, nextjd);
                                Elms[i, j].IsDestroed = true;
                                Elms[i, nextjd].StartAnimation((i, j), tickcount * (j - nextjd), delta, false);
                            }
                            else if (Elms[i, j].getorgij.Item2 < j && nextju <= 7 && nextju >= 0)
                            {
                                score++;
                                ExchangeElms(i, j, i, nextju);
                                Elms[i, j].IsDestroed = true;
                                Elms[i, nextju].StartAnimation((i, j), tickcount * (nextju - j), -delta, false);
                            }
                            else //if ((Elms[i, j].getorgij.Item2 > j && nextjd < 0) || (Elms[i, j].getorgij.Item2 < j && nextju > 7))
                            {
                                Elms[i, j].IsDestroed = true;
                                Elms[i, j].isanimated = false;
                                Elms[i, j].ReaperType = 0;

                            }

                        }
                        MakeGravityJ(i, 7);
                    }

                    else if (!Elms[i, j].isanimated || Elms[i, j].IsAnmFinished())
                    {
                        //draw by indixes

                        if (Elms[i, j].waittospawn > 0 && Elms[i, j].isanimated)
                            Elms[i, j].waittospawn = 0;
                        Elms[i, j].isanimated = false;
                        if (Elms[i, j].waittospawn > 0) Elms[i, j].waittospawn--;
                        else DrawElm(CalcMatrixTxyByElmIndexes(i, j), i, j);
                    }
                }

            //bomb expl
            for (ushort k = 0; k < 3; k++)
            {
                if (activebombs[k].tickcounter == 1)
                    for (ushort l = (ushort)(activebombs[k].i - 1); l <= 7 && l <= (ushort)(activebombs[k].i + 1); l++)
                    {
                        if (l < 0) continue;
                        ushort down = (ushort)(activebombs[k].j - 1);
                        if (down < 0) down = 0;
                        ushort up = (ushort)(activebombs[k].j + 1);
                        if (up > 7) up = 7;
                        DestroyLineJ(l, down, up);
                    }
                if (activebombs[k].tickcounter > 0)
                    activebombs[k].tickcounter--;
            }
            //reapers spawn
            for (ushort k = 0; k < 3; k++)
            {
                if (activereapers[k].tickcounter == 1)
                {
                    int tickcount = (int)(elmdx / delta);
                    //Spawn reaper
                    DestroyElm(activereapers[k].i, activereapers[k].j);
                    if (activereapers[k].type == 1)
                    {
                        score++;
                        if (activereapers[k].i - 1 >= 0)
                        {
                            //to do: this element destroed too!
                            Elms[activereapers[k].i - 1, activereapers[k].j] = new zElm(5, activereapers[k].type);
                            Elms[activereapers[k].i - 1, activereapers[k].j].StartAnimation((activereapers[k].i, activereapers[k].j), tickcount, -delta, true);
                        }
                        if (activereapers[k].i + 1 <= 7)
                        {
                            Elms[activereapers[k].i + 1, activereapers[k].j] = new zElm(5, activereapers[k].type);
                            Elms[activereapers[k].i + 1, activereapers[k].j].StartAnimation((activereapers[k].i, activereapers[k].j), tickcount, delta, true);
                        }
                        MakeGravityJ(activereapers[k].i, activereapers[k].j);
                    }
                    if (activereapers[k].type == 2)
                    {
                        score++;
                        if (activereapers[k].j - 1 >= 0)
                        {
                            Elms[activereapers[k].i, activereapers[k].j - 1] = new zElm(5, activereapers[k].type);
                            Elms[activereapers[k].i, activereapers[k].j - 1].StartAnimation((activereapers[k].i, activereapers[k].j), tickcount, delta, false);
                        }
                        if (activereapers[k].j + 1 <= 7)
                        {
                            Elms[activereapers[k].i, activereapers[k].j + 1] = new zElm(5, activereapers[k].type);
                            Elms[activereapers[k].i, activereapers[k].j + 1].StartAnimation((activereapers[k].i, activereapers[k].j), tickcount, -delta, false);
                        }
                    }

                }
                if (activereapers[k].tickcounter > 0)
                    activereapers[k].tickcounter--;
            }

            if (!animationexist && waittomatch >= 60)
            {
                GlobalCalcMatch();
                waittomatch = 0;
            }
            else
                waittomatch++;

            /*var transform = Matrix4.Identity;
            //transform = transform * Matrix4.CreateTranslation(-0.85f - danimation, 0.85f, 0.0f);
            
            _texture.Use(TextureUnit.Texture0);
            _shader.Use();
            _shader.SetMatrix4("transform", transform);*/

            // System.IntPtr ar;
            // GL.GetVertexAttribPointer(0, VertexAttribPointerParameter.ArrayPointer ,  ar);


            // if (! gone) GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            /*   transform = Matrix4.Identity;
               transform = transform * Matrix4.CreateTranslation(0.6f, 0.5f - danimation, 0.0f);

               _texture2.Use(TextureUnit.Texture0);
               _shader.Use();
               _shader.SetMatrix4("transform", transform);
               if (!gone) GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);*/
            System.TimeSpan dt = System.DateTime.Now - dtstarted;
            int sec = 60 - (int)dt.TotalSeconds;
            DrawText("score:" + score + "         " + sec.ToString());
            DrawControl(0.720f, 0.25f, 5.7f, 1f, "time");
            DrawControl(0.658f, -0.627f, 7.28f, 7.28f, "field");
            if (sec == 0)
                gameMode = 2;
        }
        // This function runs on every update frame.
        // Now that initialization is done, let's create our render loop.
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendColor(0.7f, 0.7f, 0.7f, 0.5f);
            GL.BindVertexArray(_vertexArrayObject);
            if(gameMode==1)
                GameThink();
            else if(gameMode==0)
                DrawControl(0.540f, -0.35f, 5.7f, 1f, "play");
            else
                DrawControl(0.665f, -0.35f, 5.7f, 1f, "gameover");
            //DrawControlObject((0.4f,0.25f), "s");
            //DrawControlObject((0.48f, 0.25f), "c");

            SwapBuffers();

            //danimation += 0.001f;

            base.OnRenderFrame(e);
        }
        void TestSetElmsI(int i1, int i2, int j, ushort type)
        {
            for (int k=i1;k<=i2;k++)
                Elms[k, j].elmtype = type;
        }
        void TestSetElmsJ(int j1, int j2, int i, ushort type)
        {
           for (int k = j1; k <= j2; k++)
                Elms[i, k].elmtype = type;
        }
        void ExchangeElms(int i1, int j1, int i2, int j2)
        {
            zElm t = Elms[i1, j1];
            Elms[i1, j1] = Elms[i2, j2];
            Elms[i2, j2] = t;
        }
        int test_keydelay;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // Check if the Escape button is currently being pressed.
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                // If it is, close the window.
                Close();
            }
            
            if (KeyboardState.IsKeyDown(Keys.F1) && test_keydelay==0)
            {
                for(ushort k=0;k<=63;k++)
                {
                    ushort i = (ushort)(k % 8);
                    ushort j = (ushort)(k / 8);
                    if (i % 2 == j%2)
                        Elms[i, j].elmtype = 0;
                    else
                        Elms[i, j].elmtype = 1;
                }
                ushort type = ChoiceRandomFromRangeExceptOne(0,1);

                TestSetElmsI(3, 7, 7, type);
                ExchangeElms(5, 7, 5, 6);

                TestSetElmsI(1, 3, 0, type);
                TestSetElmsJ(0, 2, 1, type);
                ExchangeElms(0, 0, 1, 0);

                TestSetElmsJ(3, 6, 4, type);
                ExchangeElms(4, 5, 3, 5);
                TestSetElmsI(5, 7, 5, type);
                ExchangeElms(7, 5, 7, 4);

               test_keydelay = 40;
            }
            if (KeyboardState.IsKeyDown(Keys.F2) && test_keydelay == 0)
            {
                for (ushort k = 0; k <= 3; k++)
                {
                    ushort i = (ushort)rand.Next(0, 8);
                    ushort j = (ushort)rand.Next(0, 8);
                    
                    Elms[i, j].IsBomb = true;
                   
                    Elms[i, j].ReaperType = (ushort)rand.Next(1, 3);

                }
                test_keydelay = 40;
            }
            if (KeyboardState.IsKeyDown(Keys.F3) && test_keydelay == 0)
            {
                for (ushort k = 0; k <= 3; k++)
                {
                    ushort i = (ushort)rand.Next(0, 8);
                    ushort j = (ushort)rand.Next(0, 8);

                    
                    Elms[i, j].ReaperType = (ushort)rand.Next(1, 3);

                }
                test_keydelay = 40;
            }
            if (KeyboardState.IsKeyReleased(Keys.Space))
            {
                
            }
            if (test_keydelay > 0) test_keydelay--;
            base.OnUpdateFrame(e);
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            // When the window gets resized, we have to call GL.Viewport to resize OpenGL's viewport to match the new size.
            // If we don't, the NDC will no longer be correct.
            GL.Viewport(0, 0, Size.X, Size.Y);
            base.OnResize(e);
        }

        // Now, for cleanup. This isn't technically necessary since C# and OpenGL will clean up all resources automatically when
        // the program closes, but it's very important to know how anyway.
        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);

            GL.DeleteProgram(_shader.Handle);
            // Don't forget to dispose of the texture too!
            foreach (var txtr in _texture)
                GL.DeleteTexture(txtr.Handle);
            
            base.OnUnload();
        }

        //public static void DrawCircle(float x, float y, float radius, Color4 c)
        //{
            //GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            //GL.Begin(PrimitiveType.TriangleFan);
            
            //GL.Color4(c);

            //GL.Vertex2(x, y);
          //  for (int i = 0; i < 360; i++)
            //{
              //  GL.Vertex2(x + Math.Cos(i) * radius, y + Math.Sin(i) * radius);
            //}

            //GL.End();
            //GL.Disable(EnableCap.Blend);
        //}

        bool mouseDown, mouseclick, locked;
        Vector2? newPos, initialPos;

       /* protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (MouseState.IsButtonDown(MouseButton.Left))
            {
                mouseDown = true;
                newPos = new Vector2(MouseState.X, MouseState.Y);
            }
            base.OnMouseMove(e);
        }*/

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            
            if (e.Button == MouseButton.Left)
            {
                if (locked)
                {
                    mouseclick = !mouseclick;
                }
            }

            //if(initialPos is null)
            initialPos = new Vector2(MouseState.X, MouseState.Y);
            

            base.OnMouseDown(e);
        }
        private (float,float) CalcNDCByMouse(float x, float y)
        {
            float nX = -1f + 2f * x/ this.ClientSize.X;
            float nY = 1f - 2f * y / this.ClientSize.Y;
            return (nX, nY);
        }
        private (short, short) CalcGFieldijByNDC((float, float) xy)
        {
            short ielm = (short)((xy.Item1 - (-0.1f - 0.661f + 0.02f)) / 0.18f);
            short jelm = (short)((-xy.Item2 + (0.1f + 0.6287f - 0.02f)) / 0.18f);
            return (ielm, jelm);
        }
        (ushort, ushort) toushortuple((short, short) elm) => ((ushort) elm.Item1, (ushort) elm.Item2);
        void StartExchangeAnimation((short,short) elm1,(short,short) elm2, bool fake)
        {
            int tickcount = (int)(elmdx / delta);
            int ry = elm2.Item2 - elm1.Item2;
            int rx = elm2.Item1 - elm1.Item1;
            int sign = ry * ry == 1 || rx==0 ? ry : -rx;
            Elms[elm1.Item1, elm1.Item2].StartAnimation(toushortuple(elm1), tickcount, -delta * sign, ry == 0, false);
            Elms[elm2.Item1, elm2.Item2].StartAnimation(toushortuple(elm2), tickcount, delta * sign, ry == 0, fake);
            //exchange
            zElm t = Elms[elm1.Item1, elm1.Item2];
            Elms[elm1.Item1, elm1.Item2] = Elms[elm2.Item1, elm2.Item2];
            Elms[elm2.Item1, elm2.Item2] = t;
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (initialPos is null) //may be down out of window
            { //to do: check down and up at one field
               base.OnMouseUp(e);
                return;
            }
            //process mouse events
            
            (float, float) ndcXY = CalcNDCByMouse(initialPos.Value.X, initialPos.Value.Y);
            (short, short) Elmijorg = CalcGFieldijByNDC(ndcXY);

            ndcXY = CalcNDCByMouse(MouseState.X, MouseState.Y);
            (short, short) Elmij = CalcGFieldijByNDC(ndcXY);

            if(gameMode==0)
            {
                if (ndcXY.Item1 <= -0.522 && ndcXY.Item1 >= -0.8375 && ndcXY.Item2 >= 0.119 && ndcXY.Item2 <= 0.38)
                {
                    dtstarted = System.DateTime.Now;
                    gameMode = 1;
                    GenerateField();
                    score = 0;
                    for(ushort k=0;k<3;k++)
                    {
                        activebombs[k].tickcounter = 0;
                        activereapers[k].tickcounter = 0;
                    }
                }
            }
            else if(gameMode == 2)
            {
                if (ndcXY.Item1 >= -0.334 && ndcXY.Item1 <= 0.4375 && ndcXY.Item2 >= 0.203 && ndcXY.Item2 <= 0.3533)
                {
                    gameMode = 0;
                }
            }
            else if (gameMode == 1 && Elmij.Item1 >= 0 && Elmij .Item2>= 0 && Elmij.Item1 <= 7 && Elmij.Item2 <= 7 && Elmij != ichosenelm && Elmij== Elmijorg) //if click in field and previos selected i,j is different (-1,-1 or other elm) 
            {
                int ry = ichosenelm.Item2 - Elmij.Item2;
                int rx = ichosenelm.Item1 - Elmij.Item1;
                if (ichosenelm != (-1, -1) && ((ichosenelm.Item1 == Elmij.Item1 && ry * ry == 1) || (ichosenelm.Item2 == Elmij.Item2 && rx * rx == 1))) //is real and other elm
                {
                    //Elms[ichosenelm.Item1, ichosenelm.Item2].rotangle = -1f;
                    StartExchangeAnimation(Elmij, ichosenelm, true);
                    ichosenelm = (-1, -1);
                }
                else //that first elm is not neighbor ergo this other choice
                {
                   //if(ichosenelm != (-1, -1))
                   //     Elms[ichosenelm.Item1, ichosenelm.Item2].rotangle = -1f;
                    ichosenelm = Elmij;
                    Elms[Elmij.Item1, Elmij.Item2].rotangle = 0;
                }
            }

            //end process mouse events
            initialPos = null;
            base.OnMouseUp(e);
        }

    /*    public void DrawRectangle(Vector2 oldPos, Vector2 newPos)
        {
            float[] rec = new float[] { oldPos.X, oldPos.Y, newPos.X, oldPos.Y, newPos.X, newPos.Y, oldPos.X, newPos.Y };
            byte[] indices = new byte[] { 0, 2, 1, 0, 2, 3 };
            int VertexBufferCursor, _vao, VBO;
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count() * sizeof(byte), indices, BufferUsageHint.StaticDraw);
            VertexBufferCursor = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferCursor);
            GL.BufferData(BufferTarget.ArrayBuffer, rec.Count() * sizeof(float), rec, BufferUsageHint.StaticDraw);
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.BindVertexArray(0);
            shader3.Use();
            Matrix4 model = Matrix4.Identity;
            Matrix4 projectionM = Matrix4.CreateScale(new Vector3(1f / this.Width, 1f / this.Height, 1.0f));
            projectionM = Matrix4.CreateOrthographicOffCenter(0.0f, this.Width, this.Height, 0.0f, -1.0f, 1.0f);
            GL.UniformMatrix4(0, false, ref model);
            GL.UniformMatrix4(1, false, ref projectionM);
            shader3.SetFloat("color", new Vector4(0.0f, 1.0f, 1.0f, 1.0f));
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedByte, indices);
            GL.BindVertexArray(0);
            shader3.Unbind();
        }*/

    }
}
