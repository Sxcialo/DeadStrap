namespace Bloxstrap.Enums
{
    public enum CursorType
    {
        [EnumSort(Order = 1)]
        [EnumName(FromTranslation = "Common.Default")]
        Default,

        [EnumSort(Order = 3)]
        From2006,

        [EnumSort(Order = 2)]
        From2013,

        [EnumSort(Order = 4)]
        [EnumName(StaticName = "Red glow dot")]
        RedGlowDot,

        [EnumSort(Order = 5)]
        [EnumName(StaticName = "Black dot")]
        BlackDot
    }
}
