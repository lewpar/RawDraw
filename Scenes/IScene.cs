namespace RawDraw.Scenes;

public interface IScene
{
    void Render(FrameBuffer buffer, long deltaTime);
}