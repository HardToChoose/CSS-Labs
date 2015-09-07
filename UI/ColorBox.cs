using System;
using System.Windows.Media;

namespace UI
{
    public static class ColorBox
    {
        private static readonly Brush[] ColorList = new Brush[]
        {
            new SolidColorBrush(Color.FromRgb(178,223,138)),
            new SolidColorBrush(Color.FromRgb(51,160,44)),
            new SolidColorBrush(Color.FromRgb(166,206,227)),
            new SolidColorBrush(Color.FromRgb(31,120,180)),
            new SolidColorBrush(Color.FromRgb(251,154,153)),
            new SolidColorBrush(Color.FromRgb(227,26,28)),
            new SolidColorBrush(Color.FromRgb(253,191,111)),
            new SolidColorBrush(Color.FromRgb(255,127,0)),
            new SolidColorBrush(Color.FromRgb(202,178,214)),
            new SolidColorBrush(Color.FromRgb(106,61,154)),
            new SolidColorBrush(Colors.Gold),
            new SolidColorBrush(Color.FromRgb(177,89,40))
        };

        private static int salt = 0;
        private static Random random = new Random(Environment.TickCount);

        public static void Shuffle()
        {
            salt = random.Next(0, ColorList.Length);
        }

        public static Brush GetNext(int number)
        {
            return ColorList[(number + salt) % ColorList.Length];
        }
    }
}
