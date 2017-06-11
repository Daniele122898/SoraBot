using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using ImageSharp;
using ImageSharp.Drawing.Brushes;
using SixLabors.Fonts;

namespace Sora_Bot_1.SoraBot.Services.EPService
{
    public static class ProfileImageProcessing
    {
        private static FontCollection _fontCollection;
        private static Font _iconFont;
        private static Font _textFont;
        private static Font _titleFont;

        public static void Initialize()
        {
            _fontCollection = new FontCollection();
            _iconFont = _fontCollection.Install("fonts/fontawesome-webfont.ttf");
            _titleFont = _fontCollection.Install("fonts/Lato-Bold.ttf");
            _textFont = _fontCollection.Install("fonts/Lato-Regular.ttf");
        }

        public static void GenerateProfileWithBg(string avatarUrl, string backgroundUrl, string name, int rank,
            int level, int ep, string outputPath)
        {
            using (var output = new Image<Rgba32>(900, 500))
            {
                DrawBackground(backgroundUrl, output, new ImageSharp.Size(900, 500));

                DrawMask("moreBGtemp.png", output, new ImageSharp.Size(900, 500));

                DrawStats(rank, level, ep, output, new System.Numerics.Vector2(240, 435), new System.Numerics.Vector2(530, 435), new System.Numerics.Vector2(860, 435), Rgba32.Gray);

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
                DrawBackground("profilecardtemplate.png", output, new ImageSharp.Size(1000, 150));

                DrawAvatar(avatarUrl, output, new Rectangle(26, 15, 121, 121));

                DrawMask("ProfileMASK.png", output, new ImageSharp.Size(1000, 150));

                DrawStats(rank, level, ep, output, new System.Numerics.Vector2(200, 92), new System.Numerics.Vector2(480, 92), new System.Numerics.Vector2(850, 92), Rgba32.Black);

                DrawTitle(name, output, new System.Numerics.Vector2(200, -5), Rgba32.FromHex("#2398e1"));

                

                output.Save(outputPath);
            }//dispose of output to help save memory
        }

        private static void DrawMask(string maskUrl, Image<Rgba32> output, ImageSharp.Size size)
        {
            using (Image<Rgba32> background = Image.Load(maskUrl))//900x500
            {
                //draw on the background
                output.DrawImage(background, 1, size, new Point(0, 0));
            }//once draw it can be disposed as its no onger needed in memory
        }

        private static void DrawBackground(string backgroundUrl, Image<Rgba32> output, ImageSharp.Size size)
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

            // rank
            output.DrawText($"Rank: {rank}", font, color, posRank, new ImageSharp.Drawing.TextGraphicsOptions
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            });
            output.DrawText($"Level: {level}", font, color, posLevel, new ImageSharp.Drawing.TextGraphicsOptions
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            output.DrawText($"EP: {ep}", font, color, posEP, new ImageSharp.Drawing.TextGraphicsOptions
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
