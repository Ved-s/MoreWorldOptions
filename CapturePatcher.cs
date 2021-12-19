using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Terraria;
using Terraria.Graphics.Capture;

namespace MoreWorldOptions
{
    internal class CapturePatcher
    {
        static object Lock = new object();
        static CaptureSettings Settings;

        static uint TilesProcessed, TotalTiles;

        static Stack<(Rectangle, Rectangle)> RenderStack = new Stack<(Rectangle, Rectangle)>();

        delegate bool orig_IsCapturing(object self);
        delegate bool hook_IsCapturing(orig_IsCapturing orig, object self);

        static MethodInfo get_IsCapturing = typeof(Main).Assembly
            .GetTypes().First(t => t.Name == "CaptureCamera")
            .GetProperty("IsCapturing")
            .GetGetMethod();

        static RenderTarget2D RenderTarget;
        static RenderTarget2D BufferTarget;

        static System.Drawing.Graphics Graphics;
        static System.Drawing.Bitmap Bitmap;

        static System.Drawing.Bitmap Buffer;
        static uint[] BufferData;

        static bool Scaled = false;

        public static void ApplyPatch()
        {
            On.Terraria.Graphics.Capture.CaptureCamera.Capture += CaptureCamera_Capture;
            On.Terraria.Graphics.Capture.CaptureCamera.GetProgress += CaptureCamera_GetProgress;
            On.Terraria.Graphics.Capture.CaptureCamera.DrawTick += CaptureCamera_DrawTick;

            IL.Terraria.Graphics.Capture.CaptureInterface.DrawCameraLock += CaptureInterface_DrawCameraLock;

            HookEndpointManager.Add(get_IsCapturing, (hook_IsCapturing)CaptureCamera_IsCapturing);
        }
        public static void RemovePatch()
        {
            On.Terraria.Graphics.Capture.CaptureCamera.Capture -= CaptureCamera_Capture;
            On.Terraria.Graphics.Capture.CaptureCamera.GetProgress -= CaptureCamera_GetProgress;
            On.Terraria.Graphics.Capture.CaptureCamera.DrawTick -= CaptureCamera_DrawTick;

            IL.Terraria.Graphics.Capture.CaptureInterface.DrawCameraLock -= CaptureInterface_DrawCameraLock;

            HookEndpointManager.Remove(get_IsCapturing, (hook_IsCapturing)CaptureCamera_IsCapturing);
        }

