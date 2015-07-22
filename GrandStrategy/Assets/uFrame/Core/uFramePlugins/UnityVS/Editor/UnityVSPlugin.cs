using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.IOC;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEditor;
using UnityEngine;

namespace Assets.uFrameComplete.uFrame.Editor.DiagramPlugins.UnityVS
{
    public class UnityVSPlugin : DiagramPlugin, ICompileEvents
    {
        public override bool EnabledByDefault
        {
            get { return false; }
        }

        public override void Initialize(UFrameContainer container)
        {
            ListenFor<ICompileEvents>();
        }

        public void PreCompile(INodeRepository repository, IGraphData diagramData)
        {
            
        }

        public void FileGenerated(CodeFileGenerator generator)
        {
           
        }

        public void PostCompile(INodeRepository repository, IGraphData diagramData)
        {
            EditorApplication.ExecuteMenuItem("Visual Studio Tools/Generate Project Files");
        }

        public void FileSkipped(CodeFileGenerator codeFileGenerator)
        {
            
        }
    }
}
