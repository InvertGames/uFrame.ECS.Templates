using System;
using System.CodeDom;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.IOC;
using Invert.uFrame.ECS;
using uFrame.Actions;
using uFrame.Attributes;
using uFrame.ECS;
using uFrame.Kernel;
using UnityEditor;

namespace Invert.uFrame.ECS.Templates
{
    [InitializeOnLoad]
    public class EcsTemplates : DiagramPlugin
    {
        static EcsTemplates()
        {
            InvertApplication.CachedAssembly(typeof(EcsTemplates).Assembly);
            InvertApplication.CachedAssembly(typeof(UFAction).Assembly);
            InvertApplication.CachedTypeAssembly(typeof(UFAction).Assembly);
        }
        public override void Initialize(UFrameContainer container)
        {
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<SystemNode, SystemTemplate>();
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<ComponentNode, ComponentTemplate>();
            //            RegisteredTemplateGeneratorsFactory.RegisterTemplate<ComponentGroupNode,ComponentGroupTemplate>();
            //          RegisteredTemplateGeneratorsFactory.RegisterTemplate<ComponentGroupNode,ComponentGroupManagerTemplate>();
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<EventNode, EventTemplate>();
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<GroupNode, GroupTemplate>();
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<GroupNode, GroupItemTemplate>();
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<HandlerNode, HandlerTemplate>();
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<HandlerNode, EditableHandlerTemplate>();
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<uFrameDatabaseConfig, DbLoaderTemplate>();
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<uFrameDatabaseConfig, EcsEditorExtensionTemplate>();
            //RegisteredTemplateGeneratorsFactory.RegisterTemplate<PropertyChangedNode, PropertyHandlerTemplate>();
            //            RegisteredTemplateGeneratorsFactory.RegisterTemplate<EntityNode, EntityTemplate>();

            RegisteredTemplateGeneratorsFactory.RegisterTemplate<CustomActionNode, CustomActionEditableTemplate>();
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<CustomActionNode, CustomActionDesignerTemplate>();      
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<CodeActionNode, CodeActionEditableTemplate>();
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<CodeActionNode, CodeActionDesignerTemplate>();
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<SystemNode, LoaderTemplate>();
        }
    }

    [TemplateClass(TemplateLocation.DesignerFile,"{0}Loader"), AsPartial]
    [RequiresNamespace("uFrame.Kernel")]
    [RequiresNamespace("uFrame.ECS")]
    [ForceBaseType(typeof(SystemLoader))]
    [WithMetaInfo]
    public partial class LoaderTemplate : IClassTemplate<SystemNode>, ITemplateCustomFilename
    {
        public string OutputPath
        {
            get { return Path2.Combine("Modules", Ctx.Data.Name, Ctx.Data.Name); }
        }

        public string Filename
        {
            get
            {
                return Path2.Combine("Systems", Ctx.Data.Name + "Loader.cs");
            }
        }

        public bool CanGenerate
        {
            get { return true; }
        }

        public void TemplateSetup()
        {
            this.Ctx.CurrentDeclaration.Name = Ctx.Data.Name + "Loader";
        }

        public TemplateContext<SystemNode> Ctx { get; set; }
        public IEnumerable<GroupNode> Groups
        {
            get
            {
                return Ctx.Data.Graph.NodeItems.OfType<ISystemGroupProvider>().SelectMany(p => p.GetSystemGroups()).OfType<GroupNode>();
            }
        }

        [GenerateMethod, AsOverride]
        public void Load()
        {
            //Ctx._("EcsSystem system = null");
            Ctx._("this.AddSystem<{0}>()", Ctx.Data.Name);
         
        }


    }


    [TemplateClass(TemplateLocation.DesignerFile)]
    [RequiresNamespace("uFrame.Kernel")]
    [RequiresNamespace("uFrame.ECS")]
    [WithMetaInfo]
    public partial class EcsEditorExtensionTemplate : IClassTemplate<uFrameDatabaseConfig>, ITemplateCustomFilename
    {
        public string OutputPath
        {
            get { return Path2.Combine("Editor", Ctx.Data.Title); }
        }

