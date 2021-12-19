using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace MoreWorldOptions.UI
{
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