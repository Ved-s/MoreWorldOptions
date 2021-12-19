using Microsoft.Xna.Framework;
using System;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace MoreWorldOptions.UI
{
    public class UICustomWorld : UIState
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
}