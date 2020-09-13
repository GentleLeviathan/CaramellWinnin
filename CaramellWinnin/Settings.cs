namespace CaramellWinnin
{
    public struct Settings
    {
        public bool AutoLoadMusic;
        public string MusicLink;
        public bool UseImage;
        public int ColorStep;
        public int TimerInterval;
        public double OpacityPercent;

        public Settings(bool _AutoLoadMusic = true, string _MusicLink = "https://www.youtube.com/watch?v=V-KSyjmhwE0", bool _UseImage = false, int _ColorStep = 32, int _TimerInterval = 4, double _OpacityPercent = 5)
        {
            AutoLoadMusic = _AutoLoadMusic;
            MusicLink = _MusicLink;
            UseImage = _UseImage;
            ColorStep = _ColorStep;
            TimerInterval = _TimerInterval;
            OpacityPercent = _OpacityPercent;
        }
    }
}
