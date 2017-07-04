using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using ImageSharp;
using ImageSharp.Drawing.Brushes;
using SixLabors.Fonts;
using SixLabors.Primitives;

namespace Sora_Bot_1.SoraBot.Services.EPService
{
    public static class ProfileImageProcessing
    {
        private static FontCollection _fontCollection;
        private static FontFamily _titleFont;

        private static Image<Rgba32> _bgMaskImage;
        private static Image<Rgba32> _noBgMask;
        private static Image<Rgba32> _noBgMaskOverlay;

        public static void Initialize()
        {
            _fontCollection = new FontCollection();
            _titleFont = _fontCollection.Install("fonts/Lato-Bold.ttf");
            _bgMaskImage = Image.Load("moreBGtemp.png");
            _noBgMask = Image.Load("profilecardtemplate.png");
            _noBgMaskOverlay = Image.Load("ProfileMASK.png");
        }

        public static void GenerateProfileWithBg(string avatarUrl, string backgroundUrl, string name, int rank,
            int level, int ep, string outputPath)
        {
            using (var output = new Image<Rgba32>(900, 500))
            {
                DrawBackground(backgroundUrl, output, new Size(900,500));

                DrawMask(_bgMaskImage, output, new Size(900, 500));

                DrawStats(rank, level, ep, output, new System.Numerics.Vector2(240, 435), new System.Numerics.Vector2(530, 435), new System.Numerics.Vector2(850, 435), Rgba32.Gray);

                DrawTitle(name, output, new System.Numerics.Vector2(300, 300), Rgba32.White);

                DrawAvatar(avatarUrl, output, new Rectangle(73, 273, 155, 155));

                output.Save(outputPath);
            }//dispose of output to help save memory
        }

        public static void GenerateProfile(string avatarUrl, string name, int rank, int level, int ep,
            string outputPath)
        {
            using (var output = new Image<Rgba32>(890, 150))
            {
                DrawMask(_noBgMask, output, new Size(1000, 150));

                DrawAvatar(avatarUrl, output, new Rectangle(26, 15, 121, 121));

                DrawMask(_noBgMaskOverlay, output, new Size(1000, 150));

                DrawStats(rank, level, ep, output, new System.Numerics.Vector2(200, 92), new System.Numerics.Vector2(480, 92), new System.Numerics.Vector2(830, 92), Rgba32.Black);

                DrawTitle(name, output, new System.Numerics.Vector2(200, -5), Rgba32.FromHex("#2398e1"));

                

                output.Save(outputPath);
            }//dispose of output to help save memory
        }

        private static void DrawMask(Image<Rgba32> mask, Image<Rgba32> output, Size size)
        {

            output.DrawImage(mask, 1, size, new Point(0, 0));

        }

        private static void DrawBackground(string backgroundUrl, Image<Rgba32> output, Size size)
        {
            using (Image<Rgba32> background = Image.Load(backgroundUrl))//900x500
            {
                //draw on the background
                output.DrawImage(background, 1, size , new Point(0, 0));
            }//once draw it can be disposed as its no onger needed in memory
        }

        private static void DrawAvatar(string avatarUrl, Image<Rgba32> output, Rectangle rec)
        {
            var avatarPosition = rec;

            using (var avatar = Image.Load(avatarUrl)) // 57x57
            {
                avatar.Resize(new ImageSharp.Processing.ResizeOptions
                {
                    Mode = ImageSharp.Processing.ResizeMode.Crop,
                    Size = avatarPosition.Size
                });

                output.DrawImage(avatar, 1, avatarPosition.Size, avatarPosition.Location);
            }
        }

        private static void DrawStats(int rank, int level, int ep, Image<Rgba32> output, Vector2 posRank, Vector2 posLevel, Vector2 posEP, Rgba32 color)
        {

            // measure each string and split the margin between them??
            var font = new Font(_titleFont, 42, FontStyle.Bold);

            var rankText = $"Rank: {rank}";
            var levelText = $"Level: {level}";
            var epText = $"EP: {ep}";

            //var fontSpan = new FontSpan(font, 72); //imagesharp renders at 72 

            var rankSize = TextMeasurer.Measure(rankText, font, 72); // find the width of the rank text
            var epSize = TextMeasurer.Measure(epText, font, 72); // find the width of the EP text

            var left = posRank.X + rankSize.Width; // find the point the rankText stops
            var right = posEP.X - epSize.Width; // find the point the epText starts

            var posLevel2 = new Vector2(left + (right - left) / 2, posRank.Y); // find the point halfway between the 2 other bits of text

            output.DrawText(rankText, font, color, posRank, new ImageSharp.Drawing.TextGraphicsOptions
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            });
            output.DrawText(levelText, font, color, posLevel2, new ImageSharp.Drawing.TextGraphicsOptions
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            output.DrawText(epText, font, color, posEP, new ImageSharp.Drawing.TextGraphicsOptions
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            });
        }

        private static void DrawTitle(string name, Image<Rgba32> output, Vector2 pos, Rgba32 color)
        {
            output.DrawText(name, new Font(_titleFont, 60, FontStyle.Bold), color, pos);
        }

    }
}
