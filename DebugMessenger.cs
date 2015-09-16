using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.IOC;

using uFrame.Attributes;
using uFrame.ECS;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace uFrame.ECS
{

    public class DebugMessenger : DiagramPlugin, IBreakpointHit, ICommandExecuted
    {
        public override decimal LoadPriority
        {
            get { return 100; }
        }

        public override void Initialize(UFrameContainer container)
        {
            base.Initialize(container);
            DebugSystem = container.Resolve<DebugSystem>();
            if (Disposer != null)
            {
                Disposer.Dispose();
            }
            Disposer = DebugService.DebugInfo.Subscribe(SignalInfo);
        }

        public DebugSystem DebugSystem { get; set; }

        public override void Loaded(UFrameContainer container)
        {
            base.Loaded(container);

        }

        public IDisposable Disposer { get; set; }


        private void SignalInfo(DebugInfo _)
        {
            if (DebugSystem == null) return;
            DebugSystem.OnActionExecuting(_);
            if (_.Result == 1)
            {
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPaused = true;
                }
            }
            else
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.isPaused = false;
                    EditorApplication.isPlaying = true;
                }

            }

        }

        public void BreakpointHit()
        {

        }

        public void CommandExecuted(ICommand command)
        {
            if (command is ContinueCommand || command is StepCommand)
            {
                EditorApplication.isPaused = false;
                EditorApplication.isPlaying = true;
            }
        }
    }
}
