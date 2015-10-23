using System.CodeDom;
using System.Collections.Generic;
using Invert.Core.GraphDesigner;
using uFrame.ECS;
using UniRx;
using UnityEngine;

namespace Invert.uFrame.ECS.Templates
{
    [RequiresNamespace("uFrame.ECS")]
    [RequiresNamespace("UniRx")]
    [NamespacesFromItems]
    public partial class ComponentTemplate
    {

        public static int _ComponentIds = 1;
        [GenerateProperty]
        public int ComponentID
        {
            get
            {
                Ctx._("return {0}", _ComponentIds++);
                return 0;
            }
        }

        [ForEach("Properties"), GenerateProperty]
        public IObservable<PropertyChangedEvent<_ITEMTYPE_>> _Name_Observable
        {
            get
            {
                Ctx._("return _{0}Observable ?? (_{0}Observable = new Subject<PropertyChangedEvent<{1}>>())", Ctx.Item.Name,Ctx.TypedItem.RelatedTypeName);
                return null;
            }
        }

        [ForEach("Properties"), GenerateProperty, WithName]
        public _ITEMTYPE_ Property
        {
            get
            {
                var valueField = Ctx.CurrentDeclaration._private_(string.Format("{0}", Ctx.TypedItem.RelatedTypeName),
                    "_{0}", Ctx.Item.Name);
                valueField.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(SerializeField).ToCodeReference()));
                Ctx._("return {0}", valueField.Name);
                return null;
            }
            set
            {
                Ctx._("Set{0}(value)", Ctx.Item.Name);
            }
        }

        [ForEach("Properties"), GenerateMethod]
        public void Set_Name_(_ITEMTYPE_ value)
        {
            var valueFieldObservable = Ctx.CurrentDeclaration._private_(string.Format("Subject<PropertyChangedEvent<{0}>>", Ctx.TypedItem.RelatedTypeName),
                   "_{0}Observable", Ctx.Item.Name);

            var valueFieldEvent = Ctx.CurrentDeclaration._private_(string.Format("PropertyChangedEvent<{0}>", Ctx.TypedItem.RelatedTypeName),
                 "_{0}Event", Ctx.Item.Name);

            Ctx._("SetProperty(ref _{0}, value, ref _{0}Event, _{0}Observable)", Ctx.Item.Name);

        }

        //[ForEach("Collections"), GenerateProperty, WithName, WithLazyField(null,typeof(SerializeField))]
        //public List<_ITEMTYPE_> Collection { get; set; }

        [ForEach("Collections"), GenerateProperty, WithNameFormat("{0}")]
        public ReactiveCollection<_ITEMTYPE_> CollectionReactive {
            get
            {
                var valueField = Ctx.CurrentDeclaration._private_(string.Format("{0}[]", Ctx.TypedItem.RelatedTypeName),
                  "_{0}", Ctx.Item.Name);
                valueField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof (SerializeField))));

                var field = Ctx.CurrentDeclaration._private_(string.Format("ReactiveCollection<{0}>", Ctx.TypedItem.RelatedTypeName),
                    "_{0}Reactive", Ctx.Item.Name);

                Ctx._if("{0} == null", field.Name)
                    .TrueStatements._("{0} = new ReactiveCollection<{1}>(_{2})", field.Name, Ctx.TypedItem.RelatedTypeName, Ctx.Item.Name, valueField.Name);
                Ctx._("return {0}", field.Name);
                return null;
            }
        }
    }
}