/*
using Invert.uFrame;
using Invert.uFrame.Editor;


public class MyCustomElementItemTypesPlugin : DiagramPlugin
{
    public override decimal LoadPriority
    {
        get { return -1; }
    }

    public override bool Enabled
    {
        get { return true; }
        set
        {

        }
    }
    public override void Initialize(uFrameContainer container)
    {
        var typeContainer = uFrameEditor.TypesContainer;
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(int[,]), Group = "Custom Types", Label = "int[,]", IsPrimitive = false }, "int[,]");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(float[,]), Group = "Custom Types", Label = "float[,]",  IsPrimitive = false }, "float[,]");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(UnityEngine.Mesh), Group = "Custom Types", Label = "Mesh", IsPrimitive = false }, "Mesh");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(Hexagon), Group = "Custom Types", Label = "Hexagon", IsPrimitive = false }, "Hexagon");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(UnityEngine.Color), Group = "Custom Types", Label = "Color", IsPrimitive = false }, "Color");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(UnityEngine.AnimationCurve), Group = "Custom Types", Label = "Animation Curve", IsPrimitive = false }, "Animation Curve");
        typeContainer.RegisterInstance(new ElementItemType() { Type = typeof(UnitViewModel[]), Group = "Custom Types", Label = "UnitViewModel[]", IsPrimitive = false }, "UnitViewModel[]");
    }
}*/