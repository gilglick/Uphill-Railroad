using UnityEngine;

[CreateAssetMenu(fileName = "New Theme", menuName = "WSM Game Studio/Spline Mesh Renderer/Theme", order = 1)]
public class SMR_Theme : ScriptableObject
{
    public bool supportsVerticalBuilding = true;

    public Texture AddIcon;
    public Texture DeleteIcon;
    public Texture ArrowUpIcon;
    public Texture CurvedArrowUpIcon;
    public Texture CurvedArrowDownIcon;
    public Texture CurvedArrowLeftIcon;
    public Texture CurvedArrowRightIcon;
    public Texture MeshIcon;
    public Texture LogIcon;

    public Texture SmallAddIcon;
    public Texture SmallDeleteIcon;
    public Texture SmallArrowUpIcon;
    public Texture SmallCurvedArrowUpIcon;
    public Texture SmallCurvedArrowDownIcon;
    public Texture SmallCurvedArrowLeftIcon;
    public Texture SmallCurvedArrowRightIcon;
    public Texture SmallMeshIcon;
    public Texture SmallLogIcon;
}