        private static void CaptureInterface_DrawCameraLock(MonoMod.Cil.ILContext il)
        {
            int pos = Util.FindNextInstruction(il,
                x => x.MatchLdsfld<CaptureInterface>("CameraWaiting"),
                x => x.MatchLdcR4(60),
                x => x.MatchSub());

            if (pos == -1) return;

            il.Instrs.RemoveAt(pos + 1);
            il.Instrs.RemoveAt(pos + 1);
        }
        private static void CaptureCamera_DrawTick(On.Terraria.Graphics.Capture.CaptureCamera.orig_DrawTick orig, object self)
        {
            Monitor.Enter(Lock);
            if (Settings == null)
            {
                return;
            }
            if (RenderStack.Count > 0)
            {
                (Rectangle area, Rectangle scaled) = RenderStack.Pop();

                int w = Scaled ? scaled.Width : area.Width * 16;
                int h = Scaled ? scaled.Height : area.Height * 16;

                int l = w * h;

                if (BufferData == null || BufferData.Length != l)
                {
                    if (Buffer != null) Buffer.Dispose();
                    Buffer = new System.Drawing.Bitmap(w, h);

                    if (RenderTarget != null) RenderTarget.Dispose();
                    RenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, area.Width * 16, area.Height * 16, false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

                    if (Scaled)
                    {
                        if (BufferTarget != null) BufferTarget.Dispose();
                        BufferTarget = new RenderTarget2D(Main.instance.GraphicsDevice, scaled.Width, scaled.Height, false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
                    }
                    BufferData = new uint[l];
                }

                Main.instance.GraphicsDevice.SetRenderTarget(RenderTarget);
                Main.instance.GraphicsDevice.Clear(Color.Transparent);
                Main.instance.DrawCapture(area, Settings);
                Main.instance.GraphicsDevice.SetRenderTarget(null);

                if (Scaled) 
                {
                    Main.instance.GraphicsDevice.SetRenderTarget(BufferTarget);
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone);
                    Main.spriteBatch.Draw(RenderTarget, new Rectangle(0, 0, scaled.Width, scaled.Height), Color.White);
                    Main.spriteBatch.End();
                    Main.instance.GraphicsDevice.SetRenderTarget(null);
                    BufferTarget.GetData(BufferData);
                }
                else RenderTarget.GetData(BufferData);

                BitmapData bits = Buffer.LockBits(new System.Drawing.Rectangle(0, 0, Buffer.Width, Buffer.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                unsafe
                {
                    uint* ptr = (uint*)bits.Scan0;
                    for (int i = 0; i < BufferData.Length; i++)
                    {
                        uint v = BufferData[i];
                        ptr[i] = v & 0xff00ff00 | (v & 0xff) << 16 | (v & 0xff0000) >> 16;
                    }
                }

                Buffer.UnlockBits(bits);
                Graphics.DrawImage(Buffer, scaled.X, scaled.Y);
                TilesProcessed += (uint)area.Width * (uint)area.Height;
            }
            if (RenderStack.Count == 0)
            {
                Graphics.Dispose();

                string path = string.Concat(new string[]
                    {
                        Main.SavePath,
                        Path.DirectorySeparatorChar.ToString(),
                        "Captures",
                        Path.DirectorySeparatorChar.ToString(),
                        Settings.OutputName,
                        ".png"
                    });

                Bitmap.Save(path, ImageFormat.Png);
                Settings = null;
                Bitmap.Dispose();
                Bitmap = null;
                BufferData = null;

                Main.GlobalTimerPaused = false;
                CaptureInterface.EndCamera();

            }
            Monitor.Exit(Lock);
        }
        private static bool CaptureCamera_IsCapturing(orig_IsCapturing orig, object self)
        {
            return Settings != null;
        }
        private static float CaptureCamera_GetProgress(On.Terraria.Graphics.Capture.CaptureCamera.orig_GetProgress orig, object self)
        {
            return (float)TilesProcessed / TotalTiles;
        }
        private static void CaptureCamera_Capture(On.Terraria.Graphics.Capture.CaptureCamera.orig_Capture orig, object self, Terraria.Graphics.Capture.CaptureSettings settings)
        {
            const int imageSize = 27000;

            Main.GlobalTimerPaused = true;
            Monitor.Enter(Lock);
            if (Settings != null)
            {
                throw new InvalidOperationException("Capture called while another capture was already active.");
            }
            Settings = settings;
            Rectangle area = settings.Area;
            float scale = 1f;

            int w = area.Width * 16;
            int h = area.Height * 16;

            scale = Math.Min(1f, scale);

            if (w > imageSize || h > imageSize)
            {
                if (w > h)
                {
                    scale = (float)imageSize / w;
                    w = imageSize;
                    h = (int)(h * scale);
                }
                else 
                {
                    scale = (float)imageSize / h;
                    h = imageSize;
                    w = (int)(w * scale);
                }
                Scaled = true;
            }
            else Scaled = false;


            Bitmap = new System.Drawing.Bitmap(w, h, PixelFormat.Format24bppRgb);
            Graphics = System.Drawing.Graphics.FromImage(Bitmap);

            TilesProcessed = 0;
            TotalTiles = (uint)area.Width * (uint)area.Height;
            for (int i = area.X; i < area.X + area.Width; i += 254)
            {
                for (int j = area.Y; j < area.Y + area.Height; j += 254)
                {
                    int chunkW = Math.Min(256, area.X + area.Width - i);
                    int chunkH = Math.Min(256, area.Y + area.Height - j);
                    int width = (int)(scale * chunkW * 16);
                    int height = (int)(scale * chunkH * 16);
                    int x = (int)(scale * (i - area.X) * 16);
                    int y = (int)(scale * (j - area.Y) * 16);
                    RenderStack.Push((new Rectangle(i, j, chunkW, chunkH), new Rectangle(x, y, width + 1, height + 1)));
                }
            }
            Monitor.Exit(Lock);
        }


    }
}
