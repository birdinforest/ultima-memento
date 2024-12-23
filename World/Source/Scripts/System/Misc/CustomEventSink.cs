using Server.Mobiles;

namespace Server
{
    public delegate void LandChangedEventHandler(LandChangedArgs e);

    public class LandChangedArgs
    {
        public readonly PlayerMobile Mobile;
        public readonly Land OldLand;
        public readonly Land NewLand;

        public LandChangedArgs(PlayerMobile mobile, Land oldLand, Land newLand)
        {
            Mobile = mobile;
            OldLand = oldLand;
            NewLand = newLand;
        }
    }

    public class CustomEventSink
    {
        public static event LandChangedEventHandler LandChanged;

        public static void InvokeLandChanged(LandChangedArgs e)
        {
            if (LandChanged != null)
                LandChanged(e);
        }
    }
}