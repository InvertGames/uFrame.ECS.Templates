using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Invert.Common;
using Invert.Common.UI;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.IOC;
using Invert.uFrame.ECS;
using uFrame.Attributes;
using uFrame.ECS;
using Object = UnityEngine.Object;

[UnityEditor.CustomEditor(typeof(uFrame.ECS.Entity))]
public class EntityEditor : Editor
{
    private static Dictionary<int, List<string>> _markers;
    private static IRepository _repository1;
    private static IPlatformDrawer _drawer;

    private static Dictionary<int, List<string>> Markers
    {
        get { return _markers ?? (_markers = new Dictionary<int, List<string>>()); }
        set { _markers = value; }
    }

    static EntityEditor()
    {
        // Init
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
    }

    public static IRepository Repository
    {
        get { return _repository1 ?? (_repository1 = InvertApplication.Container.Resolve<IRepository>()); }
        set { _repository1 = value; }
    }

    public static IPlatformDrawer Drawer
    {
        get { return _drawer ?? (_drawer = InvertApplication.Container.Resolve<IPlatformDrawer>()); }
    }

    static void HierarchyItemCB(int instanceID, Rect selectionRect)
    {
        var go  = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go == null) return;

        var components = go.GetComponents(typeof (EcsComponent));
        var iconRect = new Rect().WithSize(16, 16).InnerAlignWithUpperRight(selectionRect).AlignHorisonallyByCenter(selectionRect).Translate(-5,0);

        foreach (var component in components)
        {
            string icon = null;

            InvertApplication.SignalEvent<IFetchIcon>(_ => icon = _.FetchIcon(component));

            if(string.IsNullOrEmpty(icon)) continue;
            var cCache = GUI.color;
            GUI.color = new Color(cCache.r, cCache.g, cCache.b, cCache.a * 0.7f);
            Drawer.DrawImage(iconRect,icon,true);
            iconRect = iconRect.LeftOf(iconRect).Translate(-5,0);
            GUI.color = cCache;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        InvertApplication.SignalEvent<IDrawUnityInspector>(_ => _.DrawInspector(target));
    }
}



public interface IFetchIcon
{
    string FetchIcon(object component);
}

public interface IDrawUnityInspector
{
    void DrawInspector(Object target);
}
public class UnityInspectors : DiagramPlugin, IDrawUnityInspector, IDataRecordPropertyChanged, IFetchIcon
{
    private WorkspaceService _workspaceService;

    public WorkspaceService WorkspaceService
    {
        get { return _workspaceService ?? (_workspaceService = Container.Resolve<WorkspaceService>()); }
    }
    private IRepository _repository;
    private IPlatformDrawer _drawer;
    private Dictionary<string, string> _iconsCache;
    //    private UserSettings _currentUser;

    public IRepository Repository
    {
        get { return _repository ?? (_repository = Container.Resolve<IRepository>()); }
    }
  
    public IPlatformDrawer Drawer
    {
        get { return _drawer ?? (_drawer = Container.Resolve<IPlatformDrawer>()); }
    }



    //public string CurrentUserId
    //{
    //    get { return EditorPrefs.GetString("UF_CurrentUserId", string.Empty); }
    //    set
    //    {
    //        EditorPrefs.SetString("UF_CurrentUserId",value);
    //    }
    //}

    //public UserSettings CurrentUser
    //{
    //    get { return _currentUser ?? (_currentUser = Repository.GetSingle<UserSettings>(CurrentUserId)); }
    //}

    public override void Loaded(UFrameContainer container)
    {
        base.Loaded(container);

    }

