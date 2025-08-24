[System.Serializable]
public class SceneElementDebug : SceneElement
{

    public SceneElementDebug(SceneElement element) : base(element.x, element.y, element.distance, element.xRotationOffset, element.action)
    {
        this.list_id = element.list_id;
    }

    public override string ToString()
    {
        return $"Debug Info";
    }
}