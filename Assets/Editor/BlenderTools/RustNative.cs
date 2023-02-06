using System.Runtime.InteropServices;

public static class Rust {
    [DllImport("setcursorpos")]
    public static extern void set_cursor_pos(int x, int y);
}