    public void DrawInspector(Object target)
    {
        GUIHelpers.IsInsepctor = true;
        var entityComponent = target as Entity;
        if (entityComponent != null)
        {
            if (Repository != null)
            {
                EditorGUILayout.HelpBox("0 = Auto Assign At Runtime", MessageType.Info);

            }

        }
        var component = target as EcsComponent;
        //if (component != null)
        //{


        if (Repository != null)
        {
            var attribute = target.GetType().GetCustomAttributes(typeof(uFrameIdentifier), true).OfType<uFrameIdentifier>().FirstOrDefault();

            if (attribute != null)
            {
                var item = Repository.GetSingle<ComponentNode>(attribute.Identifier);
                if (component != null)
                {

                    var inspectorBounds = new Rect(0, 0, Screen.width, Screen.height);
                    var iconBounds = new Rect().WithSize(16, 16).InnerAlignWithUpperRight(inspectorBounds);
                    Drawer.DrawImage(iconBounds,"CommandIcon",true);

                    //if (GUIHelpers.DoToolbarEx("System Handlers"))
                    //{
                    //    foreach (
                    //   var handlerNode in
                    //       Repository.All<HandlerNode>()
                    //           .Where(p => p.EntityGroup != null && p.EntityGroup.Item == item))
                    //    {
                    //        if (GUILayout.Button(handlerNode.Name))
                    //        {
                    //            Execute(new NavigateToNodeCommand()
                    //            {
                    //                Node = handlerNode,
                    //                Select = true
                    //            });
                    //        }

                    //    }
                    //}
                    if (GUIHelpers.DoToolbarEx("uFrame Designer"))
                    {
                        foreach (
                   var handlerNode in
                       Repository.All<HandlerNode>()
                           .Where(p => p.HandlerInputs.Any(x=>x.Item != null && x.Item.SelectComponents.Contains(item))))
                        {
                           
                            EditorGUILayout.BeginHorizontal();
                            var text = handlerNode.Name;
                            if (GUILayout.Button(text,EditorStyles.toolbarButton))
                            {
                                Execute(new NavigateToNodeCommand()
                                {
                                    Node = handlerNode,
                                    Select = true
                                });
                            }
                            var meta = handlerNode.Meta as EventMetaInfo;
                            if (meta != null && meta.Dispatcher && component.gameObject.GetComponent(meta.SystemType) == null)
                            {
                                if (GUILayout.Button("+ " + meta.SystemType.Name,EditorStyles.toolbarButton))
                                {

                                    component.gameObject.AddComponent(meta.SystemType);
                                }
                            }

                            EditorGUILayout.EndHorizontal();

                        }
                        if (GUILayout.Button("Edit In Designer",EditorStyles.toolbarButton))
                        {
                            Execute(new NavigateToNodeCommand()
                            {
                                Node = item,
                                Select = true
                            });
                        }
                    }



                }

            }
        }

        //}

    }

    //public class UserSettings : IDataRecord
    //{
    //    [IDataRecord]
    //    public string UserName { get; set; }

    //    public int EntityId { get; set; }

    //    public int StartingId { get; }

    //    public string Identifier { get; set; }
    //    public IRepository Repository { get; set; }
    //    public bool Changed { get; set; }
    //    public IEnumerable<string> ForeignKeys { get { yield break; } }
    //}
    public void PropertyChanged(IDataRecord record, string name, object previousValue, object nextValue)
    {
        var typedRecord = record as ComponentNode;
        if (typedRecord != null && name == "CustomIcon")
        {
            var typeName = typedRecord.TypeName;
            IconsCache[typeName] = (string)nextValue;
        }
    }

    public Dictionary<string, string> IconsCache
    {
        get { return _iconsCache ?? (_iconsCache = new Dictionary<string, string>()); }
        set { _iconsCache = value; }
    }

    public string FetchIcon(object component)
    {
        string icon;
        var type = component.GetType();
        var typeName = type.Name;
        if (IconsCache.TryGetValue(typeName, out icon))
            return icon;

        if (Repository != null)
        {
            var attribute =
                type
                    .GetCustomAttributes(typeof (uFrameIdentifier), true)
                    .OfType<uFrameIdentifier>()
                    .FirstOrDefault();

            if (attribute != null)
            {
                var item = Repository.GetSingle<ComponentNode>(attribute.Identifier);
                IconsCache[typeName] = item.CustomIcon;
            }
        }

        IconsCache.TryGetValue(typeName, out icon);
        return icon;
    }
}