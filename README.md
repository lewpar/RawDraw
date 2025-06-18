# Linux Raw Buffer Drawing
This code shows how you can access the raw frame buffer in a Linux environment to draw onto the screen.

## Usage
```cs
using var buffer = new FrameBuffer(new FrameBufferOptions
{
    Path = "/dev/fb0"
});

while (true)
{
    buffer.Clear(Color.Black);
    
    buffer.FillRect(new Rectangle(0, 0, 100, 100), Color.Red);
    buffer.DrawText(1, 1, "Hello World", Color.White);

    buffer.SwapBuffers();

    await Task.Delay(1);
}
```

## Notes
- Linux user must be in the `video` and `input` user groups.

## Dependencies
- `fbset` - Used to get the frame buffer information (size, color depth, etc..)