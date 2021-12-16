using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace MoreWorldOptions
{
    public class MoreWorldOptions : Mod
    {
        public static MoreWorldOptions Instance;

        public MoreWorldOptions() { Instance = this; }

        public override void Load()
        {
            base.Load();
            MenuPatcher.ApplyPatch();
            WorldGenPatches.ApplyPatches();

            On.Terraria.IO.WorldFileData.SetWorldSize += WorldFileData_SetWorldSize;
        }

        public override void Unload()
        {
            Instance = null;
            base.Unload();
            MenuPatcher.RemovePatch();
            WorldGenPatches.RemovePatches();
            GeneratorPatches.RemovePatches();

            On.Terraria.IO.WorldFileData.SetWorldSize -= WorldFileData_SetWorldSize;
        }

        public static bool DrawMenuHook(
            Main main,
            int selectedMenu,
            string[] buttonNames,
            float[] buttonScales,
            int[] buttonVerticalSpacing,
            bool[] textOnlyButtons,
            ref int offY,
            ref int spacing,
            ref int numButtons,
            ref bool backButtonDown)
        {
            if (Main.menuMode == 16)
            {
                offY = 200;
                spacing = 60;
                buttonVerticalSpacing[1] = 30;
                buttonVerticalSpacing[2] = 30;
                buttonVerticalSpacing[3] = 30;
                buttonVerticalSpacing[4] = 30;
                buttonVerticalSpacing[5] = 70;
                textOnlyButtons[0] = true;
                buttonNames[0] = Language.GetTextValue("LegacyMenu.91");
                buttonNames[1] = Language.GetTextValue("LegacyMenu.92");
                buttonNames[2] = Language.GetTextValue("LegacyMenu.93");
                buttonNames[3] = Language.GetTextValue("LegacyMenu.94");
                buttonNames[4] = Language.ActiveCulture == GameCulture.Russian ? "Настраиваемый" : "Custom";
                buttonNames[5] = Language.GetTextValue("LegacyMenu.5");
                numButtons = 6;
                if (selectedMenu == 5 || backButtonDown)
                {
                    backButtonDown = false;
                    Main.menuMode = 6;
                    Main.PlaySound(SoundID.MenuClose, -1, -1, 1, 1f, 0f);
                }
                else if (selectedMenu == 4)
                {
                    Main.MenuUI.SetState(new UICustomWorld());
                    Main.PlaySound(SoundID.MenuOpen, -1, -1, 1, 1f, 0f);
                    Main.menuMode = 888;
                }
                else if (selectedMenu > 0)
                {
                    switch (selectedMenu)
                    {
                        case 1:
                            Main.maxTilesX = 4200;
                            Main.maxTilesY = 1200;
                            break;
                        case 2:
                            Main.maxTilesX = 6400;
                            Main.maxTilesY = 1800;
                            break;
                        case 3:
                            Main.maxTilesX = 8400;
                            Main.maxTilesY = 2400;
                            break;
                    }
                    Main.clrInput();
                    Main.menuMode = -7;
                    Main.PlaySound(SoundID.MenuOpen, -1, -1, 1, 1f, 0f);
                    WorldGen.setWorldSize();
                }
                return true;
            }
            return false;
        }

        public static void ResizeWorld()
        {
            int right = (Main.maxTilesX + 1) * 16;
            int bottom = (Main.maxTilesY + 1) * 16;
            if (right > Main.rightWorld || bottom > Main.bottomWorld)
            {
                Main.rightWorld = right;
                Main.topWorld = 0f;
                Main.bottomWorld = bottom;
                Main.maxSectionsX = Main.maxTilesX / 200;
                Main.maxSectionsY = Main.maxTilesY / 150;
                Main.Map = new WorldMap(Main.maxTilesX, Main.maxTilesY);
                Main.tile = new Tile[Main.maxTilesX, Main.maxTilesY];

                Main.mapTargetX = (int)Math.Ceiling((float)Main.maxTilesX / Main.textureMaxWidth) + 1;
                Main.mapTargetY = (int)Math.Ceiling((float)Main.maxTilesY / Main.textureMaxHeight) + 1;

                Main.initMap = new bool[Main.mapTargetX, Main.mapTargetY];
                Main.mapWasContentLost = new bool[Main.mapTargetX, Main.mapTargetY];

                for (int i = 0; i < Main.instance.mapTarget.GetLength(0); i++)
                    for (int j = 0; j < Main.instance.mapTarget.GetLength(1); j++)
                        if (Main.instance.mapTarget[i, j] != null && !Main.instance.mapTarget[i, j].IsDisposed)
                            Main.instance.mapTarget[i, j].Dispose();

                Main.instance.mapTarget = new RenderTarget2D[Main.mapTargetX, Main.mapTargetY];

                typeof(WorldGen).GetField("lastMaxTilesX", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, 0);
                typeof(WorldGen).GetField("lastMaxTilesY", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, 0);
            }
        }

        private void WorldFileData_SetWorldSize(On.Terraria.IO.WorldFileData.orig_SetWorldSize orig, Terraria.IO.WorldFileData self, int x, int y)
        {
            orig(self, x, y);

            if ((x == 4200 && y == 1200)
                || (x == 6400 && y == 1800)
                || (x == 8400 && y == 2400)) return;

            self._worldSizeName = (LocalizedText)Activator.CreateInstance(
                typeof(LocalizedText), 
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { "", $"{x} x {y}" },
                null);
        }

    }

    class UICustomWorld : UIState
    {
        private readonly static Regex NotDigits = new Regex("[^0-9]*", RegexOptions.Compiled);

        private Color TypeDark = new Color(0, 0x80, 0);
        private Color TypeDarkOver = new Color(0, 0x90, 0);

        private Color TypeSelected = new Color(0, 0xDD, 0);
        private Color TypeSelectedOver = new Color(0, 0xFF, 0);

        private Selected _type;
        public Selected Type
        {
            get => _type;
            set
            {
                if (_type == value) return;
                _type = value;
                Main.PlaySound(SoundID.MenuTick);
                Small.BackgroundColor = value == Selected.Small ? TypeSelectedOver : TypeDark;
                Medium.BackgroundColor = value == Selected.Medium ? TypeSelectedOver : TypeDark;
                Big.BackgroundColor = value == Selected.Big ? TypeSelectedOver : TypeDark;
                Custom.BackgroundColor = value == Selected.Custom ? TypeSelectedOver : TypeDark;

                if (value == Selected.Custom) return;

                WWidth.CurrentString = Main.maxTilesX.ToString();
                WHeight.CurrentString = Main.maxTilesY.ToString();
            }
        }

        UIAutoScaleTextTextPanel<string> Small, Medium, Big, Custom;
        UIFocusInputTextField WWidth, WHeight;

        UIPanel Panel = new UIPanel()
        {
            Top = new StyleDimension() { Percent = .5f, Pixels = -100 },
            Left = new StyleDimension() { Percent = .5f, Pixels = -277 },
            Width = new StyleDimension() { Pixels = 555 },
            Height = new StyleDimension() { Pixels = 200 }
        };

        public UICustomWorld()
        {
            Main.maxTilesX = 4200;
            Main.maxTilesY = 1200;

            Append(Panel);

            UIAutoScaleTextTextPanel<string> back = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("LegacyMenu.5"))
            {
                Top = new StyleDimension() { Pixels = 130 },
                Left = new StyleDimension() { Pixels = 8 },
                Width = new StyleDimension() { Pixels = 120 },
                Height = new StyleDimension() { Pixels = 40 },
            };
            back.WithFadedMouseOver();
            back.OnClick += (@event, ui) =>
            {
                Main.PlaySound(SoundID.MenuClose, -1, -1, 1, 1f, 0f);
                Main.menuMode = 16;
            };

            UIAutoScaleTextTextPanel<string> done = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("LegacyMenu.28"))
            {
                Top = new StyleDimension() { Pixels = 130 },
                Left = new StyleDimension() { Pixels = 404 },
                Width = new StyleDimension() { Pixels = 120 },
                Height = new StyleDimension() { Pixels = 40 },
            };
            done.WithFadedMouseOver();
            done.OnClick += (@event, ui) =>
            {
                Main.maxTilesX = int.Parse(WWidth.CurrentString);
                Main.maxTilesY = int.Parse(WHeight.CurrentString);

                MoreWorldOptions.ResizeWorld();

                Main.clrInput();
                Main.menuMode = -7;
                Main.PlaySound(SoundID.MenuOpen, -1, -1, 1, 1f, 0f);
                WorldGen.setWorldSize();
            };

            Small = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("LegacyMenu.92"))
            {
                Top = new StyleDimension() { Pixels = 70 },
                Left = new StyleDimension() { Pixels = 8 },
                Width = new StyleDimension() { Pixels = 120 },
                Height = new StyleDimension() { Pixels = 40 },
                BackgroundColor = TypeSelected
            };
            SetupMoseOver(Small, Selected.Small);
            Small.OnClick += (@event, ui) =>
            {
                Main.maxTilesX = 4200;
                Main.maxTilesY = 1200;
                Type = Selected.Small;
            };

            Medium = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("LegacyMenu.93"))
            {
                Top = new StyleDimension() { Pixels = 70 },
                Left = new StyleDimension() { Pixels = 140 },
                Width = new StyleDimension() { Pixels = 120 },
                Height = new StyleDimension() { Pixels = 40 },
                BackgroundColor = TypeDark
            };
            SetupMoseOver(Medium, Selected.Medium);
            Medium.OnClick += (@event, ui) =>
            {
                Main.maxTilesX = 6400;
                Main.maxTilesY = 1800;
                Type = Selected.Medium;
            };

            Big = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("LegacyMenu.94"))
            {
                Top = new StyleDimension() { Pixels = 70 },
                Left = new StyleDimension() { Pixels = 272 },
                Width = new StyleDimension() { Pixels = 120 },
                Height = new StyleDimension() { Pixels = 40 },
                BackgroundColor = TypeDark
            };
            SetupMoseOver(Big, Selected.Big);
            Big.OnClick += (@event, ui) =>
            {
                Main.maxTilesX = 8400;
                Main.maxTilesY = 2400;
                Type = Selected.Big;
            };

            Custom = new UIAutoScaleTextTextPanel<string>(Language.ActiveCulture == GameCulture.Russian ? "Настраиваемый" : "Custom")
            {
                Top = new StyleDimension() { Pixels = 70 },
                Left = new StyleDimension() { Pixels = 404 },
                Width = new StyleDimension() { Pixels = 120 },
                Height = new StyleDimension() { Pixels = 40 },
                BackgroundColor = TypeDark
            };
            SetupMoseOver(Custom, Selected.Custom);
            Custom.OnClick += (@event, ui) =>
            {
                Type = Selected.Custom;
            };

            UIPanel wpanel = new UIPanel()
            {
                Top = new StyleDimension(28, 0),
                Left = new StyleDimension(170, 0),
                Width = new StyleDimension(80, 0),
                Height = new StyleDimension(30, 0),
                BackgroundColor = new Color(50, 80, 170)
            };
            UIPanel hpanel = new UIPanel()
            {
                Top = new StyleDimension(28, 0),
                Left = new StyleDimension(283, 0),
                Width = new StyleDimension(80, 0),
                Height = new StyleDimension(30, 0),
                BackgroundColor = new Color(50, 80, 170)
            };
            wpanel.SetPadding(0);
            hpanel.SetPadding(0);

            WWidth = new UIFocusInputTextField("Width")
            {
                Top = new StyleDimension(5, 0),
                Left = new StyleDimension(10, 0),
                Width = new StyleDimension(-20, 1),
                Height = new StyleDimension(20, 0),
            };

            WWidth.OnUnfocus += (s, e) =>
            {
                int w = int.Parse(WWidth.CurrentString);
                w = (int)Math.Ceiling(w / 200f) * 200;
                WWidth.CurrentString = w.ToString();
            };

            WHeight = new UIFocusInputTextField("Height")
            {
                Top = new StyleDimension(5, 0),
                Left = new StyleDimension(10, 0),
                Width = new StyleDimension(-20, 1),
                Height = new StyleDimension(20, 0),
            };

            WHeight.OnUnfocus += (s, e) =>
            {
                int h = int.Parse(WHeight.CurrentString);
                h = (int)Math.Ceiling(h / 150f) * 150;
                WHeight.CurrentString = h.ToString();
            };

            DigitsOnly(WWidth, 6);
            DigitsOnly(WHeight, 6);

            WWidth.CurrentString = Main.maxTilesX.ToString();
            WHeight.CurrentString = Main.maxTilesY.ToString();

            Panel.Append(Small);
            Panel.Append(Medium);
            Panel.Append(Big);
            Panel.Append(Custom);


            wpanel.Append(WWidth);
            hpanel.Append(WHeight);
            Panel.Append(wpanel);
            Panel.Append(hpanel);

            Panel.Append(done);
            Panel.Append(back);

            Panel.Append(new UIText("x") { Top = new StyleDimension(34, 0), Left = new StyleDimension(263, 0) });
            Panel.Append(new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("LegacyMenu.91"), 1.2f)
            {
                Top = new StyleDimension(-30, 0),
                Left = new StyleDimension(-160, .5f),
                Width = new StyleDimension(320, 0),
                Height = new StyleDimension(40, 0),
                BackgroundColor = new Color(50, 70, 140)
            });
        }

       

        private void SetupMoseOver(UIPanel ui, Selected type)
        {
            ui.OnMouseOver += delegate
            {
                Main.PlaySound(SoundID.MenuTick);
                ui.BackgroundColor = Type == type ? TypeSelectedOver : TypeDarkOver;
            };
            ui.OnMouseOut += delegate
            {
                ui.BackgroundColor = Type == type ? TypeSelected : TypeDark;
            };
        }

        private void DigitsOnly(UIFocusInputTextField inputTextField, int maxLength)
        {
            inputTextField.OnTextChange += (s, e) =>
            {
                Type = Selected.Custom;
                string text = NotDigits.Replace(inputTextField.CurrentString, "");
                if (text.Length > maxLength) text = text.Substring(0, maxLength);
                inputTextField.CurrentString = text;
            };
        }

        public enum Selected
        {
            Small, Medium, Big, Custom
        }
    }

    // Thanks TML Team for making this class internal in Terraria.ModLoader.UI
    public class UIFocusInputTextField : UIElement
    {
        public bool UnfocusOnTab { get; internal set; }

        public event EventHandler OnTextChange;

        public event EventHandler OnUnfocus;

        public event EventHandler OnTab;

        public UIFocusInputTextField(string hintText)
        {
            _hintText = hintText;
        }

        public void SetText(string text)
        {
            if (text == null)
            {
                text = "";
            }
            if (CurrentString != text)
            {
                CurrentString = text;
                OnTextChange?.Invoke(this, new EventArgs());
            }
        }

        public override void Click(UIMouseEvent evt)
        {
            Main.clrInput();
            Focused = true;
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 point = new Vector2(Main.mouseX, Main.mouseY);
            if (!ContainsPoint(point) && Main.mouseLeft && Focused)
            {
                Focused = false;
                OnUnfocus?.Invoke(this, new EventArgs());
            }
            base.Update(gameTime);
        }

        private static bool JustPressed(Keys key)
        {
            return Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (Focused)
            {
                PlayerInput.WritingText = true;
                Main.instance.HandleIME();
                string inputText = Main.GetInputText(CurrentString);
                if (!inputText.Equals(CurrentString))
                {
                    CurrentString = inputText;
                    OnTextChange?.Invoke(this, new EventArgs());
                }
                else
                {
                    CurrentString = inputText;
                }
                if (JustPressed(Keys.Tab))
                {
                    if (UnfocusOnTab)
                    {
                        Focused = false;
                        OnUnfocus?.Invoke(this, new EventArgs());
                    }
                    OnTab?.Invoke(this, new EventArgs());
                }
                int num = _textBlinkerCount + 1;
                _textBlinkerCount = num;
                if (num >= 20)
                {
                    _textBlinkerState = (_textBlinkerState + 1) % 2;
                    _textBlinkerCount = 0;
                }
            }
            string text = CurrentString;
            if (_textBlinkerState == 1 && Focused)
            {
                text += "|";
            }
            CalculatedStyle dimensions = GetDimensions();
            if (CurrentString.Length == 0 && !Focused)
            {
                Utils.DrawBorderString(spriteBatch, _hintText, new Vector2(dimensions.X, dimensions.Y), Color.Gray, 1f, 0f, 0f, -1);
                return;
            }
            Utils.DrawBorderString(spriteBatch, text, new Vector2(dimensions.X, dimensions.Y), Color.White, 1f, 0f, 0f, -1);
        }

        internal bool Focused;

        internal string CurrentString = "";

        private readonly string _hintText;

        private int _textBlinkerCount;

        private int _textBlinkerState;

        public delegate void EventHandler(object sender, EventArgs e);
    }
}