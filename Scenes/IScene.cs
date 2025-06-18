namespace RawDraw.Scenes;

public interface IScene
{
    void Input(int keyCode, bool state);
    void Render(FrameBuffer buffer, long deltaTime);
}