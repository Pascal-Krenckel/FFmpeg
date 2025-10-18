namespace FFmpeg.Helper;

internal static unsafe class StringHelper
{

    public static int StrLen(byte* p)
    {
        int i = 0;
        while (*p++ != 0)
            i++;
        return i;
    }

}
