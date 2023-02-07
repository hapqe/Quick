use mouse_rs::Mouse;

#[no_mangle]
pub extern "C" fn set_cursor_pos(x: i32, y: i32) {
    let mouse = Mouse::new();
    mouse.move_to(x, y).unwrap();
}