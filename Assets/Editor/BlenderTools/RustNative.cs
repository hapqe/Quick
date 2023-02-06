using System.Runtime.InteropServices;

public static class Rust
{

    // if windows
#if UNITY_EDITOR_WIN

    [DllImport("setcursorpos")]
    public static extern void set_cursor_pos(int x, int y);

#endif

    // if linux
#if UNITY_EDITOR_LINUX
    
    [DllImport("libsetcursorpos.so")]
    public static extern void set_cursor_pos(int x, int y);

#endif

}