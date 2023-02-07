using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RimGen.Lib
{
    public class Generator
    {
        // 3440x1440 UI x1.5
        // навык 0 - 1x36 px (ШхВ)
        // навык 1 - 6x36
        // навык 2 - 12x36
        // навык 3 - 18x36
        // навык 3 - 24x36
        // навык 5 - 29x36
        // навык 10 - 59x36
        // навык 13 - 77x36
        // навык 15 - 59x36
        // навык 16 - 94x36
        // навык 20 - 79x36
        const double oneSkillPointWidth = 5.75;

        //0 - 1px
        //5 - 24px
        //10 - 48px
        //15 - 72px

        const int height = 24;
        const int width = 24;

        // 1920x1080
        // const int topAttrStepHeight = 27;
        // const int leftOffset = 1030;
        // const int firstAttrTopOffset = 329;
        // static Point genBtnCenterPos = new Point(1325, 260);
        // const double oneSkillPointWidth = 4.8;
        //const int fire1LeftOffset = -13;//смещение 1 огонька влево
        //const int fire1TopOffset = 10;
        //const int fire2LeftOffset = -8;//смещение 2 огоньков влево
        //const int fire2TopOffset = 13;

        // 3440x1440
        const double topAttrStepHeight = 40.5; // высота навыка плюс отступ до след.
        const int leftOffset = 1824; // смещение от левого края экрана до навыка
        const int firstAttrTopOffset = 404; // смещение от верха экрана до навыка
        static Point genBtnCenterPos = new Point(2270, 305);
        const int fire1LeftOffset = -19;//смещение 1 огонька влево
        const int fire1TopOffset = 14;
        const int fire2LeftOffset = -13;//смещение 2 огоньков влево
        const int fire2TopOffset = 20;


        const int shootingTopOffset = 405;
        const int craftTopOffset = 675;

        static Color setColor = Color.FromArgb(-12566206); //"{Name=ff404142, ARGB=(255, 64, 65, 66)}"
        static Color notSetColor = Color.FromArgb(-14013652); //"{Name=ff2a2b2c, ARGB=(255, 42, 43, 44)}"

        //static Color setFireColor = Color.FromArgb(-3229); //"{Name=fffff363, ARGB=(255, 255, 243, 99)}"
        static Color setFireColor = Color.FromArgb(-2976); //"{Name=fffff363, ARGB=(255, 255, 243, 99)}"

        public static string Generate(List<Condition> conditions, int maxSecondsTimeout = 30)
        {
            var resultMsg = "";

            try
            {
                Game.Init();
            }
            catch (Exception e)
            {
                return e.Message;
            }

            var start = true;
            var startDate = DateTime.Now;

            while (DateTime.Now < startDate.AddSeconds(maxSecondsTimeout))
            {
                Game.SendGenerateNew(genBtnCenterPos);
                if (start)
                {
                    Thread.Sleep(100);
                    start = false;
                }
                else
                {
                    Thread.Sleep(15);
                }

                Bitmap screen = null;
                try
                {
                    screen = Game.GetScreenshot();                    
                    var conditionsSucceed = 0;
                    foreach (var condition in conditions)
                    {
                        int valueOffset = condition.MinValue == 0 ? 1 : (int)Math.Ceiling(condition.MinValue * oneSkillPointWidth);                        
                        int attrTopOffset = firstAttrTopOffset + (int)Math.Ceiling((int)condition.Attr * topAttrStepHeight);

                        //цвет навыка
                        var colorPoint = new Point(leftOffset + valueOffset, attrTopOffset);
                        var color = screen.GetPixel(colorPoint.X, colorPoint.Y);

                        //цвет огонька
                        var fire1ColorPoint = new Point(leftOffset + fire1LeftOffset, attrTopOffset + fire1TopOffset);
                        var fire1Color = screen.GetPixel(fire1ColorPoint.X, fire1ColorPoint.Y);
                        var fire2ColorPoint = new Point(leftOffset + fire2LeftOffset, attrTopOffset + fire2TopOffset);
                        var fire2Color = screen.GetPixel(fire2ColorPoint.X, fire2ColorPoint.Y);
                        
                        var isSkillColorMatch = color == setColor;
                        var isFire1ColorMatch = fire1Color == setFireColor;
                        var isFire2ColorMatch = fire2Color == setFireColor;

                        // Debug - отмечаем где проверяли пикселы
                        screen.SetPixel(colorPoint.X, colorPoint.Y, Color.Blue);
                        screen.SetPixel(fire1ColorPoint.X, fire1ColorPoint.Y, Color.Blue);
                        screen.SetPixel(fire2ColorPoint.X, fire2ColorPoint.Y, Color.Blue);
                        //SaveScreenForConditions(screen, conditions);

                        if (Form1.FireAttr)
                        {
                            if (isSkillColorMatch && (isFire1ColorMatch || isFire2ColorMatch))
                            {
                                conditionsSucceed++;
                            }
                        }
                        else
                        {
                            if (isSkillColorMatch)
                            {
                                conditionsSucceed++;
                            }
                        }
                    }

                    if (conditionsSucceed == conditions.Count)
                    {
                        if (Form1.SaveScreen)
                        {
                            SaveScreenForConditions(screen, conditions);
                        }
                        
                        resultMsg = Form1.Lang == "ru" ? "Готово" : "Done";
                        break;
                    }
                }
                catch (Exception e)
                {
                    resultMsg = Form1.Lang == "ru" ? "Прервано" : "Interrupted";
                    break;
                }
                finally
                {
                    if (screen != null)
                    {
                        screen.Dispose();
                    }
                }

                //var craftColor = screen.GetPixel(leftOffset + 72, craftTopOffset);
                //var argb = craftColor.ToArgb();

                //if (craftColor == setColor)
                //{
                //    isFound = true;
                //    Game.SaveScreenshot();


                //    Rectangle cropRect = new Rectangle(leftOffset - 140, craftTopOffset, 170, 24);
                //    Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
                //    using (Graphics g = Graphics.FromImage(target))
                //    {
                //        g.DrawImage(screen, new Rectangle(0, 0, target.Width, target.Height),
                //                         cropRect,
                //                         GraphicsUnit.Pixel);
                //    }
                //    target.Save("crop.png", ImageFormat.Png);
                //    target.Dispose();

                //    break;
                //}
                //else
                //{
                //}

                screen.Dispose();
            }

            return resultMsg;
        }

        private static void SaveScreenForConditions(Bitmap screen, List<Condition> conditions)
        {
            string attrsString = "";
            foreach (var cond in conditions)
            {
                attrsString += $"{cond.Attr.ToString()}-{cond.MinValue}_";
            }
            string screenshotPath = $"rim_gen_{attrsString}_{DateTime.Now.ToShortDateString()}_{DateTime.Now.ToString("HH.mm.ss")}.png";
            screen.Save(screenshotPath, ImageFormat.Png);
        }
    }
}