        public string Filename
        {
            get
            {
                return Path2.Combine("Editor", Ctx.Data.Title + "EditorExtensions.cs");
            }
        }

        public bool CanGenerate
        {
            get { return true; }
        }

        public void TemplateSetup()
        {
            this.Ctx.CurrentDeclaration.Name = Ctx.Data.Title + "EditorExtensions";
            this.Ctx.CurrentDeclaration.BaseTypes.Clear();
            this.Ctx.CurrentDeclaration.Attributes |= MemberAttributes.Static;
            this.Ctx.CurrentDeclaration.CustomAttributes.Add(
                new CodeAttributeDeclaration(new CodeTypeReference(typeof (InitializeOnLoadAttribute))));

        }

        public TemplateContext<uFrameDatabaseConfig> Ctx { get; set; }

        [GenerateMethod]
        public void AddKernel()
        {
            var codeAttributeDeclaration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(MenuItem)));
            
            codeAttributeDeclaration.Arguments.Add(
                new CodeAttributeArgument(new CodePrimitiveExpression(string.Format("GameObject/Create {0} Kernel", Ctx.Data.Title))));
            codeAttributeDeclaration.Arguments.Add(
               new CodeAttributeArgument(new CodePrimitiveExpression(false)));
            codeAttributeDeclaration.Arguments.Add(
               new CodeAttributeArgument(new CodePrimitiveExpression(0)));
            this.Ctx.CurrentMethod.CustomAttributes.Add(codeAttributeDeclaration);
            this.Ctx.CurrentMethod.Attributes |= MemberAttributes.Static;
            Ctx._("Invert.uFrame.ECS.Templates.EcsEditorExtensionTemplate.AddEcsKernelWith<{0}.{1}>()", Ctx.Data.Namespace , Ctx.Data.Title + "Loader");
        }

        public static void AddEcsKernelWith<TLoaderType>() where TLoaderType : Component
        {
            GameObject obj = new GameObject("_Kernel");
            obj.AddComponent<uFrameKernel>();
            obj.AddComponent<EcsSystemLoader>();
            obj.AddComponent<TLoaderType>();

        }
    }


    [TemplateClass(TemplateLocation.DesignerFile, "{0}Loader"), AsPartial]
    [RequiresNamespace("uFrame.Kernel")]
    [RequiresNamespace("uFrame.ECS")]
    [ForceBaseType(typeof(SystemLoader))]
    public partial class DbLoaderTemplate : IClassTemplate<uFrameDatabaseConfig>, ITemplateCustomFilename
    {
        public string OutputPath
        {
            get { return Path2.Combine( Ctx.Data.Title + "Loader.cs"); }
        }

        public string Filename
        {
            get
            {
                return Path2.Combine( Ctx.Data.Title + "Loader.cs");
            }
        }

        public bool CanGenerate
        {
            get { return Systems.Any(); }
        }

        public void TemplateSetup()
        {
            this.Ctx.CurrentDeclaration.Name = Ctx.Data.Title + "Loader";
        }

        public TemplateContext<uFrameDatabaseConfig> Ctx { get; set; }
        public IEnumerable<SystemNode> Systems
        {
            get { return Ctx.Data.Database.AllOf<SystemNode>(); }
        }

        [GenerateMethod, AsOverride]
        public void Load()
        {
            Ctx._("EcsSystem system = null");
            foreach (var system in Systems)
            {
                Ctx._("system = this.AddSystem<{0}>()", system.Name);
            }
           

        }

        

    }

    [TemplateClass(TemplateLocation.DesignerFile)]
    [RequiresNamespace("uFrame.Kernel")]
    [RequiresNamespace("UnityEngine")]
    public partial class SequenceTemplate<TType> : IClassTemplate<TType>,ITemplateCustomFilename where TType : ISequenceNode
    {
        public string Filename
        {
             get { return Path2.Combine("Handlers", Ctx.Data.Name + "Handler.cs"); }
        }
        public string OutputPath
        {
            get { return Path2.Combine("Handlers", Ctx.Data.Name + "Handler.cs"); }
        }

        public virtual bool CanGenerate
        {
            get { return Ctx.Data.CanGenerate; }
        }

        public void TemplateSetup()
        {
            this.Ctx.CurrentDeclaration.BaseTypes.Clear();
            this.Ctx.CurrentDeclaration.Name = Ctx.Data.HandlerMethodName;
        }

        public TemplateContext<TType> Ctx { get; set; }


        [GenerateMethod]
        public virtual void Execute()
        {
            if (DebugSystem.IsDebugMode)
                this.Ctx.SetType(typeof(IEnumerator));

            var csharpVisitor = new HandlerCsharpVisitor()
            {
                _ = Ctx
            };
            Ctx.Data.Accept(csharpVisitor);

            if (DebugSystem.IsDebugMode)
                Ctx._("yield break");
           
        }

       
    }

    [TemplateClass(TemplateLocation.DesignerFile)]
    [RequiresNamespace("uFrame.Kernel")]
    [RequiresNamespace("UnityEngine")]
    public partial class HandlerTemplate : SequenceTemplate<HandlerNode>
    {
        public override bool CanGenerate
        {
            get { return !Ctx.Data.CodeHandler; }
        }
    }

    [TemplateClass(TemplateLocation.Both)]
    [RequiresNamespace("uFrame.Kernel")]
    [RequiresNamespace("UnityEngine")]
    public partial class EditableHandlerTemplate : IClassTemplate<HandlerNode>, ITemplateCustomFilename
    {
        [GenerateProperty(TemplateLocation.DesignerFile), WithField]
        public object Event
        {
            get
            {

                this.Ctx.SetType(Ctx.Data.EventType);
                return null;
            }
            set
            {

            }
        }

        [GenerateProperty(TemplateLocation.DesignerFile), WithField]
        public EcsSystem System { get; set; }

        [TemplateSetup]
        public void SetName()
        {
            Ctx.CurrentDeclaration.IsPartial = true;
            Ctx.CurrentDeclaration.Name = Ctx.Data.HandlerMethodName;
            Ctx.CurrentDeclaration.BaseTypes.Clear();
            if (Ctx.IsDesignerFile) 
            foreach (var item in Ctx.Data.FilterInputs)
            {
                var context = item.FilterNode;
                if (context == null) continue;
                CreateFilterProperty(item, context);
            }

        }

        private void CreateFilterProperty(IFilterInput input, IMappingsConnectable inputFilter)
        {
            Ctx.CurrentDeclaration._public_(inputFilter.ContextTypeName, input.HandlerPropertyName);

        }
        [GenerateMethod, Inside((TemplateLocation.EditableFile))]
        public void Execute()
        {
            
        }

        public string OutputPath
        {
            get { return null; }
        }

        public bool CanGenerate
        {
            get { return Ctx.Data.CodeHandler; }
        }

        public void TemplateSetup()
        {
            
        }

        public TemplateContext<HandlerNode> Ctx { get; set; }

        public string Filename
        {
            get
            {
                if (Ctx.IsDesignerFile)
                {
                    return Path2.Combine("CustomHandlers", Ctx.Data.Name + ".designer.cs");
                }
                return Path2.Combine("CustomHandlers", Ctx.Data.Name + ".cs");
            }
        }
    }


    [TemplateClass(TemplateLocation.DesignerFile)]
    [RequiresNamespace("uFrame.Kernel")]
    [RequiresNamespace("UnityEngine")]
    public partial class PropertyHandlerTemplate : SequenceTemplate<PropertyChangedNode>
    {

    }
    //[TemplateClass(TemplateLocation.Both,"{0}PrefabPool")]
    //[RequiresNamespace("uFrame.Kernel")]
    //[RequiresNamespace("UnityEngine")]
    //[RequiresNamespace("uFrame.ECS")]
    //[ForceBaseType(typeof(EntityPrefabPool)), AsPartial]
    //public partial class EntityTemplate : IClassTemplate<EntityNode>
    //{

    //    public string OutputPath
    //    {
    //        get { return Path2.Combine(Ctx.Data.Graph.Name, "Entities"); }
    //    }

    //    public bool CanGenerate
    //    {
    //        get { return true; }
    //    }

    //    public void TemplateSetup()
    //    {
    //        Ctx.CurrentDeclaration.Name = Ctx.Data.Name + "PrefabPool";
    //        if (!Ctx.IsDesignerFile)
    //        {
    //            Ctx.CurrentDeclaration.BaseTypes.Clear();
    //        }
    //        else
    //        {
    //            foreach (var item in Ctx.Data.Components)
    //            {
    //                Ctx.CurrentDeclaration.CustomAttributes.Add(
    //                    new CodeAttributeDeclaration(typeof(RequireComponent).ToCodeReference(),
    //                        new CodeAttributeArgument(new CodeSnippetExpression(string.Format("typeof({0})", item.Name)))));
    //            }
    //        }


    //    }

    //    public TemplateContext<EntityNode> Ctx { get; set; }
    //}
     

    [TemplateClass(TemplateLocation.DesignerFile), AsPartial]
    [RequiresNamespace("uFrame.Kernel")]
    [WithMetaInfo]
    public partial class SystemTemplate : IClassTemplate<SystemNode>, ITemplateCustomFilename
    {
        public string Filename
        {
            get
            {
                if (Ctx.Data.Name == null)
                {
                     throw new Exception(Ctx.Data.Name + " Graph is null");
                } 
                return Path2.Combine("Systems", Ctx.Data.Name + ".cs");
            }
        }
        public string OutputPath
        {
            get { return Path2.Combine(Ctx.Data.Graph.Name, "Systems"); }
        }

        public IEnumerable<ComponentNode> Components
        {
            get
            {
                return Ctx.Data.Graph.NodeItems.OfType<ISystemGroupProvider>().SelectMany(p=>p.GetSystemGroups()).OfType<ComponentNode>().Distinct();

            }
        }

        public IEnumerable<GroupNode> Groups
        {
            get
            {
                return Ctx.Data.Graph.NodeItems.OfType<ISystemGroupProvider>().SelectMany(p => p.GetSystemGroups()).OfType<GroupNode>().Distinct();
            }
        }

        public IEnumerable<HandlerNode> EventHandlers
        {
            get
            {
                return Ctx.Data.EventHandlers;
            }
        }


        public bool CanGenerate
        {
            get { return true; }
        }

        public void TemplateSetup()
        {

        }

        public TemplateContext<SystemNode> Ctx { get; set; }
    }

    [TemplateClass(TemplateLocation.DesignerFile)]
    [ForceBaseType(typeof(EcsComponent)), AsPartial]
    [RequiresNamespace("uFrame.ECS")]
    [RequiresNamespace("UnityEngine")]
    [WithMetaInfo]
    public partial class ComponentTemplate : IClassTemplate<ComponentNode>, ITemplateCustomFilename
    {
        public string Filename
        {
            get
            {
                return Path2.Combine("Components", Ctx.Data.Name + ".cs");
            }
        }
        // Not used now
        public string OutputPath
        {
            get { return Path2.Combine("Extensions", Ctx.Data.Graph.Name, "Components"); }
        }

        public bool CanGenerate
        {
            get { return true; }
        }

        public void TemplateSetup()
        {
            foreach (var item in Ctx.Data.GetMembers())
            {
                Ctx.TryAddNamespace(item.MemberType.Namespace);
            }
        }

        public TemplateContext<ComponentNode> Ctx { get; set; }
    }

    [TemplateClass(TemplateLocation.DesignerFile, "{0}Group"), AsPartial]
    [RequiresNamespace("uFrame.ECS")]
    [RequiresNamespace("uFrame.Kernel")]
    [RequiresNamespace("UniRx")]
    public partial class GroupTemplate : IClassTemplate<GroupNode>,ITemplateCustomFilename
    {

        public IEnumerable<ComponentNode> SelectComponents
        {
            get
            {
                return Ctx.Data.Require.Select(p => p.SourceItem).OfType<ComponentNode>();
            }
        }
         public string Filename
        {
            get
            {
                return Path2.Combine("Groups", Ctx.Data.Name + "Group.cs");
            }
        }

        public string OutputPath { get; set; }

        public bool CanGenerate
        {
            get { return true; }
        }

        public void TemplateSetup()
        {

            this.Ctx.SetBaseType("ReactiveGroup<{0}>", Ctx.Data.Name);
        }

        public TemplateContext<GroupNode> Ctx { get; set; }
    }

    [TemplateClass(TemplateLocation.DesignerFile, "{0}"), AsPartial]
    [RequiresNamespace("uFrame.ECS")]
    [RequiresNamespace("uFrame.Kernel")]
    [RequiresNamespace("UniRx")]
    public partial class GroupItemTemplate : GroupItem, IClassTemplate<GroupNode>, ITemplateCustomFilename
    {

        public IEnumerable<ComponentNode> SelectComponents
        {
            get
            {
                return Ctx.Data.Require.Select(p => p.SourceItem).OfType<ComponentNode>();
            }
        }
        public string OutputPath
        {
            get { return Path2.Combine("Modules", Ctx.Data.Graph.Name, "Groups"); }
        }
        public string Filename
        {
            get
            {
                return Path2.Combine("Groups", Ctx.Data.Name + ".cs");
            }
        }

        public bool CanGenerate
        {
            get { return true; }
        }

        public void TemplateSetup()
        {

        }

        public TemplateContext<GroupNode> Ctx { get; set; }
    }

    [TemplateClass(TemplateLocation.DesignerFile), AsPartial]
    [RequiresNamespace("uFrame.ECS")]
    public partial class EventTemplate : IClassTemplate<EventNode>, ITemplateCustomFilename
    {
        public IEnumerable<PropertiesChildItem> Properties
        {
            get
            {
                foreach (var item in Ctx.Data.Properties)
                {
                    if (item.Name == "EntityId" && Ctx.Data.Dispatcher) continue;
                    yield return item;
                }
            }
        }
        public string Filename
        {
            get
            {
                return Path2.Combine("Events", Ctx.Data.Name + ".cs");
            }
        }
        public string OutputPath
        {
            get { return Path2.Combine("Library", Ctx.Data.Graph.Name, "Events"); }
        }

        public bool CanGenerate
        {
            get { return true; }
        }

        public void TemplateSetup()
        {
            //this.Ctx.CurrentDeclaration.CustomAttributes.Add(
            //    new CodeAttributeDeclaration(
            //        typeof(uFrameEvent).ToCodeReference(), new CodeAttributeArgument(new CodePrimitiveExpression(Ctx.Data.Name))
            //        ));
            //if (Ctx.Data.Dispatcher)
            //{
            //    this.Ctx.CurrentDeclaration.Name += "Dispatcher";
            //    this.Ctx.SetBaseType(typeof(EcsDispatcher));
            //}
            //else
            //{
                this.Ctx.CurrentDeclaration.Name = Ctx.Data.Name;
                if (!Ctx.IsDesignerFile)
                {
                    this.Ctx.CurrentDeclaration.BaseTypes.Clear();
                }
            //}
        }

        public TemplateContext<EventNode> Ctx { get; set; }
    }

    [TemplateClass(TemplateLocation.DesignerFile), ForceBaseType(typeof(UFAction)), AsPartial]
    [RequiresNamespace("uFrame.ECS")]
    [RequiresNamespace("UnityEngine")]
    public partial class CustomActionDesignerTemplate : IClassTemplate<CustomActionNode>, ITemplateCustomFilename
    {
        public string Filename
        {
            get { return Path2.Combine("CustomActions", Ctx.Data.Name + ".designer.cs"); }
        }
        public string OutputPath
        {
            get { return Path2.Combine("CustomActions", Ctx.Data.Name + ".designer.cs"); }
        }

        public bool CanGenerate
        {
            get { return true; }
        }

        public void TemplateSetup()
        {
            Ctx.CurrentDeclaration.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(ActionTitle).ToCodeReference(),
                        new CodeAttributeArgument(new CodePrimitiveExpression(string.IsNullOrEmpty(Ctx.Data.ActionTitle) ? Ctx.Data.Name : Ctx.Data.ActionTitle))));
            foreach (var item in Ctx.Data.Inputs)
            {
                var field = Ctx.CurrentDeclaration._public_(item.RelatedTypeName, item.Name);
                field.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(In).ToCodeReference(),
                    new CodeAttributeArgument(new CodePrimitiveExpression(item.Name))));
            }
            foreach (var item in Ctx.Data.Outputs)
            {
                var field = Ctx.CurrentDeclaration._public_(item.RelatedTypeName, item.Name);
                field.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(Out).ToCodeReference(),
                    new CodeAttributeArgument(new CodePrimitiveExpression(item.Name))));
            }
            foreach (var item in Ctx.Data.Branches)
            {
                var field = Ctx.CurrentDeclaration._public_(typeof(Action), item.Name);
                field.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(Out).ToCodeReference(),
                    new CodeAttributeArgument(new CodePrimitiveExpression(item.Name))));
            }
        }

        public TemplateContext<CustomActionNode> Ctx { get; set; }
        
    }

    [TemplateClass(TemplateLocation.EditableFile), ForceBaseType(typeof(UFAction)), AsPartial]
    [RequiresNamespace("uFrame.ECS")]
    public partial class CustomActionEditableTemplate : IClassTemplate<CustomActionNode>, ITemplateCustomFilename
    {

        public string OutputPath
        {
            get { return Path2.Combine("CustomActions", Ctx.Data.Name + ".cs"); }
        }

        public bool CanGenerate
        {
            get { return true; }
        }

        public void TemplateSetup()
        {
            this.Ctx.CurrentDeclaration.BaseTypes.Clear();
            Ctx.CurrentDeclaration.public_override_func(typeof(void), "Execute");
            
        }

        public TemplateContext<CustomActionNode> Ctx { get; set; }

        public string Filename
        {
            get { return Path2.Combine("CustomActions", Ctx.Data.Name + ".cs"); }
        }
    }
    [TemplateClass(TemplateLocation.DesignerFile), ForceBaseType(typeof(UFAction)), AsPartial]
    [RequiresNamespace("uFrame.ECS")]
    [RequiresNamespace("UnityEngine")]
    [RequiresNamespace("uFrame.Attributes")]
    public partial class CodeActionDesignerTemplate : IClassTemplate<CodeActionNode>, ITemplateCustomFilename
    {
        public string Filename
        {
            get { return Path2.Combine("CodeActions", Ctx.Data.Name + ".designer.cs"); }
        }
        public string OutputPath
        {
            get { return Path2.Combine("CustomActions", Ctx.Data.Name + ".designer.cs"); }
        }

        public bool CanGenerate
        {
            get { return true; }
        }

        public void TemplateSetup()
        {
            Ctx.CurrentDeclaration.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(ActionTitle).ToCodeReference(),
                        new CodeAttributeArgument(new CodePrimitiveExpression(Ctx.Data.Name))));
    
        }

        public TemplateContext<CodeActionNode> Ctx { get; set; }

    }

    [TemplateClass(TemplateLocation.EditableFile), ForceBaseType(typeof(UFAction)), AsPartial]
    [RequiresNamespace("uFrame.ECS")]
    [RequiresNamespace("uFrame.Attributes")]
    [RequiresNamespace("UnityEngine")]
    public partial class CodeActionEditableTemplate : IClassTemplate<CodeActionNode>, ITemplateCustomFilename
    {

        public string OutputPath
        {
            get { return Path2.Combine("CodeActions", Ctx.Data.Name + ".cs"); }
        }

        public bool CanGenerate
        {
            get { return true; }
        }

        public void TemplateSetup()
        {
            this.Ctx.CurrentDeclaration.BaseTypes.Clear();
            Ctx.CurrentDeclaration.public_override_func(typeof(void), "Execute");

        }

        public TemplateContext<CodeActionNode> Ctx { get; set; }

        public string Filename
        {
            get { return Path2.Combine("CustomActions", Ctx.Data.Name + ".cs"); }
        }
    }

    public class _CONTEXTITEM_ : _ITEMTYPE_
    {
        public override string TheType(TemplateContext context)
        {
            return base.TheType(context);
        }
    }

    public class _CONTEXT_ : _ITEMTYPE_
    {
        public override string TheType(TemplateContext context)
        {
            return base.TheType(context) + "Context";
        }
    }
}